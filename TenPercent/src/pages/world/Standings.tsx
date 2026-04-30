import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { ChevronDown, Loader2 } from 'lucide-react';

export default function Standings() {
  const navigate = useNavigate();
  const [leaguesData, setLeaguesData] = useState<any[]>([]);
  const [selectedLeagueId, setSelectedLeagueId] = useState<number | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchStandings = async () => {
      try {
        const response = await fetch('https://localhost:7135/api/leagues/standings');
        if (response.ok) {
          const data = await response.json();
          setLeaguesData(data);
          
          if (data.length > 0) {
            setSelectedLeagueId(data[0].id);
          }
        }
      } catch (error) {
        console.error("Failed to fetch standings:", error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchStandings();
  }, []);

  if (isLoading) {
    return <div className="flex justify-center items-center h-64"><Loader2 className="animate-spin text-yellow-500" size={40} /></div>;
  }

  if (leaguesData.length === 0) {
    return <div className="text-center text-gray-500 mt-10">No leagues available. Please import data.</div>;
  }

  const activeLeague = leaguesData.find(l => l.id === selectedLeagueId);

  return (
    <div className="bg-gray-900 border border-gray-800 rounded-xl shadow-lg overflow-hidden">
      
      {/* Header & Dropdown */}
      <div className="p-4 border-b border-gray-800 flex justify-between items-center bg-gray-900">
        <h2 className="text-lg font-bold text-white uppercase tracking-wider">League Table</h2>
        
        <div className="relative w-72">
          <select 
            value={selectedLeagueId || ''}
            onChange={(e) => setSelectedLeagueId(Number(e.target.value))}
            className="w-full appearance-none bg-gray-800 border border-gray-700 text-white font-bold py-2.5 pl-4 pr-10 rounded-lg focus:outline-none focus:border-yellow-500 cursor-pointer text-sm transition-colors"
          >
            {leaguesData.map(l => (
              <option key={l.id} value={l.id}>{l.name}</option>
            ))}
          </select>
          <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none" size={18} />
        </div>
      </div>
      
      {/* Table */}
      <div className="overflow-x-auto">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-gray-800/50 border-b border-gray-800 text-xs uppercase tracking-wider text-gray-500">
              <th className="p-4 font-medium text-center w-12">#</th>
              <th className="p-4 font-medium">Club</th>
              <th className="p-4 font-medium text-center w-12">P</th>
              <th className="p-4 font-medium text-center w-12">W</th>
              <th className="p-4 font-medium text-center w-12">D</th>
              <th className="p-4 font-medium text-center w-12">L</th>
              <th className="p-4 font-medium text-center w-16">GD</th>
              <th className="p-4 font-black text-center text-white w-16">Pts</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-800/50">
            {activeLeague?.standings.map((row: any) => (
              // ТУК правим реда кликаем и насочваме към ClubDetails
              <tr 
                key={row.clubId} 
                onClick={() => navigate(`/world/club/${row.clubId}`)}
                className="hover:bg-gray-800/80 transition-colors cursor-pointer group"
              >
                <td className="p-4 text-center">
                  <span className={`font-bold ${row.pos <= 4 ? 'text-blue-400' : row.pos >= activeLeague.standings.length - 2 ? 'text-red-400' : 'text-gray-500'}`}>
                    {row.pos}
                  </span>
                </td>
                <td className="p-4 font-bold text-gray-300 group-hover:text-yellow-400 transition-colors">
                  {row.team}
                </td>
                <td className="p-4 text-center text-gray-500">{row.p}</td>
                <td className="p-4 text-center text-gray-500">{row.w}</td>
                <td className="p-4 text-center text-gray-500">{row.d}</td>
                <td className="p-4 text-center text-gray-500">{row.l}</td>
                <td className="p-4 text-center text-gray-500">{row.gd}</td>
                <td className="p-4 text-center font-black text-yellow-500">{row.pts}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}