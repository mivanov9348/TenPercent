import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Loader2, BookmarkMinus, Search, Calendar, UserPlus } from 'lucide-react';

interface ShortlistedPlayer {
  playerId: number;
  name: string;
  age: number;
  position: string;
  nationality: string;
  marketValue: number;
  clubName: string;
  addedAt: string;
}

export default function MyShortlist() {
  const navigate = useNavigate();
  const [players, setPlayers] = useState<ShortlistedPlayer[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    fetchShortlist();
  }, []);

  const fetchShortlist = async () => {
    const userId = localStorage.getItem('userId');
    if (!userId) return;

    try {
      const response = await fetch(`https://localhost:7135/api/players/shortlist/${userId}`);
      if (response.ok) {
        const data = await response.json();
        setPlayers(data);
      }
    } catch (error) {
      console.error("Failed to fetch shortlist:", error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleRemove = async (e: React.MouseEvent, playerId: number) => {
    e.stopPropagation();
    const userId = localStorage.getItem('userId');
    if (!userId) return;

    try {
      const response = await fetch(`https://localhost:7135/api/players/${playerId}/shortlist/${userId}`, {
        method: 'DELETE',
      });

      if (response.ok) {
        // Директно премахваме играча от стейта, за да не правим нова заявка към бекенда
        setPlayers(prev => prev.filter(p => p.playerId !== playerId));
      }
    } catch (error) {
      console.error("Failed to remove from shortlist:", error);
    }
  };

  const getPosColor = (position: string) => {
    switch (position) {
      case 'ST': return 'text-blue-400';
      case 'MID': return 'text-emerald-400';
      case 'DEF': return 'text-yellow-400';
      case 'GK': return 'text-purple-400';
      default: return 'text-gray-400';
    }
  };

  return (
    <div className="space-y-6 pb-12">
      <div>
        <h1 className="text-3xl font-black text-white uppercase tracking-wider">My Shortlist</h1>
        <p className="text-gray-400 mt-1">
          Keep track of your top targets. You have <span className="text-yellow-500 font-bold">{players.length}</span> players saved.
        </p>
      </div>

      <div className="bg-gray-900 border border-gray-800 rounded-2xl overflow-hidden shadow-2xl">
        {isLoading ? (
          <div className="p-20 flex justify-center"><Loader2 className="animate-spin text-yellow-500" size={40} /></div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left">
              <thead>
                <tr className="bg-gray-800/50 text-gray-500 text-[10px] uppercase tracking-widest border-b border-gray-800">
                  <th className="px-4 py-4">Player</th>
                  <th className="px-2 py-4">Pos</th>
                  <th className="px-2 py-4 text-center">Age</th>
                  <th className="px-4 py-4">Club</th>
                  <th className="px-4 py-4 text-right">Value</th>
                  <th className="px-4 py-4 text-center">Added On</th>
                  <th className="px-2 py-4"></th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-800/50">
                {players.length > 0 ? (
                  players.map((p) => (
                    <tr 
                      key={p.playerId} 
                      className="hover:bg-gray-800/50 transition-colors group cursor-pointer" 
                      onClick={() => navigate(`/world/player/${p.playerId}`)}
                    >
                      <td className="px-4 py-3">
                        <div className="font-bold text-white group-hover:text-yellow-500 transition-colors">{p.name}</div>
                        <div className="text-[10px] text-gray-500 mt-0.5">{p.nationality}</div>
                      </td>
                      <td className={`px-2 py-3 font-black text-xs ${getPosColor(p.position)}`}>{p.position}</td>
                      <td className="px-2 py-3 text-center text-gray-300">{p.age}</td>
                      <td className="px-4 py-3 text-gray-400 text-sm">
                        {p.clubName !== "Free Agent" ? p.clubName : <span className="text-gray-600 italic">Free Agent</span>}
                      </td>
                      <td className="px-4 py-3 text-right font-mono text-white font-bold">
                        ${p.marketValue ? (p.marketValue / 1000000).toFixed(1) : 0}M
                      </td>
                      <td className="px-4 py-3 text-center text-xs text-gray-500 flex justify-center items-center gap-1">
                        <Calendar size={12} />
                        {new Date(p.addedAt).toLocaleDateString()}
                      </td>
                      <td className="px-2 py-3 text-right">
                        <div className="flex items-center justify-end gap-2">
                          <button
                            onClick={(e) => { e.stopPropagation(); navigate(`/world/player/${p.playerId}`); }}
                            className="p-1.5 bg-yellow-500/10 text-yellow-500 hover:bg-yellow-500 hover:text-black rounded-lg transition-all"
                            title="View Player"
                          >
                            <UserPlus size={14} />
                          </button>
                          <button
                            onClick={(e) => handleRemove(e, p.playerId)}
                            className="p-1.5 bg-red-500/10 text-red-500 hover:bg-red-500 hover:text-white rounded-lg transition-all"
                            title="Remove from Shortlist"
                          >
                            <BookmarkMinus size={14} />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={7} className="px-6 py-16 text-center text-gray-500">
                      <div className="flex flex-col items-center justify-center gap-3">
                        <Search size={32} className="text-gray-700" />
                        <p>Your shortlist is empty. Go scout some talent!</p>
                        <button 
                          onClick={() => navigate('/world/scouting')}
                          className="mt-2 bg-gray-800 text-white px-4 py-2 rounded-lg text-sm hover:bg-gray-700"
                        >
                          Open Scouting Pool
                        </button>
                      </div>
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}