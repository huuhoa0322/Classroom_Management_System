import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Analytics } from '@vercel/analytics/react';
import { SpeedInsights } from '@vercel/speed-insights/react';
import AppRouter from '@/app/routers/AppRouter';
import { ErrorBoundary } from '@/shared/components/ErrorBoundary';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      staleTime: 5 * 60 * 1000, // 5 phút
      refetchOnWindowFocus: false,
    },
  },
});

/**
 * App — Root component của ứng dụng CLS.
 * Wrap toàn bộ với:
 * - QueryClientProvider (TanStack Query — Server state)
 * - BrowserRouter (React Router)
 */
function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <ErrorBoundary>
          <AppRouter />
        </ErrorBoundary>
      </BrowserRouter>
      <Analytics />
      <SpeedInsights />
    </QueryClientProvider>
  );
}

export default App;
