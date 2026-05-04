import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Building, Shield, Crown, Star, Loader2, AlertCircle } from 'lucide-react';

export default function CreateAgency() {
  const navigate = useNavigate();

  const [agencyName, setAgencyName] = useState('');
  const [agentName, setAgentName] = useState('');
  const [selectedLogo, setSelectedLogo] = useState<number | null>(null);

  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const logos = [
    { id: 1, icon: <Shield size={40} />, color: 'text-blue-500' },
    { id: 2, icon: <Crown size={40} />, color: 'text-yellow-500' },
    { id: 3, icon: <Star size={40} />, color: 'text-emerald-500' },
    { id: 4, icon: <Building size={40} />, color: 'text-red-500' },
  ];

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedLogo) return setError('Моля, избери емблема!');

    setError('');
    setIsLoading(true);

    const userId = localStorage.getItem('userId');

    if (!userId) {
      setError('Критична грешка: Не сте влезли в профила си (липсва UserId).');
      return;
    }


    try {
      const response = await fetch('https://localhost:7135/api/auth/create-agency', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          userId: parseInt(userId || '0'),
          agentName,
          agencyName,
          logoId: selectedLogo
        }),
      });

      const data = await response.json();

      if (!response.ok) {
        // 1. Ако грешката идва от нашия Service (където връщаме { message: "..." })
        if (data.message) {
          throw new Error(data.message);
        }
        // 2. Ако грешката е автоматична от ASP.NET валидацията
        else if (data.errors) {
          // Събираме всички съобщения за грешки от ASP.NET в един текст
          const errorMessages = Object.values(data.errors).flat().join(' | ');
          throw new Error(`Грешка във формата: ${errorMessages}`);
        } 
        // 3. Fallback
        else {
          throw new Error(JSON.stringify(data));
        }
      }

      // Обновяваме флага, че вече имаме агенция!
      localStorage.setItem('hasAgency', 'true');

      // Влизаме в същинската игра
      navigate('/');
    } catch (err: any) {
      console.error("ДЕТАЙЛНА ГРЕШКА:", err);
      setError(err.message);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-950 flex items-center justify-center p-4">
      <div className="max-w-2xl w-full bg-gray-900 border border-gray-800 rounded-2xl p-8 shadow-2xl">
        <div className="text-center mb-8">
          <h2 className="text-3xl font-black text-white mb-2">Основи своята агенция</h2>
          <p className="text-gray-400">Първата стъпка към върха на спортния мениджмънт</p>
        </div>

        {error && (
          <div className="mb-6 p-3 bg-red-500/10 border border-red-500/50 rounded-lg flex items-center gap-3 text-red-500 text-sm">
            <AlertCircle size={18} />
            <p>{error}</p>
          </div>
        )}

        <form onSubmit={handleCreate} className="space-y-8">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label className="block text-sm font-bold text-yellow-500 mb-2 uppercase tracking-wide">Име на Агенцията</label>
              <input
                type="text"
                value={agencyName}
                onChange={e => setAgencyName(e.target.value)}
                placeholder="напр. Elite Sports Group"
                className="w-full bg-gray-950 border border-gray-700 rounded-lg py-3 px-4 text-white focus:outline-none focus:border-yellow-500 text-lg"
                required
              />
            </div>
            <div>
              <label className="block text-sm font-bold text-yellow-500 mb-2 uppercase tracking-wide">Име на Агента (Ти)</label>
              <input
                type="text"
                value={agentName}
                onChange={e => setAgentName(e.target.value)}
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
                  className={`aspect-square rounded-xl border-2 flex items-center justify-center cursor-pointer transition-all ${selectedLogo === logo.id
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
              disabled={isLoading}
              className="w-full bg-yellow-500 text-black font-black text-lg py-4 rounded-xl hover:bg-yellow-400 transition-colors shadow-[0_0_20px_rgba(234,179,8,0.4)] flex justify-center items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isLoading ? <Loader2 className="animate-spin" size={24} /> : 'СТАРТИРАЙ КАРИЕРАТА'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}