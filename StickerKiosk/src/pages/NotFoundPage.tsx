import { Link } from 'react-router-dom';
import { PATHS } from '../app/paths';

export function NotFoundPage() {
  return (
    <section className="page narrow">
      <h1>Không có trang này.</h1>
      <p>
        <Link to={PATHS.home}>Về trang chủ</Link> hoặc <Link to={PATHS.studio}>mở bàn thiết kế</Link>.
      </p>
    </section>
  );
}
