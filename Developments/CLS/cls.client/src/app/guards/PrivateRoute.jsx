import { Navigate, useLocation } from 'react-router-dom';
import { useAuthStore } from '@/app/provider/authStore';
import { ROUTE_PATHS } from '@/shared/utils/constants';

/**
 * PrivateRoute — Bảo vệ route, chỉ cho phép user đã đăng nhập truy cập.
 * Chờ Zustand persist hydrate xong trước khi kiểm tra auth (tránh race condition).
 *
 * @param {{ children: React.ReactNode, requiredRole?: string }} props
 */
export function PrivateRoute({ children, requiredRole }) {
  const { isAuthenticated, user, _hasHydrated } = useAuthStore();
  const location = useLocation();

  // Chờ Zustand đọc xong localStorage — tránh flash redirect sai
  if (!_hasHydrated) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="w-8 h-8 border-4 border-indigo-200 border-t-indigo-600 rounded-full animate-spin" />
      </div>
    );
  }

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
