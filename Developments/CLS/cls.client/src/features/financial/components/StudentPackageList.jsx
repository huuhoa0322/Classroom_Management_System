import { getStatusBadge } from '@/shared/utils/formatters';

/**
 * StudentPackageList — Hiển thị danh sách gói học của student dưới dạng cards.
 *
 * @param {{ packages: Array, isLoading: boolean }} props
 */
export function StudentPackageList({ packages, isLoading }) {
  if (isLoading) {
    return (
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {[1, 2, 3].map((i) => (
          <div key={i} className="h-40 bg-gray-100 rounded-xl animate-pulse" />
        ))}
      </div>
    );
  }

  if (!packages?.length) {
    return (
      <div className="text-center py-10 text-gray-400">
        <svg className="w-12 h-12 mx-auto mb-3 text-gray-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
        </svg>
        <p>Chưa có gói học nào.</p>
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      {packages.map((pkg) => {
        const badge = getStatusBadge(pkg.status);
        const progress = pkg.totalSessions > 0
          ? Math.round((pkg.remainingSessions / pkg.totalSessions) * 100)
          : 0;
        const progressColor =
          progress > 50 ? 'bg-green-500' :
          progress > 20 ? 'bg-yellow-500' : 'bg-red-500';

        return (
          <div
            key={pkg.id}
            className="bg-white border border-gray-200 rounded-xl p-5 shadow-sm hover:shadow-md transition-shadow"
          >
            {/* Header */}
            <div className="flex items-start justify-between mb-3">
              <h4 className="font-semibold text-gray-800 text-sm">{pkg.packageName}</h4>
              <span className={`text-xs font-medium px-2.5 py-0.5 rounded-full ${badge.className}`}>
                {badge.label}
              </span>
            </div>

            {/* Sessions info */}
            <div className="mb-3">
              <div className="flex justify-between text-sm mb-1">
                <span className="text-gray-500">Còn lại</span>
                <span className="font-semibold text-gray-800">
                  {pkg.remainingSessions} / {pkg.totalSessions} buổi
                </span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div
                  className={`h-2 rounded-full transition-all ${progressColor}`}
                  style={{ width: `${progress}%` }}
                />
              </div>
            </div>

            {/* Dates */}
            <div className="text-xs text-gray-400 space-y-1">
              <div className="flex justify-between">
                <span>Bắt đầu:</span>
                <span>{pkg.startDate}</span>
              </div>
              <div className="flex justify-between">
                <span>Hết hạn:</span>
                <span>{pkg.endDate}</span>
              </div>
            </div>
          </div>
        );
      })}
    </div>
  );
}
