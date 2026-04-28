import { Link, useNavigate } from 'react-router-dom';
import { Mail, Lock, User } from 'lucide-react';

export default function Register() {
  const navigate = useNavigate();

  const handleRegister = (e: React.FormEvent) => {
    e.preventDefault();
    // След успешна регистрация, пращаме потребителя да си създаде агенцията
    navigate('/create-agency');
  };

  return (
    <div className="min-h-screen bg-gray-950 flex items-center justify-center p-4">
      <div className="max-w-md w-full bg-gray-900 border border-gray-800 rounded-2xl p-8 shadow-2xl">
        <div className="text-center mb-8">
          <h2 className="text-2xl font-bold text-white mb-2">Нов Агент</h2>
          <p className="text-gray-400">Създай своя акаунт, за да започнеш</p>
        </div>

        <form onSubmit={handleRegister} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-400 mb-1">Потребителско име</label>
            <div className="relative">
              <User className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500" size={20} />
              <input type="text" className="w-full bg-gray-950 border border-gray-800 rounded-lg py-3 pl-10 pr-4 text-white focus:outline-none focus:border-yellow-500" required />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-400 mb-1">Email</label>
            <div className="relative">
              <Mail className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500" size={20} />
              <input type="email" className="w-full bg-gray-950 border border-gray-800 rounded-lg py-3 pl-10 pr-4 text-white focus:outline-none focus:border-yellow-500" required />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-400 mb-1">Парола</label>
            <div className="relative">
              <Lock className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500" size={20} />
              <input type="password" className="w-full bg-gray-950 border border-gray-800 rounded-lg py-3 pl-10 pr-4 text-white focus:outline-none focus:border-yellow-500" required />
            </div>
          </div>

          <button type="submit" className="w-full bg-white text-black font-bold py-3 rounded-lg hover:bg-gray-200 transition-colors mt-4">
            СЪЗДАЙ АКАУНТ
          </button>
        </form>

        <p className="text-center text-gray-500 mt-6">
          Вече имаш акаунт? <Link to="/login" className="text-yellow-500 hover:underline">Влез тук</Link>
        </p>
      </div>
    </div>
  );
}