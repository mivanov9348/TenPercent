import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Search, UserPlus, Loader2, ChevronLeft, ChevronRight, Users, UserMinus, SlidersHorizontal, ArrowUpDown } from 'lucide-react';

export default function ScoutingPool() {
  const navigate = useNavigate();

  const [players, setPlayers] = useState<any[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [pagination, setPagination] = useState({ totalCount: 0, totalPages: 1, page: 1 });
  const [showAdvanced, setShowAdvanced] = useState(false);

  // Филтри
  const [search, setSearch] = useState('');
  const [pos, setPos] = useState('All');
  const [nationality, setNationality] = useState('');
  const [minAge, setMinAge] = useState(15);
  const [maxAge, setMaxAge] = useState(40);
  const [sortBy, setSortBy] = useState('Value');
  const [hasAgency, setHasAgency] = useState<string>('false');

  const fetchPool = async (targetPage: number = 1) => {
    setIsLoading(true);
    try {
      const agencyParam = hasAgency === 'all' ? '' : `&hasAgency=${hasAgency}`;
      const natParam = nationality ? `&nationality=${nationality}` : '';

      const url = `https://localhost:7135/api/players/get-pool?search=${search}&position=${pos}&minAge=${minAge}&maxAge=${maxAge}&sortBy=${sortBy}${agencyParam}${natParam}&page=${targetPage}&pageSize=20`;

      const response = await fetch(url);
      if (response.ok) {
        const data = await response.json();
        setPlayers(data.players);
        setPagination({
          totalCount: data.totalCount,
          totalPages: data.totalPages,
          page: data.page
        });
      }
    } catch (error) {
      console.error("Failed to fetch pool:", error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    const delayDebounceFn = setTimeout(() => {
      fetchPool(1);
    }, 400);
    return () => clearTimeout(delayDebounceFn);
  }, [search, pos, nationality, minAge, maxAge, sortBy, hasAgency]);

  const handlePageChange = (newPage: number) => {
    if (newPage >= 1 && newPage <= pagination.totalPages) {
      fetchPool(newPage);
      window.scrollTo({ top: 0, behavior: 'smooth' });
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

  // Логика за сортиране при клик на колона
  const handleSort = (column: string) => {
    if (sortBy === column) {
       setSortBy(`${column}Desc`); // Ако вече е възходящо, сменяме на низходящо
    } else {
       setSortBy(column);
    }
  };

  return (
    <div className="space-y-6 pb-12">
      <div className="flex justify-between items-end">
        <div>
          <h1 className="text-3xl font-black text-white uppercase tracking-wider">Scouting Pool</h1>
          <p className="text-gray-400 mt-1">Discover players and expand your influence. Total found: <span className="text-yellow-500 font-bold">{pagination.totalCount}</span></p>
        </div>
      </div>

      {/* Филтри */}
      <div className="bg-gray-900 border border-gray-800 rounded-2xl p-5 shadow-xl transition-all">
        {/* Основни Филтри */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-600" size={16} />
            <input
              type="text"
              placeholder="Search by name..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="w-full bg-gray-950 border border-gray-800 rounded-xl py-2.5 pl-9 pr-4 text-white focus:border-yellow-500 outline-none text-sm"
            />
          </div>

          <select value={pos} onChange={(e) => setPos(e.target.value)} className="w-full bg-gray-950 border border-gray-800 text-white px-3 py-2.5 rounded-xl focus:border-yellow-500 outline-none text-sm cursor-pointer">
            <option value="All">All Positions</option>
            <option value="ST">Strikers (ST)</option>
            <option value="MID">Midfielders (MID)</option>
            <option value="DEF">Defenders (DEF)</option>
            <option value="GK">Goalkeepers (GK)</option>
          </select>

           <select 
            value={hasAgency}
            onChange={(e) => setHasAgency(e.target.value)}
            className="w-full bg-gray-950 border border-gray-800 text-white px-3 py-2.5 rounded-xl focus:border-yellow-500 outline-none cursor-pointer text-sm"
          >
            <option value="all">Agent Status: Everyone</option>
            <option value="false">Agent Status: Unrepresented</option>
            <option value="true">Agent Status: Has Agent</option>
          </select>

          <button
            onClick={() => setShowAdvanced(!showAdvanced)}
            className={`w-full py-2.5 rounded-xl text-sm font-bold flex items-center justify-center gap-2 transition-colors border ${showAdvanced ? 'bg-yellow-500/10 border-yellow-500/50 text-yellow-500' : 'bg-gray-800 border-gray-700 text-gray-400 hover:text-white'}`}
          >
            <SlidersHorizontal size={16} /> {showAdvanced ? 'Hide Advanced' : 'Advanced Filters'}
          </button>
        </div>

        {/* Напреднали Филтри */}
        {showAdvanced && (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 pt-4 border-t border-gray-800 animate-in fade-in slide-in-from-top-2">
            <div>
              <label className="text-[10px] text-gray-500 uppercase font-bold mb-1 block">Nationality</label>
              <input type="text" placeholder="e.g. Brazil" value={nationality} onChange={(e) => setNationality(e.target.value)} className="w-full bg-gray-950 border border-gray-800 rounded-xl py-2 px-3 text-white focus:border-yellow-500 outline-none text-sm" />
            </div>

            <div>
              <label className="text-[10px] text-gray-500 uppercase font-bold mb-1 block">Min. Age: {minAge}</label>
              <input type="range" min="15" max="40" value={minAge} onChange={(e) => setMinAge(Number(e.target.value))} className="w-full h-2 bg-gray-800 rounded-lg appearance-none cursor-pointer accent-yellow-500 mt-2" />
            </div>

            <div>
              <label className="text-[10px] text-gray-500 uppercase font-bold mb-1 block">Max. Age: {maxAge}</label>
              <input type="range" min="15" max="40" value={maxAge} onChange={(e) => setMaxAge(Number(e.target.value))} className="w-full h-2 bg-gray-800 rounded-lg appearance-none cursor-pointer accent-yellow-500 mt-2" />
            </div>
          </div>
        )}
      </div>

      {/* Таблица */}
      <div className="bg-gray-900 border border-gray-800 rounded-2xl overflow-hidden shadow-2xl">
        {isLoading ? (
          <div className="p-20 flex justify-center"><Loader2 className="animate-spin text-yellow-500" size={40} /></div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left">
              <thead>
                <tr className="bg-gray-800/50 text-gray-500 text-xs uppercase tracking-widest border-b border-gray-800">
                  <th className="px-6 py-4">Player</th>
                  <th className="px-6 py-4">Pos</th>
                  <th 
                    className="px-6 py-4 text-center cursor-pointer hover:text-white flex items-center justify-center gap-1"
                    onClick={() => handleSort('Age')}
                  >
                    Age <ArrowUpDown size={12} />
                  </th>
                  <th className="px-6 py-4">Current Club</th>
                  <th className="px-6 py-4">Status</th>
                  <th 
                    className="px-6 py-4 text-right cursor-pointer hover:text-white flex items-center justify-end gap-1"
                    onClick={() => handleSort('Value')}
                  >
                    Est. Value <ArrowUpDown size={12} />
                  </th>
                  <th className="px-6 py-4"></th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-800/50">
                {players.map((p) => (
                  <tr
                    key={p.id}
                    className="hover:bg-gray-800/30 transition-colors group cursor-pointer"
                    onClick={() => navigate(`/world/player/${p.id}`)}
                  >
                    <td className="px-6 py-4">
                      <div className="font-bold text-white group-hover:text-yellow-500 transition-colors">{p.name}</div>
                      <div className="text-[10px] text-gray-500">{p.nationality}</div>
                    </td>
                    <td className={`px-6 py-4 font-bold text-xs ${getPosColor(p.position)}`}>{p.position}</td>
                    <td className="px-6 py-4 text-center text-gray-400">{p.age}</td>
                    <td className="px-6 py-4 text-gray-400 text-sm">{p.clubName}</td>
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-2 text-xs">
                        {p.hasAgency ?
                          <span className="text-purple-400 flex items-center gap-1"><Users size={12} /> Represented</span> :
                          <span className="text-gray-500 flex items-center gap-1"><UserMinus size={12} /> No Agent</span>
                        }
                      </div>
                    </td>
                    <td className="px-6 py-4 text-right font-mono text-white font-bold">${(p.marketValue / 1000000).toFixed(1)}M</td>
                    <td className="px-6 py-4 text-right">
                      {!p.hasAgency && (
                        <button
                          onClick={(e) => { e.stopPropagation(); }}
                          className="p-2 bg-yellow-500/10 text-yellow-500 hover:bg-yellow-500 hover:text-black rounded-lg transition-all"
                          title="Sign Player"
                        >
                          <UserPlus size={16} />
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Pagination Controls */}
      {!isLoading && pagination.totalPages > 1 && (
        <div className="flex justify-center items-center gap-4 mt-8">
          <button onClick={() => handlePageChange(pagination.page - 1)} disabled={pagination.page === 1} className="p-2 bg-gray-800 text-gray-400 rounded-lg hover:bg-gray-700 disabled:opacity-30 transition-colors">
            <ChevronLeft />
          </button>
          <div className="text-gray-400 text-sm font-bold">
            Page <span className="text-white">{pagination.page}</span> of {pagination.totalPages}
          </div>
          <button onClick={() => handlePageChange(pagination.page + 1)} disabled={pagination.page === pagination.totalPages} className="p-2 bg-gray-800 text-gray-400 rounded-lg hover:bg-gray-700 disabled:opacity-30 transition-colors">
            <ChevronRight />
          </button>
        </div>
      )}
    </div>
  );
}