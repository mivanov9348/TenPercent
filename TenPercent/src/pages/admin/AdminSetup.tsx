import { useState, useRef, useEffect } from 'react';
import { Database, Loader2, CheckCircle2, AlertCircle, Upload, Landmark, FileText, Power, Globe } from 'lucide-react';

export default function AdminSetup() {
  const [isLoading, setIsLoading] = useState(false);
  const [message, setMessage] = useState<{ text: string, type: 'success' | 'error' } | null>(null);
  const [isEngineOffline, setIsEngineOffline] = useState(false);

  // Рефове за файловете
  const positionFileInputRef = useRef<HTMLInputElement>(null);
  const leagueFileInputRef = useRef<HTMLInputElement>(null);
  const clubFileInputRef = useRef<HTMLInputElement>(null);
  const scoutFileInputRef = useRef<HTMLInputElement>(null);

  // Проверяваме дали светът съществува
  useEffect(() => {
    const checkWorld = async () => {
      try {
        const response = await fetch('https://localhost:7135/api/admin/world-state');
        if (response.status === 404) setIsEngineOffline(true);
      } catch (e) {
        console.error(e);
      }
    };
    checkWorld();
  }, []);

  const handleInitializeWorld = async () => {
    setIsLoading(true);
    setMessage(null);
    try {
      const response = await fetch('https://localhost:7135/api/admin/initialize-world', { method: 'POST' });
      const data = await response.json();
      if (!response.ok) throw new Error(data.message || "Failed to initialize world");
      setMessage({ text: data.message, type: 'success' });
      setIsEngineOffline(false);
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

  if (isEngineOffline) {
    return (
      <div className="flex flex-col items-center justify-center p-8 relative overflow-hidden h-[80vh]">
        <div className="z-10 text-center max-w-lg">
          <div className="w-24 h-24 bg-blue-500/10 text-blue-500 rounded-full flex items-center justify-center mx-auto mb-8 animate-pulse border border-blue-500/20">
            <Power size={48} />
          </div>
          <h1 className="text-5xl font-black text-white uppercase tracking-widest mb-4">World Engine Offline</h1>
          <p className="text-gray-400 mb-12 text-lg">The simulation database is empty. You need to initialize the World Engine before you can import leagues, clubs, or players.</p>
          <button onClick={handleInitializeWorld} disabled={isLoading} className="w-full py-5 bg-blue-600 hover:bg-blue-500 text-white font-black rounded-2xl transition-all hover:scale-105 active:scale-95 flex justify-center items-center gap-3 text-lg shadow-[0_0_30px_rgba(37,99,235,0.3)] disabled:opacity-50">
            {isLoading ? <Loader2 className="animate-spin" size={24} /> : <><Globe size={24} /> INITIALIZE WORLD ENGINE</>}
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-6xl mx-auto space-y-6">
      <header className="bg-gray-900 p-6 rounded-2xl border border-gray-800 shadow-xl flex items-center gap-4">
        <div className="w-14 h-14 bg-cyan-500/10 rounded-2xl flex items-center justify-center border border-cyan-500/20">
          <Database size={28} className="text-cyan-500" />
        </div>
        <div>
          <h1 className="text-2xl font-black text-white uppercase tracking-widest leading-tight">World Setup</h1>
          <p className="text-gray-400 text-sm font-bold mt-1">Import base data and initialize economy</p>
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
          <p className="text-gray-500 text-sm mb-6 flex-1">Upload positions.csv. Defines tactical roles.</p>
          <input type="file" accept=".csv" className="hidden" ref={positionFileInputRef} onChange={(e) => handleFileUpload(e, 'import-positions')} />
          <button onClick={() => positionFileInputRef.current?.click()} disabled={isLoading} className="w-full py-3 bg-cyan-600 hover:bg-cyan-500 text-white font-bold rounded-xl transition-colors flex justify-center items-center gap-2 disabled:opacity-50 mt-auto">
            {isLoading ? <Loader2 className="animate-spin" /> : 'UPLOAD POSITIONS'}
          </button>
        </div>

        <div className="bg-gray-900 border border-gray-800 p-6 rounded-2xl flex flex-col items-center text-center shadow-lg">
          <div className="w-16 h-16 bg-blue-500/10 text-blue-500 rounded-2xl flex items-center justify-center mb-4"><Upload size={32} /></div>
          <h2 className="text-xl font-bold text-white mb-2">2. Import Leagues</h2>
          <p className="text-gray-500 text-sm mb-6 flex-1">Upload leagues.csv. Creates base league structures.</p>
          <input type="file" accept=".csv" className="hidden" ref={leagueFileInputRef} onChange={(e) => handleFileUpload(e, 'import-leagues')} />
          <button onClick={() => leagueFileInputRef.current?.click()} disabled={isLoading} className="w-full py-3 bg-blue-600 hover:bg-blue-500 text-white font-bold rounded-xl transition-colors flex justify-center items-center gap-2 disabled:opacity-50 mt-auto">
            {isLoading ? <Loader2 className="animate-spin" /> : 'UPLOAD LEAGUES'}
          </button>
        </div>

        <div className="bg-gray-900 border border-gray-800 p-6 rounded-2xl flex flex-col items-center text-center shadow-lg">
          <div className="w-16 h-16 bg-yellow-500/10 text-yellow-500 rounded-2xl flex items-center justify-center mb-4"><Database size={32} /></div>
          <h2 className="text-xl font-bold text-white mb-2">3. Import Clubs</h2>
          <p className="text-gray-500 text-sm mb-6 flex-1">Upload clubs.csv. Maps clubs to leagues.</p>
          <input type="file" accept=".csv" className="hidden" ref={clubFileInputRef} onChange={(e) => handleFileUpload(e, 'import-clubs')} />
          <button onClick={() => clubFileInputRef.current?.click()} disabled={isLoading} className="w-full py-3 bg-yellow-600 hover:bg-yellow-500 text-white font-bold rounded-xl transition-colors flex justify-center items-center gap-2 disabled:opacity-50 mt-auto">
            {isLoading ? <Loader2 className="animate-spin" /> : 'UPLOAD CLUBS'}
          </button>
        </div>

        <div className="bg-gray-900 border border-gray-800 p-6 rounded-2xl flex flex-col items-center text-center shadow-lg">
          <div className="w-16 h-16 bg-pink-500/10 text-pink-500 rounded-2xl flex items-center justify-center mb-4"><FileText size={32} /></div>
          <h2 className="text-xl font-bold text-white mb-2">4. Scout Templates</h2>
          <p className="text-gray-500 text-sm mb-6 flex-1">Upload scout_templates.csv for dynamic phrases.</p>
          <input type="file" accept=".csv" className="hidden" ref={scoutFileInputRef} onChange={(e) => handleFileUpload(e, 'import-scout-templates')} />
          <button onClick={() => scoutFileInputRef.current?.click()} disabled={isLoading} className="w-full py-3 bg-pink-600 hover:bg-pink-500 text-white font-bold rounded-xl transition-colors flex justify-center items-center gap-2 disabled:opacity-50 mt-auto">
            {isLoading ? <Loader2 className="animate-spin" /> : 'UPLOAD TEMPLATES'}
          </button>
        </div>

        <div className="bg-gray-900 border border-green-500/30 p-6 rounded-2xl flex flex-col items-center text-center shadow-[0_0_15px_rgba(34,197,94,0.1)] relative overflow-hidden">
          <div className="w-16 h-16 bg-green-500/20 text-green-400 rounded-2xl flex items-center justify-center mb-4 z-10"><Landmark size={32} /></div>
          <h2 className="text-xl font-bold text-white mb-2 z-10">5. Global Economy</h2>
          <p className="text-gray-500 text-sm mb-6 flex-1 z-10">Creates Central Bank & distributes budgets.</p>
          <button onClick={handleInitializeEconomy} disabled={isLoading} className="w-full py-3 bg-green-600 hover:bg-green-500 text-white font-bold rounded-xl transition-colors flex justify-center items-center gap-2 disabled:opacity-30 z-10 mt-auto">
            {isLoading ? <Loader2 className="animate-spin" /> : 'INITIALIZE ECONOMY'}
          </button>
        </div>
      </div>
    </div>
  );
}