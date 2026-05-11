import { Routes, Route, Navigate } from 'react-router-dom';
import { PrivateRoute } from '@/app/guards/PrivateRoute';
import { MainLayout } from '@/app/layouts/MainLayout';
import { AuthLayout } from '@/app/layouts/AuthLayout';
import { useAuthStore } from '@/app/provider/authStore';
import { USER_ROLES, ROUTE_PATHS } from '@/shared/utils/constants';

// ── Pages ──────────────────────────────────────────────────────────────────
import LoginPage from '@/features/auth/pages/LoginPage';
import DashboardPage from '@/features/academic/DashboardPage';
import StudentPage from '@/features/student/pages/StudentPage';
import ClassPage from '@/features/class/pages/ClassPage';
import RoomPage from '@/features/room/pages/RoomPage';
import PackagePage from '@/features/package/pages/PackagePage';
import UserPage from '@/features/user/pages/UserPage';
import SessionPage from '@/features/session/pages/SessionPage';
import AttendancePage from '@/features/attendance/pages/AttendancePage';
import TimetablePage from '@/features/attendance/pages/TimetablePage';
import FeedbackListPage from '@/features/feedback/pages/FeedbackListPage';
import FeedbackFormPage from '@/features/feedback/pages/FeedbackFormPage';
import StudentFinancialPage from '@/features/financial/pages/StudentFinancialPage';
import PaymentManagementPage from '@/features/financial/pages/PaymentManagementPage';
import RenewalAlertsPage from '@/features/retention/pages/RenewalAlertsPage';
import ActivityLogPage from '@/features/activity-log/pages/ActivityLogPage';

/**
 * RoleBasedIndex — Redirect theo role khi truy cập trang gốc (/).
 * - Admin   → Dashboard
 * - Teacher → Thời khóa biểu (/timetable)
 */
function RoleBasedIndex() {
  const user = useAuthStore((s) => s.user);

  if (user?.role === USER_ROLES.TEACHER) {
    return <Navigate to={ROUTE_PATHS.TIMETABLE} replace />;
  }

  return <DashboardPage />;
}

/**
 * AppRouter — Định nghĩa toàn bộ cấu trúc route của ứng dụng CLS.
 * Protected routes được bảo vệ bởi <PrivateRoute />.
 */
export default function AppRouter() {
  return (
    <Routes>
      {/* ── Auth Routes (Public) ───────────────────────────────────────── */}
      <Route element={<AuthLayout />}>
        <Route path="/login" element={<LoginPage />} />
      </Route>

      {/* ── Protected Routes ──────────────────────────────────────────── */}
      <Route
        element={
          <PrivateRoute>
            <MainLayout />
          </PrivateRoute>
        }
      >
        <Route index element={<RoleBasedIndex />} />
        <Route path="students" element={<StudentPage />} />
        <Route path="students/:id/financials" element={<StudentFinancialPage />} />
        <Route path="payments" element={<PaymentManagementPage />} />
        <Route path="classes" element={<ClassPage />} />
        <Route path="rooms" element={<RoomPage />} />
        <Route path="packages" element={<PackagePage />} />
        <Route path="users" element={<UserPage />} />
        <Route path="sessions" element={<SessionPage />} />
        <Route path="attendance" element={<AttendancePage />} />
        <Route path="timetable" element={<TimetablePage />} />
        <Route path="timetable/:sessionId/attendance" element={<AttendancePage />} />
        <Route path="timetable/:sessionId/feedback" element={<FeedbackListPage />} />
        <Route path="timetable/:sessionId/feedback/:studentId" element={<FeedbackFormPage />} />
        <Route path="renewal-alerts" element={<RenewalAlertsPage />} />
        <Route path="activity-logs" element={<ActivityLogPage />} />
      </Route>

      {/* ── Fallback ───────────────────────────────────────────────────── */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

