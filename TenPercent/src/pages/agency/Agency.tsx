import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Building, Users, Wallet, TrendingUp, Target, Trophy, Shield, Crown, Star, Loader2, AlertCircle, Bookmark, DollarSign, Crown as CrownIcon } from 'lucide-react';
import { API_URL } from '../../config';
import { useAuth } from '../../hooks/useAuth';
import { useAgencyStore } from '../../store/useAgencyStore';

interface AgencyData {
  id: number;
  name: string;
  agentName: string;
  logoId: number;
  budget: number;
  reputation: number;
  level: number;
  establishedAt: string;
  totalPlayersCount: number;
  projectedSeasonIncome: number;
  topEarnerName: string;
  totalContractsValue: number;
}

export default function Agency() {
  const [agency, setAgency] = useState<AgencyData | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  
  const { getUserIdOrRedirect } = useAuth();
  const { budget, setBudget } = useAgencyStore(); 

  useEffect(() => {
    const fetchAgency = async () => {
      const userId = getUserIdOrRedirect();
      if (!userId) return;

      try {
        const response = await fetch(`${API_URL}/agency/${userId}`);
        if (!response.ok) throw new Error('Неуспешно зареждане на данните за агенцията.');

        const data = await response.json();
        setAgency(data);
        setBudget(data.budget); 
      } catch (err: any) {
        setError(err.message);
      } finally {
        setIsLoading(false);
      }
    };

    fetchAgency();
  }, [getUserIdOrRedirect, setBudget]);

  const formatMoney = (amount: number) => {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(amount);
  };

  const renderLogo = (logoId: number) => {
    switch (logoId) {
      case 1: return <Shield size={48} />;
      case 2: return <Crown size={48} />;
      case 3: return <Star size={48} />;
      case 4: return <Building size={48} />;
      default: return <Trophy size={48} />;
    }
  };

  if (isLoading) {
    return <div className="flex h-64 items-center justify-center"><Loader2 className="animate-spin text-yellow-500" size={48} /></div>;
  }

  if (error || !agency) {
    return (
      <div className="flex items-center gap-3 text-red-500 bg-red-500/10 p-4 rounded-lg">
        <AlertCircle /> <p>{error || 'Агенцията не е намерена.'}</p>
      </div>
    );
  }

  const facilities = [
    { id: 1, name: "Scouting Network", level: 2, maxLevel: 5, description: "Подобрява качеството на генерираните играчи на пазара." },
    { id: 2, name: "Legal Department", level: 4, maxLevel: 5, description: "Увеличава твоя процент (%) при преговори за договори." },
    { id: 3, name: "PR & Media", level: 1, maxLevel: 5, description: "Вдига репутацията на агенцията по-бързо след успешен мач." },
  ];

  return (
    <div className="space-y-8">
      {/* 1. Хедър на агенцията */}
      <div className="bg-gray-800 border border-gray-700 rounded-2xl p-6 flex flex-col md:flex-row items-center justify-between shadow-lg relative overflow-hidden gap-6">
        <div className="absolute -right-10 -top-10 text-gray-700/20 pointer-events-none">
          <Building size={200} />
        </div>

        <div className="relative z-10 flex flex-col md:flex-row items-center gap-6 w-full">
          <div className="w-24 h-24 bg-gray-900 border-2 border-yellow-500 rounded-xl flex items-center justify-center text-yellow-500 shadow-[0_0_15px_rgba(234,179,8,0.2)] shrink-0">
            {renderLogo(agency.logoId)}
          </div>
          <div className="text-center md:text-left flex-1">
            <div className="flex flex-col md:flex-row items-center gap-3 mb-1">
              <h1 className="text-3xl font-black text-white uppercase tracking-wider">{agency.name}</h1>
              <span className="bg-yellow-500 text-black text-xs font-bold px-2 py-1 rounded shadow-sm">LEVEL {agency.level}</span>
            </div>
            <p className="text-gray-400 text-lg">CEO: <span className="text-gray-200">{agency.agentName}</span></p>
            
            {/* Прогрес бар за репутацията */}
            <div className="flex items-center justify-center md:justify-start gap-3 mt-3 text-sm">
              <span className="text-gray-500 font-bold uppercase tracking-wider text-[10px]">Reputation</span>
              <div className="w-48 h-2 bg-gray-900 rounded-full overflow-hidden border border-gray-700">
                <div
                  className="h-full bg-gradient-to-r from-yellow-600 to-yellow-400"
                  style={{ width: `${agency.reputation}%` }}
                ></div>
              </div>
              <span className="text-yellow-500 font-bold font-mono">{agency.reputation}/100</span>
            </div>
          </div>
        </div>

        <div className="relative z-10 flex flex-col gap-3 w-full md:w-auto shrink-0">
          <Link
            to="/my-shortlist"
            className="flex items-center justify-center gap-2 bg-gray-900 border border-gray-700 hover:border-yellow-500 text-white px-6 py-3 rounded-xl font-bold transition-all hover:shadow-[0_0_15px_rgba(234,179,8,0.2)] group whitespace-nowrap"
          >
            <Bookmark className="text-yellow-500 group-hover:fill-yellow-500 transition-all" size={20} />
            My Shortlist
          </Link>
        </div>
      </div>

      {/* 2. Бързи статистики (ОБНОВЕНИ) */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {/* Бюджет */}
        <div className="bg-gray-800 border border-gray-700 rounded-xl p-5 flex items-center gap-4 hover:border-gray-500 transition-colors shadow-md">
          <div className="p-3 bg-gray-900 border border-gray-700 rounded-lg text-yellow-500 shrink-0"><Wallet size={24} /></div>
          <div className="min-w-0">
            <p className="text-xs text-gray-500 font-bold uppercase tracking-wider truncate">Available Budget</p>
            <p className="text-xl font-black text-white font-mono truncate">{budget !== null ? formatMoney(budget) : '...'}</p>
          </div>
        </div>

        {/* Прогнозни приходи */}
        <div className="bg-gray-800 border border-gray-700 rounded-xl p-5 flex items-center gap-4 hover:border-gray-500 transition-colors shadow-md">
          <div className="p-3 bg-gray-900 border border-emerald-900/50 rounded-lg text-emerald-500 shrink-0"><TrendingUp size={24} /></div>
          <div className="min-w-0">
            <p className="text-xs text-gray-500 font-bold uppercase tracking-wider truncate">Season Projection</p>
            <p className="text-xl font-black text-emerald-400 font-mono truncate">+{formatMoney(agency.projectedSeasonIncome)}</p>
          </div>
        </div>

        {/* Обща стойност на договорите */}
        <div className="bg-gray-800 border border-gray-700 rounded-xl p-5 flex items-center gap-4 hover:border-gray-500 transition-colors shadow-md">
          <div className="p-3 bg-gray-900 border border-blue-900/50 rounded-lg text-blue-500 shrink-0"><DollarSign size={24} /></div>
          <div className="min-w-0">
            <p className="text-xs text-gray-500 font-bold uppercase tracking-wider truncate">Total Contracts Value</p>
            <p className="text-xl font-black text-white font-mono truncate">{formatMoney(agency.totalContractsValue)}</p>
          </div>
        </div>

        {/* VIP Клиент */}
        <div className="bg-gray-800 border border-gray-700 rounded-xl p-5 flex items-center gap-4 hover:border-gray-500 transition-colors shadow-md">
          <div className="p-3 bg-gray-900 border border-purple-900/50 rounded-lg text-purple-400 shrink-0"><CrownIcon size={24} /></div>
          <div className="min-w-0">
            <p className="text-xs text-gray-500 font-bold uppercase tracking-wider truncate">VIP Client</p>
            <p className="text-xl font-black text-white truncate">{agency.topEarnerName}</p>
          </div>
        </div>
      </div>

      {/* 3. Инфраструктура */}
      <div>
        <h2 className="text-xl font-bold text-white mb-4 flex items-center gap-2">
          <Target className="text-yellow-500" />
          Agency Infrastructure
        </h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {facilities.map((facility) => (
            <div key={facility.id} className="bg-gray-800 border border-gray-700 rounded-xl p-5 flex flex-col shadow-md hover:shadow-lg transition-shadow">
              <div className="flex justify-between items-start mb-2">
                <h3 className="text-lg font-bold text-white">{facility.name}</h3>
                <span className="text-xs font-mono font-bold bg-gray-900 text-gray-400 px-2 py-1 rounded border border-gray-700">
                  Lvl {facility.level}/{facility.maxLevel}
                </span>
              </div>
              <p className="text-gray-400 text-sm mb-6 flex-1">{facility.description}</p>

              <button
                className={`w-full py-3 rounded-lg font-black text-sm tracking-wide transition-all ${facility.level === facility.maxLevel
                    ? 'bg-gray-900 border border-gray-800 text-gray-600 cursor-not-allowed'
                    : 'bg-gray-700 border border-gray-600 text-white hover:bg-yellow-500 hover:border-yellow-400 hover:text-black shadow-[0_0_10px_rgba(0,0,0,0)] hover:shadow-[0_0_15px_rgba(234,179,8,0.3)]'
                  }`}
                disabled={facility.level === facility.maxLevel}
              >
                {facility.level === facility.maxLevel ? 'MAX LEVEL REACHED' : 'UPGRADE ($50,000)'}
              </button>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}