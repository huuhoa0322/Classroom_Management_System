import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getUsers, createUser, updateUser, updateUserStatus, resetUserPassword } from '../services/userService';

// ── Query key factory ─────────────────────────────────────────────────────
export const userKeys = {
  all: ['users'],
  list: (params) => ['users', 'list', params],
};

/** Danh sách tài khoản phân trang. */
export function useUserList(page = 1, pageSize = 10) {
  return useQuery({
    queryKey: userKeys.list({ page, pageSize }),
    queryFn: () => getUsers({ page, pageSize }),
  });
}

/** Tạo tài khoản Teacher mới. */
export function useCreateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: createUser,
    onSuccess: () => qc.invalidateQueries({ queryKey: userKeys.all }),
  });
}

/** Cập nhật thông tin tài khoản. */
export function useUpdateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }) => updateUser(id, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: userKeys.all }),
  });
}

/** Đổi trạng thái tài khoản. */
export function useUpdateUserStatus() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, status }) => updateUserStatus(id, { status }),
    onSuccess: () => qc.invalidateQueries({ queryKey: userKeys.all }),
  });
}

/** Đặt lại mật khẩu ngẫu nhiên. */
export function useResetPassword() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => resetUserPassword(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: userKeys.all }),
  });
}
