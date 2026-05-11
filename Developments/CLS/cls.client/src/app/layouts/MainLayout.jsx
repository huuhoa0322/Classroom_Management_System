import { useState } from 'react';
import { Outlet, NavLink, useNavigate } from 'react-router-dom';
import { useQueryClient } from '@tanstack/react-query';
import { useAuthStore } from '@/app/provider/authStore';
import { ROUTE_PATHS } from '@/shared/utils/constants';
import { Toast } from '@/shared/components/Toast';
import { useNotificationHub } from '@/shared/hooks/useNotificationHub';
import { useNotificationStore } from '@/shared/stores/notificationStore';
import { USER_ROLES } from '@/shared/utils/constants';

const adminNavItems = [
  { label: 'Dashboard',  path: ROUTE_PATHS.DASHBOARD,  icon: '📊' },
  { label: 'Học sinh',   path: ROUTE_PATHS.STUDENTS,   icon: '👨‍🎓' },
  { label: 'Lớp học',    path: ROUTE_PATHS.CLASSES,    icon: '🏫' },
  { label: 'Phòng học',  path: ROUTE_PATHS.ROOMS,      icon: '🚪' },
  { label: 'Gói học',    path: ROUTE_PATHS.PACKAGES,   icon: '📦' },
  { label: 'Tài khoản',  path: ROUTE_PATHS.USERS,     icon: '👤' },
  { label: 'Lịch học',   path: ROUTE_PATHS.SESSIONS,   icon: '📅' },
  { label: 'Thông báo gia hạn', path: ROUTE_PATHS.RENEWAL_ALERTS, icon: '🔔', showBadge: true },
];

const teacherNavItems = [
  { label: 'Lịch dạy',   path: ROUTE_PATHS.TIMETABLE,  icon: '📅' },
];

/**
 * MainLayout — Layout chính của ứng dụng CLS (Topbar + Sidebar + Content).
 * Sidebar ẩn trên mobile, hiển thị khi màn hình ≥ lg.
 */
export function MainLayout() {
  const { user, logout } = useAuthStore();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const unreadCount = useNotificationStore((s) => s.unreadCount);

  // Role-based navigation
  const navItems = user?.role === USER_ROLES.TEACHER ? teacherNavItems : adminNavItems;

  // Connect to SignalR NotificationHub
  useNotificationHub();

  const handleLogout = () => {
    queryClient.clear();   // Xóa toàn bộ cache Query — tránh hiển thị data của user cũ
    logout();
    navigate(ROUTE_PATHS.LOGIN, { replace: true });
  };

  return (
    <div className="flex h-screen bg-gray-50">
      {/* ── Sidebar ────────────────────────────────────────────────────── */}
      <aside
        className={`
          fixed inset-y-0 left-0 z-50 w-64 bg-white border-r border-gray-200 shadow-sm
          transform transition-transform duration-200 ease-in-out
          ${sidebarOpen ? 'translate-x-0' : '-translate-x-full'}
          lg:relative lg:translate-x-0
        `}
      >
        {/* Logo */}
        <div className="flex items-center gap-2 h-16 px-6 border-b border-gray-100">
          <span className="text-2xl">🎓</span>
          <span className="text-lg font-bold text-indigo-600">CLS</span>
          <span className="text-xs text-gray-400 ml-1">Classroom</span>
        </div>

        {/* Navigation */}
        <nav className="flex-1 px-3 py-4 space-y-1">
          {navItems.map((item) => (
            <NavLink
              key={item.path}
              to={item.path}
              end={item.path === '/'}
              onClick={() => setSidebarOpen(false)}
              className={({ isActive }) =>
                `flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors
                ${isActive
                  ? 'bg-indigo-50 text-indigo-700'
                  : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
                }`
              }
            >
              <span>{item.icon}</span>
              <span className="flex-1">{item.label}</span>
              {item.showBadge && unreadCount > 0 && (
                <span className="ml-auto inline-flex items-center justify-center w-5 h-5 text-[10px] font-bold text-white bg-red-500 rounded-full">
                  {unreadCount > 99 ? '99+' : unreadCount}
                </span>
              )}
            </NavLink>
          ))}
        </nav>
      </aside>

      {/* Overlay (mobile) */}
      {sidebarOpen && (
        <div
          className="fixed inset-0 z-40 bg-black/30 lg:hidden"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      {/* ── Main Content ───────────────────────────────────────────────── */}
      <div className="flex-1 flex flex-col min-w-0 overflow-hidden">
        {/* Topbar */}
        <header className="h-16 bg-white border-b border-gray-200 flex items-center justify-between px-4 lg:px-6 flex-shrink-0">
          {/* Hamburger (mobile only) */}
          <button
            className="lg:hidden p-2 rounded-md text-gray-500 hover:bg-gray-100"
            onClick={() => setSidebarOpen(true)}
          >
            ☰
          </button>

          <div className="text-sm font-semibold text-gray-700 lg:ml-0 ml-2">
            Classroom Management System
          </div>

          {/* User Info + Logout */}
          <div className="flex items-center gap-3">
            {/* 🔔 Notification Bell */}
            <button
              onClick={() => navigate(ROUTE_PATHS.RENEWAL_ALERTS)}
              className="relative p-2 rounded-lg text-gray-500 hover:bg-gray-100 hover:text-gray-700 transition-colors"
              title="Thông báo gia hạn"
            >
              <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="w-5 h-5">
                <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
                <path d="M13.73 21a2 2 0 0 1-3.46 0" />
              </svg>
              {unreadCount > 0 && (
                <span className="absolute -top-0.5 -right-0.5 inline-flex items-center justify-center min-w-[18px] h-[18px] px-1 text-[10px] font-bold text-white bg-red-500 rounded-full animate-pulse">
                  {unreadCount > 99 ? '99+' : unreadCount}
                </span>
              )}
            </button>

            <div className="hidden sm:block text-right">
              <div className="text-sm font-medium text-gray-800">{user?.fullName ?? 'Người dùng'}</div>
              <div className="text-xs text-gray-400">{user?.role}</div>
            </div>
            <div className="w-9 h-9 rounded-full bg-indigo-100 flex items-center justify-center text-indigo-600 font-semibold text-sm">
              {user?.fullName?.[0] ?? 'U'}
            </div>
            <button
              onClick={handleLogout}
              className="px-3 py-1.5 text-xs font-medium text-red-600 hover:bg-red-50 rounded-md transition-colors border border-red-200"
            >
              Đăng xuất
            </button>
          </div>
        </header>

        {/* Page Content */}
        <main className="flex-1 overflow-auto p-4 lg:p-6">
          <Outlet />
        </main>
      </div>

      {/* Global Toast Notifications */}
      <Toast />
    </div>
  );
}
