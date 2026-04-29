export default function Scorers() {
  const topScorers = [
    { rank: 1, name: "Erling Nordic", team: "Manchester Blue", goals: 14, matches: 13 },
    { rank: 2, name: "Marcus Rashford", team: "Manchester Red", goals: 11, matches: 14 },
    { rank: 3, name: "Bukayo Star", team: "London Cannons", goals: 9, matches: 14 },
    { rank: 4, name: "Mo Pharaoh", team: "Liverpool Reds", goals: 8, matches: 12 },
    { rank: 5, name: "Ollie Watkins", team: "Aston Lions", goals: 7, matches: 14 },
  ];

  return (
    <div className="bg-gray-800 border border-gray-700 rounded-xl shadow-lg p-6 max-w-4xl">
      <h2 className="text-xl font-bold text-white mb-6">Global Golden Boot Race</h2>
      
      <div className="space-y-2">
        {topScorers.map((scorer) => (
          <div key={scorer.rank} className="flex items-center justify-between p-4 bg-gray-900 border border-gray-700 rounded-lg hover:border-gray-500 transition-colors">
            <div className="flex items-center gap-6">
              <span className={`text-2xl font-black w-8 text-center ${scorer.rank === 1 ? 'text-yellow-500' : scorer.rank === 2 ? 'text-gray-300' : scorer.rank === 3 ? 'text-amber-600' : 'text-gray-600'}`}>
                {scorer.rank}
              </span>
              <div>
                <p className={`text-lg font-bold ${scorer.name === 'Marcus Rashford' ? 'text-yellow-500' : 'text-white'}`}>
                  {scorer.name}
                  {scorer.name === 'Marcus Rashford' && <span className="ml-2 text-[10px] bg-yellow-500/20 text-yellow-500 px-2 py-0.5 rounded uppercase">Your Client</span>}
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