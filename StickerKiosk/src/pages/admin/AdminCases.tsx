import { useEffect, useMemo, useRef, useState, type FormEvent } from 'react';
import { ArrowCounterClockwise, Calculator, FloppyDisk, PaintBrush, Plus, Prohibit, Trash } from '@phosphor-icons/react';
import { createCase, deleteCase, saveCaseFit, updateCase, uploadFile, type CaseInput } from '../../api/admin';
import { price } from '../../app/copy';
import { Glyph } from '../../app/glyph';
import {
  DEFAULT_NEST,
  DEFAULT_POSE,
  countSet,
  paintMask,
  useKiosk,
  useSession,
  worldToBack,
  type AssetPose,
  type CaseId,
  type CaseOffer,
  type ModelId,
  type OccupancyMask,
  type PhoneModel,
} from '../../kiosk';
import type { BrushMode, FitterHandle } from '../../stage';
import { useExpiry } from './AdminLayout';
import { PoseEditor } from './PoseEditor';

function phoneHeightMm(model: PhoneModel | undefined): number | null {
  const first = model?.dimensions?.match(/[\d.,]+/)?.[0];
  const value = first ? Number(first.replace(',', '.')) : NaN;
  return Number.isFinite(value) && value > 20 ? value : null;
}

export function AdminCases() {
  const session = useSession();
  const kiosk = useKiosk();
  const catalog = session.catalog.status === 'ready' ? session.catalog.catalog : null;
  const [selected, setSelected] = useState<CaseId | 'new' | null>(null);
  const [newModel, setNewModel] = useState<ModelId | null>(null);

  const rows = useMemo(() => (catalog?.models ?? []).flatMap((model) => model.cases.map((offer) => ({ model, offer }))), [catalog]);
  const current = rows.find((row) => row.offer.id === selected) ?? null;

  return (
    <section className="admin-split wide">
      <div className="panel">
        <header className="row spread">
          <h1>Ốp lưng</h1>
          <button
            type="button"
            className="button small"
            disabled={!catalog?.models.length}
            onClick={() => {
              setSelected('new');
              setNewModel(catalog?.models[0]?.id ?? null);
            }}
          >
            <Glyph icon={Plus} />
            Thêm ốp
          </button>
        </header>
        {!catalog ? <p className="empty">Đang tải…</p> : null}
        <ul className="list">
          {rows.map(({ model, offer }) => (
            <li key={offer.id}>
              <button type="button" className={offer.id === selected ? 'list-item on' : 'list-item'} onClick={() => setSelected(offer.id)}>
                <strong>{offer.label}</strong>
                <small>
                  {model.label}, {offer.sku ?? 'chưa có SKU'}, {price(offer.price)}, tồn {offer.stock ?? 'không rõ'}
                </small>
                <span className={offer.fit ? 'tag ok' : 'tag'}>{offer.fit ? 'Đã có model 3D' : 'Chưa có model 3D'}</span>
              </button>
            </li>
          ))}
        </ul>
      </div>
      {selected === 'new' && catalog ? (
        <CaseEditor
          key="new"
          models={catalog.models}
          model={catalog.models.find((m) => m.id === newModel) ?? catalog.models[0]!}
          offer={null}
          onModel={setNewModel}
          onSaved={(id) => {
            kiosk.reloadCatalog();
            setSelected(id);
          }}
        />
      ) : null}
      {current && catalog ? (
        <CaseEditor
          key={current.offer.id}
          models={catalog.models}
          model={current.model}
          offer={current.offer}
          onSaved={() => kiosk.reloadCatalog()}
          onDeleted={() => {
            setSelected(null);
            kiosk.reloadCatalog();
          }}
        />
      ) : null}
      {!selected ? <p className="empty">Chọn một ốp để sửa giá, tồn kho, hoặc nhập model 3D.</p> : null}
    </section>
  );
}

