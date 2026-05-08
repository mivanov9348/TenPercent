import { Link, useLocation, useNavigate } from 'react-router-dom';
import { Home, Users, Briefcase, Wallet, LogOut, Globe, Search, Mail } from 'lucide-react';
import { useAgencyStore } from '../../store/useAgencyStore';

export default function Navbar() {
  const location = useLocation();
  const navigate = useNavigate();
  
  // Взимаме бюджета от глобалния стейт
  const { budget } = useAgencyStore();

  const handleLogout = () => {
    localStorage.removeItem('userId');
    localStorage.removeItem('hasAgency');
    localStorage.removeItem('role');
    navigate('/login');
  };

  const navLinks = [
    { name: 'Home', path: '/', icon: <Home size={18} /> },
    { name: 'Inbox', path: '/inbox', icon: <Mail size={18} /> }, // НОВО: Inbox е тук
    { name: 'My Agency', path: '/agency', icon: <Briefcase size={18} /> },
    { name: 'My Clients', path: '/players', icon: <Users size={18} /> },
    { name: 'Finance', path: '/finance', icon: <Wallet size={18} /> },
    { name: 'Scouting Pool', path: '/scouting-pool', icon: <Search size={18} /> },
    { name: 'World', path: '/world', icon: <Globe size={18} /> },
  ];

  // Форматиране на парите за хедъра
  const formatMoney = (amount: number) => {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(amount);
  };

  // ФЕЙК БРОЙ НЕПРОЧЕТЕНИ СЪОБЩЕНИЯ (Засега)
  const unreadMessagesCount = 2;

  return (
    <nav className="bg-gray-950 border-b border-gray-800 shrink-0">
      <div className="max-w-[1600px] mx-auto px-4 flex justify-between items-center h-16">
        
        {/* Лява част - Лого */}
        <div className="flex items-center gap-2">
          <h1 className="text-xl font-black tracking-tighter">
            <span className="text-white">TEN</span>
            <span className="text-yellow-500 underline decoration-yellow-500/50">PERCENT</span>
          </h1>
        </div>

        {/* Средна част - Линкове */}
        <div className="hidden lg:flex gap-1">
          {navLinks.map((link) => {
            const isActive = location.pathname === link.path || (link.path !== '/' && location.pathname.startsWith(link.path));
            return (
              <Link
                key={link.name}
                to={link.path}
                className={`flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-bold transition-all ${
                  isActive 
                    ? 'bg-yellow-500 text-black shadow-[0_0_10px_rgba(234,179,8,0.3)]' 
                    : 'text-gray-400 hover:text-white hover:bg-gray-800'
                }`}
              >
                {link.icon}
                {link.name}
                
                {/* БАДЖ ЗА НЕПРОЧЕТЕНИ СЪОБЩЕНИЯ ДО ТЕКСТА */}
                {link.name === 'Inbox' && unreadMessagesCount > 0 && (
                  <span className={`px-1.5 py-0.5 rounded-full text-[10px] ml-1 ${isActive ? 'bg-black text-yellow-500' : 'bg-red-500 text-white'}`}>
                    {unreadMessagesCount}
                  </span>
                )}
              </Link>
            );
          })}
        </div>

        {/* Дясна част - Бюджет и Изход (Камбанката е премахната) */}
        <div className="flex items-center gap-4">
          
          {/* БЮДЖЕТ */}
          <div className="flex items-center gap-2 bg-gray-900 border border-gray-800 px-3 py-1.5 rounded-lg shadow-inner">
            <span className="text-[10px] text-gray-500 font-bold uppercase tracking-wider">Budget</span>
            <span className="text-yellow-500 font-black font-mono text-sm">
              {budget !== null ? formatMoney(budget) : '...'}
            </span>
          </div>

          <div className="w-px h-6 bg-gray-800"></div>
          
          <button 
            onClick={handleLogout} 
            className="text-gray-500 hover:text-red-500 p-2 rounded-lg hover:bg-red-500/10 transition-colors"
            title="Logout"
          >
            <LogOut size={18} />
          </button>
        </div>

      </div>
    </nav>
  );
}