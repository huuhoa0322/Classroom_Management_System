import { useState, useEffect } from 'react';

/**
 * ClassForm — Form tạo/sửa lớp học (modal content).
 */
export function ClassForm({ onSubmit, onCancel, isSubmitting, defaultValues }) {
  const [name, setName] = useState(defaultValues?.name || '');
  const isEdit = !!defaultValues;

  useEffect(() => {
    setName(defaultValues?.name || '');
  }, [defaultValues]);

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!name.trim()) return;
    onSubmit({ name: name.trim() });
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-5">
      <div>
        <label htmlFor="class-name" className="block text-sm font-medium text-gray-700 mb-1.5">
          Tên lớp <span className="text-red-500">*</span>
        </label>
        <input
          id="class-name"
          type="text"
          value={name}
          onChange={(e) => setName(e.target.value)}
          required
          autoFocus
          maxLength={100}
          placeholder="VD: Toán Nâng Cao - K12"
          className="w-full rounded-xl border border-gray-200 bg-gray-50 px-4 py-3 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:bg-white transition-colors"
        />
      </div>

      <div className="flex justify-end gap-2 pt-2">
        <button
          type="button"
          onClick={onCancel}
          className="px-4 py-2 text-sm text-gray-600 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors"
        >
          Hủy
        </button>
        <button
          type="submit"
          disabled={isSubmitting || !name.trim()}
          className="px-4 py-2 text-sm text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 disabled:opacity-50 transition-colors"
        >
          {isSubmitting ? 'Đang lưu...' : isEdit ? 'Cập nhật' : 'Tạo lớp'}
        </button>
      </div>
    </form>
  );
}
