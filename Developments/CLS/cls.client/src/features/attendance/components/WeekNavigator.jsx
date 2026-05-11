import { useMemo } from 'react';

/**
 * Lấy Monday (đầu tuần ISO) của ngày d.
 */
function getMonday(d) {
  const date = new Date(d);
  const day = date.getDay();
  const diff = day === 0 ? -6 : 1 - day;
  date.setDate(date.getDate() + diff);
  date.setHours(0, 0, 0, 0);
  return date;
}

/**
 * Tạo danh sách tuần (Monday) cho một năm.
 * Trả về mảng { index, monday, label }.
 */
function getWeeksOfYear(year) {
  const weeks = [];
  // Bắt đầu từ Monday đầu tiên thuộc hoặc gần ngày 1/1
  let d = getMonday(new Date(year, 0, 4)); // ISO: tuần chứa 4/1

  let index = 0;
  while (d.getFullYear() <= year) {
    if (d.getFullYear() > year) break;

    const end = new Date(d);
    end.setDate(end.getDate() + 6);

    const fmt = (dt) =>
      dt.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' });

    weeks.push({
      index,
      monday: new Date(d),
      mondayTime: d.getTime(),
      label: `${fmt(d)} – ${fmt(end)}`,
    });

    index++;
    d.setDate(d.getDate() + 7);
  }

  return weeks;
}

/**
 * WeekNavigator — Dropdown Năm + Tuần để lọc lịch dạy.
 * Hiển thị badge "Hiện tại" khi tuần đang chọn là tuần hiện tại.
 * Nút "Hôm nay" để quay về tuần hiện tại khi đang xem tuần khác.
 *
 * @param {{ weekStart: Date, onJumpToWeek: (monday: Date) => void }} props
 */
export default function WeekNavigator({ weekStart, onJumpToWeek }) {
  const weekEnd = new Date(weekStart);
  weekEnd.setDate(weekEnd.getDate() + 6);

  const isCurrentWeek = () => {
    const today = new Date();
    return today >= weekStart && today <= weekEnd;
  };

  // ── Năm hiện tại & danh sách năm (± 2 năm) ────────────────────────────
  const currentYear = weekStart.getFullYear();
  const yearOptions = useMemo(() => {
    const now = new Date().getFullYear();
    const years = [];
    for (let y = now - 2; y <= now + 2; y++) years.push(y);
    return years;
  }, []);

  // ── Danh sách tuần của năm đang chọn ───────────────────────────────────
  const weeks = useMemo(() => getWeeksOfYear(currentYear), [currentYear]);

  // Tìm index tuần đang chọn dựa trên monday timestamp
  const weekStartTime = weekStart.getTime();
  const selectedWeekIndex = useMemo(() => {
    const found = weeks.findIndex((w) => w.mondayTime === weekStartTime);
    return found >= 0 ? found : 0;
  }, [weeks, weekStartTime]);

  // ── Handlers ───────────────────────────────────────────────────────────
  const handleYearChange = (e) => {
    const newYear = parseInt(e.target.value, 10);
    const newWeeks = getWeeksOfYear(newYear);
    // Nhảy đến cùng index tuần trong năm mới, fallback tuần đầu
    const target = newWeeks[Math.min(selectedWeekIndex, newWeeks.length - 1)] || newWeeks[0];
    onJumpToWeek(target.monday);
  };

  const handleWeekChange = (e) => {
    const idx = parseInt(e.target.value, 10);
    const target = weeks[idx];
    if (target) onJumpToWeek(target.monday);
  };

  const handleGoToday = () => {
    onJumpToWeek(getMonday(new Date()));
  };

  return (
    <div className="flex items-center gap-3">
      {/* Dropdown Năm */}
      <select
        id="timetable-year-filter"
        value={currentYear}
        onChange={handleYearChange}
        className="appearance-none bg-white border border-gray-200 rounded-lg px-3 py-2 text-sm font-medium text-gray-700 hover:border-indigo-300 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 cursor-pointer transition-colors"
        title="Chọn năm"
      >
        {yearOptions.map((y) => (
          <option key={y} value={y}>
            {y}
          </option>
        ))}
      </select>

      {/* Dropdown Tuần (chỉ hiện ngày, không có số tuần) */}
      <select
        id="timetable-week-filter"
        value={selectedWeekIndex}
        onChange={handleWeekChange}
        className="appearance-none bg-white border border-gray-200 rounded-lg px-3 py-2 text-sm font-medium text-gray-700 hover:border-indigo-300 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 cursor-pointer transition-colors"
        title="Chọn tuần"
      >
        {weeks.map((w) => (
          <option key={w.index} value={w.index}>
            {w.label}
          </option>
        ))}
      </select>

      {/* Badge "Hiện tại" */}
      {isCurrentWeek() && (
        <span className="text-xs bg-blue-100 text-blue-600 px-2.5 py-1 rounded-full font-medium whitespace-nowrap">
          Hiện tại
        </span>
      )}

      {/* Nút "Hôm nay" — chỉ hiện khi không phải tuần hiện tại */}
      {!isCurrentWeek() && (
        <button
          onClick={handleGoToday}
          className="px-3 py-1.5 text-xs font-medium text-indigo-600 bg-indigo-50 hover:bg-indigo-100 rounded-lg transition-colors border border-indigo-200 whitespace-nowrap"
          title="Về tuần hiện tại"
        >
          Hôm nay
        </button>
      )}
    </div>
  );
}
