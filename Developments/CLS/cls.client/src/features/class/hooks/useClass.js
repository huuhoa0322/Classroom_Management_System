import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  getClasses,
  getClassById,
  createClass,
  updateClass,
  updateClassStatus,
  getClassStudents,
  enrollStudents,
  unenrollStudent,
} from '../services/classService';

// ── Query key factory ─────────────────────────────────────────────────────
export const classKeys = {
  all: ['classes'],
  list: (params) => ['classes', 'list', params],
  detail: (id) => ['classes', 'detail', id],
  students: (id) => ['classes', 'students', id],
};

/** Danh sách lớp phân trang. */
export function useClassList(page = 1, pageSize = 10) {
  return useQuery({
    queryKey: classKeys.list({ page, pageSize }),
    queryFn: () => getClasses({ page, pageSize }),
  });
}

/** Chi tiết 1 lớp. */
export function useClassDetail(id) {
  return useQuery({
    queryKey: classKeys.detail(id),
    queryFn: () => getClassById(id),
    enabled: !!id,
  });
}

/** Tạo lớp mới. */
export function useCreateClass() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: createClass,
    onSuccess: () => qc.invalidateQueries({ queryKey: classKeys.all }),
  });
}

/** Cập nhật tên lớp. */
export function useUpdateClass() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }) => updateClass(id, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: classKeys.all }),
  });
}

/** Đổi trạng thái lớp. */
export function useUpdateClassStatus() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, status }) => updateClassStatus(id, { status }),
    onSuccess: () => qc.invalidateQueries({ queryKey: classKeys.all }),
  });
}

/** Học sinh trong lớp. */
export function useClassStudents(classId) {
  return useQuery({
    queryKey: classKeys.students(classId),
    queryFn: () => getClassStudents(classId),
    enabled: !!classId,
  });
}

/** Đăng ký nhiều HS vào lớp. */
export function useEnrollStudents() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ classId, studentIds }) => enrollStudents(classId, studentIds),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: classKeys.students(variables.classId) });
      qc.invalidateQueries({ queryKey: classKeys.all });
    },
  });
}

/** Hủy đăng ký HS. */
export function useUnenrollStudent() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ classId, studentId }) => unenrollStudent(classId, studentId),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: classKeys.students(variables.classId) });
      qc.invalidateQueries({ queryKey: classKeys.all });
    },
  });
}
