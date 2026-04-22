# CLS Frontend Coding Conventions

> **Scope:** React 19, Vite 8, JavaScript (ES6+), Tailwind CSS 4.  
> **Source:** [ADR-001](../Documents/05_ADR/ADR-001-adopt-modular-monolith-dotnet-react.md)  
> **Audience:** AI agents, frontend developers, code reviewers.  
> **Related:** [coding-conventions-backend.md](./coding-conventions-backend.md) | [api-design-rules.md](./api-design-rules.md)

---

## 1. Tech Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| **Framework** | React | 19 |
| **Build Tool** | Vite | 8 |
| **Language** | JavaScript | ES6+ (no TypeScript) |
| **Styling** | Tailwind CSS | 4 |
| **UI Components** | Shadcn/ui or Headless UI | — |
| **State (server)** | TanStack Query (React Query) | 5+ |
| **State (global)** | Zustand | — |
| **HTTP Client** | Axios | — |
| **Routing** | React Router | 7 |
| **Forms** | React Hook Form + Zod | — |
| **Icons** | Lucide React | — |

---

## 2. Project Structure

```
Developments/CLS/cls.client/
└── src/
    ├── app/                           ← App shell: routing, guards, layouts
    │   ├── guards/                    ← PrivateRoute, RoleGuard
    │   ├── layouts/                   ← MainLayout, AuthLayout
    │   ├── provider/                  ← AuthProvider, ThemeProvider
    │   ├── routers/                   ← AppRouter.jsx (route definitions)
    │   └── App.jsx                    ← Root component
    ├── assets/                        ← Images, icons, fonts (static files)
    ├── features/                      ← Feature modules (domain-driven)
    │   ├── academic/                  ← Scheduling, class management
    │   │   ├── components/            ← Feature-specific components
    │   │   ├── hooks/                 ← Feature-specific custom hooks
    │   │   ├── pages/                 ← Route-level page components
    │   │   └── schemas/               ← Zod validation schemas
    │   ├── student/                   ← Enrollment, attendance, packages
    │   ├── parent/                    ← Parent portal, notifications
    │   └── auth/                      ← Login, session management
    ├── shared/                        ← Cross-feature reusable code
    │   ├── components/                ← Button, Modal, Table, Badge, etc.
    │   ├── hoc/                       ← withAuth, withLoading
    │   ├── hooks/                     ← useAuth, useFetch, useDebounce
    │   ├── services/                  ← Axios API client + service modules
    │   └── utils/                     ← formatDate, validators, constants
    └── styles/                        ← Tailwind config + global CSS
```

---

## 3. Naming Conventions (React / JavaScript)

| Element | Convention | Example |
|---------|-----------|---------|
| **Component file** | PascalCase `.jsx` | `StudentTable.jsx` |
| **Hook file** | camelCase, prefix `use` | `useStudents.js` |
| **Service file** | camelCase, suffix `Service` | `studentService.js` |
| **Util file** | camelCase | `dateFormatter.js` |
| **Page component file** | PascalCase, suffix `Page` | `StudentsPage.jsx` |
| **Component function** | PascalCase | `function StudentTable()` |
| **Variable / function** | camelCase | `const fetchStudents = ...` |
| **Constant** | SCREAMING_SNAKE_CASE | `const API_BASE_URL = ...` |
| **Event handler** | Prefix `handle` | `handleSubmit`, `handleDelete` |
| **Boolean prop/state** | Prefix `is`/`has`/`can` | `isLoading`, `hasError`, `canEdit` |
| **CSS class** | Tailwind utilities only | `className="flex gap-4 p-2"` |

---

## 4. Component Rules

```jsx
// ✅ CORRECT — Named export, JSDoc for shared components
import { useState } from 'react';

/**
 * StudentTable — Hiển thị danh sách học viên với phân trang
 * @param {Object} props
 * @param {number} props.classId - ID của lớp học cần lọc
 * @param {Function} props.onSelect - Callback khi chọn một học viên
 */
export function StudentTable({ classId, onSelect }) {
  const [isLoading, setIsLoading] = useState(false);

  return (
    <div className="overflow-x-auto rounded-lg border border-gray-200">
      {/* table content */}
    </div>
  );
}

// ❌ WRONG — Default export for non-page component, no JSDoc, inline styles
export default function studentTable({ classId }) {
  return <div style={{ overflow: 'auto' }}>...</div>;
}
```

