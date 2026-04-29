import { useState } from 'react';
import { ChevronDown } from 'lucide-react';

export default function Standings() {
  const [selectedLeague, setSelectedLeague] = useState('English Premier Division');

  const leagues = ["English Premier Division", "Spanish Elite Liga", "Italian Pro League"];

  const leagueTable = [
    { pos: 1, team: "Manchester Red", p: 14, w: 11, d: 2, l: 1, gd: "+24", pts: 35 },
    { pos: 2, team: "London Blue", p: 14, w: 10, d: 3, l: 1, gd: "+18", pts: 33 },
    { pos: 3, team: "London Cannons", p: 14, w: 9, d: 4, l: 1, gd: "+15", pts: 31 },
    { pos: 4, team: "Liverpool Reds", p: 14, w: 8, d: 4, l: 2, gd: "+12", pts: 28 },
    { pos: 5, team: "Newcastle Magpies", p: 14, w: 7, d: 5, l: 2, gd: "+5", pts: 26 },
  ];

  return (
    <div className="bg-gray-800 border border-gray-700 rounded-xl shadow-lg overflow-hidden">
      <div className="p-4 border-b border-gray-700 flex justify-between items-center bg-gray-800/80">
        <h2 className="text-lg font-bold text-white">League Table</h2>
        
        <div className="relative w-64">
          <select 
            value={selectedLeague}
            onChange={(e) => setSelectedLeague(e.target.value)}
            className="w-full appearance-none bg-gray-900 border border-gray-700 text-white font-bold py-2 pl-4 pr-10 rounded-lg focus:outline-none focus:border-yellow-500 cursor-pointer text-sm"
          >
            {leagues.map(l => <option key={l} value={l}>{l}</option>)}
          </select>
          <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none" size={16} />
        </div>
      </div>
      
      <div className="overflow-x-auto">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-gray-900/50 border-b border-gray-700 text-xs uppercase tracking-wider text-gray-400">
              <th className="p-4 font-medium text-center">#</th>
              <th className="p-4 font-medium">Club</th>
              <th className="p-4 font-medium text-center">P</th>
              <th className="p-4 font-medium text-center">W</th>
              <th className="p-4 font-medium text-center">D</th>
              <th className="p-4 font-medium text-center">L</th>
              <th className="p-4 font-medium text-center">GD</th>
              <th className="p-4 font-black text-center text-white">Pts</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-700">
            {leagueTable.map((row) => (
              <tr key={row.pos} className="hover:bg-gray-750 transition-colors">
                <td className="p-4 text-center">
                  <span className={`font-bold ${row.pos <= 4 ? 'text-blue-400' : 'text-gray-400'}`}>{row.pos}</span>
                </td>
                <td className="p-4 font-bold text-white">{row.team}</td>
                <td className="p-4 text-center text-gray-400">{row.p}</td>
                <td className="p-4 text-center text-gray-400">{row.w}</td>
                <td className="p-4 text-center text-gray-400">{row.d}</td>
                <td className="p-4 text-center text-gray-400">{row.l}</td>
                <td className="p-4 text-center text-gray-400">{row.gd}</td>
                <td className="p-4 text-center font-black text-yellow-500">{row.pts}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}