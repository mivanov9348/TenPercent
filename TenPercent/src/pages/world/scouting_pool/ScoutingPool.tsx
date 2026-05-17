import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { Search, Loader2, ChevronLeft, ChevronRight, ArrowUpDown, BookmarkPlus, Shield } from 'lucide-react';
import ScoutingFilters, { type FilterState } from './ScoutingFilters';

export default function ScoutingPool() {
    const navigate = useNavigate();
    const [searchParams, setSearchParams] = useSearchParams();

    const [players, setPlayers] = useState<any[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [pagination, setPagination] = useState({ totalCount: 0, totalPages: 1 });

    // Инициализираме стейта директно от URL параметрите (или слагаме дефолтни)
    const [filters, setFilters] = useState<FilterState>({
        search: searchParams.get('search') || '',
        pos: searchParams.get('pos') || 'All',
        nationality: searchParams.get('nationality') || '',
        minAge: Number(searchParams.get('minAge')) || 15,
        maxAge: Number(searchParams.get('maxAge')) || 40,
        hasAgency: searchParams.get('hasAgency') || 'all',
        agencyName: searchParams.get('agencyName') || ''
    });

    const [activeQuickFilter, setActiveQuickFilter] = useState(searchParams.get('quick') || 'all');
    const [sortBy, setSortBy] = useState(searchParams.get('sortBy') || 'ValueDesc');
    const [page, setPage] = useState(Number(searchParams.get('page')) || 1);

    const [visibleColumns, setVisibleColumns] = useState({
        Age: true, Club: true, Status: true, Wage: false, Value: true,
        Pace: false, Shooting: false, Passing: false, Dribbling: false, Defending: false, Physical: false,
        Apps: false, Goals: false, Assists: false, Rating: false,
    });

    // 1. Синхронизираме State -> URL
    useEffect(() => {
        const params = new URLSearchParams();
        if (filters.search) params.set('search', filters.search);
        if (filters.pos !== 'All') params.set('pos', filters.pos);
        if (filters.nationality) params.set('nationality', filters.nationality);
        if (filters.minAge !== 15) params.set('minAge', filters.minAge.toString());
        if (filters.maxAge !== 40) params.set('maxAge', filters.maxAge.toString());
        if (filters.hasAgency !== 'all') params.set('hasAgency', filters.hasAgency);
        if (filters.agencyName) params.set('agencyName', filters.agencyName);

        params.set('sortBy', sortBy);
        params.set('page', page.toString());
        params.set('quick', activeQuickFilter);

        setSearchParams(params, { replace: true });
    }, [filters, sortBy, page, activeQuickFilter, setSearchParams]);

    // 2. Fetch функция
    const fetchPool = async () => {
        setIsLoading(true);
        try {
            const agencyParam = filters.hasAgency === 'all' ? '' : `&hasAgency=${filters.hasAgency}`;
            const natParam = filters.nationality ? `&nationality=${filters.nationality}` : '';
            const agencyNameParam = filters.agencyName ? `&AgencyName=${filters.agencyName}` : '';
            
            const url = `https://localhost:7135/api/players/get-pool?search=${filters.search}&position=${filters.pos}&minAge=${filters.minAge}&maxAge=${filters.maxAge}&sortBy=${sortBy}${agencyParam}${natParam}${agencyNameParam}&page=${page}&pageSize=20`;

            const response = await fetch(url);
            if (response.ok) {
                const data = await response.json();
                setPlayers(data.items || data.players || []);
                setPagination({
                    totalCount: data.totalCount || 0,
                    totalPages: data.totalPages || 1
                });
            }
        } catch (error) {
            console.error("Failed to fetch pool:", error);
            setPlayers([]);
        } finally {
            setIsLoading(false);
        }
    };

    // 3. Debounce за Fetch (слуша промени в State)
    useEffect(() => {
        const delayDebounceFn = setTimeout(() => {
            fetchPool();
        }, 400);
        return () => clearTimeout(delayDebounceFn);
    }, [filters, sortBy, page]);

    // --- ХЕНДЛЪРИ ЗА ПРОМЕНИ ---
    const handleFilterChange = (key: keyof FilterState, value: any) => {
        setFilters(prev => ({ ...prev, [key]: value }));
        setPage(1); // Ресетваме на първа страница при промяна на филтър
    };

    const handleQuickFilter = (filterId: string) => {
        setActiveQuickFilter(filterId);
        setPage(1); // Ресетваме страницата
        
        let newFilters = { ...filters, search: '', pos: 'All', nationality: '', agencyName: '' };

        switch (filterId) {
            case 'wonderkids':
                setFilters({ ...newFilters, minAge: 15, maxAge: 21, hasAgency: 'false' }); setSortBy('ValueDesc'); break;
            case 'free_agents':
                setFilters({ ...newFilters, minAge: 15, maxAge: 40, hasAgency: 'false' }); setSortBy('ValueDesc'); break;
            case 'prime':
                setFilters({ ...newFilters, minAge: 24, maxAge: 29, hasAgency: 'all' }); setSortBy('ValueDesc'); break;
            case 'veterans':
                setFilters({ ...newFilters, minAge: 32, maxAge: 40, hasAgency: 'all' }); setSortBy('ValueDesc'); break;
            default:
                setFilters({ ...newFilters, minAge: 15, maxAge: 40, hasAgency: 'all' }); setSortBy('ValueDesc'); break;
        }
    };

    const handleSort = (column: string) => {
        // Ако вече сме на този колона като Descending, сменяме на Ascending
        const isDesc = sortBy === `${column}Desc`;
        setSortBy(isDesc ? column : `${column}Desc`);
        setActiveQuickFilter('custom');
        setPage(1); // Ресетваме страницата
    };

    const handlePageChange = (newPage: number) => {
        if (newPage >= 1 && newPage <= pagination.totalPages) {
            setPage(newPage);
            window.scrollTo({ top: 0, behavior: 'smooth' });
        }
    };

    const toggleColumn = (columnName: string) => {
        setVisibleColumns(prev => ({ ...prev, [columnName as keyof typeof visibleColumns]: !prev[columnName as keyof typeof visibleColumns] }));
    };

    const handleAddToShortlist = async (e: React.MouseEvent, playerId: number) => {
        e.stopPropagation();
        const userId = localStorage.getItem('userId');
        if (!userId) { alert("You must be logged in to use the shortlist."); return; }

        try {
            const response = await fetch(`https://localhost:7135/api/players/${playerId}/shortlist`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ userId })
            });
            const data = await response.json();
            if (response.ok) alert("✅ " + data.message);
            else alert("❌ " + data.message);
        } catch (error) { console.error("Failed to add to shortlist", error); }
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
            <div className="flex justify-between items-end">
                <div>
                    <h1 className="text-3xl font-black text-white uppercase tracking-wider">Scouting Pool</h1>
                    <p className="text-gray-400 mt-1">Discover players and expand your influence. Total found: <span className="text-yellow-500 font-bold">{pagination.totalCount}</span></p>
                </div>
            </div>

            <ScoutingFilters
                filters={filters}
                onFilterChange={handleFilterChange}
                activeQuickFilter={activeQuickFilter}
                handleQuickFilter={handleQuickFilter}
                visibleColumns={visibleColumns}
                toggleColumn={toggleColumn}
            />

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

                                    {visibleColumns.Age && <th className="px-2 py-4 text-center cursor-pointer hover:text-white transition-colors" onClick={() => handleSort('Age')}><div className="flex items-center justify-center gap-1">Age <ArrowUpDown size={10} className={sortBy.includes('Age') ? 'text-yellow-500' : ''} /></div></th>}

                                    {visibleColumns.Pace && <th className="px-1 py-4 text-center cursor-pointer hover:text-white transition-colors" onClick={() => handleSort('Pace')}><div className="flex items-center justify-center gap-1">PAC <ArrowUpDown size={10} /></div></th>}
                                    {visibleColumns.Shooting && <th className="px-1 py-4 text-center cursor-pointer hover:text-white transition-colors" onClick={() => handleSort('Shooting')}><div className="flex items-center justify-center gap-1">SHO <ArrowUpDown size={10} /></div></th>}
                                    {visibleColumns.Passing && <th className="px-1 py-4 text-center cursor-pointer hover:text-white transition-colors" onClick={() => handleSort('Passing')}><div className="flex items-center justify-center gap-1">PAS <ArrowUpDown size={10} /></div></th>}
                                    {visibleColumns.Dribbling && <th className="px-1 py-4 text-center cursor-pointer hover:text-white transition-colors" onClick={() => handleSort('Dribbling')}><div className="flex items-center justify-center gap-1">DRI <ArrowUpDown size={10} /></div></th>}
                                    {visibleColumns.Defending && <th className="px-1 py-4 text-center cursor-pointer hover:text-white transition-colors" onClick={() => handleSort('Defending')}><div className="flex items-center justify-center gap-1">DEF <ArrowUpDown size={10} /></div></th>}
                                    {visibleColumns.Physical && <th className="px-1 py-4 text-center cursor-pointer hover:text-white transition-colors" onClick={() => handleSort('Physical')}><div className="flex items-center justify-center gap-1">PHY <ArrowUpDown size={10} /></div></th>}

                                    {visibleColumns.Apps && <th className="px-2 py-4 text-center text-gray-500">Apps</th>}
                                    {visibleColumns.Goals && <th className="px-2 py-4 text-center cursor-pointer hover:text-white transition-colors" onClick={() => handleSort('Goals')}><div className="flex items-center justify-center gap-1">G <ArrowUpDown size={10} /></div></th>}
                                    {visibleColumns.Assists && <th className="px-2 py-4 text-center cursor-pointer hover:text-white transition-colors" onClick={() => handleSort('Assists')}><div className="flex items-center justify-center gap-1">A <ArrowUpDown size={10} /></div></th>}
                                    {visibleColumns.Rating && <th className="px-2 py-4 text-center cursor-pointer hover:text-white transition-colors" onClick={() => handleSort('Rating')}><div className="flex items-center justify-center gap-1">Avg <ArrowUpDown size={10} /></div></th>}

                                    {visibleColumns.Club && <th className="px-4 py-4 min-w-[150px]">Current Club</th>}
                                    {visibleColumns.Status && <th className="px-2 py-4">Agency</th>}
                                    {visibleColumns.Wage && <th className="px-4 py-4 text-right cursor-pointer hover:text-white transition-colors" onClick={() => handleSort('Wage')}><div className="flex items-center justify-end gap-1">Wage <ArrowUpDown size={10} className={sortBy.includes('Wage') ? 'text-yellow-500' : ''} /></div></th>}
                                    {visibleColumns.Value && <th className="px-4 py-4 text-right cursor-pointer hover:text-white transition-colors" onClick={() => handleSort('Value')}><div className="flex items-center justify-end gap-1">Value <ArrowUpDown size={10} className={sortBy.includes('Value') ? 'text-yellow-500' : ''} /></div></th>}
                                    <th className="px-2 py-4"></th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-800/50">
                                {players && players.length > 0 ? (
                                    players.map((p) => {
                                        const getAttrColor = (valStr: string | number) => {
                                            let maxVal = typeof valStr === 'string' && valStr.includes('-') ? parseInt(valStr.split('-')[1]) : Number(valStr);
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

                                                {visibleColumns.Pace && <td className={`px-1 py-3 text-center text-xs ${getAttrColor(p.pace)}`}>{p.pace}</td>}
                                                {visibleColumns.Shooting && <td className={`px-1 py-3 text-center text-xs ${getAttrColor(p.shooting)}`}>{p.shooting}</td>}
                                                {visibleColumns.Passing && <td className={`px-1 py-3 text-center text-xs ${getAttrColor(p.passing)}`}>{p.passing}</td>}
                                                {visibleColumns.Dribbling && <td className={`px-1 py-3 text-center text-xs ${getAttrColor(p.dribbling)}`}>{p.dribbling}</td>}
                                                {visibleColumns.Defending && <td className={`px-1 py-3 text-center text-xs ${getAttrColor(p.defending)}`}>{p.defending}</td>}
                                                {visibleColumns.Physical && <td className={`px-1 py-3 text-center text-xs ${getAttrColor(p.physical)}`}>{p.physical}</td>}

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
                                                        <div className="flex items-center gap-1 text-[11px] font-bold tracking-wider">
                                                            {p.hasAgency && p.agencyName ? (
                                                                <span className="text-purple-400 bg-purple-500/10 px-2 py-1 rounded flex items-center gap-1 truncate max-w-[150px]" title={p.agencyName}>
                                                                    <Shield size={12} className="shrink-0" /> {p.agencyName}
                                                                </span>
                                                            ) : (
                                                                <span className="text-gray-600 italic px-2">Free Agent</span>
                                                            )}
                                                        </div>
                                                    </td>
                                                )}

                                                {visibleColumns.Wage && <td className="px-4 py-3 text-right font-mono text-gray-400 text-[11px] whitespace-nowrap">{p.weeklyWage > 0 ? `$${p.weeklyWage.toLocaleString()}/w` : '-'}</td>}
                                                {visibleColumns.Value && <td className="px-4 py-3 text-right font-mono text-white font-bold whitespace-nowrap">${p.marketValue ? (p.marketValue / 1000000).toFixed(1) : 0}M</td>}

                                                <td className="px-2 py-3 text-right">
                                                    <div className="flex items-center justify-end gap-2">
                                                        <button onClick={(e) => handleAddToShortlist(e, p.id)} className="p-1.5 bg-gray-800 text-gray-400 hover:bg-yellow-500 hover:text-black rounded-lg transition-all" title="Add to Shortlist"><BookmarkPlus size={14} /></button>
                                                    </div>
                                                </td>
                                            </tr>
                                        );
                                    })
                                ) : (
                                    <tr><td colSpan={20} className="px-6 py-16 text-center text-gray-500"><div className="flex flex-col items-center justify-center gap-3"><Search size={32} className="text-gray-700" /><p>No players found matching your criteria.</p></div></td></tr>
                                )}
                            </tbody>
                        </table>
                    </div>
                )}
            </div>

            {!isLoading && pagination.totalPages > 1 && (
                <div className="flex justify-center items-center gap-4 mt-8">
                    <button onClick={() => handlePageChange(page - 1)} disabled={page === 1} className="p-2 bg-gray-800 text-gray-400 rounded-lg hover:bg-gray-700 disabled:opacity-30 transition-colors"><ChevronLeft /></button>
                    <div className="text-gray-400 text-sm font-bold">Page <span className="text-white">{page}</span> of {pagination.totalPages}</div>
                    <button onClick={() => handlePageChange(page + 1)} disabled={page === pagination.totalPages} className="p-2 bg-gray-800 text-gray-400 rounded-lg hover:bg-gray-700 disabled:opacity-30 transition-colors"><ChevronRight /></button>
                </div>
            )}
        </div>
    );
}