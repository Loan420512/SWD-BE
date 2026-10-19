import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { FloppyDisk, PencilSimple, Plus, X } from '@phosphor-icons/react';
import { createSticker, listStickers, updateSticker, uploadFile, type AdminSticker, type StickerInput } from '../../api/admin';
import { price, size } from '../../app/copy';
import { Glyph } from '../../app/glyph';
import { useKiosk } from '../../kiosk';
import { useExpiry } from './AdminLayout';

const EMPTY: StickerInput = { name: '', imageUrl: '', widthMm: 40, heightMm: 40, price: 30000, active: true };

export function AdminStickers() {
  const kiosk = useKiosk();
  const expire = useExpiry();
  const [stickers, setStickers] = useState<AdminSticker[] | null>(null);
  const [editing, setEditing] = useState<AdminSticker | null>(null);
  const [form, setForm] = useState<StickerInput>(EMPTY);
  const [aspect, setAspect] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const load = useCallback(async () => {
    const result = await listStickers();
    if (result.ok) setStickers(result.data);
    else {
      expire(result.expired);
      setError(result.message);
    }
  }, [expire]);

  useEffect(() => {
    void load();
  }, [load]);

  async function onFile(file: File | undefined) {
    if (!file) return;
    setBusy(true);
    const uploaded = await uploadFile('sticker', file);
    setBusy(false);
    if (!uploaded.ok) {
      expire(uploaded.expired);
      setError(uploaded.message);
      return;
    }
    const image = new Image();
    image.onload = () => {
      const ratio = image.naturalHeight / image.naturalWidth;
      setAspect(ratio);
      setForm((f) => ({ ...f, imageUrl: uploaded.data, heightMm: Math.round(f.widthMm * ratio * 10) / 10, name: f.name || file.name.replace(/\.[^.]+$/, '') }));
    };
    image.src = uploaded.data;
  }

  function setWidth(widthMm: number) {
    setForm((f) => ({ ...f, widthMm, heightMm: aspect ? Math.round(widthMm * aspect * 10) / 10 : f.heightMm }));
  }

  async function submit(event: FormEvent) {
    event.preventDefault();
    if (!form.imageUrl) {
      setError('Tải ảnh sticker lên trước.');
      return;
    }
    setBusy(true);
    const result = editing ? await updateSticker(editing.id, form) : await createSticker(form);
    setBusy(false);
    if (!result.ok) {
      expire(result.expired);
      setError(result.message);
      return;
    }
    setError(null);
    setEditing(null);
    setForm(EMPTY);
    setAspect(null);
    await load();
    kiosk.reloadCatalog();
  }

  function edit(sticker: AdminSticker) {
    setEditing(sticker);
    setAspect(sticker.heightMm / sticker.widthMm);
    setForm({ name: sticker.name, imageUrl: sticker.imageUrl, widthMm: sticker.widthMm, heightMm: sticker.heightMm, price: sticker.price, active: sticker.active });
  }

  return (
    <section className="admin-split">
      <form className="form panel" onSubmit={submit}>
        <h1>{editing ? `Sửa ${editing.name}` : 'Thêm sticker'}</h1>
        <label>
          Ảnh sticker
          <input type="file" accept="image/png,image/webp" onChange={(e) => void onFile(e.target.files?.[0])} />
        </label>
        {form.imageUrl ? (
          <div className="sticker-art preview">
            <img src={form.imageUrl} alt="" />
            <span className="ruler">{size(form.widthMm, form.heightMm)}</span>
          </div>
        ) : null}
        <label>
          Tên
          <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required maxLength={150} />
        </label>
        <div className="row">
          <label className="grow">
            Rộng (mm)
            <input type="number" min={5} max={200} step={0.5} value={form.widthMm} onChange={(e) => setWidth(Number(e.target.value))} />
          </label>
          <label className="grow">
            Cao (mm)
            <input type="number" min={5} max={200} step={0.5} value={form.heightMm} onChange={(e) => setForm({ ...form, heightMm: Number(e.target.value) })} />
          </label>
        </div>
        <label>
          Giá (đồng)
          <input type="number" min={0} step={1000} value={form.price} onChange={(e) => setForm({ ...form, price: Number(e.target.value) })} />
        </label>
        <label className="check">
          <input type="checkbox" checked={form.active} onChange={(e) => setForm({ ...form, active: e.target.checked })} />
          Đang bán
        </label>
        {error ? <p className="error">{error}</p> : null}
        <div className="row">
          {editing ? (
            <button
              type="button"
              className="button ghost"
              onClick={() => {
                setEditing(null);
                setForm(EMPTY);
              }}
            >
              <Glyph icon={X} />
              Huỷ sửa
            </button>
          ) : null}
          <button type="submit" className="button" disabled={busy}>
            <Glyph icon={editing ? FloppyDisk : Plus} />
            {busy ? 'Đang lưu…' : editing ? 'Lưu thay đổi' : 'Thêm sticker'}
          </button>
        </div>
      </form>

      <div>
        <h2>{stickers ? `${stickers.length} sticker` : 'Đang tải…'}</h2>
        <div className="sticker-grid compact">
          {(stickers ?? []).map((sticker) => (
            <article key={sticker.id} className={sticker.active ? 'sticker-card' : 'sticker-card muted'}>
              <div className="sticker-art">
                <img src={sticker.imageUrl} alt={sticker.name} />
                <span className="ruler">{size(sticker.widthMm, sticker.heightMm)}</span>
              </div>
              <h3>{sticker.name}</h3>
              <p className="meta">
                {price(sticker.price)}
                {sticker.active ? '' : ', ngừng bán'}
              </p>
              <button type="button" className="button small ghost" onClick={() => edit(sticker)}>
                <Glyph icon={PencilSimple} />
                Sửa
              </button>
            </article>
          ))}
        </div>
      </div>
    </section>
  );
}
