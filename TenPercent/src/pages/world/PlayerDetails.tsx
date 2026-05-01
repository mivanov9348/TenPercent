import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, User, Shield, Building2, TrendingUp, DollarSign, Activity, Loader2 } from 'lucide-react';

export default function PlayerDetails() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [player, setPlayer] = useState<any>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchPlayer = async () => {
      try {
        const response = await fetch(`https://localhost:7135/api/players/${id}`);
        if (response.ok) {
          const data = await response.json();
          setPlayer(data);
        }
      } catch (error) {
        console.error("Failed to fetch player details:", error);
      } finally {
        setIsLoading(false);
      }
    };
    fetchPlayer();
  }, [id]);

  if (isLoading) {
    return <div className="flex justify-center items-center h-64"><Loader2 className="animate-spin text-yellow-500" size={40} /></div>;
  }

  if (!player) {
    return <div className="text-center text-red-500 mt-10">Player not found!</div>;
  }

  // Помощна функция за "прогрес бар" на статистиките
  const renderStatBar = (label: string, value: number) => {
    const getColor = (val: number) => {
      if (val >= 85) return 'bg-green-500';
      if (val >= 70) return 'bg-yellow-500';
      if (val >= 50) return 'bg-orange-500';
      return 'bg-red-500';
    };

    return (
      <div className="mb-3">
        <div className="flex justify-between text-xs font-bold mb-1">
          <span className="text-gray-400 uppercase tracking-wider">{label}</span>
          <span className="text-white">{value}</span>
        </div>
        <div className="h-2 w-full bg-gray-800 rounded-full overflow-hidden">
          <div 
            className={`h-full rounded-full transition-all duration-1000 ${getColor(value)}`} 
            style={{ width: `${value}%` }} 
          />
        </div>
      </div>
    );
  };

  return (
    <div className="space-y-6 max-w-4xl mx-auto">
      {/* Back Button */}
      <button 
        onClick={() => navigate(-1)} 
        className="flex items-center gap-2 text-gray-400 hover:text-white transition-colors"
      >
        <ArrowLeft size={20} /> Back
      </button>

      {/* Player Header */}
      <div className="bg-gray-900 border border-gray-800 rounded-3xl p-8 flex flex-col md:flex-row gap-8 items-center md:items-start shadow-xl relative overflow-hidden">
        
        {/* Background accent */}
        <div className="absolute top-0 right-0 w-64 h-64 bg-yellow-500/5 blur-[100px] rounded-full pointer-events-none" />

        {/* Player Avatar placeholder */}
        <div className="w-32 h-32 bg-gray-800 border-2 border-gray-700 rounded-2xl flex items-center justify-center shrink-0 z-10">
          <User size={64} className="text-gray-500" />
        </div>

        <div className="flex-1 text-center md:text-left z-10 w-full">
          <div className="flex flex-col md:flex-row md:justify-between md:items-start gap-4 mb-4">
            <div>
              <h1 className="text-3xl font-black text-white tracking-wide mb-1">{player.name}</h1>
              <p className="text-gray-400 flex items-center justify-center md:justify-start gap-2">
                {player.nationality} • {player.age} years old
              </p>
            </div>
            
            {/* OVR & POT Badge */}
            <div className="flex gap-3 justify-center">
              <div className="bg-gray-800 border border-gray-700 rounded-xl p-3 text-center min-w-[80px]">
                <p className="text-xs text-gray-500 font-bold uppercase mb-1">OVR</p>
                <p className="text-2xl font-black text-white">{player.overall}</p>
              </div>
              <div className="bg-gray-800 border border-green-500/30 rounded-xl p-3 text-center min-w-[80px]">
                <p className="text-xs text-green-500/70 font-bold uppercase mb-1">POT</p>
                <p className="text-2xl font-black text-green-400">{player.potential}</p>
              </div>
            </div>
          </div>

          <div className="flex flex-wrap justify-center md:justify-start gap-3 mt-4">
            <span className="px-3 py-1 bg-yellow-500/10 border border-yellow-500/20 text-yellow-500 rounded-lg text-sm font-bold flex items-center gap-1">
              <Activity size={16} /> Position: {player.position}
            </span>
            
            {player.clubId ? (
              <button 
                onClick={() => navigate(`/world/club/${player.clubId}`)}
                className="px-3 py-1 bg-blue-500/10 border border-blue-500/20 text-blue-400 rounded-lg text-sm font-bold flex items-center gap-1 hover:bg-blue-500/20 transition-colors"
              >
                <Shield size={16} /> {player.clubName}
              </button>
            ) : (
              <span className="px-3 py-1 bg-gray-800 border border-gray-700 text-gray-400 rounded-lg text-sm font-bold flex items-center gap-1">
                <Shield size={16} /> Free Agent
              </span>
            )}

            {player.agencyName ? (
              <span className="px-3 py-1 bg-purple-500/10 border border-purple-500/20 text-purple-400 rounded-lg text-sm font-bold flex items-center gap-1">
                <Building2 size={16} /> {player.agencyName}
              </span>
            ) : (
              <span className="px-3 py-1 bg-gray-800 border border-gray-700 text-gray-400 rounded-lg text-sm font-bold flex items-center gap-1">
                <Building2 size={16} /> No Agency
              </span>
            )}
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        
        {/* Attributes Panel */}
        <div className="lg:col-span-2 bg-gray-900 border border-gray-800 rounded-2xl p-6 shadow-lg">
          <h2 className="text-xl font-bold text-white mb-6 flex items-center gap-2 border-b border-gray-800 pb-3">
            <TrendingUp className="text-yellow-500" /> Core Attributes
          </h2>
          
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-8 gap-y-2">
            {renderStatBar("Pace", player.pace)}
            {renderStatBar("Shooting", player.shooting)}
            {renderStatBar("Passing", player.passing)}
            {renderStatBar("Dribbling", player.dribbling)}
            {renderStatBar("Defending", player.defending)}
            {renderStatBar("Physical", player.physical)}
          </div>
        </div>

        {/* Contract & Finances Panel */}
        <div className="bg-gray-900 border border-gray-800 rounded-2xl p-6 shadow-lg">
          <h2 className="text-xl font-bold text-white mb-6 flex items-center gap-2 border-b border-gray-800 pb-3">
            <DollarSign className="text-green-500" /> Contract Details
          </h2>
          
          <div className="space-y-6">
            <div>
              <p className="text-sm text-gray-500 font-bold uppercase mb-1">Estimated Value</p>
              <p className="text-2xl font-mono font-black text-white">
                ${(player.marketValue / 1000000).toFixed(2)}M
              </p>
            </div>
            
            <div>
              <p className="text-sm text-gray-500 font-bold uppercase mb-1">Weekly Wage</p>
              <p className="text-xl font-mono font-bold text-gray-300">
                {player.weeklyWage > 0 ? `$${player.weeklyWage.toLocaleString()}` : "N/A"}
              </p>
            </div>

            <div>
              <p className="text-sm text-gray-500 font-bold uppercase mb-1">Contract Status</p>
              {player.contractYearsLeft > 0 ? (
                <p className="text-gray-300 font-medium">{player.contractYearsLeft} years remaining</p>
              ) : (
                <p className="text-red-400 font-medium">Expired / Free Agent</p>
              )}
            </div>

            <div>
              <p className="text-sm text-gray-500 font-bold uppercase mb-1">Current Form</p>
              <p className="text-green-400 font-medium">{player.form}</p>
            </div>
          </div>
        </div>

      </div>
    </div>
  );
}