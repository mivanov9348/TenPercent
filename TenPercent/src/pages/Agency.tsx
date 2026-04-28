import { Building, Users, Wallet, TrendingUp, Star, Trophy, Target } from 'lucide-react';

export default function Agency() {
  // Фейк данни, които по-късно ще идват от C# бекенда
  const agencyInfo = {
    name: "Elite Sports Group",
    agentName: "Agent Smith",
    level: 3,
    reputation: 74, // От 100
    established: "2026",
  };

  const quickStats = [
    { id: 1, title: "Total Players", value: "14", icon: <Users size={24} />, color: "text-blue-500" },
    { id: 2, title: "Active Contracts", value: "11", icon: <Building size={24} />, color: "text-purple-500" },
    { id: 3, title: "Weekly Income", value: "+$45,000", icon: <TrendingUp size={24} />, color: "text-emerald-500" },
    { id: 4, title: "Total Revenue", value: "$1,250,000", icon: <Wallet size={24} />, color: "text-yellow-500" },
  ];

  const facilities = [
    { id: 1, name: "Scouting Network", level: 2, maxLevel: 5, description: "Подобрява качеството на генерираните играчи на пазара." },
    { id: 2, name: "Legal Department", level: 4, maxLevel: 5, description: "Увеличава твоя процент (%) при преговори за договори." },
    { id: 3, name: "PR & Media", level: 1, maxLevel: 5, description: "Вдига репутацията на агенцията по-бързо след успешен мач." },
  ];

  return (
    <div className="space-y-8">
      
      {/* 1. Хедър на агенцията */}
      <div className="bg-gray-800 border border-gray-700 rounded-2xl p-6 flex items-center justify-between shadow-lg relative overflow-hidden">
        {/* Декоративен бекграунд елемент */}
        <div className="absolute -right-10 -top-10 text-gray-700/20">
          <Building size={200} />
        </div>

        <div className="relative z-10 flex items-center gap-6">
          <div className="w-24 h-24 bg-gray-900 border-2 border-yellow-500 rounded-xl flex items-center justify-center text-yellow-500 shadow-[0_0_15px_rgba(234,179,8,0.2)]">
            <Trophy size={48} />
          </div>
          <div>
            <div className="flex items-center gap-3 mb-1">
              <h1 className="text-3xl font-black text-white uppercase tracking-wider">{agencyInfo.name}</h1>
              <span className="bg-yellow-500 text-black text-xs font-bold px-2 py-1 rounded">LEVEL {agencyInfo.level}</span>
            </div>
            <p className="text-gray-400 text-lg">CEO: <span className="text-gray-200">{agencyInfo.agentName}</span></p>
            <div className="flex items-center gap-2 mt-2 text-sm">
              <span className="text-gray-500">Reputation:</span>
              <div className="w-32 h-2 bg-gray-900 rounded-full overflow-hidden">
                <div 
                  className="h-full bg-gradient-to-r from-yellow-600 to-yellow-400" 
                  style={{ width: `${agencyInfo.reputation}%` }}
                ></div>
              </div>
              <span className="text-yellow-500 font-bold">{agencyInfo.reputation}/100</span>
            </div>
          </div>
        </div>
      </div>

      {/* 2. Бързи статистики (4 колони) */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {quickStats.map((stat) => (
          <div key={stat.id} className="bg-gray-800 border border-gray-700 rounded-xl p-5 flex items-center gap-4 hover:border-gray-500 transition-colors">
            <div className={`p-3 bg-gray-900 rounded-lg ${stat.color}`}>
              {stat.icon}
            </div>
            <div>
              <p className="text-sm text-gray-500 font-medium">{stat.title}</p>
              <p className="text-xl font-bold text-white">{stat.value}</p>
            </div>
          </div>
        ))}
      </div>

      {/* 3. Инфраструктура / Ъпгрейди */}
      <div>
        <h2 className="text-xl font-bold text-white mb-4 flex items-center gap-2">
          <Target className="text-yellow-500" />
          Agency Infrastructure
        </h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {facilities.map((facility) => (
            <div key={facility.id} className="bg-gray-800 border border-gray-700 rounded-xl p-5 flex flex-col">
              <div className="flex justify-between items-start mb-2">
                <h3 className="text-lg font-bold text-white">{facility.name}</h3>
                <span className="text-xs font-mono bg-gray-900 text-gray-400 px-2 py-1 rounded border border-gray-700">
                  Lvl {facility.level}/{facility.maxLevel}
                </span>
              </div>
              <p className="text-gray-400 text-sm mb-6 flex-1">{facility.description}</p>
              
              {/* Бутон за ъпгрейд (заключен ако е на макс) */}
              <button 
                className={`w-full py-2 rounded-lg font-bold text-sm transition-colors ${
                  facility.level === facility.maxLevel 
                    ? 'bg-gray-900 text-gray-600 cursor-not-allowed' 
                    : 'bg-gray-700 text-white hover:bg-yellow-500 hover:text-black'
                }`}
                disabled={facility.level === facility.maxLevel}
              >
                {facility.level === facility.maxLevel ? 'MAX LEVEL' : 'UPGRADE ($50,000)'}
              </button>
            </div>
          ))}
        </div>
      </div>

    </div>
  );
}