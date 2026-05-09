import { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { Database, Calendar, LogOut, Loader2, CheckCircle2, AlertCircle, Upload, ShieldAlert, TrendingUp, Users, Globe, FastForward, Play, Power, PlayCircle, Trophy, CalendarDays, Info, Clock, Landmark, FileText } from 'lucide-react';

export default function AdminDashboard() {
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);
  const [isEngineLoading, setIsEngineLoading] = useState(true);
  const [message, setMessage] = useState<{ text: string, type: 'success' | 'error' } | null>(null);

  const [worldState, setWorldState] = useState<any>(null);
  const [isInitialized, setIsInitialized] = useState<boolean>(false);

  const [squadReport, setSquadReport] = useState<any[] | null>(null);
  const [showReportModal, setShowReportModal] = useState(false);
  
  const [simulationResultModal, setSimulationResultModal] = useState<{ show: boolean, message: string }>({ show: false, message: '' });

  // РЕФОВЕ ЗА ФАЙЛОВЕТЕ
  const positionFileInputRef = useRef<HTMLInputElement>(null);
  const leagueFileInputRef = useRef<HTMLInputElement>(null);
  const clubFileInputRef = useRef<HTMLInputElement>(null);
  const scoutFileInputRef = useRef<HTMLInputElement>(null); // НОВО: Реф за скаутските шаблони

  const fetchWorldState = async () => {
    setIsEngineLoading(true);
    try {
      const response = await fetch('https://localhost:7135/api/admin/world-state');
      if (response.ok) {
        const data = await response.json();
        setWorldState(data);
        setIsInitialized(true);
      } else if (response.status === 404) {
        setIsInitialized(false);
        setWorldState(null);
      }
    } catch (error) {
      console.error("Failed to fetch world state:", error);
    } finally {
      setIsEngineLoading(false);
    }
  };

  useEffect(() => {
    fetchWorldState();
  }, []);

  const handleLogout = () => {
    localStorage.clear();
    navigate('/login');
  };

  const handleInitializeWorld = async () => {
    setIsLoading(true);
    setMessage(null);
    try {
      const response = await fetch('https://localhost:7135/api/admin/initialize-world', { method: 'POST' });
      const data = await response.json();
      
      if (!response.ok) throw new Error(data.message || "Failed to initialize world");
      setMessage({ text: data.message, type: 'success' });
      await fetchWorldState(); 
    } catch (err: any) {
      setMessage({ text: err.message, type: 'error' });
    } finally {
      setIsLoading(false);
    }
  };

  const handleFileUpload = async (event: React.ChangeEvent<HTMLInputElement>, endpoint: string) => {
    const file = event.target.files?.[0];
    if (!file) return;

    setIsLoading(true);
    setMessage(null);

    const formData = new FormData();
    formData.append('file', file);

    try {
      const response = await fetch(`https://localhost:7135/api/admin/${endpoint}`, {
        method: 'POST',
        body: formData,
      });
      
      const data = await response.json();
      
      if (!response.ok) throw new Error(data.message || data || "Import failed");
      setMessage({ text: data.message, type: 'success' });
    } catch (err: any) {
      setMessage({ text: err.message, type: 'error' });
    } finally {
      setIsLoading(false);
      if(event.target) event.target.value = ''; 
    }
  };

  const handleInitializeEconomy = async () => {
    const amountStr = window.prompt("Enter the starting budget for the World Central Bank:", "100000000000");
    
    if (amountStr === null) return; 

    const amount = parseFloat(amountStr);
    if (isNaN(amount) || amount <= 0) {
      setMessage({ text: "Invalid budget amount entered.", type: 'error' });
      return;
    }

    setIsLoading(true);
    setMessage(null);
    try {
      const response = await fetch(`https://localhost:7135/api/finance/initialize?bankBudget=${amount}`, { method: 'POST' });
      const data = await response.json();
      
      if (!response.ok) throw new Error(data.message || "Failed to initialize economy");
      setMessage({ text: data.message, type: 'success' });
    } catch (err: any) {
      setMessage({ text: err.message, type: 'error' });
    } finally {
      setIsLoading(false);
    }
  };

  const handleSquadReport = async () => {
    setIsLoading(true);
    setMessage(null);
    setSquadReport(null);
    try {
      const response = await fetch('https://localhost:7135/api/admin/squad-report');
      const data = await response.json();
      
      if (!response.ok) throw new Error(data.message || "Failed to generate report");
      
      if (data.issues && data.issues.length > 0) {
        setSquadReport(data.issues);
        setShowReportModal(true);
        setMessage({ text: `Открити проблеми в ${data.issues.length} отбора!`, type: 'error' });
      } else {
        setMessage({ text: data.message, type: 'success' });
      }
    } catch (err: any) {
      setMessage({ text: err.message, type: 'error' });
    } finally {
      setIsLoading(false);
    }
  };

  const handleSquadFix = async () => {
    setIsLoading(true);
    setMessage(null);
    try {
      const response = await fetch('https://localhost:7135/api/admin/squad-autofix', { method: 'POST' });
      const data = await response.json();
      
      if (!response.ok) throw new Error(data.message || "Failed to fix squads");
      setMessage({ text: data.message, type: 'success' });
      setSquadReport(null);
      setShowReportModal(false);
    } catch (err: any) {
      setMessage({ text: err.message, type: 'error' });
    } finally {
      setIsLoading(false);
    }
  };

  const handleInitializeStandings = async () => {
    setIsLoading(true);
    setMessage(null);
    try {
      const response = await fetch('https://localhost:7135/api/admin/initialize-standings', { method: 'POST' });
      const data = await response.json();
      if (!response.ok) throw new Error(data.message || "Failed to initialize standings");
      setMessage({ text: data.message, type: 'success' });
    } catch (err: any) {
      setMessage({ text: err.message, type: 'error' });
    } finally {
      setIsLoading(false);
    }
  };

  const handleGenerateSchedule = async () => {
    setIsLoading(true);
    setMessage(null);
    try {
      const response = await fetch('https://localhost:7135/api/admin/generate-schedule', { method: 'POST' });
      const data = await response.json();
      if (!response.ok) throw new Error(data.message || data || "Failed to generate schedule");
      setMessage({ text: data.message, type: 'success' });
      fetchWorldState(); 
    } catch (err: any) {
      setMessage({ text: err.message, type: 'error' });
    } finally {
      setIsLoading(false);
    }
  };

  const handleGenerateFreeAgents = async () => {
    setIsLoading(true);
    setMessage(null);
    try {
      const response = await fetch('https://localhost:7135/api/players/generate-free-agents?count=50', { method: 'POST' });
      const data = await response.json();
      if (!response.ok) throw new Error(data.message || "Failed to generate free agents");
      setMessage({ text: data.message, type: 'success' });
    } catch (err: any) {
      setMessage({ text: err.message, type: 'error' });
    } finally {
      setIsLoading(false);
    }
  };

  const handleStartNewSeason = async () => {
    setIsLoading(true);
    setMessage(null);
    try {
      const response = await fetch('https://localhost:7135/api/season/start', { method: 'POST' });
      const data = await response.json();
      if (!response.ok) throw new Error(data.message || "Failed to start new season");
      setMessage({ text: data.message, type: 'success' });
      fetchWorldState(); 
    } catch (err: any) {
      setMessage({ text: err.message, type: 'error' });
    } finally {
      setIsLoading(false);
    }
  };

  const handleEndSeason = async () => {
    if (!window.confirm("Are you sure you want to END the current season? This will lock all standings and delete played fixtures!")) return;
    setIsLoading(true);
    setMessage(null);
    try {
      const response = await fetch('https://localhost:7135/api/season/end', { method: 'POST' });
      const data = await response.json();
      if (!response.ok) throw new Error(data.message || "Failed to end season");
      setMessage({ text: data.message, type: 'success' });
      fetchWorldState(); 
    } catch (err: any) {
      setMessage({ text: err.message, type: 'error' });
    } finally {
      setIsLoading(false);
    }
  };

  const handleSimulateMatchday = async () => {
    setIsLoading(true);
    setMessage(null);
    try {
      const response = await fetch('https://localhost:7135/api/simulation/play-gameweek', { method: 'POST' });
      const data = await response.json();
      
      if (!response.ok) throw new Error(data.message || "Failed to simulate gameweek");
      
      setSimulationResultModal({ show: true, message: data.message });
      fetchWorldState(); 
    } catch (err: any) {
      setMessage({ text: err.message, type: 'error' });
    } finally {
      setIsLoading(false);
    }
  };

  if (isEngineLoading) {
    return <div className="min-h-screen bg-gray-950 flex items-center justify-center text-white"><Loader2 className="animate-spin mr-2" /> Booting Engine...</div>;
  }

  if (!isInitialized) {
    return (
      <div className="min-h-screen bg-gray-950 flex flex-col items-center justify-center p-8 relative overflow-hidden">
        <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_center,_var(--tw-gradient-stops))] from-blue-900/20 via-gray-950 to-gray-950"></div>
        <div className="z-10 text-center max-w-lg">
          <div className="w-24 h-24 bg-blue-500/10 text-blue-500 rounded-full flex items-center justify-center mx-auto mb-8 animate-pulse border border-blue-500/20">
            <Power size={48} />
          </div>
          <h1 className="text-5xl font-black text-white uppercase tracking-widest mb-4">World Engine Offline</h1>
          <p className="text-gray-400 mb-12 text-lg">The simulation database is empty. You need to initialize the World Engine before you can import leagues, clubs, or players.</p>
          <button onClick={handleInitializeWorld} disabled={isLoading} className="w-full py-5 bg-blue-600 hover:bg-blue-500 text-white font-black rounded-2xl transition-all hover:scale-105 active:scale-95 flex justify-center items-center gap-3 text-lg shadow-[0_0_30px_rgba(37,99,235,0.3)] disabled:opacity-50 disabled:hover:scale-100">
            {isLoading ? <Loader2 className="animate-spin" size={24} /> : <><Globe size={24} /> INITIALIZE WORLD ENGINE</>}
          </button>
          <button onClick={handleLogout} className="mt-8 text-gray-500 hover:text-white transition-colors text-sm font-bold flex items-center justify-center gap-2 mx-auto">
            <LogOut size={14} /> Return to Login
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-950 p-4 md:p-8 relative">
      <div className="max-w-6xl mx-auto space-y-6">
        
        <header className="bg-gray-900 p-6 rounded-2xl border border-gray-800 shadow-xl flex flex-col md:flex-row justify-between items-center gap-6">
          <div className="flex items-center gap-4">
            <div className="w-14 h-14 bg-blue-500/10 rounded-2xl flex items-center justify-center border border-blue-500/20">
              <Globe size={28} className="text-blue-500" />
            </div>
            <div>
              <h1 className="text-2xl font-black text-white uppercase tracking-widest leading-tight">World Engine</h1>
              <div className="flex items-center gap-3 text-sm font-bold mt-1">
                {worldState && (
                  <>
                    <span className="text-yellow-500">Season: {worldState.seasonNumber || '-'}</span>
                    <span className="text-gray-600">•</span>
                    <span className="text-blue-400">Gameweek: {worldState.currentGameweek} / {worldState.totalGameweeks}</span>
                    <span className="text-gray-600">•</span>
                    {worldState.nextMatchdayDate && (
                      <>
                        <span className="text-purple-400">Date: {new Date(worldState.nextMatchdayDate).toLocaleDateString()}</span>
                        <span className="text-gray-600">•</span>
                      </>
                    )}
                    <span className={worldState.isSeasonActive ? "text-emerald-400" : "text-red-400"}>
                      Status: {worldState.isSeasonActive ? "ACTIVE" : "ENDED"}
                    </span>
                  </>
                )}
              </div>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <button onClick={handleEndSeason} disabled={isLoading || !worldState?.isSeasonActive} className="flex items-center gap-2 bg-red-600/10 hover:bg-red-600 border border-red-600/50 hover:border-red-600 text-red-500 hover:text-white px-4 py-2.5 rounded-xl transition-all disabled:opacity-30 disabled:hover:bg-transparent font-bold text-sm">
              <FastForward size={16} /> END SEASON
            </button>
            <button onClick={handleLogout} className="flex items-center gap-2 bg-gray-800 hover:bg-gray-700 text-gray-300 px-4 py-2.5 rounded-xl transition-colors text-sm font-bold">
              <LogOut size={16} /> LOGOUT
            </button>
          </div>
        </header>

        {message && (
          <div className={`p-4 rounded-xl flex items-center gap-3 border shadow-lg ${message.type === 'success' ? 'bg-emerald-500/10 border-emerald-500/30 text-emerald-400' : 'bg-red-500/10 border-red-500/30 text-red-400'}`}>
            {message.type === 'success' ? <CheckCircle2 /> : <AlertCircle />}
            <p className="font-bold">{message.text}</p>
          </div>
        )}

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          
          <div className="bg-gray-900 border border-gray-800 p-6 rounded-2xl flex flex-col items-center text-center shadow-lg">
            <div className="w-16 h-16 bg-cyan-500/10 text-cyan-500 rounded-2xl flex items-center justify-center mb-4"><Upload size={32} /></div>
            <h2 className="text-xl font-bold text-white mb-2">1. Import Positions</h2>
            <p className="text-gray-500 text-sm mb-6 flex-1">Upload positions.csv. Defines tactical roles (GK, CB, CM, ST).</p>
            <input type="file" accept=".csv" className="hidden" ref={positionFileInputRef} onChange={(e) => handleFileUpload(e, 'import-positions')} />
            <button onClick={() => positionFileInputRef.current?.click()} disabled={isLoading} className="w-full py-3 bg-cyan-600 hover:bg-cyan-500 text-white font-bold rounded-xl transition-colors flex justify-center items-center gap-2 disabled:opacity-50 mt-auto">
              {isLoading ? <Loader2 className="animate-spin" /> : 'UPLOAD POSITIONS'}
            </button>
          </div>

          <div className="bg-gray-900 border border-gray-800 p-6 rounded-2xl flex flex-col items-center text-center shadow-lg">
            <div className="w-16 h-16 bg-blue-500/10 text-blue-500 rounded-2xl flex items-center justify-center mb-4"><Upload size={32} /></div>
            <h2 className="text-xl font-bold text-white mb-2">2. Import Leagues</h2>
            <p className="text-gray-500 text-sm mb-6 flex-1">Upload leagues.csv. Creates the base league structures.</p>
            <input type="file" accept=".csv" className="hidden" ref={leagueFileInputRef} onChange={(e) => handleFileUpload(e, 'import-leagues')} />
            <button onClick={() => leagueFileInputRef.current?.click()} disabled={isLoading} className="w-full py-3 bg-blue-600 hover:bg-blue-500 text-white font-bold rounded-xl transition-colors flex justify-center items-center gap-2 disabled:opacity-50 mt-auto">
              {isLoading ? <Loader2 className="animate-spin" /> : 'UPLOAD LEAGUES'}
            </button>
          </div>

          <div className="bg-gray-900 border border-gray-800 p-6 rounded-2xl flex flex-col items-center text-center shadow-lg">
            <div className="w-16 h-16 bg-yellow-500/10 text-yellow-500 rounded-2xl flex items-center justify-center mb-4"><Database size={32} /></div>
            <h2 className="text-xl font-bold text-white mb-2">3. Import Clubs</h2>
            <p className="text-gray-500 text-sm mb-6 flex-1">Upload clubs.csv. Maps clubs to leagues but does NOT generate players.</p>
            <input type="file" accept=".csv" className="hidden" ref={clubFileInputRef} onChange={(e) => handleFileUpload(e, 'import-clubs')} />
            <button onClick={() => clubFileInputRef.current?.click()} disabled={isLoading} className="w-full py-3 bg-yellow-600 hover:bg-yellow-500 text-white font-bold rounded-xl transition-colors flex justify-center items-center gap-2 disabled:opacity-50 mt-auto">
              {isLoading ? <Loader2 className="animate-spin" /> : 'UPLOAD CLUBS'}
            </button>
          </div>

          {/* НОВО КАРЕ: IMPORT SCOUT TEMPLATES */}
          <div className="bg-gray-900 border border-gray-800 p-6 rounded-2xl flex flex-col items-center text-center shadow-lg">
            <div className="w-16 h-16 bg-pink-500/10 text-pink-500 rounded-2xl flex items-center justify-center mb-4"><FileText size={32} /></div>
            <h2 className="text-xl font-bold text-white mb-2">4. Scout Templates</h2>
            <p className="text-gray-500 text-sm mb-6 flex-1">Upload scout_templates.csv to define dynamic phrases for scouting reports.</p>
            <input type="file" accept=".csv" className="hidden" ref={scoutFileInputRef} onChange={(e) => handleFileUpload(e, 'import-scout-templates')} />
            <button onClick={() => scoutFileInputRef.current?.click()} disabled={isLoading} className="w-full py-3 bg-pink-600 hover:bg-pink-500 text-white font-bold rounded-xl transition-colors flex justify-center items-center gap-2 disabled:opacity-50 mt-auto">
              {isLoading ? <Loader2 className="animate-spin" /> : 'UPLOAD TEMPLATES'}
            </button>
          </div>

          <div className="bg-gray-900 border border-green-500/30 p-6 rounded-2xl flex flex-col items-center text-center shadow-[0_0_15px_rgba(34,197,94,0.1)] relative overflow-hidden">
            <div className="w-16 h-16 bg-green-500/20 text-green-400 rounded-2xl flex items-center justify-center mb-4 z-10"><Landmark size={32} /></div>
            <h2 className="text-xl font-bold text-white mb-2 z-10">5. Global Economy</h2>
            <p className="text-gray-500 text-sm mb-6 flex-1 z-10">Creates the Central Bank and distributes initial transfer & wage budgets to all imported clubs.</p>
            <button onClick={handleInitializeEconomy} disabled={isLoading} className="w-full py-3 bg-green-600 hover:bg-green-500 text-white font-bold rounded-xl transition-colors flex justify-center items-center gap-2 disabled:opacity-30 z-10 mt-auto">
              {isLoading ? <Loader2 className="animate-spin" /> : 'INITIALIZE ECONOMY'}
            </button>
          </div>

          <div className="bg-gray-900 border border-gray-800 p-6 rounded-2xl flex flex-col items-center text-center relative shadow-lg">
            <div className="w-16 h-16 bg-red-500/10 text-red-500 rounded-2xl flex items-center justify-center mb-4"><ShieldAlert size={32} /></div>
            <h2 className="text-xl font-bold text-white mb-2">6. Squad Compliance</h2>
            <p className="text-gray-500 text-sm mb-4 flex-1">Generates players for all clubs and signs them using the newly allocated club budgets.</p>
            {squadReport && !showReportModal && (
               <button onClick={() => setShowReportModal(true)} className="text-sm text-red-400 hover:text-red-300 underline mb-4 font-bold">View Current Report</button>
            )}
            <div className="flex w-full gap-2 mt-auto">
              <button onClick={handleSquadReport} disabled={isLoading} className="flex-1 py-3 bg-gray-700 hover:bg-gray-600 text-white font-bold rounded-xl transition-colors disabled:opacity-50 flex justify-center items-center">{isLoading ? <Loader2 className="animate-spin" size={20} /> : 'REPORT'}</button>
              <button onClick={handleSquadFix} disabled={isLoading} className="flex-1 py-3 bg-red-600 hover:bg-red-500 text-white font-bold rounded-xl transition-colors disabled:opacity-50 flex justify-center items-center">{isLoading ? <Loader2 className="animate-spin" size={20} /> : 'AUTO-FIX'}</button>
            </div>
          </div>

          <div className="bg-gray-900 border border-gray-800 p-6 rounded-2xl flex flex-col items-center text-center shadow-lg">
            <div className="w-16 h-16 bg-emerald-500/10 text-emerald-500 rounded-2xl flex items-center justify-center mb-4"><Users size={32} /></div>
            <h2 className="text-xl font-bold text-white mb-2">7. Scouting Pool</h2>
            <p className="text-gray-500 text-sm mb-6 flex-1">Generates new free agents into the global pool.</p>
            <button onClick={handleGenerateFreeAgents} disabled={isLoading} className="w-full py-3 bg-emerald-600 hover:bg-emerald-500 text-white font-bold rounded-xl transition-colors flex justify-center items-center gap-2 disabled:opacity-50 mt-auto">
              {isLoading ? <Loader2 className="animate-spin" /> : 'GENERATE AGENTS'}
            </button>
          </div>

          <div className="bg-gray-900 border border-emerald-500/30 p-6 rounded-2xl flex flex-col items-center text-center shadow-[0_0_15px_rgba(16,185,129,0.1)] relative overflow-hidden">
            <div className="absolute top-0 right-0 w-32 h-32 bg-emerald-500/10 blur-[50px] rounded-full pointer-events-none" />
            <div className="w-16 h-16 bg-emerald-500/20 text-emerald-400 rounded-2xl flex items-center justify-center mb-4 z-10"><Play size={32} className="ml-1" /></div>
            <h2 className="text-xl font-bold text-white mb-2 z-10">8. Start New Season</h2>
            <p className="text-gray-500 text-sm mb-6 flex-1 z-10">Initializes the next season. Warning: Only works if there is no currently active season.</p>
            <button onClick={handleStartNewSeason} disabled={isLoading || worldState?.isSeasonActive} className="w-full py-3 bg-emerald-600 hover:bg-emerald-500 text-white font-bold rounded-xl transition-colors flex justify-center items-center gap-2 disabled:opacity-30 z-10 mt-auto">
              {isLoading ? <Loader2 className="animate-spin" /> : 'INITIALIZE SEASON'}
            </button>
          </div>

          <div className="bg-gray-900 border border-gray-800 p-6 rounded-2xl flex flex-col items-center text-center shadow-lg">
            <div className="w-16 h-16 bg-orange-500/10 text-orange-500 rounded-2xl flex items-center justify-center mb-4"><TrendingUp size={32} /></div>
            <h2 className="text-xl font-bold text-white mb-2">9. Initialize Standings</h2>
            <p className="text-gray-500 text-sm mb-6 flex-1">Creates the starting live standings (0 points) for all clubs. Required before generating fixtures.</p>
            <button onClick={handleInitializeStandings} disabled={isLoading || !worldState?.isSeasonActive} className="w-full py-3 bg-orange-600 hover:bg-orange-500 text-white font-bold rounded-xl transition-colors flex justify-center items-center gap-2 disabled:opacity-50 mt-auto">
              {isLoading ? <Loader2 className="animate-spin" /> : 'CREATE STANDINGS'}
            </button>
          </div>

          <div className="bg-gray-900 border border-gray-800 p-6 rounded-2xl flex flex-col items-center text-center shadow-lg">
            <div className="w-16 h-16 bg-purple-500/10 text-purple-500 rounded-2xl flex items-center justify-center mb-4"><Calendar size={32} /></div>
            <h2 className="text-xl font-bold text-white mb-2">10. Match Schedule</h2>
            <p className="text-gray-500 text-sm mb-6 flex-1">Generates the Round-Robin fixtures based on the active standings table.</p>
            <button onClick={handleGenerateSchedule} disabled={isLoading || !worldState?.isSeasonActive} className="w-full py-3 bg-purple-600 hover:bg-purple-500 text-white font-bold rounded-xl transition-colors flex justify-center items-center gap-2 disabled:opacity-50 mt-auto">
              {isLoading ? <Loader2 className="animate-spin" /> : 'GENERATE FIXTURES'}
            </button>
          </div>

          <div className="bg-gray-900 border border-indigo-500/30 p-6 rounded-2xl flex flex-col items-center text-center shadow-[0_0_15px_rgba(99,102,241,0.1)] relative overflow-hidden md:col-span-2 lg:col-span-2">
            <div className="w-16 h-16 bg-indigo-500/20 text-indigo-400 rounded-2xl flex items-center justify-center mb-4 z-10"><PlayCircle size={32} /></div>
            <h2 className="text-xl font-bold text-white mb-2 z-10">11. Simulate Matchday</h2>
            <p className="text-gray-500 text-sm mb-2 flex-1 z-10">Runs the match engine and processes all weekly financial transactions (wages, commissions).</p>
            {worldState?.nextMatchdayDate && (
                <p className="text-xs font-bold text-indigo-400 mb-4 bg-indigo-500/10 px-3 py-1 rounded z-10">
                  Target Date: {new Date(worldState.nextMatchdayDate).toLocaleDateString()}
                </p>
            )}
            <button onClick={handleSimulateMatchday} disabled={isLoading || !worldState?.isSeasonActive} className="w-full md:w-1/2 py-4 bg-indigo-600 hover:bg-indigo-500 text-white font-bold rounded-xl transition-colors flex justify-center items-center gap-2 disabled:opacity-30 z-10 mx-auto">
              {isLoading ? <Loader2 className="animate-spin" /> : 'PLAY MATCHES & PAY WAGES'}
            </button>
          </div>

        </div>
      </div>
      {simulationResultModal.show && (
        <div className="fixed inset-0 bg-black/80 flex items-center justify-center z-50 p-4 animate-in fade-in">
          <div className="bg-gray-900 border border-indigo-500/50 rounded-2xl max-w-md w-full flex flex-col shadow-2xl overflow-hidden">
            <div className="bg-indigo-600 p-6 flex flex-col items-center text-center">
              <Trophy size={48} className="text-white mb-3" />
              <h3 className="text-2xl font-black text-white uppercase tracking-wider">Matchday Complete!</h3>
            </div>
            <div className="p-8 text-center">
              <p className="text-gray-300 text-lg leading-relaxed">{simulationResultModal.message}</p>
              <p className="text-gray-500 text-sm mt-4">Head over to the <span className="text-indigo-400 font-bold">World &gt; Fixtures</span> tab in the App to view detailed scores and player performances.</p>
            </div>
            <div className="p-4 border-t border-gray-800 bg-gray-900 flex justify-center">
               <button 
                onClick={() => setSimulationResultModal({ show: false, message: '' })}
                className="px-8 py-3 bg-indigo-600 hover:bg-indigo-500 text-white font-bold rounded-xl transition-colors w-full"
              >
                CONTINUE
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}