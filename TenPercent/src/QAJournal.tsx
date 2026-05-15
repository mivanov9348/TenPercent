import { useState, useEffect } from 'react';
import { Trash2, Terminal, ChevronDown } from 'lucide-react';

export default function QAJournal() {
  const [logs, setLogs] = useState<string[]>([]);
  const [isMinimized, setIsMinimized] = useState(false);

  useEffect(() => {
    // 1. Зареждаме стари логове, ако сме рефрешнали страницата
    const savedLogs = JSON.parse(localStorage.getItem('qa_journal') || '[]');
    setLogs(savedLogs);

    // 2. Глобалният "слушател" за кликове
    const handleGlobalClick = (e: MouseEvent) => {
      const target = e.target as HTMLElement;
      
      // ВАЖНО: Игнорираме кликове вътре в самия QA Journal, за да не се спами
      if (target.closest('#qa-journal-overlay')) return;

      // Търсим дали сме цъкнали бутон или линк
      const clickable = target.closest('button') || target.closest('a');
      
      if (clickable) {
        const actionText = (clickable.innerText || clickable.id || 'Icon/Unknown').replace(/\s+/g, ' ').trim();
        const time = new Date().toLocaleTimeString();
        const newLog = `[${time}] 🖱️ ${actionText}`;
        
        setLogs(prev => {
          // Слагаме новото действие НАЙ-ОТГОРЕ и пазим само последните 50, за да не лагва
          const updated = [newLog, ...prev].slice(0, 50); 
          localStorage.setItem('qa_journal', JSON.stringify(updated));
          return updated;
        });
      }
    };

    window.addEventListener('click', handleGlobalClick);
    return () => window.removeEventListener('click', handleGlobalClick);
  }, []);

  const clearLogs = () => {
    setLogs([]);
    localStorage.removeItem('qa_journal');
  };

  // Ако е минимизиран, показваме само едно малко бутонче
  if (isMinimized) {
    return (
      <button 
        id="qa-journal-overlay"
        onClick={() => setIsMinimized(false)}
        className="fixed bottom-4 right-4 z-[9999] bg-gray-900 border border-gray-700 text-yellow-500 p-3 rounded-full shadow-[0_0_15px_rgba(0,0,0,0.5)] hover:bg-gray-800 hover:scale-110 transition-all opacity-70 hover:opacity-100 group"
        title="Open QA Journal"
      >
        <Terminal size={20} />
      </button>
    );
  }

  // Разгънат вид
  return (
    <div 
      id="qa-journal-overlay" 
      className="fixed bottom-4 right-4 z-[9999] w-80 bg-gray-900/95 backdrop-blur-md border border-gray-600 rounded-xl shadow-2xl overflow-hidden flex flex-col"
    >
      {/* Хедър */}
      <div className="bg-gray-800 p-3 border-b border-gray-700 flex justify-between items-center">
        <h3 className="text-yellow-500 font-black text-xs uppercase tracking-wider flex items-center gap-2">
          <Terminal size={14} /> QA Journal
        </h3>
        <div className="flex items-center gap-3">
          <button onClick={clearLogs} className="text-gray-400 hover:text-red-500 transition-colors" title="Изчисти лога">
            <Trash2 size={14} />
          </button>
          <button onClick={() => setIsMinimized(true)} className="text-gray-400 hover:text-white transition-colors" title="Минимизирай">
            <ChevronDown size={16} />
          </button>
        </div>
      </div>
      
      {/* Списък с действия */}
      <div className="h-64 overflow-y-auto p-3 flex flex-col gap-2 text-xs font-mono scroll-smooth">
        {logs.length === 0 ? (
          <div className="h-full flex flex-col items-center justify-center text-gray-600 italic">
            <p>Ready to track clicks...</p>
          </div>
        ) : (
          logs.map((log, i) => (
            <div key={i} className="text-gray-300 border-b border-gray-800 pb-1 break-words leading-relaxed">
              <span className="text-gray-500">{log.split(']')[0]}]</span> 
              <span className="text-blue-400 ml-1">{log.split(']')[1]}</span>
            </div>
          ))
        )}
      </div>
    </div>
  );
}