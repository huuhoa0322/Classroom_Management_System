import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getPackages, createPackage, updatePackage, updatePackageStatus } from '../services/packageService';

// ── Query key factory ─────────────────────────────────────────────────────
export const packageKeys = {
  all: ['packages'],
  list: (params) => ['packages', 'list', params],
};

/** Danh sách gói phân trang. */
export function usePackageList(page = 1, pageSize = 10) {
  return useQuery({
    queryKey: packageKeys.list({ page, pageSize }),
    queryFn: () => getPackages({ page, pageSize }),
  });
}

/** Tạo gói mới. */
export function useCreatePackage() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: createPackage,
    onSuccess: () => qc.invalidateQueries({ queryKey: packageKeys.all }),
  });
}

/** Cập nhật thông tin gói. */
export function useUpdatePackage() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }) => updatePackage(id, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: packageKeys.all }),
  });
}

/** Đổi trạng thái gói. */
export function useUpdatePackageStatus() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, status }) => updatePackageStatus(id, { status }),
    onSuccess: () => qc.invalidateQueries({ queryKey: packageKeys.all }),
  });
}
