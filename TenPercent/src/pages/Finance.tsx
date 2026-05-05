import { useState, useEffect } from 'react';
import { Wallet, TrendingUp, TrendingDown, DollarSign, ArrowUpRight, ArrowDownRight, CreditCard, Loader2, AlertCircle } from 'lucide-react';

// Дефинираме типовете, които очакваме от бекенда
interface Transaction {
  id: number;
  type: 'income' | 'expense';
  description: string;
  amount: number;
  date: string;
}

interface FinancialData {
  balance: number;
  totalIncome: number;
  totalExpenses: number;
  netProfit: number;
  recentTransactions: Transaction[];
}

export default function Finance() {
  const [financials, setFinancials] = useState<FinancialData | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchFinanceData = async () => {
      const userId = localStorage.getItem('userId');
      if (!userId) {
        setError('Не сте влезли в системата.');
        setIsLoading(false);
        return;
      }

      try {
        const response = await fetch(`https://localhost:7135/api/agency/${userId}/finance`);
        if (!response.ok) throw new Error('Грешка при зареждане на финансовите данни.');
        
        const data = await response.json();
        setFinancials(data);
      } catch (err: any) {
        setError(err.message);
      } finally {
        setIsLoading(false);
      }
    };

    fetchFinanceData();
  }, []);

  // Форматиране на пари
  const formatMoney = (amount: number) => {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(amount);
  };

  // Форматиране на дата
  const formatDate = (dateString: string) => {
    const options: Intl.DateTimeFormatOptions = { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' };
    return new Date(dateString).toLocaleDateString('en-US', options);
  };

  if (isLoading) {
    return <div className="flex justify-center items-center h-64 text-yellow-500"><Loader2 className="animate-spin" size={48} /></div>;
  }

  if (error || !financials) {
    return (
      <div className="p-4 bg-red-500/10 border border-red-500/30 rounded-xl flex items-center gap-3 text-red-500">
        <AlertCircle /> <p className="font-bold">{error || "Няма намерени данни."}</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      
      {/* 1. Главен финансов преглед (Overview) */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {/* Баланс - Основна карта */}
        <div className="bg-gray-900 border border-yellow-500/50 p-6 rounded-2xl shadow-[0_0_20px_rgba(234,179,8,0.1)] relative overflow-hidden">
          <div className="absolute -right-4 -top-4 text-yellow-500/10">
            <Wallet size={120} />
          </div>
          <div className="relative z-10">
            <p className="text-gray-400 text-sm font-bold uppercase tracking-wider mb-1">Agency Balance</p>
            <h2 className="text-4xl font-black text-yellow-500 font-mono">{formatMoney(financials.balance)}</h2>
          </div>
        </div>

        {/* Приходи */}
        <div className="bg-gray-800 border border-gray-700 p-6 rounded-2xl flex items-center justify-between">
          <div>
            <p className="text-gray-400 text-sm font-bold uppercase tracking-wider mb-1">Total Income</p>
            <h3 className="text-2xl font-bold text-emerald-400 font-mono">{formatMoney(financials.totalIncome)}</h3>
          </div>
          <div className="w-12 h-12 bg-emerald-500/10 rounded-xl flex items-center justify-center text-emerald-500">
            <TrendingUp size={24} />
          </div>
        </div>

        {/* Разходи */}
        <div className="bg-gray-800 border border-gray-700 p-6 rounded-2xl flex items-center justify-between">
          <div>
            <p className="text-gray-400 text-sm font-bold uppercase tracking-wider mb-1">Total Expenses</p>
            <h3 className="text-2xl font-bold text-red-400 font-mono">{formatMoney(financials.totalExpenses)}</h3>
          </div>
          <div className="w-12 h-12 bg-red-500/10 rounded-xl flex items-center justify-center text-red-500">
            <TrendingDown size={24} />
          </div>
        </div>

        {/* Нетна печалба */}
        <div className="bg-gray-800 border border-gray-700 p-6 rounded-2xl flex items-center justify-between">
          <div>
            <p className="text-gray-400 text-sm font-bold uppercase tracking-wider mb-1">Net Profit</p>
            <h3 className={`text-2xl font-bold font-mono ${financials.netProfit >= 0 ? 'text-white' : 'text-red-400'}`}>
              {formatMoney(financials.netProfit)}
            </h3>
          </div>
          <div className="w-12 h-12 bg-gray-900 border border-gray-700 rounded-xl flex items-center justify-center text-white">
            <DollarSign size={24} />
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        
        {/* 2. Детайли за приходите и разходите (Засега статични, докато не ги разбием по категории в бекенда) */}
        <div className="lg:col-span-1 space-y-6">
          {/* Income Breakdown */}
          <div className="bg-gray-800 border border-gray-700 rounded-xl p-5 shadow-lg">
            <h3 className="text-lg font-bold text-white mb-4 border-b border-gray-700 pb-2">Financial Health</h3>
            <div className="space-y-4">
               <p className="text-sm text-gray-400 leading-relaxed">
                 Вашата агенция автоматично отчита приходи от комисионни и плаща 10% корпоративен данък към Световната Банка.
               </p>
               <div className="p-3 bg-gray-900 border border-gray-700 rounded-lg flex justify-between items-center text-sm">
                 <span className="text-gray-400">Profit Margin</span>
                 <span className="font-bold text-yellow-500">
                   {financials.totalIncome > 0 ? Math.round((financials.netProfit / financials.totalIncome) * 100) : 0}%
                 </span>
               </div>
            </div>
          </div>
        </div>

        {/* 3. История на транзакциите */}
        <div className="lg:col-span-2 bg-gray-800 border border-gray-700 rounded-xl overflow-hidden shadow-lg flex flex-col">
          <div className="p-5 border-b border-gray-700 flex items-center gap-3 bg-gray-800">
            <CreditCard className="text-gray-400" />
            <h2 className="text-xl font-bold text-white">Recent Transactions</h2>
          </div>
          
          <div className="flex-1 p-0 overflow-y-auto max-h-[400px]">
            {financials.recentTransactions.length === 0 ? (
               <div className="p-8 text-center text-gray-500 italic">Няма записани транзакции все още.</div>
            ) : (
              <div className="divide-y divide-gray-700">
                {financials.recentTransactions.map((tx) => (
                  <div key={tx.id} className="p-4 hover:bg-gray-750 transition-colors flex items-center justify-between group">
                    <div className="flex items-center gap-4">
                      <div className={`w-10 h-10 rounded-full flex items-center justify-center shrink-0 ${
                        tx.type === 'income' ? 'bg-emerald-500/10 text-emerald-500' : 'bg-red-500/10 text-red-500'
                      }`}>
                        {tx.type === 'income' ? <ArrowDownRight size={20} /> : <ArrowUpRight size={20} />}
                      </div>
                      <div>
                        <p className="font-bold text-white group-hover:text-yellow-500 transition-colors line-clamp-1">{tx.description}</p>
                        <p className="text-xs text-gray-500">{formatDate(tx.date)}</p>
                      </div>
                    </div>
                    <div className={`font-mono font-bold text-lg whitespace-nowrap pl-4 ${
                      tx.type === 'income' ? 'text-emerald-400' : 'text-red-400'
                    }`}>
                      {tx.type === 'income' ? '+' : '-'}{formatMoney(tx.amount)}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

      </div>
    </div>
  );
}