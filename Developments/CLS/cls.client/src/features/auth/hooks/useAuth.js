import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { authService } from '../services/authService';
import { useAuthStore } from '@/app/provider/authStore';

/**
 * Hook đăng nhập — gọi API, lưu token vào Zustand store, redirect về dashboard.
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
      navigate('/', { replace: true });
    },
  });
};
