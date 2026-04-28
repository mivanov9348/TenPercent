import { Link, useNavigate } from 'react-router-dom';
import { Mail, Lock } from 'lucide-react';

export default function Login() {
  const navigate = useNavigate();

  const handleLogin = (e: React.FormEvent) => {
    e.preventDefault();
    // Тук по-късно ще има логика за бекенда. Засега директно влизаме в играта.
    navigate('/');
  };

  return (
    <div className="min-h-screen bg-gray-950 flex items-center justify-center p-4">
      <div className="max-w-md w-full bg-gray-900 border border-gray-800 rounded-2xl p-8 shadow-2xl">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-black tracking-tighter mb-2">
            <span className="text-white">TEN</span>
            <span className="text-yellow-500 underline decoration-yellow-500/50">PERCENT</span>
          </h1>
          <p className="text-gray-400">Влез във своя агентски профил</p>
        </div>

        <form onSubmit={handleLogin} className="space-y-6">
          <div>
            <label className="block text-sm font-medium text-gray-400 mb-2">Email</label>
            <div className="relative">
              <Mail className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500" size={20} />
              <input 
                type="email" 
                className="w-full bg-gray-950 border border-gray-800 rounded-lg py-3 pl-10 pr-4 text-white focus:outline-none focus:border-yellow-500 transition-colors"
                placeholder="agent@tenpercent.com"
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-400 mb-2">Парола</label>
            <div className="relative">
              <Lock className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500" size={20} />
              <input 
                type="password" 
                className="w-full bg-gray-950 border border-gray-800 rounded-lg py-3 pl-10 pr-4 text-white focus:outline-none focus:border-yellow-500 transition-colors"
                placeholder="••••••••"
              />
            </div>
          </div>

          <button 
            type="submit" 
            className="w-full bg-yellow-500 text-black font-bold py-3 rounded-lg hover:bg-yellow-400 transition-colors shadow-[0_0_15px_rgba(234,179,8,0.3)]"
          >
            ВХОД
          </button>
        </form>

        <p className="text-center text-gray-500 mt-6">
          Нямаш акаунт? <Link to="/register" className="text-yellow-500 hover:underline">Регистрирай се</Link>
        </p>
      </div>
    </div>
  );
}