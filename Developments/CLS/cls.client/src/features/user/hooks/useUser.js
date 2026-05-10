import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getUsers, createUser, updateUser, updateUserStatus, resetUserPassword } from '../services/userService';

export const userKeys = { all: ['users'], list: (p) => ['users', 'list', p] };

export function useUserList(page = 1, pageSize = 10) {
  return useQuery({ queryKey: userKeys.list({ page, pageSize }), queryFn: () => getUsers({ page, pageSize }) });
}
export function useCreateUser() { const qc = useQueryClient(); return useMutation({ mutationFn: createUser, onSuccess: () => qc.invalidateQueries({ queryKey: userKeys.all }) }); }
export function useUpdateUser() { const qc = useQueryClient(); return useMutation({ mutationFn: ({ id, ...d }) => updateUser(id, d), onSuccess: () => qc.invalidateQueries({ queryKey: userKeys.all }) }); }
export function useUpdateUserStatus() { const qc = useQueryClient(); return useMutation({ mutationFn: ({ id, status }) => updateUserStatus(id, { status }), onSuccess: () => qc.invalidateQueries({ queryKey: userKeys.all }) }); }
export function useResetPassword() { const qc = useQueryClient(); return useMutation({ mutationFn: (id) => resetUserPassword(id), onSuccess: () => qc.invalidateQueries({ queryKey: userKeys.all }) }); }
