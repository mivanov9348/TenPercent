import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, Shield, MapPin, Trophy, DollarSign, Loader2 } from 'lucide-react';

export default function ClubDetails() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [club, setClub] = useState<any>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchClub = async () => {
      try {
        const response = await fetch(`https://localhost:7135/api/clubs/${id}`);
        if (response.ok) {
          const data = await response.json();
          setClub(data);
        }
      } catch (error) {
        console.error("Failed to fetch club details:", error);
      } finally {
        setIsLoading(false);
      }
    };
    fetchClub();
  }, [id]);

  if (isLoading) {
    return <div className="flex justify-center items-center h-64"><Loader2 className="animate-spin text-yellow-500" size={40} /></div>;
  }

  if (!club) {
    return <div className="text-center text-red-500 mt-10">Club not found!</div>;
  }

  // Помощна функция за рендиране на таблица с играчи
  const renderSquadTable = (title: string, players: any[]) => (
    <div className="bg-gray-900 border border-gray-800 rounded-2xl p-6 mb-6 shadow-lg">
      <h3 className="text-xl font-bold text-white mb-4 flex items-center gap-2 border-b border-gray-800 pb-2">
        {title} <span className="text-sm font-normal text-gray-500">({players.length})</span>
      </h3>
      
      {players.length === 0 ? (
        <p className="text-red-400 text-sm">Няма играчи на тази позиция!</p>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full text-left">
            <thead>
              <tr className="text-gray-500 text-sm border-b border-gray-800">
                <th className="pb-2 font-medium">Name</th>
                <th className="pb-2 font-medium">Age</th>
                <th className="pb-2 font-medium text-center">OVR</th>
                <th className="pb-2 font-medium text-center">POT</th>
                <th className="pb-2 font-medium text-right">Value</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-800/50">
              {players.map((p: any) => (
                <tr key={p.id} className="hover:bg-gray-800/30 transition-colors group cursor-pointer">
                  <td className="py-3 text-white font-medium group-hover:text-yellow-400 transition-colors">
                    {p.name}
                  </td>
                  <td className="py-3 text-gray-400">{p.age}</td>
                  <td className="py-3 text-center">
                    <span className="bg-gray-800 text-white px-2 py-1 rounded font-bold">{p.overall}</span>
                  </td>
                  <td className="py-3 text-center text-green-400 font-bold">{p.potential}</td>
                  <td className="py-3 text-right text-gray-400 font-mono">
                    ${(p.marketValue / 1000000).toFixed(1)}M
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );

  return (
    <div className="space-y-6">
      {/* Back Button */}
      <button 
        onClick={() => navigate(-1)} 
        className="flex items-center gap-2 text-gray-400 hover:text-white transition-colors"
      >
        <ArrowLeft size={20} /> Back to World
      </button>

      {/* Club Header - Динамичен цвят според PrimaryColor */}
      <div 
        className="relative rounded-3xl p-8 overflow-hidden shadow-2xl border border-gray-800"
        style={{ backgroundColor: club.primaryColor || '#1f2937' }}
      >
        {/* Лек градиент за четимост */}
        <div className="absolute inset-0 bg-gradient-to-r from-black/80 to-transparent"></div>
        
        <div className="relative z-10 flex items-end gap-6">
          <div className="w-24 h-24 bg-white/10 rounded-2xl flex items-center justify-center border-2 border-white/20 backdrop-blur-sm">
            <Shield size={48} className="text-white drop-shadow-md" />
          </div>
          <div>
            <h1 className="text-4xl font-black text-white tracking-wider mb-2 drop-shadow-lg">{club.name}</h1>
            <div className="flex flex-wrap gap-4 text-sm font-medium text-gray-200">
              <span className="flex items-center gap-1"><Trophy size={16} /> {club.leagueName}</span>
              <span className="flex items-center gap-1"><MapPin size={16} /> {club.city}, {club.country}</span>
              <span className="flex items-center gap-1 bg-black/40 px-2 py-0.5 rounded text-yellow-400">
                Reputation: {club.reputation}
              </span>
            </div>
          </div>
        </div>
      </div>

      {/* Finances Overview */}
      <div className="grid grid-cols-2 gap-4">
        <div className="bg-gray-900 border border-gray-800 rounded-2xl p-5 flex items-center gap-4">
          <div className="p-3 bg-green-500/10 text-green-500 rounded-xl"><DollarSign size={24} /></div>
          <div>
            <p className="text-gray-500 text-sm font-bold uppercase">Transfer Budget</p>
            <p className="text-2xl font-mono text-white font-bold">${(club.transferBudget / 1000000).toFixed(1)}M</p>
          </div>
        </div>
        <div className="bg-gray-900 border border-gray-800 rounded-2xl p-5 flex items-center gap-4">
          <div className="p-3 bg-blue-500/10 text-blue-500 rounded-xl"><DollarSign size={24} /></div>
          <div>
            <p className="text-gray-500 text-sm font-bold uppercase">Wage Budget</p>
            <p className="text-2xl font-mono text-white font-bold">${(club.wageBudget / 1000000).toFixed(2)}M / week</p>
          </div>
        </div>
      </div>

      {/* Squad Tables */}
      <div className="mt-8">
        <h2 className="text-2xl font-black text-white mb-6 uppercase tracking-wider">First Team Squad</h2>
        {renderSquadTable("Goalkeepers", club.squad.goalkeepers)}
        {renderSquadTable("Defenders", club.squad.defenders)}
        {renderSquadTable("Midfielders", club.squad.midfielders)}
        {renderSquadTable("Strikers", club.squad.strikers)}
      </div>

    </div>
  );
}