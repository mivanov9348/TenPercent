import { useState, useRef, useEffect } from 'react';
import { Search, SlidersHorizontal, LayoutTemplate, Users, Star, Flame, Briefcase } from 'lucide-react';

// Дефинираме интерфейс за всички филтри, за да е строго типизирано
export interface FilterState {
  search: string;
  pos: string;
  nationality: string;
  minAge: number;
  maxAge: number;
  hasAgency: string;
  agencyName: string;
}

interface ScoutingFiltersProps {
  filters: FilterState;
  setFilters: React.Dispatch<React.SetStateAction<FilterState>>;
  activeQuickFilter: string;
  handleQuickFilter: (filterId: string) => void;
  visibleColumns: any;
  toggleColumn: (columnName: string) => void;
}

export default function ScoutingFilters({
  filters,
  setFilters,
  activeQuickFilter,
  handleQuickFilter,
  visibleColumns,
  toggleColumn
}: ScoutingFiltersProps) {
  const [showAdvanced, setShowAdvanced] = useState(false);
  const [showColumnMenu, setShowColumnMenu] = useState(false);
  const columnMenuRef = useRef<HTMLDivElement>(null);

  // Затваряне на менюто за колони при клик отвън
  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (columnMenuRef.current && !columnMenuRef.current.contains(event.target as Node)) {
        setShowColumnMenu(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  // Помощна функция за ъпдейтване само на 1 филтър
  const updateFilter = (key: keyof FilterState, value: any) => {
    setFilters(prev => ({ ...prev, [key]: value }));
  };

  const quickFilters = [
    { id: 'all', label: 'All Database', icon: <Users size={16} /> },
    { id: 'wonderkids', label: 'Wonderkids (U21)', icon: <Star size={16} /> },
    { id: 'prime', label: 'Prime Age (24-29)', icon: <Flame size={16} /> },
    { id: 'free_agents', label: 'Free Agents', icon: <Briefcase size={16} /> },
  ];

  return (
    <div className="space-y-4">
      {/* QUICK FILTERS */}
      <div className="flex gap-3 overflow-x-auto hide-scrollbar pb-2">
        {quickFilters.map(qf => (
          <button
            key={qf.id}
            onClick={() => handleQuickFilter(qf.id)}
            className={`flex items-center gap-2 px-4 py-2.5 rounded-xl font-bold text-sm whitespace-nowrap transition-all ${
              activeQuickFilter === qf.id
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
          
          {/* ТЪРСАЧКА */}
          <div className="relative lg:col-span-2">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-600" size={16} />
            <input
              type="text"
              placeholder="Search by name..."
              value={filters.search}
              onChange={(e) => updateFilter('search', e.target.value)}
              className="w-full bg-gray-950 border border-gray-800 rounded-xl py-2.5 pl-9 pr-4 text-white focus:border-yellow-500 outline-none text-sm"
            />
          </div>

          <select
            value={filters.pos}
            onChange={(e) => updateFilter('pos', e.target.value)}
            className="w-full bg-gray-950 border border-gray-800 text-white px-3 py-2.5 rounded-xl focus:border-yellow-500 outline-none text-sm cursor-pointer"
          >
            <option value="All">All Positions</option>
            <option value="ST">Strikers (ST)</option>
            <option value="MID">Midfielders (MID)</option>
            <option value="DEF">Defenders (DEF)</option>
            <option value="GK">Goalkeepers (GK)</option>
          </select>

          <select
            value={filters.hasAgency}
            onChange={(e) => updateFilter('hasAgency', e.target.value)}
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
                    
                    <div className="text-[10px] text-gray-500 uppercase font-bold mt-2 mb-1 px-2">Core Info</div>
                    {['Age', 'Club', 'Status', 'Wage', 'Value'].map(key => (
                      <label key={key} className="flex items-center gap-3 p-2 hover:bg-gray-800 rounded-lg cursor-pointer transition-colors">
                        <input type="checkbox" checked={visibleColumns[key as keyof typeof visibleColumns]} onChange={() => toggleColumn(key)} className="accent-yellow-500 w-4 h-4 cursor-pointer" />
                        <span className="text-sm font-medium text-gray-300">{key}</span>
                      </label>
                    ))}

                    <div className="text-[10px] text-gray-500 uppercase font-bold mt-4 mb-1 px-2">Attributes</div>
                    {['Pace', 'Shooting', 'Passing', 'Dribbling', 'Defending', 'Physical'].map(key => (
                      <label key={key} className="flex items-center gap-3 p-2 hover:bg-gray-800 rounded-lg cursor-pointer transition-colors">
                        <input type="checkbox" checked={visibleColumns[key as keyof typeof visibleColumns]} onChange={() => toggleColumn(key)} className="accent-yellow-500 w-4 h-4 cursor-pointer" />
                        <span className="text-sm font-medium text-gray-300">{key}</span>
                      </label>
                    ))}

                    <div className="text-[10px] text-gray-500 uppercase font-bold mt-4 mb-1 px-2">Season Stats</div>
                    {['Apps', 'Goals', 'Assists', 'Rating'].map(key => (
                      <label key={key} className="flex items-center gap-3 p-2 hover:bg-gray-800 rounded-lg cursor-pointer transition-colors">
                        <input type="checkbox" checked={visibleColumns[key as keyof typeof visibleColumns]} onChange={() => toggleColumn(key)} className="accent-yellow-500 w-4 h-4 cursor-pointer" />
                        <span className="text-sm font-medium text-gray-300">{key}</span>
                      </label>
                    ))}
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* ADVANCED FILTERS */}
        {showAdvanced && (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6 pt-5 border-t border-gray-800 animate-in fade-in slide-in-from-top-2">
            <div>
              <label className="text-[10px] text-gray-500 uppercase font-bold mb-1 block">Nationality</label>
              <input type="text" placeholder="e.g. Brazil" value={filters.nationality} onChange={(e) => updateFilter('nationality', e.target.value)} className="w-full bg-gray-950 border border-gray-800 rounded-xl py-2 px-3 text-white focus:border-yellow-500 outline-none text-sm" />
            </div>
            <div className="bg-gray-950 p-3 rounded-xl border border-gray-800">
              <label className="text-[10px] text-gray-500 uppercase font-bold flex justify-between">
                <span>Min Age</span> <span className="text-yellow-500">{filters.minAge}</span>
              </label>
              <input type="range" min="15" max="40" value={filters.minAge} onChange={(e) => updateFilter('minAge', Number(e.target.value))} className="w-full h-1 bg-gray-800 rounded-lg appearance-none cursor-pointer accent-yellow-500 mt-3" />
            </div>
            <div className="bg-gray-950 p-3 rounded-xl border border-gray-800">
              <label className="text-[10px] text-gray-500 uppercase font-bold flex justify-between">
                <span>Max Age</span> <span className="text-yellow-500">{filters.maxAge}</span>
              </label>
              <input type="range" min="15" max="40" value={filters.maxAge} onChange={(e) => updateFilter('maxAge', Number(e.target.value))} className="w-full h-1 bg-gray-800 rounded-lg appearance-none cursor-pointer accent-yellow-500 mt-3" />
            </div>
          </div>
        )}
      </div>
    </div>
  );
}