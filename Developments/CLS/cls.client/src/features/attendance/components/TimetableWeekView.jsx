import SessionCard from './SessionCard';

const DAYS_VN = ['Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'Chủ nhật'];

/**
 * TimetableWeekView — Grid tuần (Mon–Sun) hiển thị session cards.
 */
export default function TimetableWeekView({ weekStart, sessions = [] }) {
  // Build 7 days starting from weekStart (Monday)
  const days = Array.from({ length: 7 }, (_, i) => {
    const d = new Date(weekStart);
    d.setDate(d.getDate() + i);
    return d;
  });

  const today = new Date();
  today.setHours(0, 0, 0, 0);

  const isSameDay = (d1, d2) =>
    d1.getFullYear() === d2.getFullYear() &&
    d1.getMonth() === d2.getMonth() &&
    d1.getDate() === d2.getDate();

  const isDayToday = (d) => isSameDay(d, new Date());

  const isFutureDay = (d) => {
    const dayStart = new Date(d);
    dayStart.setHours(0, 0, 0, 0);
    return dayStart > today;
  };

  return (
    <div className="grid grid-cols-1 md:grid-cols-7 gap-3">
      {days.map((day, idx) => {
        const daySessions = sessions.filter((s) =>
          isSameDay(new Date(s.startTime), day)
        );
        const future = isFutureDay(day);

        return (
          <div
            key={day.toISOString()}
            className={`rounded-xl border p-3 min-h-[120px] transition-colors ${
              isDayToday(day)
                ? 'border-indigo-300 bg-indigo-50/50'
                : future
                  ? 'border-gray-100 bg-gray-50/60 opacity-50'
                  : 'border-gray-200 bg-white'
            }`}
          >
            {/* Day header */}
            <div className="text-center mb-2 pb-2 border-b border-gray-100">
              <p className={`text-xs font-medium ${
                isDayToday(day) ? 'text-indigo-600' : 'text-gray-500'
              }`}>
                {DAYS_VN[idx]}
              </p>
              <p className={`text-lg font-bold ${
                isDayToday(day) ? 'text-indigo-700' : 'text-gray-800'
              }`}>
                {day.getDate()}
              </p>
            </div>

            {/* Sessions */}
            <div className="space-y-2">
              {daySessions.length > 0 ? (
                daySessions.map((session) => (
                  <SessionCard key={session.id} session={session} isFuture={future} />
                ))
              ) : (
                <p className="text-xs text-gray-300 text-center py-4">
                  Không có lịch
                </p>
              )}
            </div>
          </div>
        );
      })}
    </div>
  );
}
