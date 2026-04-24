import { useState } from 'react';
import { Link } from 'react-router-dom';
import {
  useAllSessions,
  useCreateSession,
  useUpdateSession,
  useDeleteSession,
} from '../../schedule/hooks/useSession';
import { SessionTable } from '../../schedule/components/SessionTable';
import { SessionForm } from '../../schedule/components/SessionForm';

/**
 * SessionPage — Trang quản lý lịch dạy (CLS-004 + CLS-005).
 * Route: /sessions
 */
export default function SessionPage() {
  const [page, setPage] = useState(1);
  const [showForm, setShowForm] = useState(false);
  const [editingSession, setEditingSession] = useState(null);

  // Data hook
  const { data: sessions, isLoading, isError } = useAllSessions(page);

  // Mutation hooks
  const createSession = useCreateSession();
  const updateSession = useUpdateSession();
  const deleteSession = useDeleteSession();

  const handleCreate = (data) => {
    createSession.mutate(data, {
      onSuccess: () => {
        setShowForm(false);
        createSession.reset();
      },
    });
  };

  const handleUpdate = (data) => {
    if (!editingSession) return;
    updateSession.mutate(
      { id: editingSession.id, ...data },
      {
        onSuccess: () => {
          setEditingSession(null);
          setShowForm(false);
          updateSession.reset();
        },
      }
    );
  };

  const handleEdit = (session) => {
    setEditingSession(session);
    setShowForm(true);
  };

  const handleDelete = (id) => {
    if (window.confirm('Bạn có chắc chắn muốn xóa buổi học này?')) {
      deleteSession.mutate(id);
    }
  };

  const handleCloseForm = () => {
    setShowForm(false);
    setEditingSession(null);
    createSession.reset();
    updateSession.reset();
  };

  const isSubmitting = createSession.isPending || updateSession.isPending;
  const apiError =
    createSession.error?.message || updateSession.error?.message || null;

  return (
    <div className="p-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <Link
            to="/"
            className="text-sm text-blue-600 hover:text-blue-800 flex items-center gap-1 mb-2"
          >
            ← Quay lại Dashboard
          </Link>
          <h1 className="text-2xl font-bold text-gray-800">Quản lý lịch dạy</h1>
          <p className="text-sm text-gray-500 mt-1">
            Tạo, chỉnh sửa và quản lý buổi học — CLS-004 + CLS-005
          </p>
        </div>
        <button
          onClick={() => {
            setEditingSession(null);
            setShowForm(true);
          }}
          className="px-4 py-2.5 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 transition-colors flex items-center gap-2 shadow-sm"
        >
          <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
          </svg>
          Tạo buổi học
        </button>
      </div>

      {/* Error */}
      {isError && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-sm text-red-700 mb-6 flex items-center gap-2">
          <svg className="w-5 h-5 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z" />
          </svg>
          Không thể tải dữ liệu lịch dạy. Vui lòng thử lại sau.
        </div>
      )}

      {/* Table */}
      <div className="bg-white border border-gray-200 rounded-xl shadow-sm p-4">
        <SessionTable
          sessions={sessions}
          isLoading={isLoading}
          page={page}
          onPageChange={setPage}
          onEdit={handleEdit}
          onDelete={handleDelete}
          isDeleting={deleteSession.isPending}
        />
      </div>

      {/* Modal — Tạo/Sửa buổi học */}
      {showForm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-2xl shadow-2xl p-6 w-full max-w-lg mx-4 max-h-[90vh] overflow-y-auto">
            <h3 className="text-lg font-semibold text-gray-800 mb-4">
              {editingSession ? 'Chỉnh sửa buổi học' : 'Tạo buổi học mới'}
            </h3>
            <SessionForm
              onSubmit={editingSession ? handleUpdate : handleCreate}
              onCancel={handleCloseForm}
              isSubmitting={isSubmitting}
              defaultValues={
                editingSession
                  ? {
                      classId: String(editingSession.classId),
                      teacherId: String(editingSession.teacherId),
                      roomId: String(editingSession.roomId),
                      startTime: editingSession.startTime?.slice(0, 16),
                      endTime: editingSession.endTime?.slice(0, 16),
                    }
                  : undefined
              }
              apiError={apiError}
            />
          </div>
        </div>
      )}
    </div>
  );
}
