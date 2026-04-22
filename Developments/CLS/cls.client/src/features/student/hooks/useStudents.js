import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { studentService } from '../services/studentService';

/** Query key factory — tránh magic strings */
export const studentKeys = {
  all: ['students'],
  list: (params) => ['students', 'list', params],
  detail: (id) => ['students', 'detail', id],
};

/**
 * Hook lấy danh sách học sinh phân trang.
 * @param {{ page?: number, pageSize?: number, status?: string }} params
 */
export const useStudentList = (params = {}) => {
  return useQuery({
    queryKey: studentKeys.list(params),
    queryFn: () => studentService.getAll(params),
    staleTime: 1000 * 60 * 2, // 2 phút
  });
};

/**
 * Hook lấy chi tiết 1 học sinh.
 * @param {number} id
 */
export const useStudentDetail = (id) => {
  return useQuery({
    queryKey: studentKeys.detail(id),
    queryFn: () => studentService.getById(id),
    enabled: !!id,
  });
};

/**
 * Hook tạo mới học sinh (CLS-001).
 */
export const useCreateStudent = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data) => studentService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: studentKeys.all });
    },
  });
};

/**
 * Hook cập nhật thông tin học sinh (CLS-002).
 * @param {number} id
 */
export const useUpdateStudent = (id) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data) => studentService.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: studentKeys.all });
      queryClient.invalidateQueries({ queryKey: studentKeys.detail(id) });
    },
  });
};

/**
 * Hook đổi trạng thái vòng đời học sinh (CLS-002 AC1).
 */
export const useUpdateStudentStatus = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, status }) => studentService.updateStatus(id, status),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: studentKeys.all });
    },
  });
};
