import { useState, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { Database, Calendar, LogOut, Loader2, CheckCircle2, AlertCircle, Upload, ShieldAlert, X, Users } from 'lucide-react';

export default function AdminDashboard() {
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);
  const [message, setMessage] = useState<{ text: string, type: 'success' | 'error' } | null>(null);

  // Състояние за репорта
  const [squadReport, setSquadReport] = useState<any[] | null>(null);
  const [showReportModal, setShowReportModal] = useState(false);

  // References to trigger hidden file inputs
  const leagueFileInputRef = useRef<HTMLInputElement>(null);
  const clubFileInputRef = useRef<HTMLInputElement>(null);

  const handleLogout = () => {
    localStorage.clear();
    navigate('/login');
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
      if(event.target) {
         event.target.value = ''; 
      }
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
      // Изчистваме репорта след успешен fix
      setSquadReport(null);
      setShowReportModal(false);
    } catch (err: any) {
      setMessage({ text: err.message, type: 'error' });
    } finally {
      setIsLoading(false);
    }
  };

  // НОВО: Метод за генериране на Свободни Агенти в пула
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

  return (
    <div className="min-h-screen bg-gray-950 p-8 relative">
      <div className="max-w-6xl mx-auto">
        
        <header className="flex justify-between items-center bg-gray-900 p-6 rounded-2xl border border-gray-800 mb-8 shadow-lg">
          <div>
            <h1 className="text-3xl font-black text-red-500 uppercase tracking-widest">Admin Control Panel</h1>
            <p className="text-gray-400 mt-1">Server, database, and game world management</p>
          </div>
          <button onClick={handleLogout} className="flex items-center gap-2 bg-gray-800 hover:bg-red-500/20 hover:text-red-500 text-gray-300 px-4 py-2 rounded-lg transition-colors">
            <LogOut size={18} /> Logout
          </button>
        </header>

        {message && (
          <div className={`mb-8 p-4 rounded-xl flex items-center gap-3 border ${
            message.type === 'success' ? 'bg-emerald-500/10 border-emerald-500/50 text-emerald-400' : 'bg-red-500/10 border-red-500/50 text-red-400'
          }`}>
            {message.type === 'success' ? <CheckCircle2 /> : <AlertCircle />}
            <p className="font-medium">{message.text}</p>
          </div>
        )}

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          
          {/* Action 1: Import Leagues */}
          <div className="bg-gray-900 border border-gray-800 p-6 rounded-2xl flex flex-col items-center text-center shadow-lg">
            <div className="w-16 h-16 bg-blue-500/10 text-blue-500 rounded-2xl flex items-center justify-center mb-4">
              <Upload size={32} />
            </div>
            <h2 className="text-xl font-bold text-white mb-2">1. Import Leagues</h2>
            <p className="text-gray-500 text-sm mb-6 flex-1">Upload leagues.csv. Initializes the world state and creates the league structures.</p>
            
            <input 
              type="file" 
              accept=".csv" 
              className="hidden" 
              ref={leagueFileInputRef} 
              onChange={(e) => handleFileUpload(e, 'import-leagues')} 
            />
            <button 
              onClick={() => leagueFileInputRef.current?.click()}
              disabled={isLoading}
              className="w-full py-3 bg-blue-600 hover:bg-blue-500 text-white font-bold rounded-xl transition-colors flex justify-center items-center gap-2 disabled:opacity-50"
            >
              {isLoading ? <Loader2 className="animate-spin" /> : 'UPLOAD LEAGUES'}
            </button>
          </div>

          {/* Action 2: Import Clubs */}
          <div className="bg-gray-900 border border-gray-800 p-6 rounded-2xl flex flex-col items-center text-center shadow-lg">
            <div className="w-16 h-16 bg-yellow-500/10 text-yellow-500 rounded-2xl flex items-center justify-center mb-4">
              <Database size={32} />
            </div>
            <h2 className="text-xl font-bold text-white mb-2">2. Import Clubs</h2>
            <p className="text-gray-500 text-sm mb-6 flex-1">Upload clubs.csv. This maps clubs to leagues but does NOT generate players.</p>
            
            <input 
              type="file" 
              accept=".csv" 
              className="hidden" 
              ref={clubFileInputRef} 
              onChange={(e) => handleFileUpload(e, 'import-clubs')} 
            />
            <button 
              onClick={() => clubFileInputRef.current?.click()}
              disabled={isLoading}
              className="w-full py-3 bg-yellow-600 hover:bg-yellow-500 text-white font-bold rounded-xl transition-colors flex justify-center items-center gap-2 disabled:opacity-50"
            >
              {isLoading ? <Loader2 className="animate-spin" /> : 'UPLOAD CLUBS'}
            </button>
          </div>

          {/* Action 3: Generate Schedule */}
          <div className="bg-gray-900 border border-gray-800 p-6 rounded-2xl flex flex-col items-center text-center shadow-lg">
            <div className="w-16 h-16 bg-purple-500/10 text-purple-500 rounded-2xl flex items-center justify-center mb-4">
              <Calendar size={32} />
            </div>
            <h2 className="text-xl font-bold text-white mb-2">3. Match Schedule</h2>
            <p className="text-gray-500 text-sm mb-6 flex-1">Generates the Round-Robin fixtures for all imported teams for Season 1.</p>
            <button 
              onClick={handleGenerateSchedule}
              disabled={isLoading}
              className="w-full py-3 bg-purple-600 hover:bg-purple-500 text-white font-bold rounded-xl transition-colors flex justify-center items-center gap-2 disabled:opacity-50 mt-auto"
            >
              {isLoading ? <Loader2 className="animate-spin" /> : 'GENERATE FIXTURES'}
            </button>
          </div>

          {/* Action 4: Squad Compliance */}
          <div className="bg-gray-900 border border-gray-800 p-6 rounded-2xl flex flex-col items-center text-center relative shadow-lg">
            <div className="w-16 h-16 bg-red-500/10 text-red-500 rounded-2xl flex items-center justify-center mb-4">
              <ShieldAlert size={32} />
            </div>
            <h2 className="text-xl font-bold text-white mb-2">4. Squad Compliance</h2>
            <p className="text-gray-500 text-sm mb-4 flex-1">Checks if teams have the required positions (GK, DEF, MID, ST) and fills empty spots.</p>
            
            {/* Изобразяване на бутон за отваряне на модала, ако има репорт */}
            {squadReport && !showReportModal && (
               <button onClick={() => setShowReportModal(true)} className="text-sm text-red-400 hover:text-red-300 underline mb-4">
                  View Current Report
               </button>
            )}

            <div className="flex w-full gap-2 mt-auto">
              <button 
                onClick={handleSquadReport}
                disabled={isLoading}
                className="flex-1 py-3 bg-gray-700 hover:bg-gray-600 text-white font-bold rounded-xl transition-colors disabled:opacity-50 flex justify-center items-center"
              >
                {isLoading ? <Loader2 className="animate-spin" size={20} /> : 'REPORT'}
              </button>
              <button 
                onClick={handleSquadFix}
                // Бутонът е активен САМО ако имаме проблемни отбори в репорта
                disabled={isLoading || !squadReport || squadReport.length === 0}
                className="flex-1 py-3 bg-red-600 hover:bg-red-500 text-white font-bold rounded-xl transition-colors disabled:opacity-50 flex justify-center items-center"
              >
                {isLoading ? <Loader2 className="animate-spin" size={20} /> : 'AUTO-FIX'}
              </button>
            </div>
          </div>

          {/* Action 5: Generate Free Agents (НОВО) */}
          <div className="bg-gray-900 border border-gray-800 p-6 rounded-2xl flex flex-col items-center text-center shadow-lg">
            <div className="w-16 h-16 bg-emerald-500/10 text-emerald-500 rounded-2xl flex items-center justify-center mb-4">
              <Users size={32} />
            </div>
            <h2 className="text-xl font-bold text-white mb-2">5. Scouting Pool</h2>
            <p className="text-gray-500 text-sm mb-6 flex-1">Generates 50 new free agents (mixed tiers: Wonderkid, Prospect, Veteran, Backup) into the global pool.</p>
            <button 
              onClick={handleGenerateFreeAgents}
              disabled={isLoading}
              className="w-full py-3 bg-emerald-600 hover:bg-emerald-500 text-white font-bold rounded-xl transition-colors flex justify-center items-center gap-2 disabled:opacity-50 mt-auto"
            >
              {isLoading ? <Loader2 className="animate-spin" /> : 'GENERATE AGENTS'}
            </button>
          </div>

        </div>
      </div>

      {/* Modal for Squad Report */}
      {showReportModal && squadReport && (
        <div className="fixed inset-0 bg-black/80 flex items-center justify-center z-50 p-4">
          <div className="bg-gray-900 border border-red-500/50 rounded-2xl max-w-2xl w-full max-h-[80vh] flex flex-col shadow-2xl">
            <div className="p-6 border-b border-gray-800 flex justify-between items-center">
              <h3 className="text-xl font-bold text-white flex items-center gap-2">
                <AlertCircle className="text-red-500" />
                Compliance Issues ({squadReport.length} clubs)
              </h3>
              <button onClick={() => setShowReportModal(false)} className="text-gray-400 hover:text-white transition-colors">
                <X size={24} />
              </button>
            </div>
            
            {/* Scrollable content area */}
            <div className="p-6 overflow-y-auto space-y-4">
              {squadReport.map((club, index) => (
                <div key={index} className="bg-gray-800 p-4 rounded-xl border border-gray-700">
                   <div className="flex justify-between items-center mb-2">
                     <span className="font-bold text-yellow-500">{club.clubName}</span>
                     <span className="text-xs text-gray-400">Squad size: {club.currentSquadSize}</span>
                   </div>
                   <ul className="list-disc list-inside text-sm text-red-300">
                     {club.missing.map((issue: string, i: number) => (
                       <li key={i}>{issue}</li>
                     ))}
                   </ul>
                </div>
              ))}
            </div>

            <div className="p-6 border-t border-gray-800 bg-gray-900 rounded-b-2xl flex justify-end">
               <button 
                onClick={() => {
                    setShowReportModal(false);
                    handleSquadFix();
                }}
                className="px-6 py-3 bg-red-600 hover:bg-red-500 text-white font-bold rounded-xl transition-colors flex justify-center items-center gap-2"
              >
                FIX ALL ISSUES
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}