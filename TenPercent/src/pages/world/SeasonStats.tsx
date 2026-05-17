import { useState, useEffect } from 'react';
import { Star, Flame, ShieldAlert, Target, Loader2 } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

export default function SeasonStats() {
  const navigate = useNavigate();
  const [stats, setStats] = useState<any>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchStats = async () => {
      try {
        const response = await fetch('https://localhost:7135/api/stats/season');
        if (response.ok) {
          const data = await response.json();
          setStats(data);
        }
      } catch (error) {
        console.error("Failed to fetch season stats:", error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchStats();
  }, []);

  if (isLoading) {
    return <div className="flex justify-center items-center h-64"><Loader2 className="animate-spin text-yellow-500" size={40} /></div>;
  }

  // СИГУРНА ПРОВЕРКА: Използваме ?. за да предотвратим крашване, ако масивите липсват
  const isDataEmpty = !stats || (
    (stats.topScorers?.length || 0) === 0 && 
    (stats.topRatings?.length || 0) === 0 &&
    (stats.topAssists?.length || 0) === 0 &&
    (stats.mostCards?.length || 0) === 0
  );

  if (isDataEmpty) {
    return <div className="text-center text-gray-500 mt-10 font-bold bg-gray-900 p-8 rounded-xl border border-gray-800 shadow-lg max-w-2xl mx-auto">No statistics available yet. Simulate some matches to see the leaderboards!</div>;
  }

  return (
    <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
      
      {/* 1. Golden Boot (Goals) */}
      <div className="bg-gray-800 border border-gray-700 rounded-xl p-6 shadow-lg flex flex-col h-[500px]">
        <h2 className="text-xl font-black text-white mb-6 flex items-center gap-3 shrink-0">
          <Target className="text-emerald-400" /> Golden Boot Race
        </h2>
        <div className="space-y-3 overflow-y-auto pr-2 custom-scrollbar">
          {stats.topScorers?.map((player: any, idx: number) => (
            <div key={player.id} onClick={() => navigate(`/world/player/${player.id}`)} className="bg-gray-900 border border-gray-700 p-3 rounded-lg flex items-center justify-between hover:border-gray-500 transition-colors cursor-pointer group">
              <div className="flex items-center gap-4">
                <span className={`font-black w-6 text-center text-lg ${idx === 0 ? 'text-yellow-500' : idx === 1 ? 'text-gray-300' : idx === 2 ? 'text-amber-600' : 'text-gray-600'}`}>{idx + 1}</span>
                <div>
                  <p className="font-bold text-white group-hover:text-emerald-400 transition-colors">{player.name}</p>
                  <p className="text-[10px] text-gray-400">{player.club}</p>
                </div>
              </div>
              <div className="flex items-center gap-4 text-center shrink-0">
                <div className="hidden sm:block">
                  <p className="text-[10px] text-gray-500 uppercase">Matches</p>
                  <p className="text-sm font-bold text-gray-400">{player.matches}</p>
                </div>
                <div className="bg-emerald-500/10 px-3 py-1 rounded">
                  <p className="text-[10px] text-emerald-500/70 uppercase font-bold">Goals</p>
                  <p className="font-black text-emerald-400 text-lg">{player.value}</p>
                </div>
              </div>
            </div>
          ))}
          {(stats.topScorers?.length || 0) === 0 && <p className="text-center text-gray-500 py-4">No goals scored yet.</p>}
        </div>
      </div>

      {/* 2. MVP Race (Avg Rating) */}
      <div className="bg-gray-800 border border-gray-700 rounded-xl p-6 shadow-lg flex flex-col h-[500px]">
        <h2 className="text-xl font-black text-white mb-6 flex items-center gap-3 shrink-0">
          <Star className="text-yellow-500" /> MVP Race (Avg Rating)
        </h2>
        <div className="space-y-3 overflow-y-auto pr-2 custom-scrollbar">
          {stats.topRatings?.map((player: any, idx: number) => (
            <div key={player.id} onClick={() => navigate(`/world/player/${player.id}`)} className="bg-gray-900 border border-gray-700 p-3 rounded-lg flex items-center justify-between hover:border-gray-500 transition-colors cursor-pointer group">
              <div className="flex items-center gap-4">
                <span className={`font-black w-6 text-center text-lg ${idx === 0 ? 'text-yellow-500' : idx === 1 ? 'text-gray-300' : idx === 2 ? 'text-amber-600' : 'text-gray-600'}`}>{idx + 1}</span>
                <div>
                  <p className="font-bold text-white group-hover:text-yellow-400 transition-colors">{player.name}</p>
                  <p className="text-[10px] text-gray-400">{player.club}</p>
                </div>
              </div>
              <div className="flex items-center gap-4 text-center shrink-0">
                <div className="hidden sm:block">
                  <p className="text-[10px] text-gray-500 uppercase">Matches</p>
                  <p className="text-sm font-bold text-gray-400">{player.matches}</p>
                </div>
                <div className="bg-yellow-500/10 px-3 py-1 rounded w-16 text-center">
                  <p className="font-black text-yellow-500 text-lg">{player.value.toFixed(2)}</p>
                </div>
              </div>
            </div>
          ))}
          {(stats.topRatings?.length || 0) === 0 && <p className="text-center text-gray-500 py-4">No ratings available.</p>}
        </div>
      </div>

      {/* 3. Top Playmakers (Assists) */}
      <div className="bg-gray-800 border border-gray-700 rounded-xl p-6 shadow-lg flex flex-col h-[500px]">
        <h2 className="text-xl font-black text-white mb-6 flex items-center gap-3 shrink-0">
          <Flame className="text-orange-500" /> Top Playmakers
        </h2>
        <div className="space-y-3 overflow-y-auto pr-2 custom-scrollbar">
          {stats.topAssists?.map((player: any, idx: number) => (
            <div key={player.id} onClick={() => navigate(`/world/player/${player.id}`)} className="bg-gray-900 border border-gray-700 p-3 rounded-lg flex items-center justify-between hover:border-gray-500 transition-colors cursor-pointer group">
              <div className="flex items-center gap-4">
                <span className={`font-black w-6 text-center text-lg ${idx === 0 ? 'text-orange-500' : idx === 1 ? 'text-gray-300' : idx === 2 ? 'text-amber-600' : 'text-gray-600'}`}>{idx + 1}</span>
                <div>
                  <p className="font-bold text-white group-hover:text-orange-400 transition-colors">{player.name}</p>
                  <p className="text-[10px] text-gray-400">{player.club}</p>
                </div>
              </div>
              <div className="flex items-center gap-4 text-center shrink-0">
                <div className="hidden sm:block">
                  <p className="text-[10px] text-gray-500 uppercase">Matches</p>
                  <p className="text-sm font-bold text-gray-400">{player.matches}</p>
                </div>
                <div className="bg-orange-500/10 px-3 py-1 rounded">
                  <p className="text-[10px] text-orange-500/70 uppercase font-bold">Assists</p>
                  <p className="font-black text-orange-400 text-lg">{player.value}</p>
                </div>
              </div>
            </div>
          ))}
          {(stats.topAssists?.length || 0) === 0 && <p className="text-center text-gray-500 py-4">No assists recorded yet.</p>}
        </div>
      </div>

      {/* 4. Most Cards */}
      <div className="bg-gray-800 border border-gray-700 rounded-xl p-6 shadow-lg flex flex-col h-[500px]">
        <h2 className="text-xl font-black text-white mb-6 flex items-center gap-3 shrink-0">
          <ShieldAlert className="text-red-500" /> Disciplinary Report
        </h2>
        <div className="space-y-3 overflow-y-auto pr-2 custom-scrollbar">
          {stats.mostCards?.map((player: any, idx: number) => (
            <div key={player.id} onClick={() => navigate(`/world/player/${player.id}`)} className="bg-gray-900 border border-gray-700 p-3 rounded-lg flex items-center justify-between hover:border-gray-500 transition-colors cursor-pointer group">
              <div className="flex items-center gap-4">
                <span className={`font-black w-6 text-center text-lg ${idx === 0 ? 'text-red-500' : 'text-gray-500'}`}>{idx + 1}</span>
                <div>
                  <p className="font-bold text-white group-hover:text-red-400 transition-colors">{player.name}</p>
                  <p className="text-[10px] text-gray-400">{player.club}</p>
                </div>
              </div>
              <div className="flex items-center gap-2 shrink-0">
                {player.yellow > 0 && <span className="bg-yellow-500/20 border border-yellow-500/30 text-yellow-500 font-black px-2 py-1 rounded text-xs">{player.yellow} Y</span>}
                {player.red > 0 && <span className="bg-red-500/20 border border-red-500/30 text-red-500 font-black px-2 py-1 rounded text-xs">{player.red} R</span>}
              </div>
            </div>
          ))}
          {(stats.mostCards?.length || 0) === 0 && <p className="text-center text-gray-500 py-4">No cards issued yet.</p>}
        </div>
      </div>

    </div>
  );
}