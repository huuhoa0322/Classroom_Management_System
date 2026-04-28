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

  // Data hook
  const { data: classes, isLoading, isError } = useClassList(page);

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

  const handleToggleStatus = (cls) => {
    const newStatus = cls.status === 'active' ? 'inactive' : 'active';
    const label = newStatus === 'active' ? 'kích hoạt' : 'tạm dừng';
    if (!window.confirm(`Bạn muốn ${label} lớp "${cls.name}"?`)) return;

    updateStatus.mutate(
      { id: cls.id, status: newStatus },
      {
        onSuccess: () => toast.success(`Đã ${label} lớp "${cls.name}".`),
        onError: (err) => toast.error(err.message || 'Không thể đổi trạng thái.'),
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
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-sm text-red-700 mb-6 flex items-center gap-2">
          <svg className="w-5 h-5 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z" />
          </svg>
          Không thể tải danh sách lớp. Vui lòng thử lại sau.
        </div>
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
    </div>
  );
}
