import { useState, useEffect } from 'react';
import { Wallet, TrendingUp, TrendingDown, DollarSign, ArrowUpRight, ArrowDownRight, CreditCard, Loader2, AlertCircle, CalendarDays, Landmark } from 'lucide-react';
import { API_URL } from '../../config';
import { useAuth } from '../../hooks/useAuth';

interface SeasonRecord {
  seasonId: number;
  seasonNumber: number;
  income: number;
  expenses: number;
  profit: number;
}

interface Transaction {
  id: number;
  type: 'income' | 'expense';
  description: string;
  amount: number;
  date: string;
}

interface FinancialData {
  balance: number;
  startupCapital: number;
  operatingIncome: number;
  operatingExpenses: number;
  netProfit: number;
  seasonalRecords: SeasonRecord[];
  recentTransactions: Transaction[];
}

export default function Finance() {
  const [financials, setFinancials] = useState<FinancialData | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  const { getUserIdOrRedirect } = useAuth();

  useEffect(() => {
    const fetchFinanceData = async () => {
      const userId = getUserIdOrRedirect();
      if (!userId) return;

      try {
        const response = await fetch(`${API_URL}/agency/${userId}/finance`);
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
  }, [getUserIdOrRedirect]);

  const formatMoney = (amount: number) => {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(amount);
  };

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
    <div className="space-y-6 pb-10">

      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-2">
        <div>
          <h1 className="text-3xl font-black text-white uppercase tracking-wider">Financial Hub</h1>
          <p className="text-gray-400 mt-1">Track your agency's operating revenue and seasonal growth</p>
        </div>
        <div className="bg-gray-800 border border-gray-700 px-4 py-2 rounded-lg flex items-center gap-3 shadow-md">
          <Landmark className="text-blue-500" size={20} />
          <div>
            <p className="text-xs text-gray-400 uppercase font-bold">Initial Capital (Excluded from profit)</p>
            <p className="text-lg font-bold text-white">{formatMoney(financials.startupCapital)}</p>
          </div>
        </div>
      </div>

      {/* 1. Главен финансов преглед (Operating Overview) */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {/* Баланс */}
        <div className="bg-gray-900 border border-yellow-500/50 p-6 rounded-2xl shadow-[0_0_20px_rgba(234,179,8,0.1)] relative overflow-hidden">
          <div className="absolute -right-4 -top-4 text-yellow-500/10">
            <Wallet size={120} />
          </div>
          <div className="relative z-10">
            <p className="text-gray-400 text-sm font-bold uppercase tracking-wider mb-1">Current Balance</p>
            <h2 className="text-4xl font-black text-yellow-500 font-mono">{formatMoney(financials.balance)}</h2>
          </div>
        </div>

        {/* Оперативни Приходи */}
        <div className="bg-gray-800 border border-gray-700 p-6 rounded-2xl flex items-center justify-between">
          <div>
            <p className="text-gray-400 text-sm font-bold uppercase tracking-wider mb-1">Operating Income</p>
            <h3 className="text-2xl font-bold text-emerald-400 font-mono">{formatMoney(financials.operatingIncome)}</h3>
          </div>
          <div className="w-12 h-12 bg-emerald-500/10 rounded-xl flex items-center justify-center text-emerald-500">
            <TrendingUp size={24} />
          </div>
        </div>

        {/* Разходи */}
        <div className="bg-gray-800 border border-gray-700 p-6 rounded-2xl flex items-center justify-between">
          <div>
            <p className="text-gray-400 text-sm font-bold uppercase tracking-wider mb-1">Total Expenses</p>
            <h3 className="text-2xl font-bold text-red-400 font-mono">{formatMoney(financials.operatingExpenses)}</h3>
          </div>
          <div className="w-12 h-12 bg-red-500/10 rounded-xl flex items-center justify-center text-red-500">
            <TrendingDown size={24} />
          </div>
        </div>

        {/* Нетна печалба */}
        <div className="bg-gray-800 border border-gray-700 p-6 rounded-2xl flex items-center justify-between">
          <div>
            <p className="text-gray-400 text-sm font-bold uppercase tracking-wider mb-1">Lifetime Net Profit</p>
            <h3 className={`text-2xl font-bold font-mono ${financials.netProfit >= 0 ? 'text-white' : 'text-red-400'}`}>
              {formatMoney(financials.netProfit)}
            </h3>
          </div>
          <div className="w-12 h-12 bg-gray-900 border border-gray-700 rounded-xl flex items-center justify-center text-white">
            <DollarSign size={24} />
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">

        {/* 2. Отчети по Сезони */}
        <div className="bg-gray-800 border border-gray-700 rounded-xl overflow-hidden shadow-lg flex flex-col h-[500px]">
          <div className="p-5 border-b border-gray-700 flex items-center gap-3 bg-gray-800 shrink-0">
            <CalendarDays className="text-blue-400" />
            <h2 className="text-xl font-bold text-white">Seasonal Reports</h2>
          </div>
          <div className="flex-1 p-5 overflow-y-auto custom-scrollbar">
            {financials.seasonalRecords.length === 0 ? (
               <div className="h-full flex flex-col items-center justify-center text-gray-500 opacity-50 border border-gray-700 border-dashed rounded-xl p-6 text-center">
                 <CalendarDays size={48} className="mb-3 opacity-50" />
                 <p className="font-bold">No seasonal data yet.</p>
                 <p className="text-sm">Complete a matchday to generate seasonal reports.</p>
               </div>
            ) : (
              <div className="space-y-4">
                {financials.seasonalRecords.map((season) => (
                  <div key={season.seasonId} className="bg-gray-900 border border-gray-700 rounded-xl p-4 hover:border-gray-600 transition-colors">
                    <div className="flex justify-between items-center mb-3 border-b border-gray-800 pb-2">
                      <h3 className="font-black text-white text-lg">Season {season.seasonNumber}</h3>
                      <span className={`font-bold px-3 py-1 rounded text-sm ${season.profit >= 0 ? 'bg-emerald-500/10 text-emerald-400' : 'bg-red-500/10 text-red-400'}`}>
                        {season.profit >= 0 ? '+' : ''}{formatMoney(season.profit)} Profit
                      </span>
                    </div>
                    <div className="flex justify-between items-center text-sm">
                      <div>
                        <p className="text-gray-500 uppercase font-bold text-[10px] tracking-wider">Revenue</p>
                        <p className="text-white font-mono">{formatMoney(season.income)}</p>
                      </div>
                      <div className="text-right">
                        <p className="text-gray-500 uppercase font-bold text-[10px] tracking-wider">Expenses / Fines</p>
                        <p className="text-red-400 font-mono">{formatMoney(season.expenses)}</p>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        {/* 3. История на транзакциите */}
        <div className="bg-gray-800 border border-gray-700 rounded-xl overflow-hidden shadow-lg flex flex-col h-[500px]">
          <div className="p-5 border-b border-gray-700 flex items-center gap-3 bg-gray-800 shrink-0">
            <CreditCard className="text-yellow-500" />
            <h2 className="text-xl font-bold text-white">Recent Transactions</h2>
          </div>

          <div className="flex-1 p-0 overflow-y-auto custom-scrollbar">
            {financials.recentTransactions.length === 0 ? (
              <div className="h-full flex flex-col items-center justify-center text-gray-500 opacity-50 border-t-0 border border-gray-700 border-dashed rounded-b-xl p-6 text-center m-4 mt-0">
                 <CreditCard size={48} className="mb-3 opacity-50" />
                 <p className="font-bold">No transactions recorded.</p>
              </div>
            ) : (
              <div className="divide-y divide-gray-700">
                {financials.recentTransactions.map((tx) => (
                  <div key={tx.id} className="p-4 hover:bg-gray-750 transition-colors flex items-center justify-between group">
                    <div className="flex items-center gap-4">
                      <div className={`w-10 h-10 rounded-full flex items-center justify-center shrink-0 ${tx.type === 'income' ? 'bg-emerald-500/10 text-emerald-500' : 'bg-red-500/10 text-red-400'
                        }`}>
                        {tx.type === 'income' ? <ArrowDownRight size={20} /> : <ArrowUpRight size={20} />}
                      </div>
                      <div>
                        <p className="font-bold text-white group-hover:text-yellow-500 transition-colors line-clamp-1 text-sm">{tx.description}</p>
                        <p className="text-xs text-gray-500">{formatDate(tx.date)}</p>
                      </div>
                    </div>
                    <div className={`font-mono font-bold whitespace-nowrap pl-4 ${tx.type === 'income' ? 'text-emerald-400' : 'text-red-400'
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