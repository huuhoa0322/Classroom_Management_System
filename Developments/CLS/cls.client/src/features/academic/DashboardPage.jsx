import { Link } from 'react-router-dom';

/** Dashboard placeholder -- se hoan thien sau */
export default function DashboardPage() {
  return (
    <div className="p-6">
      <h1 className="text-2xl font-bold text-gray-800 mb-1">Dashboard</h1>
      <p className="text-sm text-gray-500">Chào mừng đến với Hệ thống Quản lý Lớp học CLS.</p>

      <div className="mt-6 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {[
          { label: 'Học sinh', value: '—', icon: '👨‍🎓', color: 'bg-indigo-50 text-indigo-600', to: '/students' },
          { label: 'Tài chính', value: '—', icon: '💰', color: 'bg-blue-50 text-blue-600', to: '/payments' },
          { label: 'Buổi học hôm nay', value: '—', icon: '📅', color: 'bg-amber-50 text-amber-600' },
          { label: 'Cần gia hạn', value: '—', icon: '⚠️', color: 'bg-red-50 text-red-600' },
        ].map((card) =>
          card.to ? (
            <Link
              key={card.label}
              to={card.to}
              className={`rounded-xl p-4 ${card.color} border border-white shadow-sm hover:shadow-md transition-shadow`}
            >
              <div className="text-2xl mb-2">{card.icon}</div>
              <div className="text-2xl font-bold">{card.value}</div>
              <div className="text-sm font-medium mt-1">{card.label}</div>
            </Link>
          ) : (
            <div key={card.label} className={`rounded-xl p-4 ${card.color} border border-white shadow-sm`}>
              <div className="text-2xl mb-2">{card.icon}</div>
              <div className="text-2xl font-bold">{card.value}</div>
              <div className="text-sm font-medium mt-1">{card.label}</div>
            </div>
          )
        )}
      </div>

      <p className="mt-8 text-xs text-gray-400">
        📌 Số liệu thực tế sẽ được tích hợp khi hoàn thiện các Feature Slices.
      </p>
    </div>
  );
}