function CaseEditor(props: {
  readonly models: readonly PhoneModel[];
  readonly model: PhoneModel;
  readonly offer: CaseOffer | null;
  readonly onModel?: (id: ModelId) => void;
  readonly onSaved: (id: CaseId) => void;
  readonly onDeleted?: () => void;
}) {
  const { offer, model } = props;
  const expire = useExpiry();
  const fitter = useRef<FitterHandle>(null);
  const [form, setForm] = useState<CaseInput>({
    modelId: model.id,
    sku: offer?.sku ?? '',
    material: offer?.material ?? 'Silicone',
    color: offer?.color ?? '',
    price: offer?.price ?? 150000,
    stockQuantity: offer?.stock ?? 10,
  });
  const paired = props.models.find((item) => item.id === form.modelId) ?? model;
  const [url, setUrl] = useState(offer?.fit?.modelUrl ?? '');
  const [pose, setPose] = useState<AssetPose>(offer?.fit?.pose ?? DEFAULT_POSE);
  const [heightMm, setHeightMm] = useState(offer?.fit?.sheet.heightMm ?? (phoneHeightMm(model) ?? 147) + 3);
  const [loaded, setLoaded] = useState<{ widthMm: number; depthMm: number } | null>(null);
  const [mask, setMask] = useState<OccupancyMask | null>(offer?.fit?.sheet.mask ?? null);
  const [sheetSize, setSheetSize] = useState<{ widthMm: number; heightMm: number } | null>(
    offer?.fit ? { widthMm: offer.fit.sheet.widthMm, heightMm: offer.fit.sheet.heightMm } : null,
  );
  const [brush, setBrush] = useState<BrushMode | null>(null);
  const [radiusMm, setRadiusMm] = useState(3);
  const [insetMm, setInsetMm] = useState(offer?.fit?.nest.insetMm ?? DEFAULT_NEST.insetMm);
  const [phoneScale, setPhoneScale] = useState(offer?.fit?.nest.scale ?? DEFAULT_NEST.scale);
  const [message, setMessage] = useState<{ kind: 'error' | 'ok'; text: string } | null>(null);
  const [busy, setBusy] = useState(false);
  const maskRef = useRef(mask);
  maskRef.current = mask;
  const fitKey = `${url}|${pose.back}${pose.up}|${heightMm}`;
  const previousFit = useRef(fitKey);

  useEffect(() => {
    if (previousFit.current === fitKey) return;
    previousFit.current = fitKey;
    setMask(null);
    setSheetSize(null);
    setBrush(null);
    fitter.current?.stage?.showMask(null);
  }, [fitKey]);

  useEffect(() => {
    fitter.current?.stage?.setNest({ insetMm, scale: phoneScale });
  }, [insetMm, phoneScale, loaded]);

  useEffect(() => {
    const stage = fitter.current?.stage;
    if (!stage || !brush || !loaded || !sheetSize) {
      stage?.setBrush(null, radiusMm, () => {});
      return;
    }
    stage.setBrush(brush, radiusMm, (x, y) => {
      const current = maskRef.current;
      if (!current) return;
      const at = worldToBack(x, y, sheetSize);
      const next = paintMask(current, at, radiusMm, brush === 'open');
      maskRef.current = next;
      setMask(next);
      stage.showMask(next);
    });
  }, [brush, radiusMm, loaded, sheetSize]);

  async function saveCommerce(event: FormEvent) {
    event.preventDefault();
    setBusy(true);
    const result = offer ? await updateCase(offer.id, form) : await createCase(form);
    setBusy(false);
    if (!result.ok) {
      expire(result.expired);
      setMessage({ kind: 'error', text: result.message });
      return;
    }
    setMessage({ kind: 'ok', text: 'Đã lưu thông tin bán.' });
    props.onSaved(offer ? offer.id : (result.data as unknown as CaseId));
  }

  async function removeCase() {
    if (!offer) return;
    if (!window.confirm(`Xoá ốp ${offer.label} khỏi danh mục?`)) return;
    setBusy(true);
    const result = await deleteCase(offer.id);
    setBusy(false);
    if (!result.ok) {
      expire(result.expired);
      setMessage({ kind: 'error', text: result.message });
      return;
    }
    props.onDeleted?.();
  }

  async function onFile(file: File | undefined) {
    if (!file) return;
    setBusy(true);
    const uploaded = await uploadFile('case', file);
    setBusy(false);
    if (!uploaded.ok) {
      expire(uploaded.expired);
      setMessage({ kind: 'error', text: uploaded.message });
      return;
    }
    setUrl(uploaded.data);
    setPose(DEFAULT_POSE);
  }

  function runBake() {
    const stage = fitter.current?.stage;
    if (!stage) return;
    const result = stage.bake();
    stage.showMask(result.mask);
    setMask(result.mask);
    setSheetSize({ widthMm: result.widthMm, heightMm: result.heightMm });
    setMessage(
      result.printableCells
        ? { kind: 'ok', text: `Vùng dán được: ${result.printableCells} ô 1 mm. Xanh là được dán, đỏ là cấm. Kéo cọ để sửa trước khi lưu.` }
        : { kind: 'error', text: 'Không có vùng dán nào. Kiểm tra mặt lưng đã hướng về camera chưa.' },
    );
  }

  function restoreSavedMask() {
    const saved = offer?.fit;
    if (!saved || !loaded) return;
    setMask(saved.sheet.mask);
    setSheetSize({ widthMm: saved.sheet.widthMm, heightMm: saved.sheet.heightMm });
    fitter.current?.stage?.showMask(saved.sheet.mask);
  }

  async function saveFit() {
    if (!offer || !mask || !sheetSize) return;
    setBusy(true);
    const result = await saveCaseFit(offer.id, {
      modelUrl: url,
      pose,
      heightMm: sheetSize.heightMm,
      widthMm: sheetSize.widthMm,
      mask,
      nest: { insetMm, scale: phoneScale },
    });
    setBusy(false);
    if (!result.ok) {
      expire(result.expired);
      setMessage({ kind: 'error', text: result.message });
      return;
    }
    setMessage({ kind: 'ok', text: 'Đã lưu model 3D. Khách thấy ốp này ngay ở bàn thiết kế.' });
    props.onSaved(offer.id);
  }

  return (
    <div className="panel editor">
      <form className="form" onSubmit={saveCommerce}>
        <h2>{offer ? `${offer.label}, ${model.label}` : 'Ốp mới'}</h2>
        <label>
          Mẫu máy đi cùng ốp
          <select value={form.modelId} onChange={(e) => {
            const id = Number(e.target.value) as ModelId;
            setForm({ ...form, modelId: id });
            props.onModel?.(id);
          }}>
            {props.models.map((m) => (
              <option key={m.id} value={m.id}>
                {m.label}{m.phone ? '' : ' (chưa có model 3D)'}
              </option>
            ))}
          </select>
        </label>
        <div className="row">
          <label className="grow">
            SKU
            <input value={form.sku} onChange={(e) => setForm({ ...form, sku: e.target.value })} maxLength={100} />
          </label>
          <label className="grow">
            Chất liệu
            <input value={form.material} onChange={(e) => setForm({ ...form, material: e.target.value })} maxLength={100} />
          </label>
          <label className="grow">
            Màu
            <input value={form.color} onChange={(e) => setForm({ ...form, color: e.target.value })} maxLength={50} />
          </label>
        </div>
        <div className="row">
          <label className="grow">
            Giá (đồng)
            <input type="number" min={0} step={1000} value={form.price} onChange={(e) => setForm({ ...form, price: Number(e.target.value) })} />
          </label>
          <label className="grow">
            Tồn kho
            <input type="number" min={0} value={form.stockQuantity} onChange={(e) => setForm({ ...form, stockQuantity: Number(e.target.value) })} />
          </label>
        </div>
        <div className="row">
          <button type="submit" className="button small" disabled={busy}>
            <Glyph icon={offer ? FloppyDisk : Plus} />
            {offer ? 'Lưu thông tin bán' : 'Tạo ốp'}
          </button>
          {offer ? (
            <button type="button" className="button small danger" disabled={busy} onClick={() => void removeCase()}>
              <Glyph icon={Trash} />
              Xoá ốp
            </button>
          ) : null}
        </div>
      </form>

      {offer ? (
        <div className="form">
          <h2>Model 3D</h2>
          <label>
            File GLB của ốp
            <input type="file" accept=".glb,model/gltf-binary" onChange={(e) => void onFile(e.target.files?.[0])} />
          </label>
          <label>
            Hoặc đường dẫn file đã tải lên
            <input value={url} onChange={(e) => setUrl(e.target.value.trim())} placeholder="/uploads/case/…glb" />
          </label>
          <label>
            Chiều cao thật của ốp (mm)
            <input type="number" min={20} max={400} step={0.1} value={heightMm} onChange={(e) => setHeightMm(Number(e.target.value))} />
          </label>
          {url ? (
            <>
              <PoseEditor
                url={url}
                pose={pose}
                heightMm={heightMm}
                onPose={setPose}
                fitter={fitter}
                onLoaded={(size) => {
                  if (!(size && size.widthMm > 0)) return;
                  setLoaded((current) => {
                    if (current && current.widthMm === size.widthMm && current.depthMm === size.depthMm) return current;
                    return { widthMm: size.widthMm, depthMm: size.depthMm };
                  });
                  const saved = offer?.fit;
                  const samePose = saved && saved.modelUrl === url && saved.pose.back === pose.back && saved.pose.up === pose.up;
                  if (samePose && maskRef.current) fitter.current?.stage?.showMask(maskRef.current);
                }}
                phone={paired.phone ? { url: paired.phone.modelUrl, pose: paired.phone.pose } : null}
                nest={{ insetMm, scale: phoneScale }}
              />
              {loaded ? (
                <p className="meta">
                  Kích thước sau khi chỉnh: rộng {loaded.widthMm.toFixed(1)} mm, cao {heightMm} mm, dày {loaded.depthMm.toFixed(1)} mm.
                </p>
              ) : null}
              <h2>Độ sát của máy trong ốp</h2>
              {paired.phone ? null : <p className="meta">Mẫu máy này chưa có model 3D.</p>}
              <label>
                Khoảng lún của máy vào ốp: {insetMm.toFixed(1)} mm
                <input type="range" min={-4} max={12} step={0.1} value={insetMm} onChange={(e) => setInsetMm(Number(e.target.value))} />
              </label>
              <label>
                Tỉ lệ máy so với chiều cao ốp: {Math.round(phoneScale * 100)}%
                <input type="range" min={0.8} max={1.1} step={0.005} value={phoneScale} onChange={(e) => setPhoneScale(Number(e.target.value))} />
              </label>
              <h2>Vùng được dán</h2>
              <p className="meta">{mask ? `${countSet(mask)} ô đang mở` : 'Chưa có vùng dán'}</p>
              <div className="row">
                <button type="button" className="button small ghost" onClick={runBake} disabled={!loaded}>
                  <Glyph icon={Calculator} />
                  Tính vùng dán
                </button>
                <button type="button" className="button small ghost" onClick={restoreSavedMask} disabled={!offer?.fit || !loaded}>
                  <Glyph icon={ArrowCounterClockwise} />
                  Khôi phục vùng đã lưu
                </button>
                <button type="button" className={brush === 'open' ? 'button small on' : 'button small ghost'} disabled={!mask} onClick={() => setBrush(brush === 'open' ? null : 'open')}>
                  <Glyph icon={PaintBrush} />
                  Cọ mở
                </button>
                <button type="button" className={brush === 'block' ? 'button small on' : 'button small ghost'} disabled={!mask} onClick={() => setBrush(brush === 'block' ? null : 'block')}>
                  <Glyph icon={Prohibit} />
                  Cọ cấm
                </button>
              </div>
              <label>
                Bán kính cọ: {radiusMm.toFixed(1)} mm
                <input type="range" min={1} max={8} step={0.5} value={radiusMm} onChange={(e) => setRadiusMm(Number(e.target.value))} disabled={!mask} />
              </label>
              <div className="row">
                <button type="button" className="button small" onClick={() => void saveFit()} disabled={!mask || countSet(mask) === 0 || busy}>
                  <Glyph icon={FloppyDisk} />
                  Lưu model 3D
                </button>
              </div>
            </>
          ) : (
            <p className="meta">Chưa có file.</p>
          )}
        </div>
      ) : null}
      {message ? <p className={message.kind === 'error' ? 'error' : 'ok'}>{message.text}</p> : null}
    </div>
  );
}
