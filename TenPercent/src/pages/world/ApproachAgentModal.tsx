import { useState, useEffect } from 'react';
import { X, Loader2, DollarSign, AlertTriangle, Building2 } from 'lucide-react';
import { API_URL } from '../../config';
import { useAuth } from '../../hooks/useAuth';

interface ApproachAgentModalProps {
  player: any;
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (message: string) => void;
}

export default function ApproachAgentModal({ player, isOpen, onClose, onSuccess }: ApproachAgentModalProps) {
  const [buyoutOffer, setBuyoutOffer] = useState<number>(50000);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  const { getUserIdOrRedirect } = useAuth();

  useEffect(() => {
    if (isOpen) {
      setError('');
      // Залагаме разумна стартова сума базирана на пазарната стойност на играча (напр. 5%)
      if (player?.marketValue) {
        setBuyoutOffer(Math.max(50000, Math.round(player.marketValue * 0.05 / 1000) * 1000));
      } else {
        setBuyoutOffer(50000);
      }
    }
  }, [isOpen, player]);

  if (!isOpen || !player) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError('');

    const userId = getUserIdOrRedirect();
    if (!userId) return;

    try {
      const response = await fetch(`${API_URL}/negotiations/approach-agent?userId=${userId}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ 
          targetPlayerId: player.id || player.playerId, 
          offeredAmount: buyoutOffer 
        })
      });

      const data = await response.json();

      if (!response.ok) throw new Error(data.message || 'Error sending approach.');
      
      onSuccess(data.message);
    } catch (err: any) {
      setError(err.message);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black/80 flex items-center justify-center z-50 p-4 animate-in fade-in">
      <div className="bg-gray-900 border border-purple-500/50 rounded-2xl max-w-md w-full flex flex-col shadow-2xl overflow-hidden relative">
        <button onClick={onClose} className="absolute top-4 right-4 text-gray-500 hover:text-white transition-colors z-10">
          <X size={24} />
        </button>

        <div className="bg-gray-800 p-6 border-b border-gray-700 relative overflow-hidden">
          <h3 className="text-2xl font-black text-white uppercase tracking-wider mb-1 flex items-center gap-2">
            <Building2 className="text-purple-500" /> Agency Approach
          </h3>
          <p className="text-gray-400 text-sm">
            Запитване към: <span className="text-purple-400 font-bold">{player.agencyName}</span>
          </p>
          <p className="text-gray-400 text-sm">
            Относно: <span className="text-white font-bold">{player.name}</span>
          </p>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-5">
          {error && (
            <div className="p-3 bg-red-500/10 border border-red-500/50 rounded-lg text-red-500 text-sm font-bold flex gap-2 items-center">
              <AlertTriangle size={18} className="shrink-0"/> {error}
            </div>
          )}

          <div className="p-4 bg-purple-500/10 border border-purple-500/30 rounded-xl text-purple-200 text-sm mb-4">
            <p>Изпращате официално запитване за откупуване на правата (Buyout). Ако агенцията приеме офертата, вие ще трябва да договорите новите условия директно с играча.</p>
          </div>

          <div>
            <label className="block text-sm font-bold text-gray-400 mb-2 flex items-center gap-2">
              <DollarSign size={16}/> Offered Buyout Fee
            </label>
            <input 
              type="number" 
              value={buyoutOffer} 
              onChange={e => setBuyoutOffer(Number(e.target.value))} 
              className="w-full bg-gray-950 border border-gray-700 rounded-lg py-4 px-4 text-white focus:border-purple-500 font-mono text-xl transition-all" 
              min="0" 
              step="5000" 
            />
          </div>

          <div className="pt-4 border-t border-gray-800">
            <button 
              type="submit" 
              disabled={isLoading} 
              className="w-full py-4 font-black rounded-xl transition-all flex justify-center items-center gap-2 disabled:opacity-50 bg-purple-600 text-white hover:bg-purple-500"
            >
              {isLoading ? <Loader2 className="animate-spin" /> : 'SEND INQUIRY'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}