import { useState, useEffect } from 'react';
import { Calendar, Star, AlertTriangle, TrendingUp, Clock, CalendarDays, Loader2, Info, Trophy, Swords, User, CheckCircle2 } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

export default function Home() {
  const navigate = useNavigate();
  const [timeLeft, setTimeLeft] = useState({ days: 0, hours: 0, minutes: 0, seconds: 0 });
  const [dashboardData, setDashboardData] = useState<any>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        const response = await fetch('https://localhost:7135/api/dashboard/home');
        if (response.ok) {
          const data = await response.json();
          setDashboardData(data);

          if (data.isInitialized && data.worldState?.nextMatchdayDate) {
            startTimer(new Date(data.worldState.nextMatchdayDate));
          }
        }
      } catch (error) {
        console.error("Failed to fetch dashboard data:", error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchDashboardData();
  }, []);

  const startTimer = (targetDate: Date) => {
    const interval = setInterval(() => {
      const now = new Date().getTime();
      const difference = targetDate.getTime() - now;

      if (difference > 0) {
        setTimeLeft({
          days: Math.floor(difference / (1000 * 60 * 60 * 24)),
          hours: Math.floor((difference % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60)),
          minutes: Math.floor((difference % (1000 * 60 * 60)) / (1000 * 60)),
          seconds: Math.floor((difference % (1000 * 60)) / 1000),
        });
      } else {
        clearInterval(interval);
      }
    }, 1000);
    return () => clearInterval(interval);
  };

  const formatTime = (time: number) => time.toString().padStart(2, '0');

  if (isLoading) {
    return <div className="flex justify-center items-center h-64"><Loader2 className="animate-spin text-yellow-500" size={32} /></div>;
  }

  if (dashboardData && !dashboardData.isInitialized) {
    return (
      <div className="flex flex-col items-center justify-center h-[60vh] text-center space-y-4">
        <div className="w-20 h-20 bg-gray-800 rounded-full flex items-center justify-center border border-gray-700">
          <Info size={40} className="text-gray-500" />
        </div>
        <h2 className="text-2xl font-bold text-white">World Engine is Offline</h2>
        <p className="text-gray-400 max-w-md">The simulation has not been initialized yet. Please contact the administrator to start the world engine.</p>
      </div>
    );
  }

  const { worldState, upcomingMatches, previousMatches, topPlayers, clientMatches, clientReports } = dashboardData;

  return (
    <div className="space-y-6 pb-12">

      {/* 1. Хедър и Отброяване */}
      <div className="bg-gray-800 border border-gray-700 p-6 rounded-2xl flex flex-col xl:flex-row justify-between items-center shadow-lg gap-6 relative overflow-hidden">
        {!worldState?.isSeasonActive && (
          <div className="absolute inset-0 bg-gray-900/80 backdrop-blur-sm flex items-center justify-center z-10">
            <p className="text-yellow-500 font-bold tracking-widest uppercase flex items-center gap-2">
              <Info size={18} /> No Active Season
            </p>
          </div>
        )}

        <div className="flex items-center gap-4 w-full xl:w-auto z-0">
          <div className="w-16 h-16 bg-gray-900 border border-gray-700 rounded-xl flex flex-col items-center justify-center shadow-inner">
            <span className="text-xs text-gray-500 font-bold uppercase">Week</span>
            <span className="text-2xl font-black text-yellow-500">{worldState?.currentGameweek || 0}</span>
          </div>
          <div>
            <h1 className="text-2xl font-black text-white">Season {worldState?.seasonNumber || 0}</h1>
            <p className="text-gray-400 text-sm flex items-center gap-1 mt-1 font-medium">
              <CalendarDays size={14} className="text-blue-400" />
              Gameweek {worldState?.currentGameweek || 0} of {worldState?.totalGameweeks || 0}
            </p>
          </div>
        </div>

        {/* Светещ Таймер */}
        <div className="bg-gray-900 border border-yellow-500/30 p-4 rounded-xl flex flex-col md:flex-row items-center gap-6 shadow-[0_0_20px_rgba(234,179,8,0.05)] w-full xl:w-auto z-0">
          <div className="flex items-center gap-2 text-gray-400 font-bold uppercase text-xs tracking-wider">
            <Clock size={16} className="text-yellow-500" />
            Next Matchday:
          </div>
          <div className="flex items-center gap-3">
            <div className="flex flex-col items-center min-w-[50px]"><span className="text-3xl font-mono font-black text-white">{formatTime(timeLeft.days)}</span><span className="text-[10px] text-gray-500 uppercase font-bold tracking-widest mt-1">Days</span></div>
            <span className="text-2xl text-gray-700 pb-4 animate-pulse">:</span>
            <div className="flex flex-col items-center min-w-[50px]"><span className="text-3xl font-mono font-black text-white">{formatTime(timeLeft.hours)}</span><span className="text-[10px] text-gray-500 uppercase font-bold tracking-widest mt-1">Hrs</span></div>
            <span className="text-2xl text-gray-700 pb-4 animate-pulse">:</span>
            <div className="flex flex-col items-center min-w-[50px]"><span className="text-3xl font-mono font-black text-white">{formatTime(timeLeft.minutes)}</span><span className="text-[10px] text-gray-500 uppercase font-bold tracking-widest mt-1">Min</span></div>
            <span className="text-2xl text-gray-700 pb-4 animate-pulse">:</span>
            <div className="flex flex-col items-center min-w-[50px]"><span className="text-3xl font-mono font-black text-yellow-500 drop-shadow-[0_0_8px_rgba(234,179,8,0.5)]">{formatTime(timeLeft.seconds)}</span><span className="text-[10px] text-yellow-500/70 uppercase font-bold tracking-widest mt-1">Sec</span></div>
          </div>
        </div>
      </div>

      {/* Грид за Агенцията */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-gray-800 border border-gray-700 rounded-xl p-6 shadow-lg flex flex-col h-[350px]">
          <h2 className="text-xl font-bold text-white mb-4 flex items-center gap-2 shrink-0"><Calendar className="text-blue-400" /> My Client Matches</h2>
          <div className="space-y-3 flex-1 overflow-y-auto pr-2 custom-scrollbar">
            {clientMatches.length > 0 ? clientMatches.map((match: any) => (<div key={match.id}></div>)) : (
              <div className="h-full flex flex-col items-center justify-center text-gray-500 opacity-50 bg-gray-900/50 rounded-xl border border-gray-800/50 border-dashed"><Calendar size={48} className="mb-3 opacity-50" /><p className="font-bold">No upcoming client matches.</p></div>
            )}
          </div>
        </div>

        <div className="bg-gray-800 border border-gray-700 rounded-xl p-6 shadow-lg flex flex-col h-[350px]">
          <h2 className="text-xl font-bold text-white mb-4 flex items-center gap-2 shrink-0"><TrendingUp className="text-emerald-400" /> Client Reports (Last Round)</h2>
          <div className="space-y-3 flex-1 overflow-y-auto pr-2 custom-scrollbar">
            {clientReports.length > 0 ? clientReports.map((report: any) => (<div key={report.id}></div>)) : (
              <div className="h-full flex flex-col items-center justify-center text-gray-500 opacity-50 bg-gray-900/50 rounded-xl border border-gray-800/50 border-dashed"><TrendingUp size={48} className="mb-3 opacity-50" /><p className="font-bold">No reports available.</p></div>
            )}
          </div>
        </div>
      </div>

      {/* Грид за Глобалния Свят (3 колони на големи екрани) */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">

        {/* Предстоящи мачове */}
        <div className="bg-gray-800 border border-gray-700 rounded-xl p-6 shadow-lg flex flex-col h-[400px]">
          <h2 className="text-xl font-bold text-white mb-4 flex items-center gap-2 shrink-0">
            <Swords className="text-purple-400" /> Global Matchday {worldState?.currentGameweek}
          </h2>
          <div className="space-y-3 flex-1 overflow-y-auto pr-2 custom-scrollbar">
            {upcomingMatches.length > 0 ? (
              upcomingMatches.map((match: any) => (
                <div key={match.id} className="bg-gray-900 border border-gray-700 p-3 rounded-lg flex flex-col justify-center hover:border-gray-600 transition-colors">
                  <div className="flex justify-between items-center text-white font-bold text-sm">
                    <span className="truncate">{match.homeTeam}</span>
                    <span className="text-gray-600 mx-2 text-xs">VS</span>
                    <span className="truncate flex-1 text-right">{match.awayTeam}</span>
                  </div>
                  <div className="flex justify-between items-center mt-2">
                    <span className="text-[10px] text-gray-500 uppercase tracking-wider">{match.league}</span>
                    <span className="text-[10px] bg-purple-500/10 text-purple-400 px-2 py-0.5 rounded font-bold">
                      {new Date(match.date).toLocaleDateString()}
                    </span>
                  </div>
                </div>
              ))
            ) : (
              <div className="h-full flex flex-col items-center justify-center text-gray-500 opacity-50 bg-gray-900/50 rounded-xl border border-gray-800/50 border-dashed">
                <CalendarDays size={48} className="mb-3 opacity-50" />
                <p className="font-bold">No global matches scheduled.</p>
              </div>
            )}
          </div>
        </div>

        {/* НОВО: Минали мачове */}
        <div className="bg-gray-800 border border-gray-700 rounded-xl p-6 shadow-lg flex flex-col h-[400px]">
          <h2 className="text-xl font-bold text-white mb-4 flex items-center gap-2 shrink-0">
            <CheckCircle2 className="text-emerald-500" /> Last Matchday Results
          </h2>
          <div className="space-y-3 flex-1 overflow-y-auto pr-2 custom-scrollbar">
            {previousMatches && previousMatches.length > 0 ? (
              previousMatches.map((match: any) => (
                <div key={match.id} className="bg-gray-900 border border-gray-700 p-3 rounded-lg flex flex-col justify-center hover:border-gray-600 transition-colors">
                  <div className="flex justify-between items-center text-white font-bold text-sm">
                    <span className="truncate">{match.homeTeam}</span>
                    <span className="text-yellow-500 mx-2 bg-gray-800 px-2 rounded border border-gray-700">
                      {match.homeGoals} - {match.awayGoals}
                    </span>
                    <span className="truncate flex-1 text-right">{match.awayTeam}</span>
                  </div>
                  <div className="flex justify-between items-center mt-2">
                    <span className="text-[10px] text-gray-500 uppercase tracking-wider">{match.league}</span>
                    <span className="text-[10px] bg-emerald-500/10 text-emerald-400 px-2 py-0.5 rounded font-bold">
                      FT
                    </span>
                  </div>
                </div>
              ))
            ) : (
              <div className="h-full flex flex-col items-center justify-center text-gray-500 opacity-50 bg-gray-900/50 rounded-xl border border-gray-800/50 border-dashed">
                <AlertTriangle size={48} className="mb-3 opacity-50" />
                <p className="font-bold">No previous results.</p>
              </div>
            )}
          </div>
        </div>

        {/* Топ Играчи */}
        <div className="bg-gray-800 border border-gray-700 rounded-xl p-6 shadow-lg flex flex-col h-[400px]">
          <h2 className="text-xl font-bold text-white mb-4 flex items-center gap-2 shrink-0">
            <Trophy className="text-yellow-500" /> World Top Performers
          </h2>
          <div className="space-y-3 flex-1 overflow-y-auto pr-2 custom-scrollbar">
            {topPlayers.length > 0 ? (
              topPlayers.map((player: any, index: number) => (
                <div key={player.id} className="bg-gray-900 border border-gray-700 p-3 rounded-lg flex items-center justify-between hover:border-gray-600 transition-colors cursor-pointer group" onClick={() => navigate(`/world/player/${player.id}`)}>
                  <div className="flex items-center gap-3">
                    <div className={`w-6 h-6 rounded-full flex items-center justify-center text-xs font-black shrink-0 ${index === 0 ? 'bg-yellow-500 text-black' : index === 1 ? 'bg-gray-300 text-black' : index === 2 ? 'bg-orange-700 text-white' : 'bg-gray-800 text-gray-500'}`}>
                      {index + 1}
                    </div>
                    <div className="min-w-0">
                      <p className="font-bold text-white group-hover:text-yellow-500 transition-colors truncate">{player.name}</p>
                      <p className="text-[10px] text-gray-400 truncate">{player.club} • {player.position}</p>                        </div>
                  </div>
                  <div className="text-right shrink-0">
                    <span className="bg-gray-800 text-white px-2 py-1 rounded font-black text-sm">{player.ovr}</span>
                  </div>
                </div>
              ))
            ) : (
              <div className="h-full flex flex-col items-center justify-center text-gray-500 opacity-50 bg-gray-900/50 rounded-xl border border-gray-800/50 border-dashed">
                <User size={48} className="mb-3 opacity-50" />
                <p className="font-bold">No players found.</p>
              </div>
            )}
          </div>
        </div>

      </div>
    </div>
  );
}