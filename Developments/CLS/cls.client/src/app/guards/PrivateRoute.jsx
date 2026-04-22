import { Navigate, useLocation } from 'react-router-dom';
import { useAuthStore } from '@/app/provider/authStore';
import { ROUTE_PATHS } from '@/shared/utils/constants';

/**
 * PrivateRoute — Bảo vệ route, chỉ cho phép user đã đăng nhập truy cập.
 * Có thể truyền thêm `requiredRole` để giới hạn theo vai trò.
 *
 * @param {{ children: React.ReactNode, requiredRole?: string }} props
 */
export function PrivateRoute({ children, requiredRole }) {
  const { isAuthenticated, user } = useAuthStore();
  const location = useLocation();

  // Chưa đăng nhập → Redirect về /login, lưu lại URL hiện tại để redirect ngược sau khi login
  if (!isAuthenticated) {
    return <Navigate to={ROUTE_PATHS.LOGIN} state={{ from: location }} replace />;
  }

  // Có yêu cầu role cụ thể → Kiểm tra role của user
  if (requiredRole && user?.role !== requiredRole) {
    return <Navigate to={ROUTE_PATHS.DASHBOARD} replace />;
  }

  return children;
}
