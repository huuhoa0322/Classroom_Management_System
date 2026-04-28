import { create } from 'zustand';
import { persist } from 'zustand/middleware';

/**
 * Zustand Auth Store — Quản lý trạng thái xác thực toàn cục của ứng dụng CLS.
 * Persist token + user info cơ bản (id, fullName, email, role) để duy trì phiên sau refresh.
 */
export const useAuthStore = create(
  persist(
    (set) => ({
      // ── State ──────────────────────────────────────────────────────────────
      user: null,               // { id, fullName, email, role }
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,
      _hasHydrated: false,      // true sau khi persist đọc xong localStorage

      // ── Actions ────────────────────────────────────────────────────────────

      /**
       * Gọi sau khi đăng nhập thành công.
       * @param {{ id: number, fullName: string, email: string, role: string }} userData
       * @param {{ accessToken: string, refreshToken: string }} tokens
       */
      login: (userData, tokens) =>
        set({
          user: userData,
          accessToken: tokens.accessToken,
          refreshToken: tokens.refreshToken,
          isAuthenticated: true,
        }),

      /**
       * Gọi khi đăng xuất hoặc token hết hạn (401).
       */
      logout: () =>
        set({
          user: null,
          accessToken: null,
          refreshToken: null,
          isAuthenticated: false,
        }),

      /**
       * Cập nhật access token mới sau khi refresh (nếu có refresh flow sau này).
       * @param {string} newAccessToken
       */
      updateToken: (newAccessToken) =>
        set({ accessToken: newAccessToken }),

      setHasHydrated: (state) => set({ _hasHydrated: state }),
    }),
    {
      name: 'cls-auth-storage',  // Tên key trong localStorage
      // Persist token + user info cơ bản để duy trì phiên sau F5 refresh
      partialize: (state) => ({
        user: state.user,
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        isAuthenticated: state.isAuthenticated,
      }),
      onRehydrateStorage: () => (state) => {
        // Đánh dấu hydration hoàn tất — PrivateRoute sẽ không render trước bước này
        state?.setHasHydrated(true);
      },
    }
  )
);
