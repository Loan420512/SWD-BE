import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { SignIn } from '@phosphor-icons/react';
import { login } from '../../api/admin';
import { PATHS } from '../../app/paths';
import { Glyph } from '../../app/glyph';

export function AdminLogin() {
  const navigate = useNavigate();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function submit(event: FormEvent) {
    event.preventDefault();
    setBusy(true);
    const result = await login(username.trim(), password);
    setBusy(false);
    if (result.ok) navigate(PATHS.admin, { replace: true });
    else setError(result.message);
  }

  return (
    <section className="login">
      <form className="form" onSubmit={submit}>
        <h1>Đăng nhập quản trị</h1>
        <label>
          Tên đăng nhập
          <input value={username} onChange={(e) => setUsername(e.target.value)} autoComplete="username" required />
        </label>
        <label>
          Mật khẩu
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} autoComplete="current-password" required />
        </label>
        {error ? <p className="error">{error}</p> : null}
        <button type="submit" className="button" disabled={busy}>
          <Glyph icon={SignIn} />
          {busy ? 'Đang kiểm tra…' : 'Đăng nhập'}
        </button>
        <Link to={PATHS.home} className="link">
          Về trang khách
        </Link>
      </form>
    </section>
  );
}
