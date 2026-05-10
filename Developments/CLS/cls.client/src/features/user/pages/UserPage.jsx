import { useState } from 'react';
import { useUserList, useCreateUser, useUpdateUser, useUpdateUserStatus, useResetPassword } from '../hooks/useUser';
import { UserTable } from '../components/UserTable';
import { UserForm } from '../components/UserForm';
import { toast } from '@/shared/stores/toastStore';

export default function UserPage() {
  const [page, setPage] = useState(1);
  const [showForm, setShowForm] = useState(false);
  const [editingUser, setEditingUser] = useState(null);
  const [confirmingUser, setConfirmingUser] = useState(null);
  const [resetConfirmUser, setResetConfirmUser] = useState(null);
  const [resetResult, setResetResult] = useState(null);

  const { data: users, isLoading, isError } = useUserList(page);
  const createUser = useCreateUser();
  const updateUser = useUpdateUser();
  const updateStatus = useUpdateUserStatus();
  const resetPw = useResetPassword();

  const handleCreate = (data) => {
    createUser.mutate(data, {
      onSuccess: () => { setShowForm(false); createUser.reset(); toast.success('Tạo tài khoản thành công!'); },
      onError: (e) => toast.error(e.message || 'Lỗi.'),
    });
  };

  const handleUpdate = (data) => {
    if (!editingUser) return;
    updateUser.mutate({ id: editingUser.id, ...data }, {
      onSuccess: () => { setEditingUser(null); setShowForm(false); updateUser.reset(); toast.success('Cập nhật tài khoản thành công!'); },
      onError: (e) => toast.error(e.message || 'Lỗi.'),
    });
  };

  const handleEdit = (user) => { setEditingUser(user); setShowForm(true); };
  const handleToggleStatus = (user) => setConfirmingUser(user);
  const handleResetPassword = (user) => setResetConfirmUser(user);

  const confirmResetPassword = () => {
    if (!resetConfirmUser) return;
    resetPw.mutate(resetConfirmUser.id, {
      onSuccess: (data) => {
        setResetResult({ userName: resetConfirmUser.fullName, newPassword: data.newPassword });
        setResetConfirmUser(null);
      },
      onError: (e) => { toast.error(e.message || 'Không thể đặt lại mật khẩu.'); setResetConfirmUser(null); },
    });
  };

  const confirmToggle = () => {
    if (!confirmingUser) return;
    const newStatus = confirmingUser.status === 'active' ? 'inactive' : 'active';
    const label = newStatus === 'active' ? 'kích hoạt' : 'tạm dừng';
    updateStatus.mutate({ id: confirmingUser.id, status: newStatus }, {
      onSuccess: () => { toast.success(`Đã ${label} tài khoản "${confirmingUser.fullName}".`); setConfirmingUser(null); },
      onError: (e) => { toast.error(e.message); setConfirmingUser(null); },
    });
  };

  const handleCloseForm = () => { setShowForm(false); setEditingUser(null); };
  const isSubmitting = createUser.isPending || updateUser.isPending;
  const confirmLabel = confirmingUser ? (confirmingUser.status === 'active' ? 'tạm dừng' : 'kích hoạt') : '';
  const isDeactivate = confirmingUser?.status === 'active';

  return (
    <div className="p-6 max-w-7xl mx-auto">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-800">Quản lý tài khoản</h1>
          <p className="text-sm text-gray-500 mt-1">Tạo và quản lý tài khoản giáo viên</p>
        </div>
        <button onClick={() => { setEditingUser(null); setShowForm(true); }} className="px-4 py-2.5 bg-indigo-600 text-white text-sm font-medium rounded-lg hover:bg-indigo-700 transition-colors flex items-center gap-2 shadow-sm">
          <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" /></svg>
          Tạo tài khoản
        </button>
      </div>

      {isError && <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-sm text-red-700 mb-6">Không thể tải danh sách tài khoản.</div>}

      <div className="bg-white border border-gray-200 rounded-xl shadow-sm p-4">
        <UserTable users={users} isLoading={isLoading} page={page} onPageChange={setPage} onEdit={handleEdit} onToggleStatus={handleToggleStatus} onResetPassword={handleResetPassword} isTogglingStatus={updateStatus.isPending} />
      </div>

      {/* Modal — Tạo/Sửa */}
      {showForm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-2xl shadow-2xl p-6 w-full max-w-md mx-4">
            <h3 className="text-lg font-semibold text-gray-800 mb-4">{editingUser ? 'Chỉnh sửa tài khoản' : 'Tạo tài khoản giáo viên'}</h3>
            <UserForm onSubmit={editingUser ? handleUpdate : handleCreate} onCancel={handleCloseForm} isSubmitting={isSubmitting} defaultValues={editingUser ? { fullName: editingUser.fullName, email: editingUser.email, phone: editingUser.phone || '' } : undefined} />
          </div>
        </div>
      )}

      {/* Modal — Xác nhận đổi trạng thái */}
      {confirmingUser && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-2xl shadow-2xl p-6 w-full max-w-sm mx-4">
            <div className={`w-12 h-12 mx-auto mb-4 rounded-full flex items-center justify-center ${isDeactivate ? 'bg-yellow-100' : 'bg-green-100'}`}>
              <span className="text-2xl">{isDeactivate ? '⏸' : '▶'}</span>
            </div>
            <h3 className="text-lg font-semibold text-gray-900 text-center">Xác nhận {confirmLabel}</h3>
            <p className="text-sm text-gray-500 text-center mt-2">
              Bạn có chắc muốn <span className="font-medium text-gray-700">{confirmLabel}</span> tài khoản <span className="font-semibold text-gray-900">&quot;{confirmingUser.fullName}&quot;</span>?
            </p>
            {isDeactivate && <p className="text-xs text-yellow-600 text-center mt-1">Tài khoản sẽ không thể đăng nhập sau khi bị tạm dừng.</p>}
            <div className="flex gap-3 mt-6">
              <button onClick={() => setConfirmingUser(null)} disabled={updateStatus.isPending} className="flex-1 px-4 py-2.5 text-sm font-medium text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 disabled:opacity-50">Hủy</button>
              <button onClick={confirmToggle} disabled={updateStatus.isPending} className={`flex-1 px-4 py-2.5 text-sm font-medium text-white rounded-lg disabled:opacity-50 ${isDeactivate ? 'bg-yellow-500 hover:bg-yellow-600' : 'bg-green-600 hover:bg-green-700'}`}>
                {updateStatus.isPending ? 'Đang xử lý...' : isDeactivate ? '⏸ Tạm dừng' : '▶ Kích hoạt'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal — Xác nhận Reset Password */}
      {resetConfirmUser && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-2xl shadow-2xl p-6 w-full max-w-sm mx-4">
            <div className="w-12 h-12 mx-auto mb-4 rounded-full bg-amber-100 flex items-center justify-center">
              <span className="text-2xl">🔑</span>
            </div>
            <h3 className="text-lg font-semibold text-gray-900 text-center">Xác nhận đặt lại mật khẩu</h3>
            <p className="text-sm text-gray-500 text-center mt-2">
              Bạn có chắc muốn đặt lại mật khẩu cho <span className="font-semibold text-gray-900">&quot;{resetConfirmUser.fullName}&quot;</span>?
            </p>
            <p className="text-xs text-red-500 text-center mt-1">⚠ Mật khẩu hiện tại sẽ bị thay thế vĩnh viễn bằng mật khẩu ngẫu nhiên mới.</p>
            <div className="flex gap-3 mt-6">
              <button onClick={() => setResetConfirmUser(null)} disabled={resetPw.isPending} className="flex-1 px-4 py-2.5 text-sm font-medium text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 disabled:opacity-50">Hủy</button>
              <button onClick={confirmResetPassword} disabled={resetPw.isPending} className="flex-1 px-4 py-2.5 text-sm font-medium text-white bg-amber-500 rounded-lg hover:bg-amber-600 disabled:opacity-50">
                {resetPw.isPending ? 'Đang xử lý...' : '🔑 Đặt lại'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal — Hiển thị mật khẩu mới */}
      {resetResult && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-2xl shadow-2xl p-6 w-full max-w-sm mx-4">
            <div className="w-12 h-12 mx-auto mb-4 rounded-full bg-green-100 flex items-center justify-center">
              <span className="text-2xl">✅</span>
            </div>
            <h3 className="text-lg font-semibold text-gray-900 text-center">Mật khẩu mới</h3>
            <p className="text-sm text-gray-500 text-center mt-2">
              Mật khẩu mới của <span className="font-semibold">{resetResult.userName}</span>:
            </p>
            <div className="mt-3 bg-gray-100 rounded-lg p-3 text-center font-mono text-lg text-gray-900 select-all">
              {resetResult.newPassword}
            </div>
            <p className="text-xs text-amber-600 text-center mt-2">⚠ Hãy gửi mật khẩu này cho giáo viên. Mật khẩu chỉ hiển thị 1 lần.</p>
            <button onClick={() => setResetResult(null)} className="w-full mt-4 px-4 py-2.5 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700">Đã ghi nhận</button>
          </div>
        </div>
      )}
    </div>
  );
}
