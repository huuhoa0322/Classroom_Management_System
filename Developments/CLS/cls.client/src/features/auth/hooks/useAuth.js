import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { authService } from '../services/authService';
import { useAuthStore } from '@/app/provider/authStore';
import { toast } from '@/shared/stores/toastStore';
import { ROUTE_PATHS, USER_ROLES } from '@/shared/utils/constants';

/**
 * Hook đăng nhập — gọi API, lưu token vào Zustand store, redirect theo role.
 * - Admin  → Dashboard (/)
 * - Teacher → Thời khóa biểu (/timetable)
 */
export const useLogin = () => {
  const navigate   = useNavigate();
  const storeLogin = useAuthStore((s) => s.login);

  return useMutation({
    mutationFn: (credentials) => authService.login(credentials),
    onSuccess: (data) => {
      // data đã được unwrap bởi apiClient interceptor → đây là LoginResponse
      storeLogin(data.user, {
        accessToken:  data.accessToken,
        refreshToken: data.refreshToken,
      });
      toast.success(`Chào mừng ${data.user.fullName}!`);

      // Redirect theo role: Teacher → Timetable, Admin → Dashboard
      const redirectPath = data.user.role === USER_ROLES.TEACHER
        ? ROUTE_PATHS.TIMETABLE
        : ROUTE_PATHS.DASHBOARD;
      navigate(redirectPath, { replace: true });
    },
  });
};