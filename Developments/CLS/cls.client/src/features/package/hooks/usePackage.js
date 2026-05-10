import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getPackages, createPackage, updatePackage, updatePackageStatus } from '../services/packageService';

export const packageKeys = { all: ['packages'], list: (p) => ['packages', 'list', p] };

export function usePackageList(page = 1, pageSize = 10) {
  return useQuery({ queryKey: packageKeys.list({ page, pageSize }), queryFn: () => getPackages({ page, pageSize }) });
}
export function useCreatePackage() { const qc = useQueryClient(); return useMutation({ mutationFn: createPackage, onSuccess: () => qc.invalidateQueries({ queryKey: packageKeys.all }) }); }
export function useUpdatePackage() { const qc = useQueryClient(); return useMutation({ mutationFn: ({ id, ...d }) => updatePackage(id, d), onSuccess: () => qc.invalidateQueries({ queryKey: packageKeys.all }) }); }
export function useUpdatePackageStatus() { const qc = useQueryClient(); return useMutation({ mutationFn: ({ id, status }) => updatePackageStatus(id, { status }), onSuccess: () => qc.invalidateQueries({ queryKey: packageKeys.all }) }); }
