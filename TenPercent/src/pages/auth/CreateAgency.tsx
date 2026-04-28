import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Building, Shield, Crown, Star } from 'lucide-react';

export default function CreateAgency() {
  const navigate = useNavigate();
  const [selectedLogo, setSelectedLogo] = useState<number | null>(null);

  const logos = [
    { id: 1, icon: <Shield size={40} />, color: 'text-blue-500' },
    { id: 2, icon: <Crown size={40} />, color: 'text-yellow-500' },
    { id: 3, icon: <Star size={40} />, color: 'text-emerald-500' },
    { id: 4, icon: <Building size={40} />, color: 'text-red-500' },
  ];

  const handleCreate = (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedLogo) return alert('Моля, избери лого!');
    // Записваме данните и влизаме в играта
    navigate('/');
  };

  return (
    <div className="min-h-screen bg-gray-950 flex items-center justify-center p-4">
      <div className="max-w-2xl w-full bg-gray-900 border border-gray-800 rounded-2xl p-8 shadow-2xl">
        <div className="text-center mb-8">
          <h2 className="text-3xl font-black text-white mb-2">Основи своята агенция</h2>
          <p className="text-gray-400">Първата стъпка към върха на спортния мениджмънт</p>
        </div>

        <form onSubmit={handleCreate} className="space-y-8">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label className="block text-sm font-bold text-yellow-500 mb-2 uppercase tracking-wide">Име на Агенцията</label>
              <input 
                type="text" 
                placeholder="напр. Elite Sports Group"
                className="w-full bg-gray-950 border border-gray-700 rounded-lg py-3 px-4 text-white focus:outline-none focus:border-yellow-500 text-lg" 
                required 
              />
            </div>
            <div>
              <label className="block text-sm font-bold text-yellow-500 mb-2 uppercase tracking-wide">Име на Агента (Ти)</label>
              <input 
                type="text" 
                placeholder="Твоето име"
                className="w-full bg-gray-950 border border-gray-700 rounded-lg py-3 px-4 text-white focus:outline-none focus:border-yellow-500 text-lg" 
                required 
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-bold text-yellow-500 mb-4 uppercase tracking-wide text-center">Избери емблема</label>
            <div className="grid grid-cols-4 gap-4">
              {logos.map((logo) => (
                <div 
                  key={logo.id}
                  onClick={() => setSelectedLogo(logo.id)}
                  className={`aspect-square rounded-xl border-2 flex items-center justify-center cursor-pointer transition-all ${
                    selectedLogo === logo.id 
                      ? 'border-yellow-500 bg-gray-800 shadow-[0_0_20px_rgba(234,179,8,0.2)]' 
                      : 'border-gray-800 bg-gray-950 hover:border-gray-600'
                  }`}
                >
                  <div className={logo.color}>{logo.icon}</div>
                </div>
              ))}
            </div>
          </div>

          <div className="pt-4">
            <button 
              type="submit" 
              className="w-full bg-yellow-500 text-black font-black text-lg py-4 rounded-xl hover:bg-yellow-400 transition-colors shadow-[0_0_20px_rgba(234,179,8,0.4)]"
            >
              СТАРТИРАЙ КАРИЕРАТА
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}