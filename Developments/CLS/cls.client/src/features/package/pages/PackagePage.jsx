import { useState } from 'react';
import { usePackageList, useCreatePackage, useUpdatePackage, useUpdatePackageStatus } from '../hooks/usePackage';
import { PackageTable } from '../components/PackageTable';
import { PackageForm } from '../components/PackageForm';
import { ConnectionErrorBanner } from '@/shared/components/ConnectionErrorBanner';
import { toast } from '@/shared/stores/toastStore';

export default function PackagePage() {
  const [page, setPage] = useState(1);
  const [showForm, setShowForm] = useState(false);
  const [editingPkg, setEditingPkg] = useState(null);
  const [confirmingPkg, setConfirmingPkg] = useState(null);

  const { data: packages, isLoading, isError, error, refetch } = usePackageList(page);
  const createPkg = useCreatePackage();
  const updatePkg = useUpdatePackage();
  const updateStatus = useUpdatePackageStatus();

  const handleCreate = (data) => { createPkg.mutate(data, { onSuccess: () => { setShowForm(false); createPkg.reset(); toast.success('Tạo gói thành công!'); }, onError: (e) => toast.error(e.message || 'Lỗi.') }); };
  const handleUpdate = (data) => { if (!editingPkg) return; updatePkg.mutate({ id: editingPkg.id, ...data }, { onSuccess: () => { setEditingPkg(null); setShowForm(false); updatePkg.reset(); toast.success('Cập nhật gói thành công!'); }, onError: (e) => toast.error(e.message || 'Lỗi.') }); };
  const handleEdit = (pkg) => { setEditingPkg(pkg); setShowForm(true); };
  const handleToggleStatus = (pkg) => setConfirmingPkg(pkg);

  const confirmToggle = () => {
    if (!confirmingPkg) return;
    const newStatus = confirmingPkg.status === 'active' ? 'inactive' : 'active';
    const label = newStatus === 'active' ? 'kích hoạt' : 'tạm dừng';
    updateStatus.mutate({ id: confirmingPkg.id, status: newStatus }, { onSuccess: () => { toast.success(`Đã ${label} gói "${confirmingPkg.name}".`); setConfirmingPkg(null); }, onError: (e) => { toast.error(e.message || 'Lỗi.'); setConfirmingPkg(null); } });
  };

  const handleCloseForm = () => { setShowForm(false); setEditingPkg(null); };
  const isSubmitting = createPkg.isPending || updatePkg.isPending;
  const confirmLabel = confirmingPkg ? (confirmingPkg.status === 'active' ? 'tạm dừng' : 'kích hoạt') : '';
  const isDeactivate = confirmingPkg?.status === 'active';

  return (
    <div className="p-6 max-w-7xl mx-auto">
      <div className="flex items-center justify-between mb-6">
        <div><h1 className="text-2xl font-bold text-gray-800">Quản lý gói học</h1><p className="text-sm text-gray-500 mt-1">Tạo và quản lý các gói học phí</p></div>
        <button onClick={() => { setEditingPkg(null); setShowForm(true); }} className="px-4 py-2.5 bg-indigo-600 text-white text-sm font-medium rounded-lg hover:bg-indigo-700 transition-colors flex items-center gap-2 shadow-sm">
          <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" /></svg>Tạo gói
        </button>
      </div>
      {isError && <ConnectionErrorBanner error={error} onRetry={() => refetch()} />}
      <div className="bg-white border border-gray-200 rounded-xl shadow-sm p-4">
        <PackageTable packages={packages} isLoading={isLoading} page={page} onPageChange={setPage} onEdit={handleEdit} onToggleStatus={handleToggleStatus} isTogglingStatus={updateStatus.isPending} />
      </div>
      {showForm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-2xl shadow-2xl p-6 w-full max-w-md mx-4">
            <h3 className="text-lg font-semibold text-gray-800 mb-4">{editingPkg ? 'Chỉnh sửa gói' : 'Tạo gói mới'}</h3>
            <PackageForm onSubmit={editingPkg ? handleUpdate : handleCreate} onCancel={handleCloseForm} isSubmitting={isSubmitting} defaultValues={editingPkg ? { name: editingPkg.name, totalSessions: editingPkg.totalSessions, durationDays: editingPkg.durationDays, price: editingPkg.price } : undefined} />
          </div>
        </div>
      )}
      {confirmingPkg && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-2xl shadow-2xl p-6 w-full max-w-sm mx-4">
            <div className={`w-12 h-12 mx-auto mb-4 rounded-full flex items-center justify-center ${isDeactivate ? 'bg-yellow-100' : 'bg-green-100'}`}><span className="text-2xl">{isDeactivate ? '⏸' : '▶'}</span></div>
            <h3 className="text-lg font-semibold text-gray-900 text-center">Xác nhận {confirmLabel} gói</h3>
            <p className="text-sm text-gray-500 text-center mt-2">Bạn có chắc muốn <span className="font-medium text-gray-700">{confirmLabel}</span> gói <span className="font-semibold text-gray-900">&quot;{confirmingPkg.name}&quot;</span>?</p>
            <div className="flex gap-3 mt-6">
              <button onClick={() => setConfirmingPkg(null)} disabled={updateStatus.isPending} className="flex-1 px-4 py-2.5 text-sm font-medium text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 disabled:opacity-50">Hủy</button>
              <button onClick={confirmToggle} disabled={updateStatus.isPending} className={`flex-1 px-4 py-2.5 text-sm font-medium text-white rounded-lg disabled:opacity-50 ${isDeactivate ? 'bg-yellow-500 hover:bg-yellow-600' : 'bg-green-600 hover:bg-green-700'}`}>
                {updateStatus.isPending ? 'Đang xử lý...' : isDeactivate ? '⏸ Tạm dừng' : '▶ Kích hoạt'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
