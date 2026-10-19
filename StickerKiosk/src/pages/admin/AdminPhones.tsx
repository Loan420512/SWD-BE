import { useState, type FormEvent } from 'react';
import { FloppyDisk, Plus, Trash } from '@phosphor-icons/react';
import { createModel, deleteModel, saveModelFit, updateModel, uploadFile } from '../../api/admin';
import { DEFAULT_POSE, useKiosk, useSession, type AssetPose, type ModelId, type PhoneModel } from '../../kiosk';
import { Glyph } from '../../app/glyph';
import { useExpiry } from './AdminLayout';
import { PoseEditor } from './PoseEditor';

export function AdminPhones() {
  const session = useSession();
  const kiosk = useKiosk();
  const expire = useExpiry();
  const catalog = session.catalog.status === 'ready' ? session.catalog.catalog : null;
  const [selected, setSelected] = useState<ModelId | null>(null);
  const [draft, setDraft] = useState({ brand: '', modelName: '', dimensions: '' });
  const [error, setError] = useState<string | null>(null);
  const current = catalog?.models.find((m) => m.id === selected) ?? null;

  async function create(event: FormEvent) {
    event.preventDefault();
    const result = await createModel(draft);
    if (!result.ok) {
      expire(result.expired);
      setError(result.message);
      return;
    }
    setError(null);
    setDraft({ brand: '', modelName: '', dimensions: '' });
    kiosk.reloadCatalog();
    setSelected(result.data as ModelId);
  }

  return (
    <section className="admin-split wide">
      <div className="panel">
        <h1>Mẫu máy</h1>
        <ul className="list">
          {(catalog?.models ?? []).map((model) => (
            <li key={model.id}>
              <button type="button" className={model.id === selected ? 'list-item on' : 'list-item'} onClick={() => setSelected(model.id)}>
                <strong>{model.label}</strong>
                <small>
                  {model.dimensions ?? 'Chưa có kích thước'}, {model.cases.length} ốp
                </small>
                <span className={model.phone ? 'tag ok' : 'tag'}>{model.phone ? 'Có model 3D' : 'Chưa có model 3D'}</span>
              </button>
            </li>
          ))}
        </ul>
        <form className="form" onSubmit={create}>
          <h2>Thêm mẫu máy</h2>
          <div className="row">
            <label className="grow">
              Hãng
              <input value={draft.brand} onChange={(e) => setDraft({ ...draft, brand: e.target.value })} required maxLength={50} />
            </label>
            <label className="grow">
              Tên mẫu
              <input value={draft.modelName} onChange={(e) => setDraft({ ...draft, modelName: e.target.value })} required maxLength={100} />
            </label>
          </div>
          <label>
            Kích thước máy
            <input value={draft.dimensions} onChange={(e) => setDraft({ ...draft, dimensions: e.target.value })} placeholder="147.6 x 71.6 x 7.8 mm" maxLength={100} />
          </label>
          {error ? <p className="error">{error}</p> : null}
          <button type="submit" className="button small">
            <Glyph icon={Plus} />
            Thêm
          </button>
        </form>
      </div>
      {current ? (
        <PhoneEditor
          key={current.id}
          model={current}
          onSaved={() => kiosk.reloadCatalog()}
          onDeleted={() => {
            setSelected(null);
            kiosk.reloadCatalog();
          }}
        />
      ) : (
        <p className="empty">Chọn một mẫu máy để sửa thông tin hoặc nhập model 3D.</p>
      )}
    </section>
  );
}

