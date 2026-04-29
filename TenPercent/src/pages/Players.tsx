import { Users, DollarSign, TrendingUp, AlertCircle, MoreHorizontal } from 'lucide-react';

export default function Players() {
  // Фейк база данни с твоите играчи
  const myPlayers = [
    { id: 1, name: "Marcus Rashford", pos: "ST", age: 24, skill: 84, potential: 88, value: "$45M", wage: "$120K", contract: 3, form: "Good" },
    { id: 2, name: "Jude Bellingham", pos: "MID", age: 21, skill: 86, potential: 94, value: "$80M", wage: "$150K", contract: 4, form: "Excellent" },
    { id: 3, name: "Ivan Ivanov", pos: "DEF", age: 19, skill: 68, potential: 82, value: "$2.5M", wage: "$8K", contract: 1, form: "Average" },
    { id: 4, name: "Alexandre Pato", pos: "ST", age: 32, skill: 76, potential: 76, value: "$5M", wage: "$45K", contract: 0, form: "Poor" },
    { id: 5, name: "Donnarumma", pos: "GK", age: 25, skill: 85, potential: 89, value: "$55M", wage: "$110K", contract: 2, form: "Good" },
  ];

  // Бързи сметки за статистиката отгоре
  const totalWage = myPlayers.reduce((acc, p) => acc + parseInt(p.wage.replace(/[^0-9]/g, '')), 0);
  const avgSkill = Math.round(myPlayers.reduce((acc, p) => acc + p.skill, 0) / myPlayers.length);

  return (
    <div className="space-y-6">
      
      {/* 1. Хедър и бързи статистики */}
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-3xl font-black text-white uppercase tracking-wider">My Roster</h1>
          <p className="text-gray-400 mt-1">Управлявай своите клиенти и техните договори</p>
        </div>
        
        <div className="flex gap-4">
          <div className="bg-gray-800 border border-gray-700 px-4 py-2 rounded-lg flex items-center gap-3">
            <Users className="text-blue-500" size={20} />
            <div>
              <p className="text-xs text-gray-400">Total Players</p>
              <p className="text-lg font-bold text-white">{myPlayers.length}</p>
            </div>
          </div>
          <div className="bg-gray-800 border border-gray-700 px-4 py-2 rounded-lg flex items-center gap-3">
            <DollarSign className="text-red-400" size={20} />
            <div>
              <p className="text-xs text-gray-400">Weekly Wage Bill</p>
              <p className="text-lg font-bold text-white">${totalWage}K</p>
            </div>
          </div>
          <div className="bg-gray-800 border border-gray-700 px-4 py-2 rounded-lg flex items-center gap-3">
            <TrendingUp className="text-emerald-500" size={20} />
            <div>
              <p className="text-xs text-gray-400">Average Skill</p>
              <p className="text-lg font-bold text-white">{avgSkill} OVR</p>
            </div>
          </div>
        </div>
      </div>

      {/* 2. Таблица с играчите */}
      <div className="bg-gray-800 border border-gray-700 rounded-xl overflow-hidden shadow-lg">
        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-gray-900 border-b border-gray-700 text-xs uppercase tracking-wider text-gray-400">
                <th className="p-4 font-medium">Player Name</th>
                <th className="p-4 font-medium">Pos</th>
                <th className="p-4 font-medium text-center">Age</th>
                <th className="p-4 font-medium text-center">OVR</th>
                <th className="p-4 font-medium text-center">POT</th>
                <th className="p-4 font-medium">Market Value</th>
                <th className="p-4 font-medium">Wage</th>
                <th className="p-4 font-medium text-center">Contract</th>
                <th className="p-4 font-medium text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-700">
              {myPlayers.map((player) => (
                <tr key={player.id} className="hover:bg-gray-750 transition-colors group">
                  {/* Име и Форма */}
                  <td className="p-4">
                    <div className="flex items-center gap-3">
                      <div className="w-8 h-8 rounded-full bg-gray-700 flex items-center justify-center text-xs font-bold text-gray-300">
                        {player.name.charAt(0)}
                      </div>
                      <div>
                        <p className="font-bold text-white group-hover:text-yellow-500 transition-colors">{player.name}</p>
                        <p className="text-xs text-gray-500">Form: {player.form}</p>
                      </div>
                    </div>
                  </td>
                  
                  {/* Позиция */}
                  <td className="p-4">
                    <span className={`text-xs font-bold px-2 py-1 rounded ${
                      player.pos === 'ST' ? 'bg-blue-500/10 text-blue-400' :
                      player.pos === 'MID' ? 'bg-emerald-500/10 text-emerald-400' :
                      player.pos === 'DEF' ? 'bg-yellow-500/10 text-yellow-400' :
                      'bg-purple-500/10 text-purple-400'
                    }`}>
                      {player.pos}
                    </span>
                  </td>
                  
                  {/* Години */}
                  <td className="p-4 text-center text-gray-300">{player.age}</td>
                  
                  {/* Умение (Skill) */}
                  <td className="p-4 text-center">
                    <span className="font-bold text-white">{player.skill}</span>
                  </td>
                  
                  {/* Потенциал */}
                  <td className="p-4 text-center">
                    <span className="font-bold text-emerald-400">{player.potential}</span>
                  </td>
                  
                  {/* Стойност */}
                  <td className="p-4 font-mono text-gray-300">{player.value}</td>
                  
                  {/* Заплата */}
                  <td className="p-4 font-mono text-gray-300">{player.wage}</td>
                  
                  {/* Договор с проверка за изтичащ такъв */}
                  <td className="p-4 text-center">
                    {player.contract === 0 ? (
                      <span className="inline-flex items-center gap-1 text-xs font-bold bg-red-500/10 text-red-500 px-2 py-1 rounded">
                        <AlertCircle size={12} /> EXPIRING
                      </span>
                    ) : player.contract === 1 ? (
                      <span className="text-yellow-500 font-bold">{player.contract} yr</span>
                    ) : (
                      <span className="text-gray-400">{player.contract} yrs</span>
                    )}
                  </td>
                  
                  {/* Действия (Actions) */}
                  <td className="p-4 text-right">
                    <button className="text-gray-400 hover:text-white p-2 rounded hover:bg-gray-700 transition-colors">
                      <MoreHorizontal size={20} />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}