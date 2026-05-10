import { useState } from 'react';
import {
  useClassList,
  useCreateClass,
  useUpdateClass,
  useUpdateClassStatus,
} from '../hooks/useClass';
import { ClassTable } from '../components/ClassTable';
import { ClassForm } from '../components/ClassForm';
import { ClassStudentManager } from '../components/ClassStudentManager';
import { ConnectionErrorBanner } from '@/shared/components/ConnectionErrorBanner';
import { toast } from '@/shared/stores/toastStore';

/**
 * ClassPage — Trang quản lý lớp học (Admin).
 * Route: /classes
 */
export default function ClassPage() {
  const [page, setPage] = useState(1);
  const [showForm, setShowForm] = useState(false);
  const [editingClass, setEditingClass] = useState(null);
  const [managingClass, setManagingClass] = useState(null);
  const [confirmingClass, setConfirmingClass] = useState(null);

  // Data hook
  const { data: classes, isLoading, isError, error, refetch } = useClassList(page);

  // Mutation hooks
  const createClass = useCreateClass();
  const updateClass = useUpdateClass();
  const updateStatus = useUpdateClassStatus();

  const handleCreate = (data) => {
    createClass.mutate(data, {
      onSuccess: () => {
        setShowForm(false);
        createClass.reset();
        toast.success('Tạo lớp học thành công!');
      },
      onError: (err) => {
        toast.error(err.message || 'Không thể tạo lớp học.');
      },
    });
  };

  const handleUpdate = (data) => {
    if (!editingClass) return;
    updateClass.mutate(
      { id: editingClass.id, ...data },
      {
        onSuccess: () => {
          setEditingClass(null);
          setShowForm(false);
          updateClass.reset();
          toast.success('Cập nhật lớp học thành công!');
        },
        onError: (err) => {
          toast.error(err.message || 'Không thể cập nhật lớp học.');
        },
      }
    );
  };

  const handleEdit = (cls) => {
    setEditingClass(cls);
    setShowForm(true);
  };

  // Mở modal xác nhận thay vì window.confirm
  const handleToggleStatus = (cls) => {
    setConfirmingClass(cls);
  };

  const confirmToggleStatus = () => {
    if (!confirmingClass) return;
    const newStatus = confirmingClass.status === 'active' ? 'inactive' : 'active';
    const label = newStatus === 'active' ? 'kích hoạt' : 'tạm dừng';

    updateStatus.mutate(
      { id: confirmingClass.id, status: newStatus },
      {
        onSuccess: () => {
          toast.success(`Đã ${label} lớp "${confirmingClass.name}".`);
          setConfirmingClass(null);
        },
        onError: (err) => {
          toast.error(err.message || 'Không thể đổi trạng thái.');
          setConfirmingClass(null);
        },
      }
    );
  };

  const handleCloseForm = () => {
    setShowForm(false);
    setEditingClass(null);
    createClass.reset();
    updateClass.reset();
  };

  const isSubmitting = createClass.isPending || updateClass.isPending;

  // Tính label cho modal xác nhận
  const confirmLabel = confirmingClass
    ? confirmingClass.status === 'active' ? 'tạm dừng' : 'kích hoạt'
    : '';
  const confirmIsDeactivate = confirmingClass?.status === 'active';

  return (
    <div className="p-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-800">Quản lý lớp học</h1>
          <p className="text-sm text-gray-500 mt-1">
            Tạo lớp, quản lý học sinh và theo dõi thông tin lớp học
          </p>
        </div>
        <button
          onClick={() => {
            setEditingClass(null);
            setShowForm(true);
          }}
          className="px-4 py-2.5 bg-indigo-600 text-white text-sm font-medium rounded-lg hover:bg-indigo-700 transition-colors flex items-center gap-2 shadow-sm"
        >
          <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
          </svg>
          Tạo lớp học
        </button>
      </div>

      {/* Error */}
      {isError && (
        <ConnectionErrorBanner error={error} onRetry={() => refetch()} />
      )}

      {/* Table */}
      <div className="bg-white border border-gray-200 rounded-xl shadow-sm p-4">
        <ClassTable
          classes={classes}
          isLoading={isLoading}
          page={page}
          onPageChange={setPage}
          onEdit={handleEdit}
          onManageStudents={setManagingClass}
          onToggleStatus={handleToggleStatus}
          isTogglingStatus={updateStatus.isPending}
        />
      </div>

      {/* Modal — Tạo/Sửa lớp */}
      {showForm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-2xl shadow-2xl p-6 w-full max-w-md mx-4">
            <h3 className="text-lg font-semibold text-gray-800 mb-4">
              {editingClass ? 'Chỉnh sửa lớp học' : 'Tạo lớp học mới'}
            </h3>
            <ClassForm
              onSubmit={editingClass ? handleUpdate : handleCreate}
              onCancel={handleCloseForm}
              isSubmitting={isSubmitting}
              defaultValues={editingClass ? { name: editingClass.name } : undefined}
            />
          </div>
        </div>
      )}

      {/* Modal — Quản lý học sinh */}
      {managingClass && (
        <ClassStudentManager
          classInfo={managingClass}
          onClose={() => setManagingClass(null)}
        />
      )}

      {/* Modal — Xác nhận đổi trạng thái */}
      {confirmingClass && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-2xl shadow-2xl p-6 w-full max-w-sm mx-4">
            {/* Icon */}
            <div className={`w-12 h-12 mx-auto mb-4 rounded-full flex items-center justify-center ${
              confirmIsDeactivate ? 'bg-yellow-100' : 'bg-green-100'
            }`}>
              <span className="text-2xl">{confirmIsDeactivate ? '⏸' : '▶'}</span>
            </div>

            {/* Title */}
            <h3 className="text-lg font-semibold text-gray-900 text-center">
              Xác nhận {confirmLabel} lớp
            </h3>

            {/* Message */}
            <p className="text-sm text-gray-500 text-center mt-2 leading-relaxed">
              Bạn có chắc muốn <span className="font-medium text-gray-700">{confirmLabel}</span> lớp
              {' '}<span className="font-semibold text-gray-900">&quot;{confirmingClass.name}&quot;</span>?
              {confirmIsDeactivate && (
                <span className="block mt-1 text-yellow-600">
                  Lớp sẽ không thể nhận học sinh mới sau khi tạm dừng.
                </span>
              )}
            </p>

            {/* Actions */}
            <div className="flex gap-3 mt-6">
              <button
                onClick={() => setConfirmingClass(null)}
                disabled={updateStatus.isPending}
                className="flex-1 px-4 py-2.5 text-sm font-medium text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors disabled:opacity-50"
              >
                Hủy
              </button>
              <button
                onClick={confirmToggleStatus}
                disabled={updateStatus.isPending}
                className={`flex-1 px-4 py-2.5 text-sm font-medium text-white rounded-lg transition-colors disabled:opacity-50 flex items-center justify-center gap-2 ${
                  confirmIsDeactivate
                    ? 'bg-yellow-500 hover:bg-yellow-600'
                    : 'bg-green-600 hover:bg-green-700'
                }`}
              >
                {updateStatus.isPending ? (
                  <>
                    <span className="w-4 h-4 border-2 border-white/40 border-t-white rounded-full animate-spin" />
                    Đang xử lý...
                  </>
                ) : (
                  <>
                    {confirmIsDeactivate ? '⏸ Tạm dừng' : '▶ Kích hoạt'}
                  </>
                )}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
