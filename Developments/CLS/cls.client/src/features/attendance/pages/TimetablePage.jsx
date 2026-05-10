import { useState, useMemo } from 'react';
import { useTimetable } from '../hooks/useAttendance';
import WeekNavigator from '../components/WeekNavigator';
import TimetableWeekView from '../components/TimetableWeekView';
import { ConnectionErrorBanner } from '@/shared/components/ConnectionErrorBanner';

/**
 * Lấy Monday (đầu tuần) của ngày d.
 */
function getMonday(d = new Date()) {
  const date = new Date(d);
  const day = date.getDay();
  const diff = day === 0 ? -6 : 1 - day; // Sunday = 0 → lùi 6 ngày
  date.setDate(date.getDate() + diff);
  date.setHours(0, 0, 0, 0);
  return date;
}

/**
 * TimetablePage — UC-07: Xem lịch dạy cá nhân.
 * Hiển thị lịch tuần (Mon–Sun) với navigation prev/next.
 */
export default function TimetablePage() {
  const [weekStart, setWeekStart] = useState(() => getMonday());

  const { from, to } = useMemo(() => {
    const f = new Date(weekStart);
    const t = new Date(weekStart);
    t.setDate(t.getDate() + 7);
    return {
      from: f.toISOString(),
      to: t.toISOString(),
    };
  }, [weekStart]);

  const { data: sessions, isLoading, isError, error, refetch } = useTimetable(from, to);

  const goPrev = () => {
    setWeekStart((prev) => {
      const d = new Date(prev);
      d.setDate(d.getDate() - 7);
      return d;
    });
  };

  const goNext = () => {
    setWeekStart((prev) => {
      const d = new Date(prev);
      d.setDate(d.getDate() + 7);
      return d;
    });
  };

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div className="flex items-center gap-3">
          <div className="p-2.5 bg-indigo-100 rounded-xl">
            <span className="text-xl">📅</span>
          </div>
          <div>
            <h1 className="text-2xl font-bold text-gray-800">Lịch dạy</h1>
            <p className="text-sm text-gray-500">Xem lịch giảng dạy theo tuần</p>
          </div>
        </div>

        <WeekNavigator
          weekStart={weekStart}
          onPrev={goPrev}
          onNext={goNext}
        />
      </div>

      {/* Content */}
      {isLoading && (
        <div className="flex justify-center py-16">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600" />
        </div>
      )}

      {isError && (
        <ConnectionErrorBanner error={error} onRetry={() => refetch()} />
      )}

      {!isLoading && !isError && (
        <TimetableWeekView weekStart={weekStart} sessions={sessions || []} />
      )}
    </div>
  );
}
