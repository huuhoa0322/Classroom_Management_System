import { useState, useEffect } from 'react';

/**
 * SlaTimer — Hiển thị countdown SLA hoặc trạng thái quá hạn.
 * @param {string} deadline - ISO string of SLA deadline (UTC)
 */
export function SlaTimer({ deadline }) {
  const [now, setNow] = useState(Date.now());

  useEffect(() => {
    const interval = setInterval(() => setNow(Date.now()), 60_000); // update every minute
    return () => clearInterval(interval);
  }, []);

  const deadlineMs = new Date(deadline).getTime();
  const diff = deadlineMs - now;
  const isExpired = diff <= 0;

  if (isExpired) {
    const overdue = Math.abs(diff);
    const hours = Math.floor(overdue / 3_600_000);
    const minutes = Math.floor((overdue % 3_600_000) / 60_000);

    return (
      <div className="flex items-center gap-2 px-3 py-1.5 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">
        <span>⚠️</span>
        <span>
          Quá hạn SLA{' '}
          <strong>
            {hours > 0 && `${hours} giờ `}
            {minutes} phút
          </strong>{' '}
          trước
        </span>
      </div>
    );
  }

  const hours = Math.floor(diff / 3_600_000);
  const minutes = Math.floor((diff % 3_600_000) / 60_000);

  const urgency =
    hours < 2
      ? 'bg-amber-50 border-amber-200 text-amber-700'
      : 'bg-green-50 border-green-200 text-green-700';

  return (
    <div className={`flex items-center gap-2 px-3 py-1.5 border rounded-lg text-sm ${urgency}`}>
      <span>⏱️</span>
      <span>
        Còn{' '}
        <strong>
          {hours > 0 && `${hours} giờ `}
          {minutes} phút
        </strong>
      </span>
    </div>
  );
}
