import { useState, useEffect, useMemo } from 'react';
import { Users, DollarSign, TrendingUp, AlertCircle, FileSignature, UserMinus, Loader2, UserPlus, FileText, Search, FilterX, BarChart2, Activity } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { API_URL } from '../../config';

export default function Players() {
  const navigate = useNavigate();
  const [myPlayers, setMyPlayers] = useState<any[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  // --- ФИЛТРИ STATE ---
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedPos, setSelectedPos] = useState('All');
  const [minAge, setMinAge] = useState<number | ''>('');
  const [maxAge, setMaxAge] = useState<number | ''>('');

  useEffect(() => {
    const fetchPlayers = async () => {
      try {
        const userId = localStorage.getItem('userId');
        if (!userId) return;
        
        const response = await fetch(`${API_URL}/agency/${userId}/players`);
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

  // --- ЛОГИКА ЗА ФИЛТРИРАНЕ НА ЖИВО ---
  const filteredPlayers = useMemo(() => {
    return myPlayers.filter(p => {
      const matchSearch = p.name.toLowerCase().includes(searchQuery.toLowerCase()) || 
                          p.clubName.toLowerCase().includes(searchQuery.toLowerCase());
      const matchPos = selectedPos === 'All' || p.pos === selectedPos;
      const matchMinAge = minAge === '' || p.age >= Number(minAge);
      const matchMaxAge = maxAge === '' || p.age <= Number(maxAge);

      return matchSearch && matchPos && matchMinAge && matchMaxAge;
    });
  }, [myPlayers, searchQuery, selectedPos, minAge, maxAge]);

  const clearFilters = () => {
    setSearchQuery('');
    setSelectedPos('All');
    setMinAge('');
    setMaxAge('');
  };

  // --- СТАТИСТИКИ ---
  const totalWage = filteredPlayers.reduce((acc, p) => acc + p.wage, 0);
  const avgSkill = filteredPlayers.length > 0 
    ? Math.round(filteredPlayers.reduce((acc, p) => acc + p.skill, 0) / filteredPlayers.length) 
    : 0;

  const formatValue = (value: number) => {
    if (value >= 1000000) return `$${(value / 1000000).toFixed(1)}M`;
    if (value >= 1000) return `$${(value / 1000).toFixed(0)}K`;
    return `$${value}`;
  };

  const formatWage = (wage: number) => {
    if (wage >= 1000) return `$${(wage / 1000).toFixed(0)}K`;
    return `$${wage}`;
  };

  const getAttrColor = (val: number) => {
    if (val >= 80) return 'text-emerald-400 bg-emerald-400/10 border-emerald-400/30';
    if (val >= 60) return 'text-yellow-400 bg-yellow-400/10 border-yellow-400/30';
    return 'text-gray-400 bg-gray-800 border-gray-700';
  };

  if (isLoading) {
    return <div className="flex justify-center items-center h-64"><Loader2 className="animate-spin text-yellow-500" size={32} /></div>;
  }

  return (
    <div className="space-y-6">
      
      {/* 1. Хедър и бързи статистики */}
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-3xl font-black text-white uppercase tracking-wider">My Clients</h1>
          <p className="text-gray-400 mt-1">Manage your active roster and their contracts</p>
        </div>
        
        <div className="flex gap-4 flex-wrap">
          <div className="bg-gray-800 border border-gray-700 px-4 py-2 rounded-lg flex items-center gap-3 shadow-md">
            <Users className="text-blue-500" size={20} />
            <div>
              <p className="text-xs text-gray-400">Showing Clients</p>
              <p className="text-lg font-bold text-white">{filteredPlayers.length} <span className="text-xs text-gray-600 font-normal">/ {myPlayers.length}</span></p>
            </div>
          </div>
          <div className="bg-gray-800 border border-gray-700 px-4 py-2 rounded-lg flex items-center gap-3 shadow-md">
            <DollarSign className="text-emerald-500" size={20} />
            <div>
              <p className="text-xs text-gray-400">Weekly Wage Bill</p>
              <p className="text-lg font-bold text-white">{formatWage(totalWage)}</p>
            </div>
          </div>
          <div className="bg-gray-800 border border-gray-700 px-4 py-2 rounded-lg flex items-center gap-3 shadow-md">
            <TrendingUp className="text-purple-500" size={20} />
            <div>
              <p className="text-xs text-gray-400">Average OVR</p>
              <p className="text-lg font-bold text-white">{avgSkill}</p>
            </div>
          </div>
        </div>
      </div>

      {/* 2. Филтър Лента */}
      {myPlayers.length > 0 && (
        <div className="bg-gray-800 border border-gray-700 p-4 rounded-xl shadow-md flex flex-wrap gap-4 items-end">
          <div className="flex-1 min-w-[200px]">
            <label className="block text-xs font-bold text-gray-400 uppercase tracking-wider mb-1">Search Player or Club</label>
            <div className="relative">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500" size={16} />
              <input 
                type="text" 
                placeholder="e.g. Haaland or Real Madrid" 
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="w-full bg-gray-900 border border-gray-700 rounded-lg py-2 pl-9 pr-4 text-white text-sm focus:border-yellow-500 focus:outline-none transition-colors"
              />
            </div>
          </div>

          <div className="w-[120px]">
            <label className="block text-xs font-bold text-gray-400 uppercase tracking-wider mb-1">Position</label>
            <select 
              value={selectedPos} 
              onChange={(e) => setSelectedPos(e.target.value)}
              className="w-full bg-gray-900 border border-gray-700 rounded-lg py-2 px-3 text-white text-sm focus:border-yellow-500 focus:outline-none transition-colors"
            >
              <option value="All">All</option>
              <option value="GK">GK</option>
              <option value="DEF">DEF</option>
              <option value="MID">MID</option>
              <option value="ST">ST</option>
            </select>
          </div>

          <div className="w-[100px]">
            <label className="block text-xs font-bold text-gray-400 uppercase tracking-wider mb-1">Min Age</label>
            <input 
              type="number" 
              placeholder="e.g. 16"
              value={minAge}
              onChange={(e) => setMinAge(e.target.value === '' ? '' : Number(e.target.value))}
              className="w-full bg-gray-900 border border-gray-700 rounded-lg py-2 px-3 text-white text-sm focus:border-yellow-500 focus:outline-none transition-colors"
            />
          </div>

          <div className="w-[100px]">
            <label className="block text-xs font-bold text-gray-400 uppercase tracking-wider mb-1">Max Age</label>
            <input 
              type="number" 
              placeholder="e.g. 35"
              value={maxAge}
              onChange={(e) => setMaxAge(e.target.value === '' ? '' : Number(e.target.value))}
              className="w-full bg-gray-900 border border-gray-700 rounded-lg py-2 px-3 text-white text-sm focus:border-yellow-500 focus:outline-none transition-colors"
            />
          </div>

          <button 
            onClick={clearFilters}
            className="bg-gray-700 hover:bg-red-500 hover:text-white text-gray-300 font-bold py-2 px-4 rounded-lg transition-colors flex items-center gap-2 text-sm"
          >
            <FilterX size={16} />
            Reset
          </button>
        </div>
      )}

      {/* 3. Таблица с клиентите */}
      <div className="bg-gray-800 border border-gray-700 rounded-xl overflow-hidden shadow-lg min-h-[400px] flex flex-col">
        {filteredPlayers.length > 0 ? (
          <div className="overflow-x-auto flex-1 custom-scrollbar">
            {/* Леко увеличих min-width на таблицата, за да съберем новата колона комфортно */}
            <table className="w-full text-left border-collapse min-w-[1300px]">
              <thead>
                <tr className="bg-gray-900 border-b border-gray-700 text-[10px] uppercase tracking-wider text-gray-400">
                  <th className="p-4 font-bold">Client Info</th>
                  <th className="p-4 font-bold text-center">Attributes</th>
                  {/* НОВА КОЛОНА: SEASON STATS */}
                  <th className="p-4 font-bold text-center">Season Stats</th>
                  <th className="p-4 font-bold text-right">Value & Wage</th>
                  <th className="p-4 font-bold text-center">Club Contract</th>
                  <th className="p-4 font-bold text-center">Agency Contract</th>
                  <th className="p-4 font-bold text-center">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-800">
                {filteredPlayers.map((player) => (
                  <tr key={player.id} className="hover:bg-gray-750 transition-colors group">
                    
                    {/* Име, Позиция и Клуб */}
                    <td className="p-4 cursor-pointer" onClick={() => navigate(`/world/player/${player.id}`)}>
                      <div className="flex items-center gap-3">
                        <div className="relative">
                          <div className="w-10 h-10 rounded-lg bg-gray-900 border border-gray-700 flex items-center justify-center text-sm font-black text-gray-300">
                            {player.skill}
                          </div>
                          <div className={`absolute -bottom-2 -right-2 text-[9px] font-black px-1.5 py-0.5 rounded ${
                            player.pos === 'ST' ? 'bg-blue-500 text-white' :
                            player.pos === 'MID' ? 'bg-emerald-500 text-white' :
                            player.pos === 'DEF' ? 'bg-yellow-500 text-black' :
                            'bg-purple-500 text-white'
                          }`}>
                            {player.pos}
                          </div>
                        </div>
                        <div className="min-w-0">
                          <p className="font-bold text-white group-hover:text-yellow-500 transition-colors truncate">{player.name}</p>
                          <p className="text-[11px] text-gray-400 truncate">{player.age} yrs • {player.nationality} • <span className="text-gray-300">{player.clubName}</span></p>
                        </div>
                      </div>
                    </td>
                    
                    {/* Атрибути (Компактни) */}
                    <td className="p-4 text-center">
                      <div className="grid grid-cols-5 gap-1 max-w-[250px] mx-auto text-[10px] font-bold">
                        <div className={`border rounded px-1 py-0.5 flex flex-col items-center ${getAttrColor(player.pace)}`}><span>PAC</span><span>{player.pace}</span></div>
                        <div className={`border rounded px-1 py-0.5 flex flex-col items-center ${getAttrColor(player.shooting)}`}><span>SHO</span><span>{player.shooting}</span></div>
                        <div className={`border rounded px-1 py-0.5 flex flex-col items-center ${getAttrColor(player.passing)}`}><span>PAS</span><span>{player.passing}</span></div>
                        <div className={`border rounded px-1 py-0.5 flex flex-col items-center ${getAttrColor(player.dribbling)}`}><span>DRI</span><span>{player.dribbling}</span></div>
                        <div className={`border rounded px-1 py-0.5 flex flex-col items-center ${getAttrColor(player.physical)}`}><span>PHY</span><span>{player.physical}</span></div>
                      </div>
                    </td>

                    {/* НОВО: СТАТИСТИКИ (Season Stats) */}
                    <td className="p-4 text-center">
                      <div className="flex justify-center items-center gap-4 text-xs">
                        <div className="flex flex-col items-center">
                          <span className="text-[10px] text-gray-500 font-bold uppercase">Apps</span>
                          <span className="font-bold text-gray-300">{player.apps ?? 0}</span>
                        </div>
                        <div className="flex flex-col items-center">
                          <span className="text-[10px] text-gray-500 font-bold uppercase">Gls</span>
                          <span className="font-bold text-white">{player.goals ?? 0}</span>
                        </div>
                        <div className="flex flex-col items-center">
                          <span className="text-[10px] text-gray-500 font-bold uppercase">Ast</span>
                          <span className="font-bold text-white">{player.assists ?? 0}</span>
                        </div>
                        <div className="flex flex-col items-center">
                          <span className="text-[10px] text-gray-500 font-bold uppercase">Avg</span>
                          <span className={`font-bold ${
                            player.avgRating >= 7.5 ? 'text-emerald-400' :
                            player.avgRating >= 6.5 ? 'text-yellow-400' :
                            'text-gray-300'
                          }`}>
                            {player.avgRating ? player.avgRating.toFixed(1) : '0.0'}
                          </span>
                        </div>
                      </div>
                    </td>
                    
                    {/* Стойност и Заплата */}
                    <td className="p-4 text-right">
                      <p className="font-black text-white">{formatValue(player.value)}</p>
                      <p className="text-xs text-emerald-400 font-mono">{formatWage(player.wage)} / wk</p>
                    </td>
                    
                    {/* Договор с Клуба */}
                    <td className="p-4 text-center">
                      {player.clubContractYearsLeft === 0 ? (
                        <span className="inline-flex items-center gap-1 text-[11px] font-bold bg-red-500/10 text-red-500 border border-red-500/20 px-2 py-1 rounded">
                          <AlertCircle size={12} /> EXPIRING
                        </span>
                      ) : (
                        <span className="text-sm font-bold text-gray-300">{player.clubContractYearsLeft} Seasons</span>
                      )}
                    </td>

                    {/* Договор с Агенцията */}
                    <td className="p-4 text-center">
                      {player.agencyContractYearsLeft === 0 ? (
                        <span className="inline-flex items-center gap-1 text-[11px] font-bold bg-red-500/10 text-red-500 border border-red-500/20 px-2 py-1 rounded">
                          <AlertCircle size={12} /> EXPIRING
                        </span>
                      ) : (
                        <span className="text-sm font-bold text-yellow-500">{player.agencyContractYearsLeft} Seasons</span>
                      )}
                    </td>
                    
                    {/* Actions */}
                    <td className="p-4">
                      <div className="flex items-center justify-center gap-2">
                        {/* Бутон: СТАТИСТИКА (Client Report) */}
                        <button 
                          className="bg-gray-900 border border-gray-700 hover:border-blue-500 text-gray-300 hover:text-blue-500 p-2 rounded transition-colors group/btn relative"
                          onClick={(e) => { e.stopPropagation(); navigate(`/world/player/${player.id}`); }}
                        >
                          <BarChart2 size={18} />
                          <span className="absolute -top-8 left-1/2 -translate-x-1/2 bg-gray-900 text-xs px-2 py-1 rounded opacity-0 group-hover/btn:opacity-100 transition-opacity pointer-events-none whitespace-nowrap z-10 shadow-lg">Client Report</span>
                        </button>

                        {/* Бутон: ПРЕПОДПИСВАНЕ */}
                        <button 
                          className="bg-gray-900 border border-gray-700 hover:border-yellow-500 text-gray-300 hover:text-yellow-500 p-2 rounded transition-colors group/btn relative"
                          onClick={(e) => { e.stopPropagation(); /* Логика за преподписване */ }}
                        >
                          <FileSignature size={18} />
                          <span className="absolute -top-8 left-1/2 -translate-x-1/2 bg-gray-900 text-xs px-2 py-1 rounded opacity-0 group-hover/btn:opacity-100 transition-opacity pointer-events-none whitespace-nowrap z-10 shadow-lg">Renew Contract</span>
                        </button>
                        
                        {/* Бутон: ОСВОБОЖДАВАНЕ */}
                        <button 
                          className="bg-gray-900 border border-gray-700 hover:border-red-500 text-gray-300 hover:text-red-500 p-2 rounded transition-colors group/btn relative"
                          onClick={(e) => { e.stopPropagation(); /* Логика за освобождаване */ }}
                        >
                          <UserMinus size={18} />
                          <span className="absolute -top-8 left-1/2 -translate-x-1/2 bg-gray-900 text-xs px-2 py-1 rounded opacity-0 group-hover/btn:opacity-100 transition-opacity pointer-events-none whitespace-nowrap z-10 shadow-lg">Release Client</span>
                        </button>
                      </div>
                    </td>

                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="flex flex-col items-center justify-center flex-1 py-16 text-center">
             <div className="w-20 h-20 bg-gray-900 rounded-full flex items-center justify-center border border-gray-700 mb-4">
                <FileText size={32} className="text-gray-500" />
             </div>
             {myPlayers.length > 0 ? (
               <>
                 <h3 className="text-xl font-bold text-white mb-2">No Clients Match Filters</h3>
                 <p className="text-gray-400 max-w-sm mb-6">Try adjusting your search or age filters.</p>
                 <button onClick={clearFilters} className="bg-gray-700 hover:bg-gray-600 text-white font-bold py-2 px-6 rounded-lg transition-colors">Clear Filters</button>
               </>
             ) : (
               <>
                 <h3 className="text-xl font-bold text-white mb-2">No Clients Yet</h3>
                 <p className="text-gray-400 max-w-sm mb-6">Your agency doesn't have any players under representation. Go scout and sign some rising stars!</p>
                 <button 
                   onClick={() => navigate('/world/scouting')}
                   className="bg-yellow-500 text-black font-black py-3 px-8 rounded-lg hover:bg-yellow-400 transition-colors shadow-[0_0_15px_rgba(234,179,8,0.3)] flex items-center gap-2"
                 >
                   <UserPlus size={20} />
                   SCOUT PLAYERS
                 </button>
               </>
             )}
          </div>
        )}
      </div>
    </div>
  );
}