function PhoneEditor(props: { readonly model: PhoneModel; readonly onSaved: () => void; readonly onDeleted: () => void }) {
  const expire = useExpiry();
  const [identity, setIdentity] = useState({
    brand: props.model.brand,
    modelName: props.model.name,
    dimensions: props.model.dimensions ?? '',
  });
  const [url, setUrl] = useState(props.model.phone?.modelUrl ?? '');
  const [pose, setPose] = useState<AssetPose>(props.model.phone?.pose ?? DEFAULT_POSE);
  const [message, setMessage] = useState<{ kind: 'error' | 'ok'; text: string } | null>(null);
  const height = Number(props.model.dimensions?.match(/[\d.,]+/)?.[0]?.replace(',', '.')) || 150;

  async function saveIdentity(event: FormEvent) {
    event.preventDefault();
    const result = await updateModel(props.model.id, identity);
    if (!result.ok) {
      expire(result.expired);
      setMessage({ kind: 'error', text: result.message });
      return;
    }
    setMessage({ kind: 'ok', text: 'Đã lưu hãng, tên và kích thước.' });
    props.onSaved();
  }

  async function remove() {
    if (props.model.cases.length > 0) {
      setMessage({ kind: 'error', text: 'Mẫu máy này còn ốp. Chuyển hoặc xoá các ốp đó trước.' });
      return;
    }
    if (!window.confirm(`Xoá mẫu ${props.model.label} khỏi danh mục?`)) return;
    const result = await deleteModel(props.model.id);
    if (!result.ok) {
      expire(result.expired);
      setMessage({ kind: 'error', text: result.message });
      return;
    }
    props.onDeleted();
  }

  async function onFile(file: File | undefined) {
    if (!file) return;
    const uploaded = await uploadFile('phone', file);
    if (!uploaded.ok) {
      expire(uploaded.expired);
      setMessage({ kind: 'error', text: uploaded.message });
      return;
    }
    setUrl(uploaded.data);
    setPose(DEFAULT_POSE);
  }

  async function save() {
    const result = await saveModelFit(props.model.id, url, pose);
    if (!result.ok) {
      expire(result.expired);
      setMessage({ kind: 'error', text: result.message });
      return;
    }
    setMessage({ kind: 'ok', text: 'Đã lưu model điện thoại.' });
    props.onSaved();
  }

  return (
    <div className="panel editor">
      <h2>{props.model.label}</h2>
      <form className="form" onSubmit={saveIdentity}>
        <h2>Thông tin mẫu</h2>
        <div className="row">
          <label className="grow">
            Hãng
            <input value={identity.brand} onChange={(e) => setIdentity({ ...identity, brand: e.target.value })} required maxLength={50} />
          </label>
          <label className="grow">
            Tên mẫu
            <input value={identity.modelName} onChange={(e) => setIdentity({ ...identity, modelName: e.target.value })} required maxLength={100} />
          </label>
        </div>
        <label>
          Kích thước máy
          <input
            value={identity.dimensions}
            onChange={(e) => setIdentity({ ...identity, dimensions: e.target.value })}
            placeholder="147.6 x 71.6 x 7.8 mm"
            maxLength={100}
          />
        </label>
        <div className="row">
          <button type="submit" className="button small">
            <Glyph icon={FloppyDisk} />
            Lưu thông tin
          </button>
          <button type="button" className="button small danger" onClick={() => void remove()}>
            <Glyph icon={Trash} />
            Xoá mẫu máy
          </button>
        </div>
      </form>
      <label>
        File GLB của điện thoại
        <input type="file" accept=".glb,model/gltf-binary" onChange={(e) => void onFile(e.target.files?.[0])} />
      </label>
      <label>
        Hoặc đường dẫn file đã tải lên
        <input value={url} onChange={(e) => setUrl(e.target.value.trim())} placeholder="/uploads/phone/…glb" />
      </label>
      {url ? (
        <>
          <PoseEditor url={url} pose={pose} heightMm={height} onPose={setPose} />
          <button type="button" className="button small" onClick={() => void save()}>
            <Glyph icon={FloppyDisk} />
            Lưu model điện thoại
          </button>
        </>
      ) : null}
      {message ? <p className={message.kind === 'error' ? 'error' : 'ok'}>{message.text}</p> : null}
    </div>
  );
}
