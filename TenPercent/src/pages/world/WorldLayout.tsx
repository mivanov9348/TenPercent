import { NavLink, Outlet } from 'react-router-dom';
import { Trophy, Globe, CalendarDays, BarChart2 } from 'lucide-react';

export default function WorldLayout() {
  const tabs = [
    { name: 'Standings', path: '/world/standings', icon: <Trophy size={18} /> },
    { name: 'Fixtures', path: '/world/fixtures', icon: <CalendarDays size={18} /> }, 
    { name: 'Season Stats', path: '/world/stats', icon: <BarChart2 size={18} /> },
  ];

  return (
    <div className="space-y-6">
      {/* World Header & Tabs */}
      <div className="bg-gray-800 border border-gray-700 p-4 rounded-2xl shadow-lg">
        <div className="flex items-center gap-4 mb-6 px-2 pt-2">
          <div className="w-12 h-12 bg-gray-900 border border-gray-700 rounded-xl flex items-center justify-center text-blue-400">
            <Globe size={24} />
          </div>
          <div>
            <h1 className="text-2xl font-black text-white uppercase tracking-wider">World Database</h1>
            <p className="text-gray-400 text-sm">Explore global leagues, fixtures, rankings, and stats.</p>
          </div>
        </div>

        {/* Inner Navigation (Tabs) */}
        <nav className="flex gap-2 border-b border-gray-700 pb-0 px-2 overflow-x-auto custom-scrollbar">
          {tabs.map((tab) => (
            <NavLink
              key={tab.path}
              to={tab.path}
              className={({ isActive }) =>
                `flex items-center gap-2 px-4 py-3 border-b-2 font-bold text-sm transition-colors whitespace-nowrap ${
                  isActive 
                    ? 'border-yellow-500 text-yellow-500 bg-gray-900/50 rounded-t-lg' 
                    : 'border-transparent text-gray-400 hover:text-white hover:bg-gray-700/50 rounded-t-lg'
                }`
              }
            >
              {tab.icon}
              {tab.name}
            </NavLink>
          ))}
        </nav>
      </div>

      {/* Dynamic Content */}
      <div className="animate-in fade-in duration-300">
        <Outlet />
      </div>
    </div>
  );
}