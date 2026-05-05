import { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { Search, UserPlus, Loader2, ChevronLeft, ChevronRight, Users, UserMinus, SlidersHorizontal, ArrowUpDown, Star, Flame, Briefcase, LayoutTemplate, BookmarkPlus } from 'lucide-react';
import OfferRepresentationModal from './world/OfferRepresentationModal';

export default function ScoutingPool() {
  const navigate = useNavigate();

  const [players, setPlayers] = useState<any[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [pagination, setPagination] = useState({ totalCount: 0, totalPages: 1, page: 1 });
  const [showAdvanced, setShowAdvanced] = useState(false);
  const [showColumnMenu, setShowColumnMenu] = useState(false);
  const columnMenuRef = useRef<HTMLDivElement>(null);

  const [activeQuickFilter, setActiveQuickFilter] = useState('all');

  const [search, setSearch] = useState('');
  const [pos, setPos] = useState('All');
  const [nationality, setNationality] = useState('');
  const [minAge, setMinAge] = useState(15);
  const [maxAge, setMaxAge] = useState(40);
  const [sortBy, setSortBy] = useState('Value');
  const [hasAgency, setHasAgency] = useState<string>('all');
  const [pitchPlayer, setPitchPlayer] = useState<any>(null);

  // НОВО: Добавени са полетата за статистика тук
  const [visibleColumns, setVisibleColumns] = useState({
    // Основни
    Age: true,
    Club: true,
    Status: true,
    Wage: false,
    Value: true,
    // Атрибути
    Pace: false,
    Shooting: false,
    Passing: false,
    Dribbling: false,
    Defending: false,
    Physical: false,
    // Статистика за сезона
    Apps: false,
    Goals: false,
    Assists: false,
    Rating: false,
  });

  const handlePitchSuccess = (message: string) => {
    setPitchPlayer(null);
    alert("🎉 " + message);
    fetchPool(pagination.page); // Презареждаме таблицата, за да му се смени статуса на Rep'd!
  };

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (columnMenuRef.current && !columnMenuRef.current.contains(event.target as Node)) {
        setShowColumnMenu(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [columnMenuRef]);

  const handleQuickFilter = (filterId: string) => {
    setActiveQuickFilter(filterId);
    setSearch('');
    setPos('All');
    setNationality('');

    switch (filterId) {
      case 'wonderkids':
        setMinAge(15); setMaxAge(21); setHasAgency('false'); setSortBy('Value'); break;
      case 'free_agents':
        setMinAge(15); setMaxAge(40); setHasAgency('false'); setSortBy('Value'); break;
      case 'prime':
        setMinAge(24); setMaxAge(29); setHasAgency('all'); setSortBy('Value'); break;
      case 'veterans':
        setMinAge(32); setMaxAge(40); setHasAgency('all'); setSortBy('Value'); break;
      default:
        setMinAge(15); setMaxAge(40); setHasAgency('all'); setSortBy('Value'); break;
    }
  };

  const fetchPool = async (targetPage: number = 1) => {
    setIsLoading(true);
    try {
      const agencyParam = hasAgency === 'all' ? '' : `&hasAgency=${hasAgency}`;
      const natParam = nationality ? `&nationality=${nationality}` : '';
      const url = `https://localhost:7135/api/players/get-pool?search=${search}&position=${pos}&minAge=${minAge}&maxAge=${maxAge}&sortBy=${sortBy}${agencyParam}${natParam}&page=${targetPage}&pageSize=20`;

      const response = await fetch(url);
      if (response.ok) {
        const data = await response.json();
        const playersList = data.items || data.players || [];
        setPlayers(playersList);
        setPagination({
          totalCount: data.totalCount || 0,
          totalPages: data.totalPages || 1,
          page: data.page || 1
        });
      }
    } catch (error) {
      console.error("Failed to fetch pool:", error);
      setPlayers([]);
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

  const handleSort = (column: string) => {
    if (sortBy === column) setSortBy(`${column}Desc`);
    else setSortBy(column);
  };

  const toggleColumn = (columnName: keyof typeof visibleColumns) => {
    setVisibleColumns(prev => ({
      ...prev,
      [columnName]: !prev[columnName]
    }));
  };

  const quickFilters = [
    { id: 'all', label: 'All Database', icon: <Users size={16} /> },
    { id: 'wonderkids', label: 'Wonderkids (U21)', icon: <Star size={16} /> },
    { id: 'prime', label: 'Prime Age (24-29)', icon: <Flame size={16} /> },
    { id: 'free_agents', label: 'Free Agents', icon: <Briefcase size={16} /> },
  ];

  const handleAddToShortlist = async (e: React.MouseEvent, playerId: number) => {
    e.stopPropagation(); // Спираме клика да не ни пренасочи към страницата на играча

    const userId = localStorage.getItem('userId');
    if (!userId) {
      alert("You must be logged in to use the shortlist.");
      return;
    }



    try {
      const response = await fetch(`https://localhost:7135/api/players/${playerId}/shortlist`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ userId })
      });

      const data = await response.json();

      if (response.ok) {
        // Може да смениш с красив toast notification по-късно
        alert("✅ " + data.message);
      } else {
        alert("❌ " + data.message);
      }
    } catch (error) {
      console.error("Failed to add to shortlist", error);
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

      {/* QUICK FILTERS */}
      <div className="flex gap-3 overflow-x-auto hide-scrollbar pb-2">
        {quickFilters.map(qf => (
          <button
            key={qf.id}
            onClick={() => handleQuickFilter(qf.id)}
            className={`flex items-center gap-2 px-4 py-2.5 rounded-xl font-bold text-sm whitespace-nowrap transition-all ${activeQuickFilter === qf.id
              ? 'bg-yellow-500 text-black shadow-[0_0_15px_rgba(234,179,8,0.3)] scale-105'
              : 'bg-gray-900 border border-gray-800 text-gray-400 hover:text-white hover:bg-gray-800'
              }`}
          >
            {qf.icon} {qf.label}
          </button>
        ))}
      </div>

      {/* MANUAL FILTERS & COLUMN CONTROLS */}
      <div className="bg-gray-900 border border-gray-800 rounded-2xl p-5 shadow-xl transition-all relative">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4 mb-4">
          <div className="relative lg:col-span-2">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-600" size={16} />
            <input
              type="text"
              placeholder="Search by name..."
              value={search}
              onChange={(e) => { setSearch(e.target.value); setActiveQuickFilter('custom'); }}
              className="w-full bg-gray-950 border border-gray-800 rounded-xl py-2.5 pl-9 pr-4 text-white focus:border-yellow-500 outline-none text-sm"
            />
          </div>

          <select
            value={pos}
            onChange={(e) => { setPos(e.target.value); setActiveQuickFilter('custom'); }}
            className="w-full bg-gray-950 border border-gray-800 text-white px-3 py-2.5 rounded-xl focus:border-yellow-500 outline-none text-sm cursor-pointer"
          >
            <option value="All">All Positions</option>
            <option value="ST">Strikers (ST)</option>
            <option value="MID">Midfielders (MID)</option>
            <option value="DEF">Defenders (DEF)</option>
            <option value="GK">Goalkeepers (GK)</option>
          </select>

          <select
            value={hasAgency}
            onChange={(e) => { setHasAgency(e.target.value); setActiveQuickFilter('custom'); }}
            className="w-full bg-gray-950 border border-gray-800 text-white px-3 py-2.5 rounded-xl focus:border-yellow-500 outline-none cursor-pointer text-sm"
          >
            <option value="all">Agent Status: Everyone</option>
            <option value="false">Agent Status: Unrepresented</option>
            <option value="true">Agent Status: Has Agent</option>
          </select>

          <div className="flex gap-2">
            <button
              onClick={() => setShowAdvanced(!showAdvanced)}
              className={`flex-1 py-2.5 rounded-xl text-sm font-bold flex items-center justify-center gap-2 transition-colors border ${showAdvanced ? 'bg-yellow-500/10 border-yellow-500/50 text-yellow-500' : 'bg-gray-800 border-gray-700 text-gray-400 hover:text-white'}`}
            >
              <SlidersHorizontal size={16} /> Filters
            </button>

            {/* COLUMN CHOOSER TOGGLE */}
            <div className="relative" ref={columnMenuRef}>
              <button
                onClick={() => setShowColumnMenu(!showColumnMenu)}
                className="h-full px-3 bg-gray-800 border border-gray-700 text-gray-400 hover:text-white hover:border-gray-500 rounded-xl transition-colors flex items-center justify-center"
                title="Customize Table Columns"
              >
                <LayoutTemplate size={18} />
              </button>

              {/* COLUMN DROPDOWN MENU */}
              {showColumnMenu && (
                <div className="absolute right-0 mt-2 w-56 bg-gray-900 border border-gray-700 rounded-xl shadow-2xl z-50 overflow-hidden animate-in fade-in slide-in-from-top-2">
                  <div className="p-3 border-b border-gray-800 bg-gray-800/50">
                    <span className="text-xs font-bold text-gray-400 uppercase tracking-wider">Visible Columns</span>
                  </div>
                  <div className="p-2 max-h-72 overflow-y-auto custom-scrollbar">

                    {/* Група: Основни */}
                    <div className="text-[10px] text-gray-500 uppercase font-bold mt-2 mb-1 px-2">Core Info</div>
                    {['Age', 'Club', 'Status', 'Wage', 'Value'].map(key => (
                      <label key={key} className="flex items-center gap-3 p-2 hover:bg-gray-800 rounded-lg cursor-pointer transition-colors">
                        <input type="checkbox" checked={visibleColumns[key as keyof typeof visibleColumns]} onChange={() => toggleColumn(key as keyof typeof visibleColumns)} className="accent-yellow-500 w-4 h-4 cursor-pointer" />
                        <span className="text-sm font-medium text-gray-300">{key}</span>
                      </label>
                    ))}

                    {/* Група: Атрибути */}
                    <div className="text-[10px] text-gray-500 uppercase font-bold mt-4 mb-1 px-2">Attributes</div>
                    {['Pace', 'Shooting', 'Passing', 'Dribbling', 'Defending', 'Physical'].map(key => (
                      <label key={key} className="flex items-center gap-3 p-2 hover:bg-gray-800 rounded-lg cursor-pointer transition-colors">
                        <input type="checkbox" checked={visibleColumns[key as keyof typeof visibleColumns]} onChange={() => toggleColumn(key as keyof typeof visibleColumns)} className="accent-yellow-500 w-4 h-4 cursor-pointer" />
                        <span className="text-sm font-medium text-gray-300">{key}</span>
                      </label>
                    ))}

                    {/* Група: Статистика */}
                    <div className="text-[10px] text-gray-500 uppercase font-bold mt-4 mb-1 px-2">Season Stats</div>
                    {['Apps', 'Goals', 'Assists', 'Rating'].map(key => (
                      <label key={key} className="flex items-center gap-3 p-2 hover:bg-gray-800 rounded-lg cursor-pointer transition-colors">
                        <input type="checkbox" checked={visibleColumns[key as keyof typeof visibleColumns]} onChange={() => toggleColumn(key as keyof typeof visibleColumns)} className="accent-yellow-500 w-4 h-4 cursor-pointer" />
                        <span className="text-sm font-medium text-gray-300">{key}</span>
                      </label>
                    ))}

                  </div>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* ADVANCED FILTERS (Остава същото) */}
        {showAdvanced && (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6 pt-5 border-t border-gray-800 animate-in fade-in slide-in-from-top-2">
            <div>
              <label className="text-[10px] text-gray-500 uppercase font-bold mb-1 block">Nationality</label>
              <input type="text" placeholder="e.g. Brazil" value={nationality} onChange={(e) => { setNationality(e.target.value); setActiveQuickFilter('custom'); }} className="w-full bg-gray-950 border border-gray-800 rounded-xl py-2 px-3 text-white focus:border-yellow-500 outline-none text-sm" />
            </div>
            <div className="bg-gray-950 p-3 rounded-xl border border-gray-800">
              <label className="text-[10px] text-gray-500 uppercase font-bold flex justify-between">
                <span>Min Age</span> <span className="text-yellow-500">{minAge}</span>
              </label>
              <input type="range" min="15" max="40" value={minAge} onChange={(e) => { setMinAge(Number(e.target.value)); setActiveQuickFilter('custom'); }} className="w-full h-1 bg-gray-800 rounded-lg appearance-none cursor-pointer accent-yellow-500 mt-3" />
            </div>
            <div className="bg-gray-950 p-3 rounded-xl border border-gray-800">
              <label className="text-[10px] text-gray-500 uppercase font-bold flex justify-between">
                <span>Max Age</span> <span className="text-yellow-500">{maxAge}</span>
              </label>
              <input type="range" min="15" max="40" value={maxAge} onChange={(e) => { setMaxAge(Number(e.target.value)); setActiveQuickFilter('custom'); }} className="w-full h-1 bg-gray-800 rounded-lg appearance-none cursor-pointer accent-yellow-500 mt-3" />
            </div>
          </div>
        )}
      </div>

      {/* 3. PLAYERS TABLE */}
      <div className="bg-gray-900 border border-gray-800 rounded-2xl overflow-hidden shadow-2xl">
        {isLoading ? (
          <div className="p-20 flex justify-center"><Loader2 className="animate-spin text-yellow-500" size={40} /></div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left">
              <thead>
                <tr className="bg-gray-800/50 text-gray-500 text-[10px] uppercase tracking-widest border-b border-gray-800">
                  <th className="px-4 py-4 min-w-[200px]">Player</th>
                  <th className="px-2 py-4">Pos</th>

                  {/* AGE */}
                  {visibleColumns.Age && (
                    <th className="px-2 py-4 text-center cursor-pointer hover:text-white transition-colors" onClick={() => { handleSort('Age'); setActiveQuickFilter('custom'); }}>
                      <div className="flex items-center justify-center gap-1">Age <ArrowUpDown size={10} className={sortBy.includes('Age') ? 'text-yellow-500' : ''} /></div>
                    </th>
                  )}

                  {/* ATTRIBUTES */}
                  {visibleColumns.Pace && <th className="px-1 py-4 text-center cursor-pointer hover:text-white transition-colors" onClick={() => { handleSort('Pace'); setActiveQuickFilter('custom'); }}><div className="flex items-center justify-center gap-1">PAC <ArrowUpDown size={10} className={sortBy.includes('Pace') ? 'text-yellow-500' : ''} /></div></th>}
                  {visibleColumns.Shooting && <th className="px-1 py-4 text-center cursor-pointer hover:text-white transition-colors" onClick={() => { handleSort('Shooting'); setActiveQuickFilter('custom'); }}><div className="flex items-center justify-center gap-1">SHO <ArrowUpDown size={10} className={sortBy.includes('Shooting') ? 'text-yellow-500' : ''} /></div></th>}
                  {visibleColumns.Passing && <th className="px-1 py-4 text-center cursor-pointer hover:text-white transition-colors" onClick={() => { handleSort('Passing'); setActiveQuickFilter('custom'); }}><div className="flex items-center justify-center gap-1">PAS <ArrowUpDown size={10} className={sortBy.includes('Passing') ? 'text-yellow-500' : ''} /></div></th>}
                  {visibleColumns.Dribbling && <th className="px-1 py-4 text-center cursor-pointer hover:text-white transition-colors" onClick={() => { handleSort('Dribbling'); setActiveQuickFilter('custom'); }}><div className="flex items-center justify-center gap-1">DRI <ArrowUpDown size={10} className={sortBy.includes('Dribbling') ? 'text-yellow-500' : ''} /></div></th>}
                  {visibleColumns.Defending && <th className="px-1 py-4 text-center cursor-pointer hover:text-white transition-colors" onClick={() => { handleSort('Defending'); setActiveQuickFilter('custom'); }}><div className="flex items-center justify-center gap-1">DEF <ArrowUpDown size={10} className={sortBy.includes('Defending') ? 'text-yellow-500' : ''} /></div></th>}
                  {visibleColumns.Physical && <th className="px-1 py-4 text-center cursor-pointer hover:text-white transition-colors" onClick={() => { handleSort('Physical'); setActiveQuickFilter('custom'); }}><div className="flex items-center justify-center gap-1">PHY <ArrowUpDown size={10} className={sortBy.includes('Physical') ? 'text-yellow-500' : ''} /></div></th>}

                  {/* STATS */}
                  {visibleColumns.Apps && <th className="px-2 py-4 text-center text-gray-500">Apps</th>}
                  {visibleColumns.Goals && <th className="px-2 py-4 text-center cursor-pointer hover:text-white transition-colors" onClick={() => { handleSort('Goals'); setActiveQuickFilter('custom'); }}><div className="flex items-center justify-center gap-1">G <ArrowUpDown size={10} className={sortBy.includes('Goals') ? 'text-yellow-500' : ''} /></div></th>}
                  {visibleColumns.Assists && <th className="px-2 py-4 text-center cursor-pointer hover:text-white transition-colors" onClick={() => { handleSort('Assists'); setActiveQuickFilter('custom'); }}><div className="flex items-center justify-center gap-1">A <ArrowUpDown size={10} className={sortBy.includes('Assists') ? 'text-yellow-500' : ''} /></div></th>}
                  {visibleColumns.Rating && <th className="px-2 py-4 text-center cursor-pointer hover:text-white transition-colors" onClick={() => { handleSort('Rating'); setActiveQuickFilter('custom'); }}><div className="flex items-center justify-center gap-1">Avg <ArrowUpDown size={10} className={sortBy.includes('Rating') ? 'text-yellow-500' : ''} /></div></th>}

                  {visibleColumns.Club && <th className="px-4 py-4 min-w-[150px]">Current Club</th>}
                  {visibleColumns.Status && <th className="px-2 py-4">Status</th>}

                  {visibleColumns.Wage && (
                    <th className="px-4 py-4 text-right cursor-pointer hover:text-white transition-colors" onClick={() => { handleSort('Wage'); setActiveQuickFilter('custom'); }}>
                      <div className="flex items-center justify-end gap-1">Wage <ArrowUpDown size={10} className={sortBy.includes('Wage') ? 'text-yellow-500' : ''} /></div>
                    </th>
                  )}

                  {visibleColumns.Value && (
                    <th className="px-4 py-4 text-right cursor-pointer hover:text-white transition-colors" onClick={() => { handleSort('Value'); setActiveQuickFilter('custom'); }}>
                      <div className="flex items-center justify-end gap-1">Value <ArrowUpDown size={10} className={sortBy.includes('Value') ? 'text-yellow-500' : ''} /></div>
                    </th>
                  )}
                  <th className="px-2 py-4"></th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-800/50">
                {players && players.length > 0 ? (
                  players.map((p) => {
                    const getAttrColor = (valStr: string | number) => {
                      let maxVal = 0;
                      if (typeof valStr === 'string' && valStr.includes('-')) {
                        maxVal = parseInt(valStr.split('-')[1]);
                      } else {
                        maxVal = Number(valStr);
                      }
                      if (maxVal >= 85) return 'text-green-400 font-black';
                      if (maxVal >= 70) return 'text-yellow-400 font-bold';
                      return 'text-gray-500';
                    };

                    return (
                      <tr key={p.id} className="hover:bg-gray-800/50 transition-colors group cursor-pointer" onClick={() => navigate(`/world/player/${p.id}`)}>
                        <td className="px-4 py-3">
                          <div className="font-bold text-white group-hover:text-yellow-500 transition-colors truncate max-w-[180px]">{p.name}</div>
                          <div className="text-[10px] text-gray-500 mt-0.5 truncate max-w-[180px]">{p.nationality}</div>
                        </td>
                        <td className={`px-2 py-3 font-black text-xs ${getPosColor(p.position)}`}>{p.position}</td>

                        {visibleColumns.Age && <td className="px-2 py-3 text-center text-gray-300">{p.age}</td>}

                        {/* ATTRIBUTES */}
                        {visibleColumns.Pace && <td className={`px-1 py-3 text-center text-xs ${getAttrColor(p.pace)}`}>{p.pace}</td>}
                        {visibleColumns.Shooting && <td className={`px-1 py-3 text-center text-xs ${getAttrColor(p.shooting)}`}>{p.shooting}</td>}
                        {visibleColumns.Passing && <td className={`px-1 py-3 text-center text-xs ${getAttrColor(p.passing)}`}>{p.passing}</td>}
                        {visibleColumns.Dribbling && <td className={`px-1 py-3 text-center text-xs ${getAttrColor(p.dribbling)}`}>{p.dribbling}</td>}
                        {visibleColumns.Defending && <td className={`px-1 py-3 text-center text-xs ${getAttrColor(p.defending)}`}>{p.defending}</td>}
                        {visibleColumns.Physical && <td className={`px-1 py-3 text-center text-xs ${getAttrColor(p.physical)}`}>{p.physical}</td>}

                        {/* STATS */}
                        {visibleColumns.Apps && <td className="px-2 py-3 text-center text-gray-400 text-xs">{p.apps}</td>}
                        {visibleColumns.Goals && <td className={`px-2 py-3 text-center text-xs font-bold ${p.goals > 0 ? 'text-white' : 'text-gray-600'}`}>{p.goals}</td>}
                        {visibleColumns.Assists && <td className={`px-2 py-3 text-center text-xs font-bold ${p.assists > 0 ? 'text-white' : 'text-gray-600'}`}>{p.assists}</td>}
                        {visibleColumns.Rating && <td className={`px-2 py-3 text-center text-xs font-bold ${p.avgRating >= 7.5 ? 'text-green-400' : p.avgRating > 0 ? 'text-yellow-400' : 'text-gray-600'}`}>
                          {p.avgRating > 0 ? p.avgRating.toFixed(2) : '-'}
                        </td>}

                        {visibleColumns.Club && (
                          <td className="px-4 py-3 text-gray-400 text-sm truncate max-w-[150px]">
                            {p.clubName !== "Free Agent" ? p.clubName : <span className="text-gray-600 italic">No Club</span>}
                          </td>
                        )}

                        {visibleColumns.Status && (
                          <td className="px-2 py-3">
                            <div className="flex items-center gap-1 text-[10px] font-bold uppercase tracking-wider">
                              {p.hasAgency ?
                                <span className="text-purple-500 bg-purple-500/10 px-1.5 py-0.5 rounded flex items-center gap-1"><Users size={10} /> Rep'd</span> :
                                <span className="text-emerald-500 bg-emerald-500/10 px-1.5 py-0.5 rounded flex items-center gap-1"><UserMinus size={10} /> Target</span>
                              }
                            </div>
                          </td>
                        )}

                        {visibleColumns.Wage && (
                          <td className="px-4 py-3 text-right font-mono text-gray-400 text-[11px] whitespace-nowrap">
                            {p.weeklyWage > 0 ? `$${p.weeklyWage.toLocaleString()}/w` : '-'}
                          </td>
                        )}

                        {visibleColumns.Value && (
                          <td className="px-4 py-3 text-right font-mono text-white font-bold whitespace-nowrap">
                            ${p.marketValue ? (p.marketValue / 1000000).toFixed(1) : 0}M
                          </td>
                        )}

                        <td className="px-2 py-3 text-right">
                          {!p.hasAgency && (
                            <button
                              onClick={(e) => { e.stopPropagation(); navigate(`/world/player/${p.id}`); }}
                              className="p-1.5 bg-yellow-500/10 text-yellow-500 hover:bg-yellow-500 hover:text-black rounded-lg transition-all"
                              title="View & Pitch"
                            >
                              <UserPlus size={14} />
                            </button>
                          )}
                        </td>
                        <td className="px-2 py-3 text-right">
                          <div className="flex items-center justify-end gap-2">
                            {/* БУТОН ЗА ШОРТЛИСТ (Показва се за всички играчи) */}
                            <button
                              onClick={(e) => handleAddToShortlist(e, p.id)}
                              className="p-1.5 bg-gray-800 text-gray-400 hover:bg-yellow-500 hover:text-black rounded-lg transition-all"
                              title="Add to Shortlist"
                            >
                              <BookmarkPlus size={14} />
                            </button>

                            {/* ПРОМЕНЕН БУТОН ЗА ОФЕРТА */}
                            {!p.hasAgency && (
                              <button
                                onClick={(e) => { e.stopPropagation(); setPitchPlayer(p); }} // ТУК Е ПРОМЯНАТА
                                className="p-1.5 bg-yellow-500/10 text-yellow-500 hover:bg-yellow-500 hover:text-black rounded-lg transition-all"
                                title="Pitch Player"
                              >
                                <UserPlus size={14} />
                              </button>
                            )}
                          </div>
                        </td>
                      </tr>
                    );
                  })
                ) : (
                  <tr>
                    <td colSpan={20} className="px-6 py-16 text-center text-gray-500">
                      <div className="flex flex-col items-center justify-center gap-3">
                        <Search size={32} className="text-gray-700" />
                        <p>No players found matching your criteria.</p>
                      </div>
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* 4. PAGINATION */}
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
      {/* НОВО: МОДАЛЪТ */}
      <OfferRepresentationModal
        player={pitchPlayer || {}}
        isOpen={!!pitchPlayer}
        onClose={() => setPitchPlayer(null)}
        onSuccess={handlePitchSuccess}
      />
    </div>
  );
}