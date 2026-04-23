/**
 * Hằng số Roles của hệ thống CLS.
 * Đồng bộ với AppRoles ở Backend C#.
 */
export const USER_ROLES = {
  ADMIN: 'Admin',
  TEACHER: 'Teacher',
};

/**
 * Đường dẫn route của ứng dụng CLS.
 */
export const ROUTE_PATHS = {
  LOGIN: '/login',
  DASHBOARD: '/',
  STUDENTS: '/students',
  STUDENT_DETAIL: '/students/:id',
  STUDENT_FINANCIALS: '/students/:id/financials',
  CLASSES: '/classes',
  SESSIONS: '/sessions',
  ATTENDANCE: '/attendance',
};

/**
 * Trạng thái chung của các entity.
 */
export const STATUS = {
  ACTIVE: 'active',
  INACTIVE: 'inactive',
  SUSPENDED: 'suspended',
};

/**
 * Trạng thái của buổi học (Session).
 */
export const SESSION_STATUS = {
  SCHEDULED: 'scheduled',
  COMPLETED: 'completed',
  CANCELLED: 'cancelled',
};

/**
 * Trạng thái gói học (Student Package).
 */
export const PACKAGE_STATUS = {
  PENDING_PAYMENT: 'pending_payment',
  ACTIVE: 'active',
  DEPLETED: 'depleted',
  ARCHIVED: 'archived',
};

/**
 * Trạng thái thanh toán (Payment — CLS-003).
 */
export const PAYMENT_STATUS = {
  PENDING: 'pending',
  CONFIRMED: 'confirmed',
  FAILED: 'failed',
  REFUNDED: 'refunded',
};

/**
 * Cấu hình phân trang mặc định.
 */
export const DEFAULT_PAGINATION = {
  PAGE: 1,
  PAGE_SIZE: 10,
};
