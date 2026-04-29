import { Wallet, TrendingUp, TrendingDown, DollarSign, ArrowUpRight, ArrowDownRight, CreditCard } from 'lucide-react';

export default function Finance() {
  // Фейк финансови данни
  const financials = {
    balance: 10540000,
    weeklyIncome: 45000,
    weeklyExpenses: 18500,
    netProfit: 26500
  };

  // История на транзакциите
  const transactions = [
    { id: 1, type: 'income', desc: "Player Wages Cut (10%)", amount: "+$35,000", date: "Today" },
    { id: 2, type: 'income', desc: "Sponsorship Bonus: J. Bellingham", amount: "+$10,000", date: "Yesterday" },
    { id: 3, type: 'expense', desc: "Scouting Network Upkeep", amount: "-$8,500", date: "2 days ago" },
    { id: 4, type: 'expense', desc: "Office Rent & Staff", amount: "-$10,000", date: "3 days ago" },
    { id: 5, type: 'income', desc: "Transfer Fee Commission (M. Rashford)", amount: "+$450,000", date: "1 week ago" },
  ];

  // Форматиране на пари
  const formatMoney = (amount: number) => {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(amount);
  };

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
            <p className="text-gray-400 text-sm font-bold uppercase tracking-wider mb-1">Weekly Income</p>
            <h3 className="text-2xl font-bold text-emerald-400 font-mono">{formatMoney(financials.weeklyIncome)}</h3>
          </div>
          <div className="w-12 h-12 bg-emerald-500/10 rounded-xl flex items-center justify-center text-emerald-500">
            <TrendingUp size={24} />
          </div>
        </div>

        {/* Разходи */}
        <div className="bg-gray-800 border border-gray-700 p-6 rounded-2xl flex items-center justify-between">
          <div>
            <p className="text-gray-400 text-sm font-bold uppercase tracking-wider mb-1">Weekly Expenses</p>
            <h3 className="text-2xl font-bold text-red-400 font-mono">{formatMoney(financials.weeklyExpenses)}</h3>
          </div>
          <div className="w-12 h-12 bg-red-500/10 rounded-xl flex items-center justify-center text-red-500">
            <TrendingDown size={24} />
          </div>
        </div>

        {/* Нетна печалба */}
        <div className="bg-gray-800 border border-gray-700 p-6 rounded-2xl flex items-center justify-between">
          <div>
            <p className="text-gray-400 text-sm font-bold uppercase tracking-wider mb-1">Net Profit</p>
            <h3 className="text-2xl font-bold text-white font-mono">{formatMoney(financials.netProfit)}</h3>
          </div>
          <div className="w-12 h-12 bg-gray-900 border border-gray-700 rounded-xl flex items-center justify-center text-white">
            <DollarSign size={24} />
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        
        {/* 2. Детайли за приходите и разходите */}
        <div className="lg:col-span-1 space-y-6">
          {/* Income Breakdown */}
          <div className="bg-gray-800 border border-gray-700 rounded-xl p-5 shadow-lg">
            <h3 className="text-lg font-bold text-white mb-4 border-b border-gray-700 pb-2">Income Sources</h3>
            <div className="space-y-3">
              <div className="flex justify-between items-center text-sm">
                <span className="text-gray-400">Player Wages (10% Cut)</span>
                <span className="font-mono text-emerald-400 font-bold">+$35,000</span>
              </div>
              <div className="flex justify-between items-center text-sm">
                <span className="text-gray-400">Sponsorship Deals</span>
                <span className="font-mono text-emerald-400 font-bold">+$10,000</span>
              </div>
            </div>
          </div>

          {/* Expenses Breakdown */}
          <div className="bg-gray-800 border border-gray-700 rounded-xl p-5 shadow-lg">
            <h3 className="text-lg font-bold text-white mb-4 border-b border-gray-700 pb-2">Expenses</h3>
            <div className="space-y-3">
              <div className="flex justify-between items-center text-sm">
                <span className="text-gray-400">Office Maintenance</span>
                <span className="font-mono text-red-400 font-bold">-$5,000</span>
              </div>
              <div className="flex justify-between items-center text-sm">
                <span className="text-gray-400">Staff Salaries</span>
                <span className="font-mono text-red-400 font-bold">-$5,000</span>
              </div>
              <div className="flex justify-between items-center text-sm">
                <span className="text-gray-400">Scouting Operations</span>
                <span className="font-mono text-red-400 font-bold">-$8,500</span>
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
          
          <div className="flex-1 p-0 overflow-y-auto">
            <div className="divide-y divide-gray-700">
              {transactions.map((tx) => (
                <div key={tx.id} className="p-4 hover:bg-gray-750 transition-colors flex items-center justify-between group">
                  <div className="flex items-center gap-4">
                    <div className={`w-10 h-10 rounded-full flex items-center justify-center ${
                      tx.type === 'income' ? 'bg-emerald-500/10 text-emerald-500' : 'bg-red-500/10 text-red-500'
                    }`}>
                      {tx.type === 'income' ? <ArrowDownRight size={20} /> : <ArrowUpRight size={20} />}
                    </div>
                    <div>
                      <p className="font-bold text-white group-hover:text-yellow-500 transition-colors">{tx.desc}</p>
                      <p className="text-xs text-gray-500">{tx.date}</p>
                    </div>
                  </div>
                  <div className={`font-mono font-bold text-lg ${
                    tx.type === 'income' ? 'text-emerald-400' : 'text-red-400'
                  }`}>
                    {tx.amount}
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

      </div>
    </div>
  );
}