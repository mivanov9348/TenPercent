import { useState, useEffect } from 'react';
import { Loader2 } from 'lucide-react';

export default function Scorers() {
  const [topScorers, setTopScorers] = useState<any[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchScorers = async () => {
      try {
        const response = await fetch('https://localhost:7135/api/dashboard/top-scorers');
        if (response.ok) {
          const data = await response.json();
          setTopScorers(data);
        }
      } catch (error) {
        console.error("Failed to fetch top scorers:", error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchScorers();
  }, []);

  if (isLoading) {
    return <div className="flex justify-center items-center h-64"><Loader2 className="animate-spin text-yellow-500" size={40} /></div>;
  }

  if (topScorers.length === 0) {
    return <div className="text-center text-gray-500 mt-10">No goals scored yet in the current season. Simulate some matches!</div>;
  }

  return (
    <div className="bg-gray-800 border border-gray-700 rounded-xl shadow-lg p-6 max-w-4xl mx-auto">
      <h2 className="text-xl font-bold text-white mb-6">Global Golden Boot Race</h2>
      
      <div className="space-y-2">
        {topScorers.map((scorer) => (
          <div key={scorer.id} className="flex items-center justify-between p-4 bg-gray-900 border border-gray-700 rounded-lg hover:border-gray-500 transition-colors">
            <div className="flex items-center gap-6">
              <span className={`text-2xl font-black w-8 text-center ${scorer.rank === 1 ? 'text-yellow-500' : scorer.rank === 2 ? 'text-gray-300' : scorer.rank === 3 ? 'text-amber-600' : 'text-gray-600'}`}>
                {scorer.rank}
              </span>
              <div>
                <p className="text-lg font-bold text-white">
                  {scorer.name}
                  {/* По-късно тук можем да проверяваме дали е твой клиент: */}
                  {/* scorer.agencyId === myAgencyId && <span className="ml-2 text-[10px] bg-yellow-500/20 text-yellow-500 px-2 py-0.5 rounded uppercase">Your Client</span> */}
                </p>
                <p className="text-sm text-gray-400">{scorer.team}</p>
              </div>
            </div>
            
            <div className="flex items-center gap-8 text-center">
              <div>
                <p className="text-xs text-gray-500 uppercase">Matches</p>
                <p className="text-lg font-bold text-gray-300">{scorer.matches}</p>
              </div>
              <div>
                <p className="text-xs text-gray-500 uppercase">Goals</p>
                <p className="text-2xl font-black text-emerald-400">{scorer.goals}</p>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}