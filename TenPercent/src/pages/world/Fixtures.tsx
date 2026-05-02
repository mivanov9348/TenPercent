import { useState, useEffect } from 'react';
import { ChevronDown, Loader2, Calendar } from 'lucide-react';

export default function Fixtures() {
  const [data, setData] = useState<any>(null);
  const [selectedLeagueId, setSelectedLeagueId] = useState<number | null>(null);
  const [selectedGameweek, setSelectedGameweek] = useState<number>(1);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchFixtures = async () => {
      try {
        const response = await fetch('https://localhost:7135/api/fixtures/all');
        if (response.ok) {
          const result = await response.json();
          setData(result);
          
          // Задаваме дефолтни стойности (Първата лига и ТЕКУЩИЯТ кръг)
          if (result.leagues && result.leagues.length > 0) {
            setSelectedLeagueId(result.leagues[0].id);
          }
          if (result.currentGameweek) {
            setSelectedGameweek(result.currentGameweek);
          }
        }
      } catch (error) {
        console.error("Failed to fetch fixtures:", error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchFixtures();
  }, []);

  if (isLoading) {
    return <div className="flex justify-center items-center h-64"><Loader2 className="animate-spin text-yellow-500" size={40} /></div>;
  }

  if (!data || !data.leagues || data.leagues.length === 0) {
    return <div className="text-center text-gray-500 mt-10">No fixtures available. Is the season running?</div>;
  }

  // Намираме активната лига
  const activeLeague = data.leagues.find((l: any) => l.id === selectedLeagueId);
  // Намираме кръга в активната лига
  const activeGameweekData = activeLeague?.gameweeks.find((gw: any) => gw.gameweek === selectedGameweek);

  return (
    <div className="bg-gray-900 border border-gray-800 rounded-xl shadow-lg overflow-hidden">
      
      {/* Header & Dropdowns */}
      <div className="p-4 border-b border-gray-800 flex flex-col md:flex-row justify-between items-start md:items-center bg-gray-900 gap-4">
        <h2 className="text-lg font-bold text-white uppercase tracking-wider flex items-center gap-2">
            <Calendar className="text-blue-400" size={20} />
            Match Schedule
        </h2>
        
        <div className="flex flex-col sm:flex-row gap-3 w-full md:w-auto">
          {/* League Dropdown */}
          <div className="relative w-full sm:w-64">
            <select 
              value={selectedLeagueId || ''}
              onChange={(e) => {
                  setSelectedLeagueId(Number(e.target.value));
                  // Можем да върнем кръга до 1 или да запазим избрания
              }}
              className="w-full appearance-none bg-gray-800 border border-gray-700 text-white font-bold py-2.5 pl-4 pr-10 rounded-lg focus:outline-none focus:border-yellow-500 cursor-pointer text-sm transition-colors"
            >
              {data.leagues.map((l: any) => (
                <option key={l.id} value={l.id}>{l.name}</option>
              ))}
            </select>
            <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none" size={18} />
          </div>

          {/* Gameweek Dropdown */}
          <div className="relative w-full sm:w-40">
            <select 
              value={selectedGameweek}
              onChange={(e) => setSelectedGameweek(Number(e.target.value))}
              className="w-full appearance-none bg-gray-800 border border-gray-700 text-yellow-500 font-bold py-2.5 pl-4 pr-10 rounded-lg focus:outline-none focus:border-yellow-500 cursor-pointer text-sm transition-colors"
            >
              {activeLeague?.gameweeks.map((gw: any) => (
                <option key={gw.gameweek} value={gw.gameweek}>Gameweek {gw.gameweek}</option>
              ))}
            </select>
            <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none" size={18} />
          </div>
        </div>
      </div>
      
      {/* Matches List */}
      <div className="p-4">
        {activeGameweekData && activeGameweekData.matches.length > 0 ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {activeGameweekData.matches.map((match: any) => (
              <div key={match.id} className="bg-gray-800 border border-gray-700 rounded-lg p-4 flex flex-col justify-center hover:border-gray-600 transition-colors">
                
                {/* Date & Status */}
                <div className="flex justify-between items-center text-[10px] uppercase font-bold tracking-widest text-gray-500 mb-3">
                  <span>{new Date(match.date).toLocaleDateString()}</span>
                  {match.isPlayed ? (
                      <span className="bg-green-500/10 text-green-400 px-2 py-0.5 rounded">FT</span>
                  ) : (
                      <span className="bg-gray-700 text-gray-300 px-2 py-0.5 rounded">{new Date(match.date).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</span>
                  )}
                </div>

                {/* Scoreboard */}
                <div className="flex justify-between items-center text-white font-bold">
                  <span className="truncate flex-1 text-right">{match.homeTeam}</span>
                  
                  <div className="px-4 shrink-0 flex items-center justify-center">
                    {match.isPlayed ? (
                      <span className="text-xl font-black bg-gray-900 px-3 py-1 rounded-md text-yellow-500 border border-gray-700">
                        {match.homeGoals} - {match.awayGoals}
                      </span>
                    ) : (
                      <span className="text-sm font-mono text-gray-600 bg-gray-900 px-2 py-1 rounded">VS</span>
                    )}
                  </div>

                  <span className="truncate flex-1 text-left">{match.awayTeam}</span>
                </div>
              </div>
            ))}
          </div>
        ) : (
            <div className="text-center text-gray-500 py-10">No matches scheduled for this gameweek.</div>
        )}
      </div>

    </div>
  );
}