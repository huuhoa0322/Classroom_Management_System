import { Routes, Route, Navigate } from 'react-router-dom';
import { PrivateRoute } from '@/app/guards/PrivateRoute';
import { MainLayout } from '@/app/layouts/MainLayout';
import { AuthLayout } from '@/app/layouts/AuthLayout';

// ── Pages ──────────────────────────────────────────────────────────────────
import LoginPage from '@/features/auth/pages/LoginPage';
import DashboardPage from '@/features/academic/DashboardPage';
import StudentPage from '@/features/student/pages/StudentPage';
import ClassPage from '@/features/class/pages/ClassPage';
import SessionPage from '@/features/session/pages/SessionPage';
import AttendancePage from '@/features/attendance/pages/AttendancePage';
import StudentFinancialPage from '@/features/financial/pages/StudentFinancialPage';

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
        <Route index element={<DashboardPage />} />
        <Route path="students" element={<StudentPage />} />
        <Route path="students/:id/financials" element={<StudentFinancialPage />} />
        <Route path="classes" element={<ClassPage />} />
        <Route path="sessions" element={<SessionPage />} />
        <Route path="attendance" element={<AttendancePage />} />
      </Route>

      {/* ── Fallback ───────────────────────────────────────────────────── */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

