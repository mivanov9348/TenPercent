import { Briefcase, X } from 'lucide-react';
import type { Message } from './Inbox';

interface OfferModalProps {
  isOpen: boolean;
  onClose: () => void;
  message: Message | undefined;
  onRespond: (isAccepted: boolean) => void;
}

export default function OfferModal({ isOpen, onClose, message, onRespond }: OfferModalProps) {
  // Ако модалът не е отворен или няма избрано съобщение, не рендираме нищо
  if (!isOpen || !message) return null;

  return (
    <div className="absolute inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4 animate-in fade-in duration-200">
      <div className="bg-gray-800 border border-gray-600 rounded-2xl shadow-2xl w-full max-w-md overflow-hidden flex flex-col animate-in zoom-in-95 duration-200">
        
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
          <p className="text-gray-400 mb-2">Изпращач: <span className="text-white font-bold">{message.senderName}</span></p>
          
          <div className="bg-gray-900/50 p-6 rounded-xl border border-yellow-500/30 text-center my-6">
            <p className="text-yellow-500 text-sm font-bold uppercase tracking-wider mb-2">Предложена Сума</p>
            <p className="text-4xl text-white font-black">${message.dataValue?.toLocaleString() || "0"}</p>
          </div>

          <p className="text-sm text-gray-400 text-center">
            Съгласни ли сте да прехвърлите правата на вашия клиент срещу посочената сума?
          </p>
        </div>

        {/* Футър (Бутони) */}
        <div className="p-4 bg-gray-900/80 border-t border-gray-700 flex gap-3 justify-end">
          <button 
            onClick={() => onRespond(false)}
            className="px-5 py-2 rounded-lg font-bold text-gray-300 hover:bg-gray-700 transition-colors"
          >
            ОТХВЪРЛИ
          </button>
          <button 
            onClick={() => onRespond(true)}
            className="px-5 py-2 bg-emerald-600 text-white rounded-lg font-bold hover:bg-emerald-500 transition-colors shadow-lg shadow-emerald-900/30"
          >
            ПРИЕМИ
          </button>
        </div>

      </div>
    </div>
  );
}