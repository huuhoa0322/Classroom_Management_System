import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  createSession,
  updateSession,
  deleteSession,
  getAllSessions,
  getClasses,
  getRooms,
  getTeachers,
} from '../services/sessionService';

/**
 * Hook: Lấy danh sách sessions (phân trang).
 */
export function useAllSessions(page = 1, pageSize = 10) {
  return useQuery({
    queryKey: ['allSessions', page, pageSize],
    queryFn: () => getAllSessions({ page, pageSize }),
  });
}

/**
 * Hook: Lấy danh sách lớp (dropdown).
 */
export function useClasses() {
  return useQuery({
    queryKey: ['classes'],
    queryFn: getClasses,
    staleTime: 5 * 60 * 1000, // 5 phút — data ít thay đổi
  });
}

/**
 * Hook: Lấy danh sách phòng (dropdown).
 */
export function useRooms() {
  return useQuery({
    queryKey: ['rooms'],
    queryFn: getRooms,
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Hook: Lấy danh sách giáo viên (dropdown).
 */
export function useTeachers() {
  return useQuery({
    queryKey: ['teachers'],
    queryFn: getTeachers,
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Hook: Tạo session mới.
 */
export function useCreateSession() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: createSession,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['allSessions'] });
    },
  });
}

/**
 * Hook: Cập nhật session.
 */
export function useUpdateSession() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }) => updateSession(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['allSessions'] });
    },
  });
}

/**
 * Hook: Xóa session.
 */
export function useDeleteSession() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: deleteSession,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['allSessions'] });
    },
  });
}
