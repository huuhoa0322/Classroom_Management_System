/**
 * Badge hiển thị trạng thái học sinh.
 * @param {{ status: string }} props
 */
export const StatusBadge = ({ status }) => {
  const styles = {
    active:   'bg-emerald-100 text-emerald-700 border border-emerald-200',
    inactive: 'bg-red-100 text-red-700 border border-red-200',
  };
  const labels = {
    active:   'Đang học',
    inactive: 'Đã nghỉ',
  };

  const cls = styles[status] ?? 'bg-gray-100 text-gray-600 border border-gray-200';
  const label = labels[status] ?? status;

  return (
    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${cls}`}>
      {label}
    </span>
  );
};
