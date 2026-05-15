import { Briefcase, X } from 'lucide-react';
import Swal from 'sweetalert2';
import type { Message } from './Inbox';

interface OfferModalProps {
  isOpen: boolean;
  onClose: () => void;
  message: Message | undefined;
  onRespond: (isAccepted: boolean) => void;
}

export default function OfferModal({ isOpen, onClose, message, onRespond }: OfferModalProps) {
  // ПРОВЕРКА 1: Какво влиза в модала? Този лог ще се покаже в конзолата, когато цъкнеш "VIEW OFFER"
  if (isOpen) {
      console.log("--- ПРОВЕРКА В OFFER MODAL ---");
      console.log("Пълен обект message:", message);
      console.log("Стойност на message.currentContract:", message?.currentContract);
  }

  if (!isOpen || !message) return null;

  const handleConfirmAction = async (isAccepted: boolean) => {
    const result = await Swal.fire({
      title: isAccepted ? 'Приемане на офертата' : 'Отхвърляне на офертата',
      text: isAccepted 
        ? `Сигурни ли сте, че искате да прехвърлите правата за $${message.dataValue?.toLocaleString()}?`
        : 'Сигурни ли сте, че искате да отхвърлите тази оферта?',
      icon: isAccepted ? 'question' : 'warning',
      showCancelButton: true,
      confirmButtonColor: isAccepted ? '#10b981' : '#ef4444', 
      cancelButtonColor: '#4b5563', 
      confirmButtonText: isAccepted ? 'Да, приеми!' : 'Да, отхвърли!',
      cancelButtonText: 'Отказ',
      background: '#1f2937',
      color: '#fff'
    });

    if (result.isConfirmed) {
      onRespond(isAccepted);
    }
  };

  return (
    <div className="absolute inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4 animate-in fade-in duration-200">
      <div className="bg-gray-800 border border-gray-600 rounded-2xl shadow-2xl w-full max-w-2xl overflow-hidden flex flex-col animate-in zoom-in-95 duration-200">
        
        {/* Хедър */}
        <div className="bg-gray-900 p-4 border-b border-gray-700 flex justify-between items-center">
          <h3 className="text-white font-bold text-lg flex items-center gap-2">
            <Briefcase className="text-yellow-500" size={20} />
            Детайли на Офертата
          </h3>
          <button 
            onClick={onClose}
            className="text-gray-400 hover:text-white transition-colors"
          >
            <X size={24} />
          </button>
        </div>

        {/* Тяло */}
        <div className="p-6">
          <p className="text-gray-400 mb-6">Оферта от: <span className="text-white font-bold">{message.senderName}</span></p>
          
          {/* GRID ЗА РАЗДЕЛЯНЕ: ОФЕРТА vs ТЕКУЩ ДОГОВОР */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-8">
            
            {/* Панел: Оферта */}
            <div className="bg-yellow-500/10 p-6 rounded-xl border border-yellow-500/30 flex flex-col items-center justify-center text-center">
              <p className="text-yellow-500 text-sm font-bold uppercase tracking-wider mb-2">Предложена Сума</p>
              <p className="text-4xl md:text-5xl text-white font-black">${message.dataValue?.toLocaleString() || "0"}</p>
            </div>

            {/* Панел: Текущ договор */}
            {message.currentContract ? (
              <div className="bg-gray-900/50 p-5 rounded-xl border border-gray-700">
                <p className="text-gray-400 text-xs font-bold uppercase tracking-wider mb-3 border-b border-gray-700 pb-2">
                  Текущ Договор: <span className="text-white">{message.currentContract.playerName}</span>
                </p>
                
                <div className="space-y-3 text-sm">
                  <div className="flex justify-between">
                    <span className="text-gray-500">Валиден до сезон:</span>
                    <span className="text-white font-bold">{message.currentContract.endSeasonNumber}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-500">Заплата (Комисионна):</span>
                    <span className="text-white font-bold">{message.currentContract.wageCommission}%</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-500">Трансфер (Комисионна):</span>
                    <span className="text-white font-bold">{message.currentContract.transferCommission}%</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-500">Откупна Клауза:</span>
                    <span className="text-white font-bold">
                      {message.currentContract.releaseClause 
                        ? `$${message.currentContract.releaseClause.toLocaleString()}` 
                        : 'Няма'}
                    </span>
                  </div>
                </div>
              </div>
            ) : (
              <div className="bg-gray-900/50 p-5 rounded-xl border border-gray-700 flex items-center justify-center text-center">
                <p className="text-gray-500 text-sm">Няма намерени данни за активен договор на този играч.</p>
              </div>
            )}

          </div>

          <p className="text-sm text-gray-400 text-center">
            Съгласни ли сте да прехвърлите правата на играча и да прекратите текущия договор?
          </p>
        </div>

        {/* Футър */}
        <div className="p-4 bg-gray-900/80 border-t border-gray-700 flex gap-3 justify-end">
          <button 
            onClick={() => handleConfirmAction(false)}
            className="px-5 py-2 rounded-lg font-bold text-gray-300 hover:bg-gray-700 transition-colors"
          >
            ОТХВЪРЛИ
          </button>
          <button 
            onClick={() => handleConfirmAction(true)}
            className="px-5 py-2 bg-emerald-600 text-white rounded-lg font-bold hover:bg-emerald-500 transition-colors shadow-lg shadow-emerald-900/30"
          >
            ПРИЕМИ
          </button>
        </div>

      </div>
    </div>
  );
}