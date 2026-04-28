import { NavLink } from 'react-router-dom';
import { Home, Building2, Users, Wallet, ShoppingCart } from 'lucide-react';

export default function Navbar() {
  const menuItems = [
    { name: 'Home', path: '/', icon: <Home size={18} /> },
    { name: 'My Agency', path: '/agency', icon: <Building2 size={18} /> },
    { name: 'My Players', path: '/players', icon: <Users size={18} /> },
    { name: 'Finance', path: '/finance', icon: <Wallet size={18} /> },
    { name: 'Market', path: '/market', icon: <ShoppingCart size={18} /> },
  ];

  return (
    <header className="w-full h-16 bg-gray-950 border-b border-gray-800 flex items-center justify-between px-6 shrink-0">
      {/* Лого */}
      <div className="flex items-center">
        <h2 className="text-xl font-black tracking-tighter">
          <span className="text-white">TEN</span>
          <span className="text-yellow-500 underline decoration-yellow-500/50">PERCENT</span>
        </h2>
      </div>
      
      {/* Навигация (Център) */}
      <nav className="hidden md:flex items-center gap-2">
        {menuItems.map((item) => (
          <NavLink
            key={item.path}
            to={item.path}
            className={({ isActive }) =>
              `flex items-center gap-2 px-4 py-2 rounded-md transition-all duration-200 text-sm font-medium ${
                isActive 
                  ? 'bg-yellow-500 text-black shadow-[0_0_10px_rgba(234,179,8,0.3)]' 
                  : 'text-gray-400 hover:bg-gray-800 hover:text-white'
              }`
            }
          >
            {item.icon}
            <span>{item.name}</span>
          </NavLink>
        ))}
      </nav>

      {/* Бюджет (Дясно) */}
      <div className="flex items-center">
        <div className="bg-gray-900 border border-gray-800 px-4 py-1.5 rounded-md flex items-center gap-2 shadow-inner">
          <span className="text-xs text-gray-500 uppercase tracking-wider font-bold">Budget</span>
          <span className="text-yellow-500 font-mono font-bold">$10,000,000</span>
        </div>
      </div>
    </header>
  );
}