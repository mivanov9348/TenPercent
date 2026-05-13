import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, User, Shield, Building2, TrendingUp, DollarSign, Activity, Loader2, FileSignature, CheckCircle2, Search, FileText, Lock } from 'lucide-react';
import OfferRepresentationModal from './OfferRepresentationModal';
import ApproachAgentModal from './ApproachAgentModal';
import { API_URL } from '../../config';

export default function PlayerDetails() {
  const { id } = useParams();
  const navigate = useNavigate();
  
  const [player, setPlayer] = useState<any>(null);
  const [myAgencyId, setMyAgencyId] = useState<number | null>(null); // 🔥 НОВО: Пазим нашето ID тук, за да знаем дали играчът е наш

  // Състояния за скаутинга
  const [hasReport, setHasReport] = useState(false);
  const [scoutReportData, setScoutReportData] = useState<any>(null);
  const [isScouting, setIsScouting] = useState(false);

  const [isLoading, setIsLoading] = useState(true);

  // UI States
  const [activeTab, setActiveTab] = useState<'attributes' | 'stats' | 'contract'>('attributes');
  const [isPitchModalOpen, setIsPitchModalOpen] = useState(false);
  const [isApproachModalOpen, setIsApproachModalOpen] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');

  const fetchPlayerAndReport = async () => {
    setIsLoading(true);
    try {
      const userId = localStorage.getItem('userId');

      // 🔥 0. ДЪРПАМЕ НАШИЯ AGENCY ID 🔥
      if (userId) {
        try {
          const agencyRes = await fetch(`${API_URL}/agency/${userId}`);
          if (agencyRes.ok) {
            const agencyData = await agencyRes.json();
            setMyAgencyId(agencyData.id); // Запазваме си нашето ID!
          }
        } catch (e) {
          console.error("Failed to fetch my agency info.");
        }
      }

      // 1. Дърпаме играча
      const playerRes = await fetch(`${API_URL}/players/${id}`);
      if (playerRes.ok) {
        const data = await playerRes.json();
        setPlayer(data);
      }

      // 2. Проверяваме дали имаме скаутски репорт
      if (userId) {
        const reportRes = await fetch(`${API_URL}/scouting/report/${userId}/${id}`);
        if (reportRes.ok) {
          const reportData = await reportRes.json();
          setScoutReportData(reportData);
          setHasReport(true);
        } else {
          setHasReport(false);
          setScoutReportData(null);
        }
      }

    } catch (error) {
      console.error("Failed to fetch player details:", error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchPlayerAndReport();
  }, [id]);

  const handleRequestReport = async () => {
    if (!window.confirm("Requesting a detailed Scout Report costs $10,000. Proceed?")) return;

    setIsScouting(true);
    try {
      const userId = localStorage.getItem('userId');
      const res = await fetch(`${API_URL}/scouting/request-report`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ userId: Number(userId), playerId: Number(id) })
      });

      const data = await res.json();
      if (!res.ok) throw new Error(data.message);

      setSuccessMessage(data.message);
      fetchPlayerAndReport();

    } catch (err: any) {
      alert(err.message);
    } finally {
      setIsScouting(false);
    }
  };

  const handlePitchSuccess = (msg: string) => {
    setIsPitchModalOpen(false);
    setSuccessMessage(msg);
    fetchPlayerAndReport();
  };

  if (isLoading && !player) {
    return <div className="flex justify-center items-center h-64"><Loader2 className="animate-spin text-yellow-500" size={40} /></div>;
  }

  if (!player) return <div className="text-center text-red-500 mt-10">Player not found!</div>;

  // ==============================================================================
  // 🔥 ФИКС ЗА БЪГА: Проверяваме дали ID-то на агенцията съвпада с нашето ID 🔥
  // ==============================================================================
  const isOurClient = myAgencyId !== null && player.agencyId === myAgencyId;
  const isRepresentedByOther = player.hasAgent && !isOurClient;

  // --- ЛОГИКА ЗА МАСКИРАНЕ НА СТАТИСТИКИТЕ ---
  const canSeeFullStats = isOurClient || (hasReport && scoutReportData?.knowledgeLevel === 5);
  const canSeeBasicStats = isOurClient || hasReport;

  const renderStatBar = (label: string, value: number, isHiddenTrait: boolean = false) => {
    if (isHiddenTrait && !canSeeFullStats) {
      return (
        <div className="mb-3">
          <div className="flex justify-between text-xs font-bold mb-1 text-gray-500">
            <span className="uppercase tracking-wider flex items-center gap-1"><Lock size={12} /> {label}</span>
            <span>?</span>
          </div>
          <div className="h-2 w-full bg-gray-800 rounded-full overflow-hidden flex items-center justify-center border border-gray-700 border-dashed">
            <span className="text-[8px] text-gray-600 font-bold uppercase tracking-widest">Scout to reveal</span>
          </div>
        </div>
      );
    }

    let displayValue: string | number = value;
    let barWidth = value;

    if (!canSeeFullStats) {
      if (hasReport && scoutReportData) {
        const range = (5 - scoutReportData.knowledgeLevel) * 4;
        const min = Math.max(1, value - range);
        const max = Math.min(99, value + range);
        displayValue = `${min}-${max}`;
        barWidth = max;
      } else {
        displayValue = '?';
        barWidth = 0;
      }
    }

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
          <span className={displayValue === '?' ? 'text-gray-600' : 'text-white'}>{displayValue}</span>
        </div>
        <div className="h-2 w-full bg-gray-800 rounded-full overflow-hidden">
          <div className={`h-full rounded-full transition-all duration-1000 ${barWidth === 0 ? 'bg-gray-700' : getColor(barWidth)}`} style={{ width: `${barWidth}%` }} />
        </div>
      </div>
    );
  };

  return (
    <div className="space-y-6 max-w-5xl mx-auto pb-12">
      <button onClick={() => navigate(-1)} className="flex items-center gap-2 text-gray-400 hover:text-white transition-colors font-bold text-sm">
        <ArrowLeft size={16} /> Back
      </button>

      {successMessage && (
        <div className="p-4 bg-emerald-500/10 border border-emerald-500/30 text-emerald-400 rounded-xl flex items-center gap-3 font-bold">
          <CheckCircle2 /> {successMessage}
        </div>
      )}

      {/* HEADER SECTION */}
      <div className="bg-gray-900 border border-gray-800 rounded-3xl p-6 md:p-8 flex flex-col lg:flex-row gap-6 items-center lg:items-start shadow-xl relative overflow-hidden">
        <div className="absolute top-0 right-0 w-64 h-64 bg-yellow-500/5 blur-[100px] rounded-full pointer-events-none" />

        <div className="w-32 h-32 bg-gray-800 border border-gray-700 rounded-2xl flex items-center justify-center shrink-0 z-10 shadow-inner">
          <User size={64} className="text-gray-600" />
        </div>

        {/* INFO & STATS */}
        <div className="flex-1 text-center lg:text-left z-10 w-full flex flex-col justify-between">
          <div className="flex flex-col lg:flex-row lg:justify-between lg:items-start gap-4 mb-4">
            <div>
              <h1 className="text-3xl md:text-4xl font-black text-white tracking-wide mb-1">{player.name}</h1>
              <p className="text-gray-400 font-medium">
                {player.nationality} • {player.age} years old
              </p>
            </div>

            <div className="flex gap-3 justify-center">
              <div className="bg-gray-800 border border-gray-700 rounded-xl p-3 text-center min-w-[70px]">
                <p className="text-[10px] text-gray-500 font-bold uppercase mb-1 tracking-widest">OVR</p>
                <p className={`text-2xl font-black leading-none ${!canSeeBasicStats ? 'text-gray-600' : 'text-white'}`}>
                  {canSeeFullStats ? player.ovr : hasReport ? `${scoutReportData?.minOVR}-${scoutReportData?.maxOVR}` : '?'}
                </p>
              </div>
              <div className="bg-gray-800 border border-green-500/30 rounded-xl p-3 text-center min-w-[70px]">
                <p className="text-[10px] text-green-500/70 font-bold uppercase mb-1 tracking-widest">POT</p>
                <p className={`text-2xl font-black leading-none ${!canSeeBasicStats ? 'text-gray-600' : 'text-green-400'}`}>
                  {canSeeFullStats ? player.pot : hasReport ? `${scoutReportData?.minPOT}-${scoutReportData?.maxPOT}` : '?'}
                </p>
              </div>
            </div>
          </div>

          <div className="flex flex-wrap justify-center lg:justify-start items-center gap-3 mt-2 lg:mt-6">
            <span className="px-4 py-2 bg-yellow-500/10 border border-yellow-500/20 text-yellow-500 rounded-xl text-sm font-bold flex items-center gap-2">
              <Activity size={16} /> {player.position}
            </span>

            <span className="px-4 py-2 bg-gray-800 border border-gray-700 text-gray-300 rounded-xl text-sm font-bold flex items-center gap-2">
              <Shield size={16} className={player.clubId ? "text-blue-400" : "text-gray-500"} />
              {player.clubName || "Free Agent"}
            </span>

            <span className={`px-4 py-2 border rounded-xl text-sm font-bold flex items-center gap-2 ${player.hasAgent ? 'bg-purple-500/10 border-purple-500/20 text-purple-400' : 'bg-gray-800 border-gray-700 text-gray-500'
              }`}>
              <Building2 size={16} /> {player.agencyName || "Unrepresented"}
            </span>
          </div>
        </div>

        {/* ACTIONS SECTION */}
        <div className="flex flex-col gap-3 w-full lg:w-48 shrink-0 z-20 mt-4 lg:mt-0">
          {/* СКАУТСКИ БУТОН */}
          {!hasReport && !isOurClient ? (
            <button
              onClick={handleRequestReport}
              disabled={isScouting}
              className="w-full px-4 py-3 bg-gray-800 hover:bg-gray-700 border border-gray-600 text-white font-bold rounded-xl transition-all flex items-center justify-center gap-2 shadow-lg text-sm"
            >
              {isScouting ? <Loader2 className="animate-spin text-blue-400" size={18} /> : <><Search size={18} className="text-blue-400" /> Scout Player</>}
            </button>
          ) : (
            <button
              onClick={() => navigate(`/world/player/${id}/report`)}
              className="w-full px-4 py-3 bg-blue-600 hover:bg-blue-500 text-white font-bold rounded-xl shadow-[0_0_15px_rgba(37,99,235,0.3)] transition-all flex items-center justify-center gap-2 text-sm"
            >
              <FileText size={18} /> View Report
            </button>
          )}

          {/* ДИНАМИЧЕН БУТОН ЗА ПОДПИСВАНЕ / ПОДХОД */}
          {!player.hasAgent ? (
            <button onClick={() => setIsPitchModalOpen(true)} className="w-full px-4 py-3 bg-yellow-500 hover:bg-yellow-400 text-black font-black uppercase tracking-wider rounded-xl shadow-[0_0_20px_rgba(234,179,8,0.3)] transition-all hover:scale-105 active:scale-95 flex items-center justify-center gap-2 text-sm">
              <FileSignature size={18} /> Pitch Player
            </button>
          ) : isRepresentedByOther ? (
            <button
              onClick={() => setIsApproachModalOpen(true)}
              className="w-full px-4 py-3 bg-purple-600 hover:bg-purple-500 text-white font-black uppercase tracking-wider rounded-xl shadow-[0_0_20px_rgba(147,51,234,0.3)] transition-all hover:scale-105 active:scale-95 flex items-center justify-center gap-2 text-sm"
            >
              <Building2 size={18} /> Approach Agent
            </button>
          ) : (
            <div className="w-full px-4 py-3 bg-emerald-500/10 border border-emerald-500/30 text-emerald-400 font-bold rounded-xl text-center text-sm">
              Your Client
            </div>
          )}
        </div>
      </div>

      {/* TABS NAVIGATION */}
      <div className="flex gap-2 p-1 bg-gray-900 border border-gray-800 rounded-2xl overflow-x-auto hide-scrollbar">
        {[
          { id: 'attributes', icon: TrendingUp, label: 'Attributes & Traits' },
          { id: 'stats', icon: Activity, label: 'Season Performance' },
          { id: 'contract', icon: DollarSign, label: 'Contract Details' }
        ].map((tab) => (
          <button
            key={tab.id}
            onClick={() => setActiveTab(tab.id as any)}
            className={`flex-1 min-w-[150px] flex items-center justify-center gap-2 py-3 px-4 rounded-xl font-bold text-sm transition-all ${activeTab === tab.id
                ? 'bg-gray-800 text-white shadow-md'
                : 'text-gray-500 hover:text-gray-300 hover:bg-gray-800/50'
              }`}
          >
            <tab.icon size={18} className={activeTab === tab.id ? 'text-yellow-500' : ''} /> {tab.label}
          </button>
        ))}
      </div>

      {/* TAB CONTENT AREAS */}
      {/* 1. ATTRIBUTES TAB */}
      {activeTab === 'attributes' && (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 animate-in slide-in-from-bottom-4 fade-in duration-300">
          <div className="bg-gray-900 border border-gray-800 rounded-2xl p-6 shadow-lg relative overflow-hidden">
            
            {/* Overlay ако нямаме скаутски репорт */}
            {!canSeeBasicStats && (
                <div className="absolute inset-0 bg-gray-900/80 backdrop-blur-sm z-10 flex flex-col items-center justify-center p-6 text-center border border-gray-800 rounded-2xl">
                    <Search size={48} className="text-gray-600 mb-4" />
                    <h3 className="text-xl font-bold text-white mb-2">Unknown Player</h3>
                    <p className="text-gray-400 text-sm mb-6">You need to send a scout to evaluate this player's technical and physical abilities.</p>
                    <button onClick={handleRequestReport} className="px-6 py-2 bg-blue-600 hover:bg-blue-500 text-white font-bold rounded-xl transition-colors">
                        Scout Now ($10k)
                    </button>
                </div>
            )}

            <h3 className="text-lg font-bold text-white mb-6 border-b border-gray-800 pb-3">Technical & Physical</h3>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-8 gap-y-2">
              {renderStatBar("Pace", player.pace)}
              {renderStatBar("Shooting", player.shooting)}
              {renderStatBar("Passing", player.passing)}
              {renderStatBar("Dribbling", player.dribbling)}
              {renderStatBar("Defending", player.defending)}
              {renderStatBar("Physical", player.physical)}
            </div>
          </div>

          <div className="bg-gray-900 border border-gray-800 rounded-2xl p-6 shadow-lg">
            <h3 className="text-lg font-bold text-white mb-6 border-b border-gray-800 pb-3">Hidden Traits (Personality)</h3>
            <p className="text-sm text-gray-500 mb-6">Тези статистики определят как играчът реагира на преговори и трансфери.</p>
            <div className="space-y-4">
              {renderStatBar("Ambition", player.ambition, true)}
              {renderStatBar("Greed", player.greed, true)}
              {renderStatBar("Loyalty", player.loyalty, true)}
            </div>
          </div>
        </div>
      )}

      {/* 2. STATS TAB */}
      {activeTab === 'stats' && (
        <div className="bg-gray-900 border border-gray-800 rounded-2xl p-6 shadow-lg animate-in slide-in-from-bottom-4 fade-in duration-300">
          <div className="flex justify-between items-center border-b border-gray-800 pb-4 mb-6">
            <h2 className="text-xl font-bold text-white flex items-center gap-2">
              <Activity className="text-blue-500" /> Season Performance
            </h2>

            {/* Формата (Последните 5 мача - Rating) */}
            <div className="flex items-center gap-2">
              <span className="text-sm text-gray-500 font-bold mr-2 uppercase tracking-widest">Form:</span>
              {player.recentMatches && player.recentMatches.length > 0 ? (
                player.recentMatches.map((match: any, index: number) => {
                  let colorClass = 'bg-gray-800 text-gray-400 border-gray-700';
                  if (match.rating >= 8.0) colorClass = 'bg-green-500/20 text-green-500 border-green-500/30';
                  else if (match.rating >= 7.0) colorClass = 'bg-yellow-500/20 text-yellow-500 border-yellow-500/30';
                  else if (match.rating > 0) colorClass = 'bg-red-500/20 text-red-500 border-red-500/30';

                  return (
                    <div key={index} title={`${match.opponentName} (Rating: ${match.rating})`} className={`w-8 h-8 rounded flex items-center justify-center font-bold text-xs border ${colorClass}`}>
                      {match.rating > 0 ? match.rating.toFixed(1) : '-'}
                    </div>
                  );
                })
              ) : (
                <span className="text-xs text-gray-500">Няма изиграни мачове</span>
              )}
            </div>
          </div>

          {/* Aggregated Stats (Общо за сезона) */}
          <div className="grid grid-cols-2 md:grid-cols-5 gap-4 mb-8">
            <div className="bg-gray-800 rounded-xl p-4 text-center border border-gray-700">
              <p className="text-gray-400 text-xs font-bold uppercase mb-1">Apps</p>
              <p className="text-2xl font-black text-white">{player.seasonAppearances}</p>
            </div>
            <div className="bg-gray-800 rounded-xl p-4 text-center border border-gray-700">
              <p className="text-gray-400 text-xs font-bold uppercase mb-1">Goals</p>
              <p className="text-2xl font-black text-white">{player.seasonGoals}</p>
            </div>
            <div className="bg-gray-800 rounded-xl p-4 text-center border border-gray-700">
              <p className="text-gray-400 text-xs font-bold uppercase mb-1">Assists</p>
              <p className="text-2xl font-black text-white">{player.seasonAssists}</p>
            </div>
            <div className="bg-gray-800 rounded-xl p-4 text-center border border-gray-700">
              <p className="text-gray-400 text-xs font-bold uppercase mb-1">Avg. Rating</p>
              <p className="text-2xl font-black text-blue-400">{player.seasonAverageRating > 0 ? player.seasonAverageRating.toFixed(2) : '0.00'}</p>
            </div>
            <div className="bg-gray-800 rounded-xl p-4 text-center border border-gray-700">
              <p className="text-gray-400 text-xs font-bold uppercase mb-1">Cards</p>
              <p className="text-xl font-black">
                <span className="text-yellow-500">{player.seasonYellowCards}</span> <span className="text-gray-600 mx-1">/</span> <span className="text-red-500">{player.seasonRedCards}</span>
              </p>
            </div>
          </div>

          {/* Match Log Table */}
          <h3 className="text-sm font-bold text-gray-500 uppercase tracking-widest mb-4">Recent Matches Log</h3>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead className="text-gray-500 uppercase font-bold border-b border-gray-800">
                <tr>
                  <th className="pb-3 px-4">GW</th>
                  <th className="pb-3 px-4">Opponent</th>
                  <th className="pb-3 px-4 text-center">Mins</th>
                  <th className="pb-3 px-4 text-center">G</th>
                  <th className="pb-3 px-4 text-center">A</th>
                  <th className="pb-3 px-4 text-center">Rating</th>
                </tr>
              </thead>
              <tbody className="text-gray-300">
                {player.recentMatches && player.recentMatches.length > 0 ? (
                  player.recentMatches.map((match: any, idx: number) => (
                    <tr key={idx} className="border-b border-gray-800/50 hover:bg-gray-800/50 transition-colors">
                      <td className="py-3 px-4 text-gray-400">{match.gameweek}</td>
                      <td className="py-3 px-4 flex items-center gap-2">
                        <Shield size={14} className={match.isHomeMatch ? "text-blue-500" : "text-gray-500"} />
                        {match.isHomeMatch ? 'vs' : '@'} {match.opponentName}
                      </td>
                      <td className="py-3 px-4 text-center">{match.minutesPlayed}'</td>
                      <td className={`py-3 px-4 text-center ${match.goals > 0 ? 'font-bold text-white' : 'text-gray-600'}`}>{match.goals}</td>
                      <td className={`py-3 px-4 text-center ${match.assists > 0 ? 'font-bold text-white' : 'text-gray-600'}`}>{match.assists}</td>
                      <td className={`py-3 px-4 text-center font-bold ${match.rating >= 8.0 ? 'text-green-400' : match.rating >= 7.0 ? 'text-yellow-500' : 'text-red-400'
                        }`}>
                        {match.rating > 0 ? match.rating.toFixed(1) : '-'}
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={6} className="py-8 text-center text-gray-500 italic">Играчът все още няма записани минути този сезон.</td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* 3. CONTRACT TAB */}
      {activeTab === 'contract' && (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 animate-in slide-in-from-bottom-4 fade-in duration-300">
          <div className="bg-gray-900 border border-gray-800 rounded-2xl p-6 shadow-lg">
            <h3 className="text-lg font-bold text-white mb-6 border-b border-gray-800 pb-3 flex items-center gap-2">
              <Shield size={20} className="text-blue-500" /> Club Contract
            </h3>
            
            {!canSeeFullStats ? (
                <div className="py-8 text-center bg-gray-800/50 rounded-xl border border-gray-700 border-dashed">
                    <Lock size={32} className="text-gray-600 mx-auto mb-3" />
                    <p className="text-gray-400 text-sm">Financial details are classified.</p>
                    <p className="text-gray-500 text-xs">A Level 5 Scout Report is required to view full contract details.</p>
                </div>
            ) : (
                <div className="space-y-5">
                <div>
                    <p className="text-[10px] text-gray-500 font-bold uppercase tracking-widest mb-1">Market Value</p>
                    <p className="text-2xl font-mono font-black text-white">${(player.marketValue / 1000000).toFixed(2)}M</p>
                </div>
                <div className="grid grid-cols-2 gap-4">
                    <div>
                    <p className="text-[10px] text-gray-500 font-bold uppercase tracking-widest mb-1">Weekly Wage</p>
                    <p className="text-lg font-mono font-bold text-emerald-400">{player.weeklyWage > 0 ? `$${player.weeklyWage.toLocaleString()}` : "N/A"}</p>
                    </div>
                    <div>
                    <p className="text-[10px] text-gray-500 font-bold uppercase tracking-widest mb-1">Time Left</p>
                    <p className="text-lg font-bold text-gray-300">{player.contractYearsLeft > 0 ? `${player.contractYearsLeft} Years` : "Expired"}</p>
                    </div>
                </div>
                </div>
            )}
          </div>

          <div className="bg-gray-900 border border-gray-800 rounded-2xl p-6 shadow-lg">
            <h3 className="text-lg font-bold text-white mb-6 border-b border-gray-800 pb-3 flex items-center gap-2">
              <Building2 size={20} className="text-purple-500" /> Agency Representation
            </h3>
            {player.hasAgent ? (
              <div className="text-center py-6">
                <p className="text-gray-400 mb-2">Represented by</p>
                <p className="text-xl font-bold text-purple-400">{player.agencyName}</p>
              </div>
            ) : (
              <div className="text-center py-6">
                <p className="text-gray-500 mb-4">Този играч няма агент и е свободен за подписване.</p>
                <button
                  onClick={() => setIsPitchModalOpen(true)}
                  className="px-6 py-2 bg-yellow-500 hover:bg-yellow-400 text-black font-bold rounded-xl transition-colors text-sm"
                >
                  Send Proposal Now
                </button>
              </div>
            )}
          </div>
        </div>
      )}

      {/* THE MODALS */}
      <OfferRepresentationModal
        player={player}
        isOpen={isPitchModalOpen}
        onClose={() => setIsPitchModalOpen(false)}
        onSuccess={handlePitchSuccess}
      />

      <ApproachAgentModal
        player={player}
        isOpen={isApproachModalOpen}
        onClose={() => setIsApproachModalOpen(false)}
        onSuccess={(msg) => {
          setIsApproachModalOpen(false);
          setSuccessMessage(msg);
          fetchPlayerAndReport();
        }}
      />
    </div>
  );
}