**Rules:**
- Use **named exports** for all components — default export only for page/route components
- One component per file (exception: tiny sub-components used only within the same file)
- Shared components (`shared/components/`) MUST have JSDoc prop documentation
- No inline styles — **Tailwind utilities only**

---

## 5. Custom Hooks

```js
// ✅ CORRECT — shared/hooks/useStudents.js
import { useState, useEffect } from 'react';
import { studentService } from '@/shared/services/studentService';

export function useStudents(classId) {
  const [students, setStudents] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (!classId) return;
    setIsLoading(true);
    studentService.getByClassId(classId)
      .then(res => setStudents(res.data.data))
      .catch(setError)
      .finally(() => setIsLoading(false));
  }, [classId]);

  return { students, isLoading, error };
}
```

**Rules:**
- Reusable hooks → `shared/hooks/`; feature-specific hooks → `features/[module]/hooks/`
- Data-fetching hooks MUST return `{ data, isLoading, error }` pattern
- NEVER call hooks conditionally (React rules of hooks)
- Hook files prefix: `use` (e.g., `useStudents.js`, `useAuth.js`)

> **Preferred:** Use **TanStack Query** (`useQuery`, `useMutation`) for server-state hooks instead of manual `useState` + `useEffect` — see Section 7.

---

## 6. API Service Layer

```js
// ✅ CORRECT — shared/services/studentService.js
import { apiClient } from './apiClient';

export const studentService = {
  getAll: (params) => apiClient.get('/students', { params }),
  getById: (id) => apiClient.get(`/students/${id}`),
  create: (data) => apiClient.post('/students', data),
  update: (id, data) => apiClient.put(`/students/${id}`, data),
  delete: (id) => apiClient.delete(`/students/${id}`),
};
```

```js
// shared/services/apiClient.js
import axios from 'axios';

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL, // e.g., https://localhost:57264/api/v1
  timeout: 10000,
});

// JWT interceptor — auto-attach token
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});
```

**Rules:**
- ALL API calls go through the `apiClient` Axios instance — never call `fetch()` directly
- Service functions MUST NOT use `try/catch` — let errors bubble up to hook/component
- Base URL from `import.meta.env.VITE_API_BASE_URL` (`.env` file)
- Each domain module has its own service file: `studentService.js`, `classSessionService.js`

---

## 7. State Management

| Scope | Tool | When to use |
|-------|------|-------------|
| **Server state** | TanStack Query | API data, lists, pagination, caching, mutations |
| **Global app state** | Zustand | Auth user info, theme, sidebar toggle |
| **Local UI state** | `useState` / `useReducer` | Modal open/close, tab selection, local toggle |
| **Form state** | React Hook Form + Zod | All form inputs and validation |

**Rules:**
- Do **NOT** use Zustand for server state — TanStack Query handles caching, refetching, invalidation
- Do **NOT** put derived data into state — compute it inline from existing state/props
- Do **NOT** use `useEffect` to sync state from props — use derived values or TanStack Query
- Form validation schema (Zod) in the same file as the form, or in `features/[module]/schemas/`

```js
// ✅ CORRECT — TanStack Query for server data
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { studentService } from '@/shared/services/studentService';

export function useStudentDetail(id) {
  return useQuery({
    queryKey: ['students', id],
    queryFn: () => studentService.getById(id).then(res => res.data.data),
    enabled: !!id,
  });
}

export function useCreateStudent() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data) => studentService.create(data).then(res => res.data.data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['students'] }),
  });
}
```

---

## 8. Form Handling (React Hook Form + Zod)

```jsx
// ✅ CORRECT
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';

const createStudentSchema = z.object({
  fullName: z.string().min(1, 'Full name is required').max(100),
  email: z.string().email('Invalid email format').optional(),
  phoneNumber: z.string().regex(/^\d{10,11}$/, 'Phone must be 10-11 digits').optional(),
});

export function CreateStudentForm({ onSuccess }) {
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm({
    resolver: zodResolver(createStudentSchema),
  });

  const onSubmit = async (data) => {
    await studentService.create(data);
    onSuccess?.();
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
      <input {...register('fullName')} placeholder="Full name" />
      {errors.fullName && <span className="text-red-500 text-sm">{errors.fullName.message}</span>}
      <button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Saving...' : 'Save'}
      </button>
    </form>
  );
}
```

