/**
 * WeekNavigator — Điều hướng tuần: "Tuần 27/04 – 03/05", nút prev/next.
 */
export default function WeekNavigator({ weekStart, onPrev, onNext }) {
  const weekEnd = new Date(weekStart);
  weekEnd.setDate(weekEnd.getDate() + 6);

  const fmt = (d) =>
    d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' });

  const isCurrentWeek = () => {
    const today = new Date();
    return today >= weekStart && today <= weekEnd;
  };

  return (
    <div className="flex items-center gap-3">
      <button
        onClick={onPrev}
        className="p-2 rounded-lg hover:bg-gray-100 transition-colors"
        title="Tuần trước"
      >
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor" className="w-5 h-5 text-gray-600">
          <path strokeLinecap="round" strokeLinejoin="round" d="M15.75 19.5 8.25 12l7.5-7.5" />
        </svg>
      </button>

      <div className="text-center min-w-[180px]">
        <span className="text-sm font-semibold text-gray-800">
          Tuần {fmt(weekStart)} – {fmt(weekEnd)}
        </span>
        {isCurrentWeek() && (
          <span className="ml-2 text-xs bg-blue-100 text-blue-600 px-2 py-0.5 rounded-full">
            Hiện tại
          </span>
        )}
      </div>

      <button
        onClick={onNext}
        className="p-2 rounded-lg hover:bg-gray-100 transition-colors"
        title="Tuần sau"
      >
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor" className="w-5 h-5 text-gray-600">
          <path strokeLinecap="round" strokeLinejoin="round" d="m8.25 4.5 7.5 7.5-7.5 7.5" />
        </svg>
      </button>
    </div>
  );
}
