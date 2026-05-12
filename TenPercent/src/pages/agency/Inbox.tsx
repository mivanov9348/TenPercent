import { useState, useEffect } from 'react';
import { Mail, MailOpen, AlertCircle, Briefcase, FileText, Loader2, Trash2 } from 'lucide-react';
import { API_URL } from '../../config'; // Увери се, че пътят е правилен

interface Message {
  id: number;
  senderName: string;
  subject: string;
  content: string;
  sentAt: string;
  isRead: boolean;
  type: string;
  relatedEntityId: number | null;
}

export default function Inbox() {
  const [messages, setMessages] = useState<Message[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [activeMessageId, setActiveMessageId] = useState<number | null>(null);

  // Взимане на съобщенията при зареждане
  const fetchMessages = async () => {
    setIsLoading(true);
    try {
      const userId = localStorage.getItem('userId');
      if (!userId) return;

      const response = await fetch(`${API_URL}/inbox/${userId}`);
      if (response.ok) {
        const data = await response.json();
        setMessages(data);
        
        // Автоматично отваряме първото непрочетено съобщение (или първото въобще)
        if (data.length > 0 && activeMessageId === null) {
          const firstUnread = data.find((m: Message) => !m.isRead);
          setActiveMessageId(firstUnread ? firstUnread.id : data[0].id);
        }
      }
    } catch (error) {
      console.error("Failed to fetch inbox:", error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchMessages();
  }, []);

  const activeMessage = messages.find(m => m.id === activeMessageId);

  // Маркиране като прочетено в бекенда и фронтенда
  const handleSelectMessage = async (msg: Message) => {
    setActiveMessageId(msg.id);
    
    if (!msg.isRead) {
      // Маркираме локално веднага за бързина
      setMessages(messages.map(m => m.id === msg.id ? { ...m, isRead: true } : m));
      
      const userId = localStorage.getItem('userId');
      try {
        await fetch(`${API_URL}/inbox/${userId}/read/${msg.id}`, { method: 'PUT' });
      } catch (err) {
        console.error("Failed to mark as read", err);
      }
    }
  };

  // Изтриване на съобщение
  const handleDeleteMessage = async (e: React.MouseEvent, id: number) => {
    e.stopPropagation(); // За да не се отвори съобщението при клик върху коша
    if (!window.confirm("Are you sure you want to delete this message?")) return;

    const userId = localStorage.getItem('userId');
    try {
      const response = await fetch(`${API_URL}/inbox/${userId}/delete/${id}`, { method: 'DELETE' });
      if (response.ok) {
        setMessages(messages.filter(m => m.id !== id));
        if (activeMessageId === id) setActiveMessageId(null);
      } else {
        alert("Това съобщение не може да бъде изтрито (вероятно е глобално).");
      }
    } catch (err) {
      console.error("Failed to delete message", err);
    }
  };

  const getIcon = (type: string, isRead: boolean) => {
    if (!isRead) return <AlertCircle className="text-yellow-500" size={20} />;
    switch (type) {
      case 'TransferOffer': return <Briefcase className="text-blue-400" size={20} />;
      case 'ScoutReport': return <FileText className="text-emerald-400" size={20} />;
      case 'Finance': return <MailOpen className="text-green-500" size={20} />;
      default: return <MailOpen className="text-gray-500" size={20} />;
    }
  };

  if (isLoading) {
    return <div className="flex justify-center items-center h-full"><Loader2 className="animate-spin text-yellow-500" size={40} /></div>;
  }

  return (
    <div className="h-[calc(100vh-8rem)] bg-gray-800 border border-gray-700 rounded-2xl shadow-lg overflow-hidden flex flex-col md:flex-row">
      
      {/* ЛЯВ ПАНЕЛ - СПИСЪК */}
      <div className="w-full md:w-1/3 border-r border-gray-700 bg-gray-900/50 flex flex-col h-full relative">
        <div className="p-4 border-b border-gray-700 flex items-center justify-between bg-gray-800 shrink-0">
          <h2 className="text-lg font-bold text-white flex items-center gap-2">
            <Mail className="text-yellow-500" size={20} />
            Inbox
          </h2>
          {messages.filter(m => !m.isRead).length > 0 && (
            <span className="bg-yellow-500 text-black text-xs font-bold px-2 py-1 rounded-full animate-pulse">
              {messages.filter(m => !m.isRead).length} new
            </span>
          )}
        </div>
        
        <div className="overflow-y-auto flex-1 p-2 space-y-2">
          {messages.length === 0 ? (
            <div className="text-center text-gray-500 mt-10 text-sm">Нямате нови съобщения.</div>
          ) : (
            messages.map((msg) => (
              <div 
                key={msg.id}
                onClick={() => handleSelectMessage(msg)}
                className={`p-3 rounded-lg cursor-pointer transition-all border group relative ${
                  activeMessageId === msg.id 
                    ? 'bg-gray-800 border-yellow-500/50 shadow-inner' 
                    : msg.isRead 
                      ? 'bg-transparent border-transparent hover:bg-gray-800' 
                      : 'bg-gray-800/80 border-l-4 border-l-yellow-500 border-t-transparent border-r-transparent border-b-transparent hover:bg-gray-800'
                }`}
              >
                <div className="flex items-start gap-3 pr-6">
                  <div className="mt-1 shrink-0">
                    {getIcon(msg.type, msg.isRead)}
                  </div>
                  <div className="min-w-0 flex-1">
                    <p className={`text-sm truncate ${msg.isRead ? 'text-gray-400' : 'text-gray-200 font-bold'}`}>{msg.senderName}</p>
                    <p className={`text-sm truncate ${msg.isRead ? 'text-gray-500' : 'text-white font-bold'}`}>{msg.subject}</p>
                    <p className="text-xs text-gray-600 mt-1">{new Date(msg.sentAt).toLocaleString()}</p>
                  </div>
                </div>

                {/* Бутон за изтриване (появява се при hover) */}
                <button 
                  onClick={(e) => handleDeleteMessage(e, msg.id)}
                  className="absolute right-2 top-1/2 -translate-y-1/2 p-2 text-gray-500 hover:text-red-500 opacity-0 group-hover:opacity-100 transition-opacity"
                  title="Delete Message"
                >
                  <Trash2 size={16} />
                </button>
              </div>
            ))
          )}
        </div>
      </div>

      {/* ДЕСЕН ПАНЕЛ - СЪДЪРЖАНИЕ */}
      <div className="flex-1 bg-gray-800 p-6 flex flex-col h-full overflow-y-auto">
        {activeMessage ? (
          <div className="animate-in fade-in duration-200 h-full flex flex-col">
            <div className="border-b border-gray-700 pb-4 mb-6">
              <h2 className="text-2xl font-black text-white mb-2 leading-tight">{activeMessage.subject}</h2>
              <div className="flex justify-between items-center text-sm text-gray-400">
                <p>From: <span className="text-gray-200 font-bold">{activeMessage.senderName}</span></p>
                <p>{new Date(activeMessage.sentAt).toLocaleString()}</p>
              </div>
            </div>
            
            <div className="text-gray-300 whitespace-pre-wrap leading-relaxed text-sm md:text-base">
              {activeMessage.content}
            </div>

            {/* ДИНАМИЧНИ БУТОНИ ЗА ДЕЙСТВИЕ */}
            {activeMessage.type === 'TransferOffer' && (
              <div className="mt-8 pt-6 border-t border-gray-700 flex gap-4">
                <button className="bg-yellow-500 text-black px-6 py-2 rounded-lg font-bold hover:bg-yellow-400 transition-colors">ENTER NEGOTIATIONS</button>
                <button className="bg-gray-900 text-gray-400 px-6 py-2 rounded-lg font-bold hover:text-white hover:bg-red-500/20 transition-colors">REJECT OFFER</button>
              </div>
            )}
            
            {activeMessage.type === 'ScoutReport' && activeMessage.relatedEntityId && (
              <div className="mt-8 pt-6 border-t border-gray-700">
                 {/* Тук по-късно може да сложим Link към /world/player/{activeMessage.relatedEntityId} */}
                <button className="bg-blue-600 text-white px-6 py-2 rounded-lg font-bold hover:bg-blue-500 transition-colors">VIEW PLAYER PROFILE</button>
              </div>
            )}
          </div>
        ) : (
          <div className="h-full flex flex-col items-center justify-center text-gray-500">
            <MailOpen size={64} className="mb-4 opacity-20" />
            <p>Select a message to read.</p>
          </div>
        )}
      </div>

    </div>
  );
}