**Rules:**
- ALL forms MUST use React Hook Form — no uncontrolled `useState` per field
- Zod schema MUST be defined outside the component (module scope)
- Display field-level errors from `formState.errors`
- Disable submit button during `isSubmitting`

---

## 9. Routing Rules

```jsx
// app/routers/AppRouter.jsx
const AppRouter = () => (
  <Routes>
    <Route path="/" element={<PrivateRoute><MainLayout /></PrivateRoute>}>
      <Route index element={<DashboardPage />} />
      <Route path="students" element={<StudentsPage />} />
      <Route path="students/:id" element={<StudentDetailPage />} />
      <Route path="class-sessions" element={<ClassSessionsPage />} />
    </Route>
    <Route path="/login" element={<LoginPage />} />
  </Routes>
);
```

**Rules:**
- Route paths: all lowercase, **kebab-case** (`/class-sessions`, NOT `/classSessions`)
- Protected routes MUST be wrapped with `<PrivateRoute>` HOC
- Page components live in `features/[module]/pages/` and named with `Page` suffix
- Use `<Outlet />` in layout components for nested routing

---

## 10. Styling Rules (Tailwind CSS)

```jsx
// ✅ CORRECT — Tailwind utilities, responsive, semantic
<div className="flex items-center gap-3 p-4 rounded-xl border border-gray-200 bg-white shadow-sm hover:shadow-md transition-shadow">
  <span className="text-sm font-medium text-gray-700">{student.fullName}</span>
  <span className="ml-auto text-xs text-gray-400">{student.remainingSessions} sessions</span>
</div>

// ❌ WRONG — Inline styles
<div style={{ display: 'flex', padding: '16px', borderRadius: '12px' }}>
```

**Rules:**
- **No inline styles** — Tailwind utilities only
- **No custom CSS files** unless defining a shared design token (e.g., custom color variable in `styles/global.css`)
- Responsive design: mobile-first (`sm:`, `md:`, `lg:` prefixes)
- Dark mode: use Tailwind `dark:` variants if dark mode is enabled
- Consistent spacing: use Tailwind spacing scale (4 = 16px, 6 = 24px, 8 = 32px)

---

## 11. Code Quality Rules

### General

- Maximum component length: **150 lines** (extract sub-components if longer)
- No magic numbers/strings — define constants in `shared/utils/constants.js`
- Remove all `console.log` before committing

### Git Commit Convention (Conventional Commits)

```
feat(student): add renewal alert banner component
fix(auth): handle token expiry redirect correctly
refactor(schedule): extract conflict indicator to shared component
docs(api): update studentService JSDoc
test(enrollment): add test for useEnrollment hook
```

Format: `<type>(<scope>): <short description>`  
Types: `feat` | `fix` | `refactor` | `docs` | `test` | `chore` | `perf`

### Branch Strategy

```
main        ← Production-ready, protected
develop     ← Integration branch
feature/    ← feature/cls-123-student-table-ui
fix/        ← fix/cls-456-date-format-display
```

---

## 12. Testing Standards (Frontend)

| Layer | Framework | What to test |
|-------|-----------|-------------|
| **Unit** | Vitest + React Testing Library | Shared components, custom hooks, utility functions |
| **E2E** | Playwright (post-MVP) | Critical user flows (login, enrollment, attendance) |

**Minimum coverage (MVP):**
- Shared components (`shared/components/`): render tests for all states (loading, error, empty, data)
- Custom hooks: test return values and state transitions
- Utility functions: 100% of `shared/utils/`

**Test file location:** Co-locate with source → `StudentTable.test.jsx` next to `StudentTable.jsx`

---

> **Last Updated:** 2026-04-21  
> **Maintained by:** Tech Lead / SA Lead  
> **Related:** [coding-conventions-backend.md](./coding-conventions-backend.md) | [api-design-rules.md](./api-design-rules.md) | [ADR-001](../Documents/05_ADR/ADR-001-adopt-modular-monolith-dotnet-react.md)
