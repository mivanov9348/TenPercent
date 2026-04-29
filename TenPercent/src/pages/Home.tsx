import { useState } from 'react';
import { Calendar, Play, Star, AlertTriangle, TrendingUp, Loader2, CheckCircle2 } from 'lucide-react';

export default function Home() {
  const [week, setWeek] = useState(14);
  const [isSimulating, setIsSimulating] = useState(false);
  const [liveResults, setLiveResults] = useState<{id: number, text: string, type: 'success' | 'warning' | 'info'}[]>([]);

  // ДАННИ
  const upcomingMatches = [
    { id: 1, player: "Marcus Rashford", club: "Manchester Utd", opponent: "Arsenal", type: "League", expectedRole: "Starter" },
    { id: 2, player: "Jude Bellingham", club: "Real Madrid", opponent: "Barcelona", type: "Derby", expectedRole: "Starter" },
    { id: 3, player: "Ivan Ivanov", club: "FC Sofia", opponent: "FC Varna", type: "League", expectedRole: "Bench" },
    { id: 4, player: "Alexandre Pato", club: "Orlando City", opponent: "LA Galaxy", type: "Cup", expectedRole: "Starter" },
  ];

  const weeklyReport = [
    { id: 1, type: "success" as const, text: "Jude Bellingham scored 2 goals and was MOTM (Rating: 9.2). Value is increasing!" },
    { id: 2, type: "warning" as const, text: "Ivan Ivanov received a yellow card and played poorly (Rating: 5.4)." },
    { id: 3, type: "info" as const, text: "Marcus Rashford played 90 minutes without scoring (Rating: 6.8)." }
  ];

  // ЛОГИКА ЗА СИМУЛАЦИЯТА
  const handleAdvanceWeek = () => {
    setIsSimulating(true);
    setLiveResults([]); // Изчистваме старите резултати

    // Фалшиви резултати, които ще излизат един по един
    const generatedResults = [
      { id: 1, type: 'info' as const, text: "Marcus Rashford's match finished 1-1. He played 75 mins. Rating: 7.1" },
      { id: 2, type: 'success' as const, text: "Jude Bellingham dominated the Derby! 1 Goal, 1 Assist. Rating: 8.9" },
      { id: 3, type: 'warning' as const, text: "Ivan Ivanov was subbed in at 80'. Didn't impact the game. Rating: 6.0" },
      { id: 4, type: 'success' as const, text: "Alexandre Pato scored a late winner in the Cup! Rating: 8.2" },
    ];

    // Показваме ги един по един през 1.5 секунди
    generatedResults.forEach((result, index) => {
      setTimeout(() => {
        setLiveResults(prev => [...prev, result]);
        
        // Ако това е последният мач, спираме симулацията след още малко време
        if (index === generatedResults.length - 1) {
          setTimeout(() => {
            setIsSimulating(false);
            setWeek(prev => prev + 1);
            // Тук в бъдеще ще извикваме бекенда за новата седмица
          }, 2000);
        }
      }, (index + 1) * 1500);
    });
  };

  return (
    <div className="space-y-6">
      
      {/* 1. Хедър и Контрол на времето */}
      <div className="bg-gray-800 border border-gray-700 p-6 rounded-2xl flex flex-col md:flex-row justify-between items-center shadow-lg relative overflow-hidden">
        <div className="flex items-center gap-4 z-10 mb-4 md:mb-0">
          <div className="w-16 h-16 bg-gray-900 border border-gray-700 rounded-xl flex flex-col items-center justify-center">
            <span className="text-xs text-gray-500 font-bold uppercase">Week</span>
            <span className="text-2xl font-black text-yellow-500">{week}</span>
          </div>
          <div>
            <h1 className="text-2xl font-black text-white">Season 2026/2027</h1>
            <p className="text-gray-400 text-sm">Next Matchday: Saturday</p>
          </div>
        </div>

        <button 
          onClick={handleAdvanceWeek}
          disabled={isSimulating}
          className={`z-10 group relative px-8 py-4 font-black text-lg rounded-xl transition-all flex items-center gap-3 transform ${
            isSimulating 
              ? 'bg-gray-700 text-gray-400 cursor-not-allowed' 
              : 'bg-yellow-500 text-black hover:bg-yellow-400 shadow-[0_0_20px_rgba(234,179,8,0.3)] hover:shadow-[0_0_30px_rgba(234,179,8,0.5)] hover:-translate-y-1'
          }`}
        >
          {isSimulating ? (
            <><Loader2 className="animate-spin" size={24} /> SIMULATING...</>
          ) : (
            <><Play className="fill-black" size={24} /> ADVANCE WEEK</>
          )}
        </button>
      </div>

      {/* ЕКРАН ЗА СИМУЛАЦИЯ (Показва се само докато тече анимацията) */}
      {isSimulating && (
        <div className="bg-gray-900 border border-yellow-500/50 rounded-xl p-6 shadow-[0_0_30px_rgba(234,179,8,0.1)] min-h-[400px]">
          <h2 className="text-xl font-bold text-yellow-500 mb-6 flex items-center gap-3 animate-pulse">
            <Activity className="animate-spin-slow" /> Matchday in Progress...
          </h2>
          <div className="space-y-4">
            {liveResults.map((res) => (
              <div key={res.id} className="p-4 bg-gray-800 border border-gray-700 rounded-lg flex items-center gap-4 animate-in fade-in slide-in-from-left-4 duration-500">
                {res.type === 'success' && <CheckCircle2 className="text-emerald-500" size={24} />}
                {res.type === 'warning' && <AlertTriangle className="text-red-500" size={24} />}
                {res.type === 'info' && <div className="w-4 h-4 rounded-full bg-blue-500 ml-1"></div>}
                <p className="text-lg text-gray-200">{res.text}</p>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* НОРМАЛЕН ИЗГЛЕД (Показва се, когато не се симулира) */}
      {!isSimulating && (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Предстоящи мачове */}
          <div className="bg-gray-800 border border-gray-700 rounded-xl p-6 shadow-lg">
            <h2 className="text-xl font-bold text-white mb-4 flex items-center gap-2">
              <Calendar className="text-blue-400" />
              Matches This Week
            </h2>
            <div className="space-y-3">
              {upcomingMatches.map((match) => (
                <div key={match.id} className="bg-gray-900 border border-gray-700 p-4 rounded-lg flex items-center justify-between">
                  <div>
                    <p className="font-bold text-yellow-500">{match.player}</p>
                    <p className="text-xs text-gray-400 mt-1">
                      {match.club} <span className="text-gray-600 mx-1">vs</span> {match.opponent}
                    </p>
                  </div>
                  <div className="text-right">
                    <span className={`text-xs font-bold px-2 py-1 rounded ${
                      match.expectedRole === 'Starter' ? 'bg-emerald-500/10 text-emerald-400' : 'bg-gray-700 text-gray-400'
                    }`}>
                      {match.expectedRole}
                    </span>
                    <p className="text-[10px] text-gray-500 uppercase mt-1">{match.type}</p>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Доклад от миналия кръг */}
          <div className="bg-gray-800 border border-gray-700 rounded-xl p-6 shadow-lg">
            <h2 className="text-xl font-bold text-white mb-4 flex items-center gap-2">
              <TrendingUp className="text-emerald-400" />
              Last Week Report
            </h2>
            <div className="space-y-3">
              {weeklyReport.map((report) => (
                <div key={report.id} className="flex gap-4 items-start p-3 bg-gray-900/50 rounded-lg">
                  <div className="mt-1 shrink-0">
                    {report.type === 'success' && <Star className="text-yellow-500 fill-yellow-500" size={18} />}
                    {report.type === 'warning' && <AlertTriangle className="text-red-500" size={18} />}
                    {report.type === 'info' && <div className="w-2 h-2 rounded-full bg-blue-500 mt-1"></div>}
                  </div>
                  <p className={`text-sm ${
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
      )}

    </div>
  );
}

// За да работи анимацията в Tailwind 4, трябва малък допълнителен компонент за Activity иконата
function Activity(props: any) {
  return <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" {...props}><path d="M22 12h-4l-3 9L9 3l-3 9H2"/></svg>;
}