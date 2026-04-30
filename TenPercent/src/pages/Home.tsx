import { useState, useEffect } from 'react';
import { Calendar, Star, AlertTriangle, TrendingUp, Clock, CalendarDays } from 'lucide-react';

export default function Home() {
  const [timeLeft, setTimeLeft] = useState({ days: 0, hours: 0, minutes: 0, seconds: 0 });

  // Логика за таймера (Отброява до следващия Вторник 15:00, но за UI целите просто върти брояч)
  useEffect(() => {
    // Създаваме примерна дата в бъдещето (напр. след 2 дни и 4 часа)
    const targetDate = new Date();
    targetDate.setDate(targetDate.getDate() + 2);
    targetDate.setHours(targetDate.getHours() + 4);

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
  }, []);

  // ДАННИ (Ще се дърпат от бекенда)
  const upcomingMatches = [
    { id: 1, player: "Marcus Rashford", club: "Man Red", opponent: "London Cannons", type: "League", expectedRole: "Starter" },
    { id: 2, player: "Jude Bellingham", club: "Madrid White", opponent: "Catalonia Red-Blue", type: "Derby", expectedRole: "Starter" },
    { id: 3, player: "Ivan Ivanov", club: "Sofia Blue", opponent: "Sofia Red", type: "League", expectedRole: "Bench" },
  ];

  const weeklyReport = [
    { id: 1, type: "success" as const, text: "Jude Bellingham scored 2 goals and was MOTM (Rating: 9.2). Value is increasing!" },
    { id: 2, type: "warning" as const, text: "Ivan Ivanov received a yellow card and played poorly (Rating: 5.4)." },
    { id: 3, type: "info" as const, text: "Marcus Rashford played 90 minutes without scoring (Rating: 6.8)." }
  ];

  // Помощна функция за добавяне на водеща нула (напр. 05 вместо 5)
  const formatTime = (time: number) => time.toString().padStart(2, '0');

  return (
    <div className="space-y-6">
      
      {/* 1. Хедър и Отброяване (Countdown) */}
      <div className="bg-gray-800 border border-gray-700 p-6 rounded-2xl flex flex-col xl:flex-row justify-between items-center shadow-lg gap-6">
        
        {/* Инфо за сезона */}
        <div className="flex items-center gap-4 w-full xl:w-auto">
          <div className="w-16 h-16 bg-gray-900 border border-gray-700 rounded-xl flex flex-col items-center justify-center">
            <span className="text-xs text-gray-500 font-bold uppercase">Week</span>
            <span className="text-2xl font-black text-yellow-500">14</span>
          </div>
          <div>
            <h1 className="text-2xl font-black text-white">Season 2026/2027</h1>
            <p className="text-gray-400 text-sm flex items-center gap-1 mt-1">
              <CalendarDays size={14} /> Global Matchday in progress
            </p>
          </div>
        </div>

        {/* Светещ Таймер */}
        <div className="bg-gray-900 border border-yellow-500/30 p-4 rounded-xl flex flex-col md:flex-row items-center gap-6 shadow-[0_0_20px_rgba(234,179,8,0.1)] w-full xl:w-auto">
          <div className="flex items-center gap-2 text-gray-400 font-bold uppercase text-xs tracking-wider">
            <Clock size={16} className="text-yellow-500" />
            Next Matchday:
          </div>
          
          <div className="flex items-center gap-3">
            {/* Дни */}
            <div className="flex flex-col items-center min-w-[50px]">
              <span className="text-3xl font-mono font-black text-white">{formatTime(timeLeft.days)}</span>
              <span className="text-[10px] text-gray-500 uppercase font-bold tracking-widest mt-1">Days</span>
            </div>
            <span className="text-2xl text-gray-700 pb-4 animate-pulse">:</span>
            
            {/* Часове */}
            <div className="flex flex-col items-center min-w-[50px]">
              <span className="text-3xl font-mono font-black text-white">{formatTime(timeLeft.hours)}</span>
              <span className="text-[10px] text-gray-500 uppercase font-bold tracking-widest mt-1">Hrs</span>
            </div>
            <span className="text-2xl text-gray-700 pb-4 animate-pulse">:</span>
            
            {/* Минути */}
            <div className="flex flex-col items-center min-w-[50px]">
              <span className="text-3xl font-mono font-black text-white">{formatTime(timeLeft.minutes)}</span>
              <span className="text-[10px] text-gray-500 uppercase font-bold tracking-widest mt-1">Min</span>
            </div>
            <span className="text-2xl text-gray-700 pb-4 animate-pulse">:</span>
            
            {/* Секунди (Светещи) */}
            <div className="flex flex-col items-center min-w-[50px]">
              <span className="text-3xl font-mono font-black text-yellow-500 drop-shadow-[0_0_8px_rgba(234,179,8,0.5)]">
                {formatTime(timeLeft.seconds)}
              </span>
              <span className="text-[10px] text-yellow-500/70 uppercase font-bold tracking-widest mt-1">Sec</span>
            </div>
          </div>
        </div>

      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        
        {/* 2. Предстоящи мачове */}
        <div className="bg-gray-800 border border-gray-700 rounded-xl p-6 shadow-lg flex flex-col">
          <h2 className="text-xl font-bold text-white mb-4 flex items-center gap-2">
            <Calendar className="text-blue-400" />
            Upcoming Client Matches
          </h2>
          <div className="space-y-3 flex-1">
            {upcomingMatches.map((match) => (
              <div key={match.id} className="bg-gray-900 border border-gray-700 p-4 rounded-lg flex items-center justify-between hover:border-gray-600 transition-colors">
                <div>
                  <p className="font-bold text-yellow-500">{match.player}</p>
                  <p className="text-sm text-gray-300 mt-1">
                    {match.club} <span className="text-gray-600 mx-1 font-mono text-xs">VS</span> {match.opponent}
                  </p>
                </div>
                <div className="text-right">
                  <span className={`text-xs font-bold px-2 py-1 rounded ${
                    match.expectedRole === 'Starter' ? 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20' : 'bg-gray-800 text-gray-400 border border-gray-700'
                  }`}>
                    {match.expectedRole}
                  </span>
                  <p className="text-[10px] text-gray-500 uppercase font-bold mt-2 tracking-wider">{match.type}</p>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* 3. Доклад от миналия кръг */}
        <div className="bg-gray-800 border border-gray-700 rounded-xl p-6 shadow-lg flex flex-col">
          <h2 className="text-xl font-bold text-white mb-4 flex items-center gap-2">
            <TrendingUp className="text-emerald-400" />
            Last Round Report
          </h2>
          <div className="space-y-3 flex-1">
            {weeklyReport.map((report) => (
              <div key={report.id} className="flex gap-4 items-start p-4 bg-gray-900/50 border border-gray-800 rounded-lg">
                <div className="mt-1 shrink-0 bg-gray-800 p-2 rounded-lg">
                  {report.type === 'success' && <Star className="text-yellow-500 fill-yellow-500" size={16} />}
                  {report.type === 'warning' && <AlertTriangle className="text-red-500" size={16} />}
                  {report.type === 'info' && <div className="w-4 h-4 rounded-full border-2 border-blue-500 flex items-center justify-center"><div className="w-1.5 h-1.5 bg-blue-500 rounded-full"></div></div>}
                </div>
                <p className={`text-sm leading-relaxed ${
                  report.type === 'success' ? 'text-gray-200' : 
                  report.type === 'warning' ? 'text-red-200' : 'text-gray-400'
                }`}>
                  {report.text}
                </p>
              </div>
            ))}
          </div>
        </div>

      </div>
    </div>
  );
}