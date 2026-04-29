import { Crown, Star, Shield } from 'lucide-react';

export default function Awards() {
  const awards = [
    { 
      id: 1, 
      title: "Ballon d'Elite", 
      desc: "World Player of the Year", 
      icon: <Crown size={48} className="text-yellow-500" />,
      winner: "To be announced (Week 38)",
      status: "pending"
    },
    { 
      id: 2, 
      title: "Golden Boy", 
      desc: "Best U-21 Player", 
      icon: <Star size={48} className="text-blue-400" />,
      winner: "Jude Bellingham (Last Season)",
      status: "completed"
    },
    { 
      id: 3, 
      title: "Guardian of the Net", 
      desc: "Best Goalkeeper", 
      icon: <Shield size={48} className="text-emerald-400" />,
      winner: "To be announced (Week 38)",
      status: "pending"
    }
  ];

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {awards.map((award) => (
        <div key={award.id} className="bg-gray-800 border border-gray-700 rounded-xl p-8 text-center shadow-lg relative overflow-hidden group hover:border-gray-500 transition-colors">
          
          <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 opacity-5 scale-150 group-hover:scale-110 transition-transform duration-500 pointer-events-none">
            {award.icon}
          </div>

          <div className="relative z-10 flex flex-col items-center">
            <div className="mb-6 drop-shadow-[0_0_15px_rgba(0,0,0,0.5)]">
              {award.icon}
            </div>
            
            <h2 className="text-2xl font-black text-white mb-1">{award.title}</h2>
            <p className="text-sm text-gray-400 mb-8">{award.desc}</p>
            
            <div className={`w-full py-3 px-4 rounded-lg border text-sm font-bold ${
              award.status === 'pending' 
                ? 'bg-gray-900 border-gray-700 text-gray-500' 
                : 'bg-yellow-500/10 border-yellow-500/50 text-yellow-500'
            }`}>
              {award.winner}
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}