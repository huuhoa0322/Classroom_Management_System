import { Outlet } from 'react-router-dom';

/**
 * AuthLayout — Wrapper tối giản cho trang Login/Register.
 * LoginPage tự render full-screen nên không cần thêm container ở đây.
 */
export function AuthLayout() {
  return <Outlet />;
}
