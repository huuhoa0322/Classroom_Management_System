import { Outlet } from 'react-router-dom';

/**
 * AuthLayout — Layout đơn giản cho trang Login.
 * Căn giữa nội dung theo chiều dọc và ngang.
 */
export function AuthLayout() {
  return (
    <div className="min-h-screen bg-gradient-to-br from-indigo-50 via-white to-indigo-100 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        {/* Logo */}
        <div className="text-center mb-8">
          <span className="text-5xl">🎓</span>
          <h1 className="mt-3 text-2xl font-bold text-indigo-700">CLS</h1>
          <p className="text-sm text-gray-500 mt-1">Classroom Management System</p>
        </div>

        {/* Form Area */}
        <div className="bg-white rounded-2xl shadow-lg p-8">
          <Outlet />
        </div>
      </div>
    </div>
  );
}
