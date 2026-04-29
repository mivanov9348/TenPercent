import { useState } from 'react';
import { Search, Filter, RefreshCw, UserPlus, Star, ShieldAlert } from 'lucide-react';

export default function Market() {
  const [searchTerm, setSearchTerm] = useState('');

  // Фейк данни за играчи на пазара (търсещи агент)
  const availablePlayers = [
    { id: 1, name: "Diego Silva", pos: "ST", age: 17, skill: 62, potential: 89, value: "$1,200,000", type: "Wonderkid" },
    { id: 2, name: "Thomas Müller", pos: "MID", age: 34, skill: 82, potential: 82, value: "$4,500,000", type: "Veteran" },
    { id: 3, name: "Liam O'Connor", pos: "DEF", age: 21, skill: 71, potential: 84, value: "$2,800,000", type: "First Team" },
    { id: 4, name: "Kaito Nakamura", pos: "MID", age: 19, skill: 68, potential: 86, value: "$1,900,000", type: "Prospect" },
    { id: 5, name: "Jean-Pierre", pos: "GK", age: 26, skill: 77, potential: 80, value: "$6,000,000", type: "First Team" },
    { id: 6, name: "Ali Hassan", pos: "ST", age: 23, skill: 74, potential: 79, value: "$3,200,000", type: "First Team" },
  ];

  // Помощна функция за цветовете на позициите
  const getPosColor = (pos: string) => {
    switch (pos) {
      case 'ST': return 'bg-blue-500/10 text-blue-400 border-blue-500/20';
      case 'MID': return 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20';
      case 'DEF': return 'bg-yellow-500/10 text-yellow-400 border-yellow-500/20';
      case 'GK': return 'bg-purple-500/10 text-purple-400 border-purple-500/20';
      default: return 'bg-gray-500/10 text-gray-400 border-gray-500/20';
    }
  };

  return (
    <div className="space-y-6">
      
      {/* 1. Хедър на пазара */}
      <div className="flex flex-col md:flex-row justify-between items-start md:items-end gap-4">
        <div>
          <h1 className="text-3xl font-black text-white uppercase tracking-wider">Scouting Network</h1>
          <p className="text-gray-400 mt-1">Find and sign unrepresented talents to your agency.</p>
        </div>
        
        <button className="flex items-center gap-2 bg-gray-800 hover:bg-gray-700 border border-gray-700 text-white px-4 py-2 rounded-lg transition-colors text-sm font-bold">
          <RefreshCw size={16} className="text-yellow-500" />
          REFRESH MARKET ($5,000)
        </button>
      </div>

      {/* 2. Търсачка и Филтри */}
      <div className="bg-gray-800 border border-gray-700 rounded-xl p-4 flex flex-col md:flex-row gap-4 shadow-lg">
        <div className="flex-1 relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500" size={20} />
          <input 
            type="text" 
            placeholder="Search by player name..." 
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full bg-gray-900 border border-gray-700 rounded-lg py-2.5 pl-10 pr-4 text-white focus:outline-none focus:border-yellow-500 transition-colors"
          />
        </div>
        
        <div className="flex gap-4">
          <div className="relative">
            <Filter className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500" size={16} />
            <select className="appearance-none bg-gray-900 border border-gray-700 rounded-lg py-2.5 pl-10 pr-8 text-white focus:outline-none focus:border-yellow-500 cursor-pointer">
              <option>All Positions</option>
              <option>Strikers (ST)</option>
              <option>Midfielders (MID)</option>
              <option>Defenders (DEF)</option>
              <option>Goalkeepers (GK)</option>
            </select>
          </div>
          
          <select className="bg-gray-900 border border-gray-700 rounded-lg py-2.5 px-4 text-white focus:outline-none focus:border-yellow-500 cursor-pointer">
            <option>Any Age</option>
            <option>Under 21 (U21)</option>
            <option>21 - 29</option>
            <option>30+ (Veterans)</option>
          </select>
        </div>
      </div>

      {/* 3. Решетка с играчи (Player Cards) */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
        {availablePlayers.map((player) => (
          <div key={player.id} className="bg-gray-800 border border-gray-700 rounded-xl overflow-hidden hover:border-yellow-500 transition-all group shadow-lg hover:shadow-[0_0_20px_rgba(234,179,8,0.15)] flex flex-col">
            
            {/* Горна част на картата (Header) */}
            <div className="p-4 border-b border-gray-700/50 bg-gray-800/50 flex justify-between items-start relative">
              <div className="w-12 h-12 rounded-full bg-gray-700 border-2 border-gray-600 flex items-center justify-center font-bold text-gray-300 text-lg z-10">
                {player.name.charAt(0)}
              </div>
              
              <div className="flex flex-col items-end z-10">
                <span className={`text-xs font-bold px-2 py-1 rounded border ${getPosColor(player.pos)}`}>
                  {player.pos}
                </span>
                <span className="text-[10px] text-gray-500 uppercase font-bold mt-1 tracking-wider">
                  {player.type}
                </span>
              </div>
            </div>

            {/* Средна част (Stats) */}
            <div className="p-4 flex-1">
              <h3 className="text-lg font-black text-white truncate group-hover:text-yellow-500 transition-colors">
                {player.name}
              </h3>
              <p className="text-gray-400 text-sm mb-4">Age: {player.age}</p>
              
              <div className="grid grid-cols-2 gap-2">
                <div className="bg-gray-900 rounded-lg p-2 border border-gray-700 flex flex-col items-center">
                  <span className="text-[10px] text-gray-500 uppercase font-bold">OVR</span>
                  <span className="text-xl font-black text-white">{player.skill}</span>
                </div>
                <div className="bg-gray-900 rounded-lg p-2 border border-gray-700 flex flex-col items-center">
                  <span className="text-[10px] text-gray-500 uppercase font-bold">POT</span>
                  <span className="text-xl font-black text-emerald-400">{player.potential}</span>
                </div>
              </div>
            </div>

            {/* Долна част (Value & Action) */}
            <div className="p-4 bg-gray-900 border-t border-gray-700">
              <div className="flex justify-between items-center mb-3">
                <span className="text-xs text-gray-500 font-bold uppercase">Est. Value</span>
                <span className="font-mono text-white font-bold">{player.value}</span>
              </div>
              
              <button className="w-full bg-gray-800 hover:bg-yellow-500 text-gray-300 hover:text-black border border-gray-600 hover:border-yellow-500 font-bold py-2.5 rounded-lg flex items-center justify-center gap-2 transition-all group/btn">
                <UserPlus size={18} className="group-hover/btn:scale-110 transition-transform" />
                APPROACH CLIENT
              </button>
            </div>

          </div>
        ))}

        {/* Карта за отключване на премиум скаутинг */}
        <div className="bg-gray-900 border border-dashed border-gray-700 rounded-xl flex flex-col items-center justify-center p-6 text-center opacity-70 hover:opacity-100 transition-opacity">
          <div className="w-16 h-16 bg-gray-800 rounded-full flex items-center justify-center text-yellow-500 mb-4">
            <ShieldAlert size={32} />
          </div>
          <h3 className="text-lg font-bold text-white mb-2">Upgrade Network</h3>
          <p className="text-sm text-gray-400 mb-4">Upgrade your Scouting Network in 'My Agency' to find higher potential players.</p>
        </div>
      </div>
    </div>
  );
}