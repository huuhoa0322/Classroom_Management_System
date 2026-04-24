import { useState } from 'react';
import { StudentList } from '../components/StudentList';
import { StudentForm } from '../components/StudentForm';
import { useCreateStudent, useUpdateStudent } from '../hooks/useStudents';
import { toast } from '@/shared/stores/toastStore';

/**
 * Trang quản lý học sinh — CLS-001 & CLS-002.
 * Tích hợp danh sách + modal tạo/sửa.
 */
const StudentPage = () => {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedStudent, setSelectedStudent] = useState(null);

  const { mutate: createStudent, isPending: isCreating } = useCreateStudent();
  const { mutate: updateStudent, isPending: isUpdating } = useUpdateStudent(selectedStudent?.id);

  const handleOpenCreate = () => {
    setSelectedStudent(null);
    setIsModalOpen(true);
  };

  const handleOpenEdit = (student) => {
    setSelectedStudent(student);
    setIsModalOpen(true);
  };

  const handleClose = () => {
    setIsModalOpen(false);
    setSelectedStudent(null);
  };

  const handleSubmit = (data) => {
    const callbacks = {
      onSuccess: () => {
        handleClose();
        toast.success(selectedStudent ? 'Cập nhật học sinh thành công!' : 'Tạo học sinh thành công!');
      },
      onError: (err) => {
        toast.error(err.message || 'Có lỗi xảy ra. Vui lòng thử lại.');
      },
    };
    if (selectedStudent) {
      updateStudent(data, callbacks);
    } else {
      createStudent(data, callbacks);
    }
  };

  return (
    <div className="p-6 max-w-7xl mx-auto">
      {/* ── Page Header ────────────────────────────────────────────────── */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Quản lý học sinh</h1>
          <p className="text-sm text-gray-500 mt-1">
            Onboard hồ sơ học sinh và quản lý vòng đời (CLS-001, CLS-002)
          </p>
        </div>
        <button
          id="btn-add-student"
          onClick={handleOpenCreate}
          className="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 transition-colors shadow-sm"
        >
          <span className="text-lg leading-none">+</span>
          Thêm học sinh
        </button>
      </div>

      {/* ── Student List ───────────────────────────────────────────────── */}
      <StudentList onEdit={handleOpenEdit} />

      {/* ── Modal Tạo / Sửa ───────────────────────────────────────────── */}
      {isModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
          <div
            className="absolute inset-0 bg-black/40 backdrop-blur-sm"
            onClick={handleClose}
          />
          <div className="relative z-10 bg-white rounded-2xl shadow-2xl w-full max-w-xl mx-4 max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between px-6 pt-5 pb-4 border-b border-gray-100">
              <h2 className="text-lg font-semibold text-gray-900">
                {selectedStudent ? 'Cập nhật học sinh' : 'Thêm học sinh mới'}
              </h2>
              <button
                onClick={handleClose}
                className="text-gray-400 hover:text-gray-600 text-2xl leading-none"
              >
                ×
              </button>
            </div>
            <div className="px-6 py-5">
              <StudentForm
                student={selectedStudent}
                onSubmit={handleSubmit}
                isLoading={isCreating || isUpdating}
                onCancel={handleClose}
              />
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default StudentPage;
