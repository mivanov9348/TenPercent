import { useState, useEffect } from 'react';
import { Landmark, TrendingDown, TrendingUp, Coins, Loader2, ArrowUpRight, ArrowDownRight } from 'lucide-react';
import { API_URL } from '../../config';

interface BankStats {
  reserveBalance: number;
  totalTaxesCollected: number;
  totalGrantsGiven: number;
  moneyInCirculation: number;
  recentTransactions: any[];
}

export default function AdminBank() {
  const [stats, setStats] = useState<BankStats | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchBankStats = async () => {
      try {
        const response = await fetch(`${API_URL}/admin/bank`);
        const data = await response.json();
        setStats(data);
      } catch (error) {
        console.error("Failed to fetch bank stats", error);
      } finally {
        setIsLoading(false);
      }
    };
    fetchBankStats();
  }, []);

  const formatMoney = (amount: number) => {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(amount);
  };

  if (isLoading || !stats) {
    return <div className="flex h-full items-center justify-center"><Loader2 className="animate-spin text-red-500" size={48} /></div>;
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-black text-white flex items-center gap-3">
          <Landmark className="text-red-500" size={32} />
          WORLD CENTRAL BANK
        </h1>
        <p className="text-gray-400">Мониторинг на глобалната икономика и паричното предлагане.</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {/* Банков Резерв */}
        <div className="bg-gray-900 border border-red-500/30 p-6 rounded-2xl relative overflow-hidden shadow-[0_0_20px_rgba(239,68,68,0.1)]">
          <Landmark className="absolute -right-4 -top-4 text-red-500/10" size={120} />
          <div className="relative z-10">
            <p className="text-gray-400 text-sm font-bold uppercase tracking-wider mb-1">Bank Reserve</p>
            <h2 className="text-3xl font-black text-white font-mono">{formatMoney(stats.reserveBalance)}</h2>
          </div>
        </div>

        {/* Събрани Данъци */}
        <div className="bg-gray-900 border border-gray-800 p-6 rounded-2xl flex items-center justify-between">
          <div>
            <p className="text-gray-400 text-sm font-bold uppercase tracking-wider mb-1">Taxes Collected</p>
            <h3 className="text-2xl font-bold text-emerald-500 font-mono">+{formatMoney(stats.totalTaxesCollected)}</h3>
          </div>
          <div className="p-3 bg-emerald-500/10 text-emerald-500 rounded-xl"><TrendingUp size={24} /></div>
        </div>

        {/* Раздадени Грантове */}
        <div className="bg-gray-900 border border-gray-800 p-6 rounded-2xl flex items-center justify-between">
          <div>
            <p className="text-gray-400 text-sm font-bold uppercase tracking-wider mb-1">Grants & Loans</p>
            <h3 className="text-2xl font-bold text-red-500 font-mono">-{formatMoney(stats.totalGrantsGiven)}</h3>
          </div>
          <div className="p-3 bg-red-500/10 text-red-500 rounded-xl"><TrendingDown size={24} /></div>
        </div>

        {/* Пари в обръщение */}
        <div className="bg-gray-900 border border-gray-800 p-6 rounded-2xl flex items-center justify-between">
          <div>
            <p className="text-gray-400 text-sm font-bold uppercase tracking-wider mb-1">Money in Circulation</p>
            <h3 className="text-2xl font-bold text-blue-400 font-mono">{formatMoney(stats.moneyInCirculation)}</h3>
          </div>
          <div className="p-3 bg-blue-500/10 text-blue-500 rounded-xl"><Coins size={24} /></div>
        </div>
      </div>

      {/* История на транзакциите */}
      <div className="bg-gray-900 border border-gray-800 rounded-2xl overflow-hidden">
        <div className="p-5 border-b border-gray-800">
          <h2 className="text-xl font-bold text-white">Bank Ledger (Latest 50 Txs)</h2>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm text-gray-400">
            <thead className="bg-gray-950 text-gray-500 font-bold uppercase text-xs">
              <tr>
                <th className="px-6 py-4">Type</th>
                <th className="px-6 py-4">Description</th>
                <th className="px-6 py-4">Category</th>
                <th className="px-6 py-4 text-right">Amount</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-800">
              {stats.recentTransactions.map((tx) => {
                const isIncome = tx.receiverType === 1; // 1 = Bank в EntityType
                return (
                  <tr key={tx.id} className="hover:bg-gray-800/50 transition-colors">
                    <td className="px-6 py-4">
                      <div className={`flex items-center gap-2 font-bold ${isIncome ? 'text-emerald-500' : 'text-red-500'}`}>
                        {isIncome ? <ArrowDownRight size={16} /> : <ArrowUpRight size={16} />}
                        {isIncome ? 'IN' : 'OUT'}
                      </div>
                    </td>
                    <td className="px-6 py-4 text-white">{tx.description}</td>
                    <td className="px-6 py-4">
                      <span className="bg-gray-800 px-2 py-1 rounded text-xs border border-gray-700">
                        Cat: {tx.category}
                      </span>
                    </td>
                    <td className={`px-6 py-4 text-right font-mono font-bold ${isIncome ? 'text-emerald-500' : 'text-red-500'}`}>
                      {isIncome ? '+' : '-'}{formatMoney(tx.amount)}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}