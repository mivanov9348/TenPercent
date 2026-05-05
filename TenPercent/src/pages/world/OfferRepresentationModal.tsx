import { useState } from 'react';
import { X, Loader2, DollarSign, Percent } from 'lucide-react';

interface OfferModalProps {
  player: any;
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (message: string) => void;
}

export default function OfferRepresentationModal({ player, isOpen, onClose, onSuccess }: OfferModalProps) {
  const [signingBonus, setSigningBonus] = useState<number>(0);
  const [wageComm, setWageComm] = useState<number>(10);
  const [transferComm, setTransferComm] = useState<number>(10);
  const [duration, setDuration] = useState<number>(3);
  
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError('');

    const userId = localStorage.getItem('userId');

    try {
      const response = await fetch(`https://localhost:7135/api/agency/${userId}/offer-contract`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          playerId: player.id,
          durationYears: duration,
          wageCommissionPercentage: wageComm,
          transferCommissionPercentage: transferComm,
          signingBonusPaid: signingBonus,
          agencyReleaseClause: signingBonus * 2 // Пример: неустойката е двойна на бонуса
        }),
      });

      const data = await response.json();

      if (!response.ok) throw new Error(data.message || 'Грешка при преговорите.');
      
      if (!data.accepted) {
        setError(data.message); // Играчът отказва
      } else {
        onSuccess(data.message); // Играчът приема!
      }
    } catch (err: any) {
      setError(err.message);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black/80 flex items-center justify-center z-50 p-4 animate-in fade-in">
      <div className="bg-gray-900 border border-yellow-500/50 rounded-2xl max-w-lg w-full flex flex-col shadow-2xl overflow-hidden relative">
        
        <button onClick={onClose} className="absolute top-4 right-4 text-gray-500 hover:text-white transition-colors">
          <X size={24} />
        </button>

        <div className="bg-gray-800 p-6 border-b border-gray-700">
          <h3 className="text-2xl font-black text-white uppercase tracking-wider mb-1">Pitch Player</h3>
          <p className="text-gray-400 text-sm">Предложи договор на <span className="text-yellow-500 font-bold">{player.name}</span></p>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-5">
          {error && <div className="p-3 bg-red-500/10 border border-red-500/50 rounded-lg text-red-500 text-sm font-bold">{error}</div>}

          <div>
            <label className="block text-sm font-bold text-gray-400 mb-2 flex items-center gap-2"><DollarSign size={16}/> Signing Bonus (Cash on Hand)</label>
            <input type="number" value={signingBonus} onChange={e => setSigningBonus(Number(e.target.value))} className="w-full bg-gray-950 border border-gray-700 rounded-lg py-3 px-4 text-white focus:border-yellow-500 font-mono text-lg" min="0" step="10000" />
            <p className="text-xs text-gray-500 mt-1">Алчните играчи ще искат повече пари на ръка, за да подпишат.</p>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-bold text-gray-400 mb-2 flex items-center gap-2"><Percent size={16}/> Wage Cut (%)</label>
              <input type="number" value={wageComm} onChange={e => setWageComm(Number(e.target.value))} className="w-full bg-gray-950 border border-gray-700 rounded-lg py-3 px-4 text-white focus:border-yellow-500 font-mono" min="0" max="15" />
            </div>
            <div>
              <label className="block text-sm font-bold text-gray-400 mb-2 flex items-center gap-2"><Percent size={16}/> Transfer Cut (%)</label>
              <input type="number" value={transferComm} onChange={e => setTransferComm(Number(e.target.value))} className="w-full bg-gray-950 border border-gray-700 rounded-lg py-3 px-4 text-white focus:border-yellow-500 font-mono" min="0" max="15" />
            </div>
          </div>

          <div>
            <label className="block text-sm font-bold text-gray-400 mb-2">Duration (Years)</label>
            <input type="range" value={duration} onChange={e => setDuration(Number(e.target.value))} className="w-full accent-yellow-500" min="1" max="5" />
            <div className="text-center font-bold text-yellow-500 mt-2">{duration} Years</div>
          </div>

          <div className="pt-4 border-t border-gray-800">
            <button type="submit" disabled={isLoading} className="w-full py-4 bg-yellow-500 hover:bg-yellow-400 text-black font-black rounded-xl transition-colors flex justify-center items-center gap-2 disabled:opacity-50">
              {isLoading ? <Loader2 className="animate-spin" /> : 'SEND PROPOSAL'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}