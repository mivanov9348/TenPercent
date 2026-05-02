import { useState, useEffect } from 'react';
import { Users, DollarSign, TrendingUp, AlertCircle, MoreHorizontal, Loader2, UserPlus } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

export default function Players() {
  const navigate = useNavigate();
  const [myPlayers, setMyPlayers] = useState<any[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchPlayers = async () => {
      try {
        // Взимаме ID-то на потребителя от localStorage (адаптирай според твоя auth flow)
        const userId = localStorage.getItem('userId') || 1; 
        
        const response = await fetch(`https://localhost:7135/api/agency/${userId}/players`);
        if (response.ok) {
          const data = await response.json();
          setMyPlayers(data);
        }
      } catch (error) {
        console.error("Failed to fetch players:", error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchPlayers();
  }, []);

  // Бързи сметки за статистиката отгоре
  const totalWage = myPlayers.reduce((acc, p) => acc + p.wage, 0);
  const avgSkill = myPlayers.length > 0 
    ? Math.round(myPlayers.reduce((acc, p) => acc + p.skill, 0) / myPlayers.length) 
    : 0;

  // Помощни функции за форматиране на пари
  const formatValue = (value: number) => {
    if (value >= 1000000) return `$${(value / 1000000).toFixed(1)}M`;
    if (value >= 1000) return `$${(value / 1000).toFixed(0)}K`;
    return `$${value}`;
  };

  const formatWage = (wage: number) => {
    if (wage >= 1000) return `$${(wage / 1000).toFixed(0)}K`;
    return `$${wage}`;
  };

  if (isLoading) {
    return <div className="flex justify-center items-center h-64"><Loader2 className="animate-spin text-blue-500" size={32} /></div>;
  }

  return (
    <div className="space-y-6">
      
      {/* 1. Хедър и бързи статистики */}
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-3xl font-black text-white uppercase tracking-wider">My Roster</h1>
          <p className="text-gray-400 mt-1">Manage your clients and their contracts</p>
        </div>
        
        <div className="flex gap-4 flex-wrap">
          <div className="bg-gray-800 border border-gray-700 px-4 py-2 rounded-lg flex items-center gap-3">
            <Users className="text-blue-500" size={20} />
            <div>
              <p className="text-xs text-gray-400">Total Players</p>
              <p className="text-lg font-bold text-white">{myPlayers.length}</p>
            </div>
          </div>
          <div className="bg-gray-800 border border-gray-700 px-4 py-2 rounded-lg flex items-center gap-3">
            <DollarSign className="text-red-400" size={20} />
            <div>
              <p className="text-xs text-gray-400">Weekly Wage Bill</p>
              <p className="text-lg font-bold text-white">{formatWage(totalWage)}</p>
            </div>
          </div>
          <div className="bg-gray-800 border border-gray-700 px-4 py-2 rounded-lg flex items-center gap-3">
            <TrendingUp className="text-emerald-500" size={20} />
            <div>
              <p className="text-xs text-gray-400">Average Skill</p>
              <p className="text-lg font-bold text-white">{avgSkill} OVR</p>
            </div>
          </div>
        </div>
      </div>

      {/* 2. Таблица с играчите */}
      <div className="bg-gray-800 border border-gray-700 rounded-xl overflow-hidden shadow-lg min-h-[400px] flex flex-col">
        {myPlayers.length > 0 ? (
          <div className="overflow-x-auto flex-1">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="bg-gray-900 border-b border-gray-700 text-xs uppercase tracking-wider text-gray-400">
                  <th className="p-4 font-medium">Player Name</th>
                  <th className="p-4 font-medium">Pos</th>
                  <th className="p-4 font-medium text-center">Age</th>
                  <th className="p-4 font-medium text-center">OVR</th>
                  <th className="p-4 font-medium text-center">POT</th>
                  <th className="p-4 font-medium">Market Value</th>
                  <th className="p-4 font-medium">Wage</th>
                  <th className="p-4 font-medium text-center">Contract</th>
                  <th className="p-4 font-medium text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-700">
                {myPlayers.map((player) => (
                  <tr key={player.id} className="hover:bg-gray-750 transition-colors group cursor-pointer" onClick={() => navigate(`/world/player/${player.id}`)}>
                    {/* Име и Форма */}
                    <td className="p-4">
                      <div className="flex items-center gap-3">
                        <div className="w-8 h-8 rounded-full bg-gray-700 flex items-center justify-center text-xs font-bold text-gray-300 shrink-0">
                          {player.name.charAt(0)}
                        </div>
                        <div className="min-w-0">
                          <p className="font-bold text-white group-hover:text-blue-400 transition-colors truncate">{player.name}</p>
                          <p className="text-xs text-gray-500">Form: {player.form}</p>
                        </div>
                      </div>
                    </td>
                    
                    {/* Позиция */}
                    <td className="p-4">
                      <span className={`text-xs font-bold px-2 py-1 rounded ${
                        player.pos === 'ST' ? 'bg-blue-500/10 text-blue-400' :
                        player.pos === 'MID' ? 'bg-emerald-500/10 text-emerald-400' :
                        player.pos === 'DEF' ? 'bg-yellow-500/10 text-yellow-400' :
                        'bg-purple-500/10 text-purple-400'
                      }`}>
                        {player.pos}
                      </span>
                    </td>
                    
                    {/* Години */}
                    <td className="p-4 text-center text-gray-300">{player.age}</td>
                    
                    {/* Умение (Skill) */}
                    <td className="p-4 text-center">
                      <span className="font-bold text-white">{player.skill}</span>
                    </td>
                    
                    {/* Потенциал */}
                    <td className="p-4 text-center">
                      <span className="font-bold text-emerald-400">{player.potential}</span>
                    </td>
                    
                    {/* Стойност */}
                    <td className="p-4 font-mono text-gray-300">{formatValue(player.value)}</td>
                    
                    {/* Заплата */}
                    <td className="p-4 font-mono text-gray-300">{formatWage(player.wage)}</td>
                    
                    {/* Договор с проверка за изтичащ такъв */}
                    <td className="p-4 text-center">
                      {player.contract === 0 ? (
                        <span className="inline-flex items-center gap-1 text-xs font-bold bg-red-500/10 text-red-500 px-2 py-1 rounded">
                          <AlertCircle size={12} /> EXPIRING
                        </span>
                      ) : player.contract === 1 ? (
                        <span className="text-yellow-500 font-bold">{player.contract} yr</span>
                      ) : (
                        <span className="text-gray-400">{player.contract} yrs</span>
                      )}
                    </td>
                    
                    {/* Действия (Actions) */}
                    <td className="p-4 text-right">
                      <button 
                        className="text-gray-400 hover:text-white p-2 rounded hover:bg-gray-700 transition-colors"
                        onClick={(e) => {
                          e.stopPropagation(); // Спира навигацията, ако се цъкне на менюто
                          // Тук в бъдеще ще се отваря dropdown за Offer Contract, Sell, Release и т.н.
                        }}
                      >
                        <MoreHorizontal size={20} />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="flex flex-col items-center justify-center flex-1 py-16 text-center">
             <div className="w-20 h-20 bg-gray-900 rounded-full flex items-center justify-center border border-gray-700 mb-4">
                <UserPlus size={32} className="text-gray-500" />
             </div>
             <h3 className="text-xl font-bold text-white mb-2">No Clients Yet</h3>
             <p className="text-gray-400 max-w-sm mb-6">Your agency doesn't have any players under contract. Head over to the Scouting pool to sign your first prospect.</p>
             <button 
               onClick={() => navigate('/world/scouting')}
               className="bg-blue-600 hover:bg-blue-500 text-white font-bold py-2 px-6 rounded-lg transition-colors shadow-lg shadow-blue-600/20"
             >
               Find Players
             </button>
          </div>
        )}
      </div>
    </div>
  );
}