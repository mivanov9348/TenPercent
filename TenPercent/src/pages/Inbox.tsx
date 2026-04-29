import { useState } from 'react';
import { Mail, MailOpen, AlertCircle, Briefcase, FileText } from 'lucide-react';

export default function Inbox() {
  // Фейк съобщения
  const [messages, setMessages] = useState([
    { 
      id: 1, 
      sender: "Manchester Red", 
      subject: "Transfer Offer: Marcus Rashford", 
      date: "Today", 
      read: false, 
      type: "offer",
      content: "Уважаеми Агент, бихме искали да предложим нов договор на Вашия клиент Marcus Rashford. Предлагаме седмична заплата от $180,000 и бонус при подписване $2,000,000. Моля, отговорете до 3 дни."
    },
    { 
      id: 2, 
      sender: "Ivan Ivanov", 
      subject: "Concerns about playing time", 
      date: "Yesterday", 
      read: false, 
      type: "player",
      content: "Шефе, не получавам достатъчно минути на терена. Ако нещата не се променят скоро, ще искам да ми намериш нов отбор през зимата."
    },
    { 
      id: 3, 
      sender: "System", 
      subject: "Weekly Financial Report", 
      date: "2 days ago", 
      read: true, 
      type: "system",
      content: "Вашият седмичен финансов отчет е готов. Агенцията е на печалба с $26,500. Можете да прегледате детайлите в секция Finance."
    },
  ]);

  const [activeMessageId, setActiveMessageId] = useState<number | null>(1);

  // Намираме кое е избраното съобщение
  const activeMessage = messages.find(m => m.id === activeMessageId);

  // Маркиране като прочетено
  const handleSelectMessage = (id: number) => {
    setActiveMessageId(id);
    setMessages(messages.map(m => m.id === id ? { ...m, read: true } : m));
  };

  const getIcon = (type: string, read: boolean) => {
    if (!read) return <AlertCircle className="text-yellow-500" size={20} />;
    switch (type) {
      case 'offer': return <Briefcase className="text-blue-400" size={20} />;
      case 'player': return <FileText className="text-emerald-400" size={20} />;
      default: return <MailOpen className="text-gray-500" size={20} />;
    }
  };

  return (
    <div className="h-[calc(100vh-8rem)] bg-gray-800 border border-gray-700 rounded-2xl shadow-lg overflow-hidden flex flex-col md:flex-row">
      
      {/* Ляв панел - Списък със съобщения */}
      <div className="w-full md:w-1/3 border-r border-gray-700 bg-gray-900/50 flex flex-col h-full">
        <div className="p-4 border-b border-gray-700 flex items-center justify-between bg-gray-800">
          <h2 className="text-lg font-bold text-white flex items-center gap-2">
            <Mail className="text-yellow-500" size={20} />
            Inbox
          </h2>
          <span className="bg-yellow-500 text-black text-xs font-bold px-2 py-1 rounded-full">
            {messages.filter(m => !m.read).length} new
          </span>
        </div>
        
        <div className="overflow-y-auto flex-1 p-2 space-y-2">
          {messages.map((msg) => (
            <div 
              key={msg.id}
              onClick={() => handleSelectMessage(msg.id)}
              className={`p-3 rounded-lg cursor-pointer transition-all border ${
                activeMessageId === msg.id 
                  ? 'bg-gray-800 border-yellow-500/50 shadow-inner' 
                  : msg.read 
                    ? 'bg-transparent border-transparent hover:bg-gray-800' 
                    : 'bg-gray-800/80 border-l-4 border-l-yellow-500 border-t-transparent border-r-transparent border-b-transparent hover:bg-gray-800'
              }`}
            >
              <div className="flex items-start gap-3">
                <div className="mt-1 shrink-0">
                  {getIcon(msg.type, msg.read)}
                </div>
                <div className="min-w-0 flex-1">
                  <p className={`text-sm truncate ${msg.read ? 'text-gray-400' : 'text-gray-200 font-bold'}`}>{msg.sender}</p>
                  <p className={`text-sm truncate ${msg.read ? 'text-gray-500' : 'text-white font-bold'}`}>{msg.subject}</p>
                  <p className="text-xs text-gray-600 mt-1">{msg.date}</p>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Десен панел - Съдържание */}
      <div className="flex-1 bg-gray-800 p-6 flex flex-col h-full">
        {activeMessage ? (
          <div className="animate-in fade-in duration-200 h-full flex flex-col">
            <div className="border-b border-gray-700 pb-4 mb-4">
              <h2 className="text-2xl font-black text-white mb-2">{activeMessage.subject}</h2>
              <div className="flex justify-between items-center text-sm text-gray-400">
                <p>From: <span className="text-gray-200 font-bold">{activeMessage.sender}</span></p>
                <p>{activeMessage.date}</p>
              </div>
            </div>
            
            <div className="text-gray-300 whitespace-pre-wrap leading-relaxed flex-1">
              {activeMessage.content}
            </div>

            {/* Бутони за действие (показват се само за определени типове съобщения) */}
            {activeMessage.type === 'offer' && (
              <div className="mt-6 pt-4 border-t border-gray-700 flex gap-4">
                <button className="bg-yellow-500 text-black px-6 py-2 rounded-lg font-bold hover:bg-yellow-400 transition-colors">ENTER NEGOTIATIONS</button>
                <button className="bg-gray-900 text-gray-400 px-6 py-2 rounded-lg font-bold hover:text-white hover:bg-red-500/20 transition-colors">REJECT</button>
              </div>
            )}
            {activeMessage.type === 'player' && (
              <div className="mt-6 pt-4 border-t border-gray-700 flex gap-4">
                <button className="bg-gray-700 text-white px-6 py-2 rounded-lg font-bold hover:bg-gray-600 transition-colors">PROMISE MORE PLAYTIME</button>
                <button className="bg-gray-900 text-gray-400 px-6 py-2 rounded-lg font-bold hover:text-white transition-colors">IGNORE</button>
              </div>
            )}
          </div>
        ) : (
          <div className="h-full flex items-center justify-center text-gray-500">
            Select a message to read.
          </div>
        )}
      </div>

    </div>
  );
}