import { useState, useEffect, useRef } from 'react';
import { useLogin } from '../hooks/useAuth';

/**
 * Trạng thái kết nối server:
 * - 'connecting': Đang ping health check (cold start)
 * - 'ready':      Server đã sẵn sàng
 * - 'error':      Không kết nối được (retry)
 */
const SERVER_STATUS = {
  CONNECTING: 'connecting',
  READY: 'ready',
  ERROR: 'error',
};

export default function LoginPage() {
  const [email, setEmail]       = useState('');
  const [password, setPassword] = useState('');
  const [showPass, setShowPass] = useState(false);

  const { mutate: login, isPending, error } = useLogin();

  // ── Warm-up health check — đánh thức server Render Free Tier ────────────
  const [serverStatus, setServerStatus] = useState(SERVER_STATUS.CONNECTING);
  const [bannerVisible, setBannerVisible] = useState(true);
  const retryCount = useRef(0);
  const maxRetries = 3;

  useEffect(() => {
    let cancelled = false;
    let hideTimer;

    const pingHealth = async () => {
      const baseUrl = import.meta.env.VITE_API_BASE_URL || '/api/v1';
      // Health endpoint nằm ở root, không nằm trong /api/v1
      const healthUrl = baseUrl.replace(/\/api\/v1\/?$/, '') + '/health';

      try {
        const res = await fetch(healthUrl, {
          method: 'GET',
          signal: AbortSignal.timeout(60000), // 60s cho cold start
        });

        if (!cancelled && res.ok) {
          setServerStatus(SERVER_STATUS.READY);
          // Auto-hide banner sau 2 giây khi đã sẵn sàng
          hideTimer = setTimeout(() => {
            if (!cancelled) setBannerVisible(false);
          }, 2000);
        } else if (!cancelled) {
          throw new Error('Health check failed');
        }
      } catch {
        if (cancelled) return;

        retryCount.current += 1;
        if (retryCount.current < maxRetries) {
          setServerStatus(SERVER_STATUS.CONNECTING);
          // Retry sau 3 giây
          setTimeout(() => { if (!cancelled) pingHealth(); }, 3000);
        } else {
          setServerStatus(SERVER_STATUS.ERROR);
        }
      }
    };

    pingHealth();

    return () => {
      cancelled = true;
      clearTimeout(hideTimer);
    };
  }, []);

  const handleSubmit = (e) => {
    e.preventDefault();
    login({ email, password });
  };

  // Trích error message — apiClient interceptor đã unwrap thành Error(message)
  const errorMsg = error?.message ?? null;

  // ── Server status banner config ──────────────────────────────────────────
  const statusConfig = {
    [SERVER_STATUS.CONNECTING]: {
      icon: '🔄',
      text: 'Đang kết nối máy chủ...',
      bgClass: 'bg-amber-50 border-amber-200 text-amber-700',
      showSpinner: true,
    },
    [SERVER_STATUS.READY]: {
      icon: '✅',
      text: 'Máy chủ đã sẵn sàng',
      bgClass: 'bg-green-50 border-green-200 text-green-700',
      showSpinner: false,
    },
    [SERVER_STATUS.ERROR]: {
      icon: '⚠️',
      text: 'Máy chủ đang khởi động, vui lòng đợi...',
      bgClass: 'bg-amber-50 border-amber-200 text-amber-700',
      showSpinner: true,
    },
  };

  const currentStatus = statusConfig[serverStatus];

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-indigo-50 via-white to-purple-50">

      {/* Server warm-up status banner — góc phải trên màn hình */}
      {bannerVisible && (
        <div className={`fixed top-4 right-4 z-50 border rounded-lg px-4 py-3 text-sm flex items-center gap-2 shadow-lg transition-all ${currentStatus.bgClass}`}>
          {currentStatus.showSpinner ? (
            <span className="w-4 h-4 border-2 border-current/30 border-t-current rounded-full animate-spin flex-shrink-0" />
          ) : (
            <span>{currentStatus.icon}</span>
          )}
          <span>{currentStatus.text}</span>
        </div>
      )}

      <div className="w-full max-w-md">

        {/* ── Card ─────────────────────────────────────────────────────── */}
        <div className="bg-white rounded-2xl shadow-xl border border-gray-100 overflow-hidden">

          {/* Header gradient */}
          <div className="bg-gradient-to-r from-indigo-600 to-purple-600 px-8 py-8 text-white">
            <div className="flex items-center gap-3 mb-2">
              <div className="w-10 h-10 bg-white/20 rounded-xl flex items-center justify-center text-xl font-bold">
                CLS
              </div>
              <span className="text-sm font-medium text-indigo-100">Classroom Management System</span>
            </div>
            <h1 className="text-2xl font-bold mt-4">Chào mừng trở lại 👋</h1>
            <p className="text-indigo-200 text-sm mt-1">Đăng nhập để quản lý lớp học của bạn</p>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} className="px-8 py-8 space-y-5">

            {/* Error banner */}
            {errorMsg && (
              <div className="bg-red-50 border border-red-200 rounded-lg px-4 py-3 text-sm text-red-700 flex items-center gap-2">
                <span>⚠️</span>
                <span>{errorMsg}</span>
              </div>
            )}

            {/* Email */}
            <div>
              <label htmlFor="login-email" className="block text-sm font-medium text-gray-700 mb-1.5">
                Email
              </label>
              <input
                id="login-email"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                autoFocus
                placeholder="admin@cls.edu.vn"
                className="w-full rounded-xl border border-gray-200 bg-gray-50 px-4 py-3 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:bg-white transition-colors"
              />
            </div>

            {/* Password */}
            <div>
              <label htmlFor="login-password" className="block text-sm font-medium text-gray-700 mb-1.5">
                Mật khẩu
              </label>
              <div className="relative">
                <input
                  id="login-password"
                  type={showPass ? 'text' : 'password'}
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                  placeholder="••••••••"
                  className="w-full rounded-xl border border-gray-200 bg-gray-50 px-4 py-3 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:bg-white transition-colors pr-12"
                />
                <button
                  type="button"
                  onClick={() => setShowPass((v) => !v)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 text-sm px-1"
                >
                  {showPass ? '🙈' : '👁️'}
                </button>
              </div>
            </div>

            {/* Submit */}
            <button
              id="btn-login"
              type="submit"
              disabled={isPending}
              className="w-full bg-gradient-to-r from-indigo-600 to-purple-600 text-white font-semibold py-3 rounded-xl hover:from-indigo-700 hover:to-purple-700 disabled:opacity-60 disabled:cursor-not-allowed transition-all shadow-md hover:shadow-lg active:scale-[0.99] mt-2"
            >
              {isPending ? (
                <span className="flex items-center justify-center gap-2">
                  <span className="w-4 h-4 border-2 border-white/40 border-t-white rounded-full animate-spin" />
                  Đang đăng nhập...
                </span>
              ) : (
                'Đăng nhập'
              )}
            </button>
          </form>

          {/* Footer */}
          <div className="px-8 pb-6 text-center text-xs text-gray-400">
            Chỉ dành cho Admin và Giáo viên của trung tâm
          </div>
        </div>

        <p className="text-center text-xs text-gray-400 mt-4">
          CLS © {new Date().getFullYear()}
        </p>
      </div>
    </div>
  );
}
