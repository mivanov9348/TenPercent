import { useState, useEffect } from 'react';
import { Settings, Save, Loader2, AlertCircle, CheckCircle2, Percent, DollarSign, Building } from 'lucide-react';
import { API_URL } from '../../config';

interface EconomySettingsDto {
  id: number;
  agencyStartupGrant: number;
  agencyIncomeTaxRate: number;
  initialBankReserve: number;
  clubBaseGrant: number;
  clubReputationMultiplier: number;
  clubWageBudgetPercentage: number;
  globalIncomeTax: number;
}

export default function AdminSettings() {
  const [settings, setSettings] = useState<EconomySettingsDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [message, setMessage] = useState<{ text: string, type: 'success' | 'error' } | null>(null);

  useEffect(() => {
    const fetchSettings = async () => {
      try {
        const response = await fetch(`${API_URL}/admin/settings`);
        if (response.ok) {
          const data = await response.json();
          setSettings(data);
        }
      } catch (error) {
        console.error("Failed to fetch settings", error);
      } finally {
        setIsLoading(false);
      }
    };
    fetchSettings();
  }, []);

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!settings) return;
    
    setIsSaving(true);
    setMessage(null);

    try {
      const response = await fetch(`${API_URL}/admin/settings`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(settings),
      });

      const data = await response.json();
      if (!response.ok) throw new Error(data.message || 'Грешка при запазване');
      
      setMessage({ text: data.message, type: 'success' });
    } catch (err: any) {
      setMessage({ text: err.message, type: 'error' });
    } finally {
      setIsSaving(false);
      setTimeout(() => setMessage(null), 3000); // Скрива съобщението след 3 секунди
    }
  };

  const handleChange = (field: keyof EconomySettingsDto, value: number) => {
    if (settings) {
      setSettings({ ...settings, [field]: value });
    }
  };

  if (isLoading || !settings) {
    return <div className="flex h-[80vh] items-center justify-center"><Loader2 className="animate-spin text-gray-500" size={48} /></div>;
  }

  return (
    <div className="space-y-6 animate-in fade-in max-w-4xl mx-auto">
      <div>
        <h1 className="text-3xl font-black text-white flex items-center gap-3 mb-2">
          <Settings className="text-gray-400" size={32} />
          ECONOMY SETTINGS
        </h1>
        <p className="text-gray-400">Управлявай глобалните данъци и формулите за разпределение на пари.</p>
      </div>

      {message && (
        <div className={`p-4 rounded-xl flex items-center gap-3 border font-bold ${message.type === 'success' ? 'bg-emerald-500/10 border-emerald-500/30 text-emerald-400' : 'bg-red-500/10 border-red-500/30 text-red-400'}`}>
          {message.type === 'success' ? <CheckCircle2 /> : <AlertCircle />}
          <p>{message.text}</p>
        </div>
      )}

      <form onSubmit={handleSave} className="space-y-8">
        
        {/* Данъци */}
        <div className="bg-gray-900 border border-gray-800 rounded-2xl p-6">
          <h2 className="text-xl font-bold text-white mb-4 flex items-center gap-2 border-b border-gray-800 pb-3">
            <Percent className="text-yellow-500" size={20} />
            Taxation Rates
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label className="block text-sm font-bold text-gray-400 mb-2">Global Income Tax (e.g. 0.10 = 10%)</label>
              <input type="number" step="0.01" min="0" max="1" value={settings.globalIncomeTax} onChange={(e) => handleChange('globalIncomeTax', Number(e.target.value))} className="w-full bg-gray-950 border border-gray-700 rounded-lg py-3 px-4 text-white focus:border-yellow-500 font-mono" />
            </div>
            <div>
              <label className="block text-sm font-bold text-gray-400 mb-2">Agency Corporate Tax</label>
              <input type="number" step="0.01" min="0" max="1" value={settings.agencyIncomeTaxRate} onChange={(e) => handleChange('agencyIncomeTaxRate', Number(e.target.value))} className="w-full bg-gray-950 border border-gray-700 rounded-lg py-3 px-4 text-white focus:border-yellow-500 font-mono" />
            </div>
          </div>
        </div>

        {/* Стартови Грантове */}
        <div className="bg-gray-900 border border-gray-800 rounded-2xl p-6">
          <h2 className="text-xl font-bold text-white mb-4 flex items-center gap-2 border-b border-gray-800 pb-3">
            <DollarSign className="text-emerald-500" size={20} />
            Startup Grants & Reserves
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label className="block text-sm font-bold text-gray-400 mb-2">Agency Startup Grant ($)</label>
              <input type="number" step="10000" min="0" value={settings.agencyStartupGrant} onChange={(e) => handleChange('agencyStartupGrant', Number(e.target.value))} className="w-full bg-gray-950 border border-gray-700 rounded-lg py-3 px-4 text-white focus:border-emerald-500 font-mono" />
            </div>
            <div>
              <label className="block text-sm font-bold text-gray-400 mb-2">Initial Bank Reserve ($)</label>
              <input type="number" step="10000000" min="0" value={settings.initialBankReserve} onChange={(e) => handleChange('initialBankReserve', Number(e.target.value))} className="w-full bg-gray-950 border border-gray-700 rounded-lg py-3 px-4 text-white focus:border-emerald-500 font-mono" />
            </div>
          </div>
        </div>

        {/* Клубове */}
        <div className="bg-gray-900 border border-gray-800 rounded-2xl p-6">
          <h2 className="text-xl font-bold text-white mb-4 flex items-center gap-2 border-b border-gray-800 pb-3">
            <Building className="text-blue-500" size={20} />
            Club Allocation Formula
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div>
              <label className="block text-sm font-bold text-gray-400 mb-2">Base Grant ($)</label>
              <input type="number" step="100000" min="0" value={settings.clubBaseGrant} onChange={(e) => handleChange('clubBaseGrant', Number(e.target.value))} className="w-full bg-gray-950 border border-gray-700 rounded-lg py-3 px-4 text-white focus:border-blue-500 font-mono" />
            </div>
            <div>
              <label className="block text-sm font-bold text-gray-400 mb-2">Reputation Multiplier ($)</label>
              <input type="number" step="50000" min="0" value={settings.clubReputationMultiplier} onChange={(e) => handleChange('clubReputationMultiplier', Number(e.target.value))} className="w-full bg-gray-950 border border-gray-700 rounded-lg py-3 px-4 text-white focus:border-blue-500 font-mono" />
            </div>
            <div>
              <label className="block text-sm font-bold text-gray-400 mb-2">Wage Budget (%)</label>
              <input type="number" step="0.01" min="0" max="1" value={settings.clubWageBudgetPercentage} onChange={(e) => handleChange('clubWageBudgetPercentage', Number(e.target.value))} className="w-full bg-gray-950 border border-gray-700 rounded-lg py-3 px-4 text-white focus:border-blue-500 font-mono" />
            </div>
          </div>
        </div>

        <button type="submit" disabled={isSaving} className="w-full py-4 bg-white hover:bg-gray-200 text-black font-black rounded-xl transition-all flex justify-center items-center gap-2 disabled:opacity-50">
          {isSaving ? <Loader2 className="animate-spin" /> : <><Save size={20}/> SAVE SETTINGS</>}
        </button>

      </form>
    </div>
  );
}