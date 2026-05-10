import { useState } from 'react';
import { useRoomList, useCreateRoom, useUpdateRoom, useUpdateRoomStatus } from '../hooks/useRoom';
import { RoomTable } from '../components/RoomTable';
import { RoomForm } from '../components/RoomForm';
import { toast } from '@/shared/stores/toastStore';

export default function RoomPage() {
  const [page, setPage] = useState(1);
  const [showForm, setShowForm] = useState(false);
  const [editingRoom, setEditingRoom] = useState(null);
  const [confirmingRoom, setConfirmingRoom] = useState(null);

  const { data: rooms, isLoading, isError } = useRoomList(page);
  const createRoom = useCreateRoom();
  const updateRoom = useUpdateRoom();
  const updateStatus = useUpdateRoomStatus();

  const handleCreate = (data) => { createRoom.mutate(data, { onSuccess: () => { setShowForm(false); createRoom.reset(); toast.success('Tạo phòng thành công!'); }, onError: (err) => toast.error(err.message || 'Không thể tạo phòng.') }); };
  const handleUpdate = (data) => { if (!editingRoom) return; updateRoom.mutate({ id: editingRoom.id, ...data }, { onSuccess: () => { setEditingRoom(null); setShowForm(false); updateRoom.reset(); toast.success('Cập nhật phòng thành công!'); }, onError: (err) => toast.error(err.message || 'Không thể cập nhật phòng.') }); };
  const handleEdit = (room) => { setEditingRoom(room); setShowForm(true); };
  const handleToggleStatus = (room) => setConfirmingRoom(room);

  const confirmToggle = () => {
    if (!confirmingRoom) return;
    const newStatus = confirmingRoom.status === 'active' ? 'inactive' : 'active';
    const label = newStatus === 'active' ? 'kích hoạt' : 'tạm dừng';
    updateStatus.mutate({ id: confirmingRoom.id, status: newStatus }, { onSuccess: () => { toast.success(`Đã ${label} phòng "${confirmingRoom.name}".`); setConfirmingRoom(null); }, onError: (err) => { toast.error(err.message || 'Lỗi.'); setConfirmingRoom(null); } });
  };

  const handleCloseForm = () => { setShowForm(false); setEditingRoom(null); };
  const isSubmitting = createRoom.isPending || updateRoom.isPending;
  const confirmLabel = confirmingRoom ? (confirmingRoom.status === 'active' ? 'tạm dừng' : 'kích hoạt') : '';
  const isDeactivate = confirmingRoom?.status === 'active';

  return (
    <div className="p-6 max-w-7xl mx-auto">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-800">Quản lý phòng học</h1>
          <p className="text-sm text-gray-500 mt-1">Tạo và quản lý thông tin phòng học</p>
        </div>
        <button onClick={() => { setEditingRoom(null); setShowForm(true); }} className="px-4 py-2.5 bg-indigo-600 text-white text-sm font-medium rounded-lg hover:bg-indigo-700 transition-colors flex items-center gap-2 shadow-sm">
          <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" /></svg>
          Tạo phòng
        </button>
      </div>

      {isError && <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-sm text-red-700 mb-6">Không thể tải danh sách phòng.</div>}

      <div className="bg-white border border-gray-200 rounded-xl shadow-sm p-4">
        <RoomTable rooms={rooms} isLoading={isLoading} page={page} onPageChange={setPage} onEdit={handleEdit} onToggleStatus={handleToggleStatus} isTogglingStatus={updateStatus.isPending} />
      </div>

      {showForm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-2xl shadow-2xl p-6 w-full max-w-md mx-4">
            <h3 className="text-lg font-semibold text-gray-800 mb-4">{editingRoom ? 'Chỉnh sửa phòng' : 'Tạo phòng mới'}</h3>
            <RoomForm onSubmit={editingRoom ? handleUpdate : handleCreate} onCancel={handleCloseForm} isSubmitting={isSubmitting} defaultValues={editingRoom ? { name: editingRoom.name, capacity: editingRoom.capacity } : undefined} />
          </div>
        </div>
      )}

      {confirmingRoom && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-2xl shadow-2xl p-6 w-full max-w-sm mx-4">
            <div className={`w-12 h-12 mx-auto mb-4 rounded-full flex items-center justify-center ${isDeactivate ? 'bg-yellow-100' : 'bg-green-100'}`}><span className="text-2xl">{isDeactivate ? '⏸' : '▶'}</span></div>
            <h3 className="text-lg font-semibold text-gray-900 text-center">Xác nhận {confirmLabel} phòng</h3>
            <p className="text-sm text-gray-500 text-center mt-2">Bạn có chắc muốn <span className="font-medium text-gray-700">{confirmLabel}</span> phòng <span className="font-semibold text-gray-900">&quot;{confirmingRoom.name}&quot;</span>?</p>
            <div className="flex gap-3 mt-6">
              <button onClick={() => setConfirmingRoom(null)} disabled={updateStatus.isPending} className="flex-1 px-4 py-2.5 text-sm font-medium text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors disabled:opacity-50">Hủy</button>
              <button onClick={confirmToggle} disabled={updateStatus.isPending} className={`flex-1 px-4 py-2.5 text-sm font-medium text-white rounded-lg transition-colors disabled:opacity-50 ${isDeactivate ? 'bg-yellow-500 hover:bg-yellow-600' : 'bg-green-600 hover:bg-green-700'}`}>
                {updateStatus.isPending ? 'Đang xử lý...' : isDeactivate ? '⏸ Tạm dừng' : '▶ Kích hoạt'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
