import { useState, useEffect } from 'react';
import { X, Loader2, DollarSign, Percent, AlertTriangle, CheckCircle2, XCircle } from 'lucide-react';
import { API_URL } from '../../config';
import { useAuth } from '../../hooks/useAuth';
import { useAgencyStore } from '../../store/useAgencyStore';

interface OfferModalProps {
  player: any;
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (message: string) => void;
}

export default function OfferRepresentationModal({ player, isOpen, onClose, onSuccess }: OfferModalProps) {
  // Състояния за нормален договор (Pitch / Renew)
  const [signingBonus, setSigningBonus] = useState<number>(0);
  const [wageComm, setWageComm] = useState<number>(10);
  const [transferComm, setTransferComm] = useState<number>(10);
  const [duration, setDuration] = useState<number>(3);
  
  // Състояние за откупуване (Approach Agent)
  const [buyoutOffer, setBuyoutOffer] = useState<number>(50000);
  
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  const [negotiationState, setNegotiationState] = useState<'idle' | 'countered' | 'rejected'>('idle');
  const [aiMessage, setAiMessage] = useState('');

  const { getUserIdOrRedirect } = useAuth();
  const { updateBudget } = useAgencyStore(); 

  useEffect(() => {
    if (isOpen) {
      setNegotiationState('idle');
      setError('');
      setAiMessage('');
      setSigningBonus(0);
      setWageComm(10);
      setTransferComm(10);
      setDuration(3);
      setBuyoutOffer(50000);
    }
  }, [isOpen, player?.id]);

  if (!isOpen) return null;

  // ЛОГИКА ЗА ИЗПРАЩАНЕ НА ОФЕРТА КЪМ ДРУГА АГЕНЦИЯ (Buyout)
  const handleApproachSubmit = async (e: React.FormEvent) => {
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

  // ЛОГИКА ЗА ПРЕГОВОРИ СЪС СВОБОДЕН ИГРАЧ ИЛИ ПРЕПОДПИСВАНЕ (Pitch / Renew)
  const handleContractSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError('');

    const userId = getUserIdOrRedirect();
    if (!userId) return;

    // ВНИМАНИЕ: Тук player.hasAgency се отнася до това дали ТИ преподписваш с него (Renew) или е свободен (Propose).
    // Тъй като ако има ДРУГ агент изобщо не влизаме в тази функция (а в handleApproachSubmit), 
    // тук player.hasAgency = true означава, че е ТВОЙ клиент!
    const endpoint = player.hasAgency ? 'renew' : 'propose';

    try {
      const response = await fetch(`${API_URL}/negotiations/${endpoint}?userId=${userId}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          playerId: player.id || player.playerId,
          durationYears: duration,
          wageCommissionPercentage: wageComm,
          transferCommissionPercentage: transferComm,
          signingBonusPaid: signingBonus,
          agencyReleaseClause: signingBonus * 2 
        }),
      });

      const data = await response.json();

      if (!response.ok) throw new Error(data.message || 'Грешка при преговорите.');
      
      if (data.status === 'Accepted') {
        updateBudget(-signingBonus);
        onSuccess(data.message);
      } 
      else if (data.status === 'CounterOffer') {
        setNegotiationState('countered');
        setAiMessage(data.message);
        if (data.counterSigningBonus !== undefined) setSigningBonus(data.counterSigningBonus);
        if (data.counterWageCommission !== undefined) setWageComm(data.counterWageCommission);
        if (data.counterTransferCommission !== undefined) setTransferComm(data.counterTransferCommission);
      } 
      else if (data.status === 'Rejected') {
        setNegotiationState('rejected');
        setAiMessage(data.message);
      }

    } catch (err: any) {
      setError(err.message);
    } finally {
      setIsLoading(false);
    }
  };

  // ОПРЕДЕЛЯМЕ КОЯ ФОРМА ДА ПОКАЖЕМ
  // Ако играчът има агент И името на агента не съвпада с името на твоята агенция, значи се опитваме да го "откраднем".
  // Тъй като в PlayerDetails подаваме player.agencyName, можем да го ползваме, но по-сигурният начин е просто да проверим `player.agencyName !== "Unrepresented"`.
  // Забележка: В PlayerDetails вече проверихме `!isOurClient`, така че щом сме тук и има агенция, значи е чужда!
  const isApproachingForeignAgent = player.agencyName && player.agencyName !== "Unrepresented";

  return (
    <div className="fixed inset-0 bg-black/80 flex items-center justify-center z-50 p-4 animate-in fade-in">
      <div className={`bg-gray-900 border ${isApproachingForeignAgent ? 'border-purple-500/50' : 'border-yellow-500/50'} rounded-2xl max-w-lg w-full flex flex-col shadow-2xl overflow-hidden relative`}>
        <button onClick={onClose} className="absolute top-4 right-4 text-gray-500 hover:text-white transition-colors z-10">
          <X size={24} />
        </button>

        <div className="bg-gray-800 p-6 border-b border-gray-700 relative overflow-hidden">
          <h3 className="text-2xl font-black text-white uppercase tracking-wider mb-1">
            {isApproachingForeignAgent ? 'Agency Approach' : (player.hasAgency ? 'Contract Renewal' : 'Contract Negotiation')}
          </h3>
          <p className="text-gray-400 text-sm">
            {isApproachingForeignAgent ? 'Предлагате сума на текущия агент: ' : 'Представител: '}
            <span className={isApproachingForeignAgent ? "text-purple-400 font-bold" : "text-yellow-500 font-bold"}>
              {isApproachingForeignAgent ? player.agencyName : player.name}
            </span>
          </p>
        </div>

        <form onSubmit={isApproachingForeignAgent ? handleApproachSubmit : handleContractSubmit} className="p-6 space-y-5">
          {error && <div className="p-3 bg-red-500/10 border border-red-500/50 rounded-lg text-red-500 text-sm font-bold flex gap-2 items-center"><AlertTriangle size={18}/> {error}</div>}

          {/* ========================================================================= */}
          {/* СЦЕНАРИЙ 1: ПИТАМЕ ДРУГА АГЕНЦИЯ ЗА ИГРАЧ (BUYOUT OFFER) */}
          {/* ========================================================================= */}
          {isApproachingForeignAgent ? (
            <>
              <div className="p-4 bg-purple-500/10 border border-purple-500/30 rounded-xl text-purple-200 text-sm mb-4">
                <p>Изпращате запитване за откупуване на правата (Buyout). Ако агенцията приеме офертата, вие ще трябва да договорите новите условия директно с играча след това.</p>
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
                <button type="submit" disabled={isLoading} className="w-full py-4 font-black rounded-xl transition-all flex justify-center items-center gap-2 disabled:opacity-50 bg-purple-600 text-white hover:bg-purple-500">
                  {isLoading ? <Loader2 className="animate-spin" /> : 'SEND INQUIRY'}
                </button>
              </div>
            </>
          ) : (
            /* ========================================================================= */
            /* СЦЕНАРИЙ 2: ПРЕГОВАРЯМЕ С ИГРАЧА ЗА ДОГОВОР (PITCH / RENEW) */
            /* ========================================================================= */
            <>
              {negotiationState === 'countered' && (
                <div className="p-4 bg-yellow-500/10 border border-yellow-500/50 rounded-lg text-yellow-500 text-sm flex gap-3 items-start">
                  <AlertTriangle size={24} className="shrink-0 mt-1" />
                  <div>
                    <p className="font-bold mb-1">КОНТРА-ОФЕРТА!</p>
                    <p>{aiMessage}</p>
                    <p className="mt-2 text-xs text-yellow-500/70">Полетата по-долу са обновени с неговите искания.</p>
                  </div>
                </div>
              )}

              {negotiationState === 'rejected' && (
                <div className="p-4 bg-red-500/10 border border-red-500/50 rounded-lg text-red-500 text-sm flex gap-3 items-start">
                  <XCircle size={24} className="shrink-0 mt-1" />
                  <div>
                    <p className="font-bold mb-1">ПРЕГОВОРИТЕ ПРОПАДНАХА!</p>
                    <p>{aiMessage}</p>
                  </div>
                </div>
              )}

              {negotiationState !== 'rejected' && (
                <>
                  <div>
                    <label className="block text-sm font-bold text-gray-400 mb-2 flex items-center gap-2">
                      <DollarSign size={16}/> Signing Bonus (Cash on Hand)
                    </label>
                    <input type="number" value={signingBonus} onChange={e => setSigningBonus(Number(e.target.value))} className={`w-full bg-gray-950 border ${negotiationState === 'countered' ? 'border-yellow-500 shadow-[0_0_10px_rgba(234,179,8,0.2)]' : 'border-gray-700'} rounded-lg py-3 px-4 text-white focus:border-yellow-500 font-mono text-lg transition-all`} min="0" step="1000" />
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-bold text-gray-400 mb-2 flex items-center gap-2"><Percent size={16}/> Wage Cut (%)</label>
                      <input type="number" value={wageComm} onChange={e => setWageComm(Number(e.target.value))} className={`w-full bg-gray-950 border ${negotiationState === 'countered' ? 'border-yellow-500' : 'border-gray-700'} rounded-lg py-3 px-4 text-white focus:border-yellow-500 font-mono transition-all`} min="0" max="15" step="0.1" />
                    </div>
                    <div>
                      <label className="block text-sm font-bold text-gray-400 mb-2 flex items-center gap-2"><Percent size={16}/> Transfer Cut (%)</label>
                      <input type="number" value={transferComm} onChange={e => setTransferComm(Number(e.target.value))} className={`w-full bg-gray-950 border ${negotiationState === 'countered' ? 'border-yellow-500' : 'border-gray-700'} rounded-lg py-3 px-4 text-white focus:border-yellow-500 font-mono transition-all`} min="0" max="15" step="0.1" />
                    </div>
                  </div>

                  <div>
                    <label className="block text-sm font-bold text-gray-400 mb-2">Duration (Seasons)</label>
                    <input type="range" value={duration} onChange={e => setDuration(Number(e.target.value))} className="w-full accent-yellow-500" min="1" max="5" />
                    <div className="text-center font-bold text-yellow-500 mt-2">{duration} Seasons</div>
                  </div>
                </>
              )}

              <div className="pt-4 border-t border-gray-800">
                {negotiationState === 'rejected' ? (
                  <button type="button" onClick={onClose} className="w-full py-4 bg-gray-800 hover:bg-gray-700 text-white font-bold rounded-xl transition-colors">CLOSE NEGOTIATIONS</button>
                ) : (
                  <button type="submit" disabled={isLoading} className={`w-full py-4 font-black rounded-xl transition-all flex justify-center items-center gap-2 disabled:opacity-50 ${negotiationState === 'countered' ? 'bg-white text-black hover:bg-gray-200' : 'bg-yellow-500 text-black hover:bg-yellow-400'}`}>
                    {isLoading ? <Loader2 className="animate-spin" /> : (negotiationState === 'countered' ? 'ACCEPT & RESUBMIT' : 'PROPOSE CONTRACT')}
                  </button>
                )}
              </div>
            </>
          )}
        </form>
      </div>
    </div>
  );
} 