import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom'
import { Building, Users, Wallet, TrendingUp, Target, Trophy, Shield, Crown, Star, Loader2, AlertCircle, Bookmark } from 'lucide-react';

// Дефинираме как изглеждат данните, които очакваме от C#
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
}

export default function Agency() {
  const [agency, setAgency] = useState<AgencyData | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    // Взимаме данните при първоначално зареждане на страницата
    const fetchAgency = async () => {
      const userId = localStorage.getItem('userId');
      if (!userId) return;

      try {
        // ВНИМАНИЕ: Смени порта с твоя!
        const response = await fetch(`https://localhost:7135/api/agency/${userId}`);
        if (!response.ok) throw new Error('Неуспешно зареждане на данните за агенцията.');
        
        const data = await response.json();
        setAgency(data);
      } catch (err: any) {
        setError(err.message);
      } finally {
        setIsLoading(false);
      }
    };

    fetchAgency();
  }, []);

  // Помощна функция за форматиране на бюджета
  const formatMoney = (amount: number) => {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(amount);
  };

  // Помощна функция за избор на икона спрямо logoId
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

  // Фейк данни за ъпгрейди (ще ги вържем към базата по-късно)
  const facilities = [
    { id: 1, name: "Scouting Network", level: 2, maxLevel: 5, description: "Подобрява качеството на генерираните играчи на пазара." },
    { id: 2, name: "Legal Department", level: 4, maxLevel: 5, description: "Увеличава твоя процент (%) при преговори за договори." },
    { id: 3, name: "PR & Media", level: 1, maxLevel: 5, description: "Вдига репутацията на агенцията по-бързо след успешен мач." },
  ];

  return (
    <div className="space-y-8">
      
      {/* 1. Хедър на агенцията */}
      <div className="bg-gray-800 border border-gray-700 rounded-2xl p-6 flex items-center justify-between shadow-lg relative overflow-hidden">
        <div className="absolute -right-10 -top-10 text-gray-700/20">
          <Building size={200} />
        </div>

        <div className="relative z-10 flex items-center gap-6">
          <div className="w-24 h-24 bg-gray-900 border-2 border-yellow-500 rounded-xl flex items-center justify-center text-yellow-500 shadow-[0_0_15px_rgba(234,179,8,0.2)]">
            {renderLogo(agency.logoId)}
          </div>
          <div>
            <div className="flex items-center gap-3 mb-1">
              <h1 className="text-3xl font-black text-white uppercase tracking-wider">{agency.name}</h1>
              <span className="bg-yellow-500 text-black text-xs font-bold px-2 py-1 rounded">LEVEL {agency.level}</span>
            </div>
            <p className="text-gray-400 text-lg">CEO: <span className="text-gray-200">{agency.agentName}</span></p>
            <div className="flex items-center gap-2 mt-2 text-sm">
              <span className="text-gray-500">Reputation:</span>
              <div className="w-32 h-2 bg-gray-900 rounded-full overflow-hidden">
                <div 
                  className="h-full bg-gradient-to-r from-yellow-600 to-yellow-400" 
                  style={{ width: `${agency.reputation}%` }}
                ></div>
              </div>
              <span className="text-yellow-500 font-bold">{agency.reputation}/100</span>
            </div>
          </div>
        </div>

        {/* НОВО: БУТОН ЗА ШОРТЛИСТА ГОРИ ВДЯСНО */}
        <div className="relative z-10">
          <Link 
            to="/my-shortlist" 
            className="flex items-center gap-2 bg-gray-900 border border-gray-700 hover:border-yellow-500 text-white px-5 py-3 rounded-xl font-bold transition-all hover:shadow-[0_0_15px_rgba(234,179,8,0.2)] group"
          >
            <Bookmark className="text-yellow-500 group-hover:fill-yellow-500 transition-all" size={20} />
            My Shortlist
          </Link>
        </div>
      </div>

      {/* 2. Бързи статистики (Вече с реални данни за бюджет и играчи) */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="bg-gray-800 border border-gray-700 rounded-xl p-5 flex items-center gap-4 hover:border-gray-500 transition-colors">
          <div className="p-3 bg-gray-900 rounded-lg text-yellow-500"><Wallet size={24} /></div>
          <div>
            <p className="text-sm text-gray-500 font-medium">Available Budget</p>
            <p className="text-xl font-bold text-white font-mono">{formatMoney(agency.budget)}</p>
          </div>
        </div>
        
        <div className="bg-gray-800 border border-gray-700 rounded-xl p-5 flex items-center gap-4 hover:border-gray-500 transition-colors">
          <div className="p-3 bg-gray-900 rounded-lg text-blue-500"><Users size={24} /></div>
          <div>
            <p className="text-sm text-gray-500 font-medium">Total Players</p>
            <p className="text-xl font-bold text-white">{agency.totalPlayersCount}</p>
          </div>
        </div>

        <div className="bg-gray-800 border border-gray-700 rounded-xl p-5 flex items-center gap-4 hover:border-gray-500 transition-colors">
          <div className="p-3 bg-gray-900 rounded-lg text-purple-500"><Building size={24} /></div>
          <div>
            <p className="text-sm text-gray-500 font-medium">Active Contracts</p>
            <p className="text-xl font-bold text-white">{agency.totalPlayersCount}</p>
          </div>
        </div>

        <div className="bg-gray-800 border border-gray-700 rounded-xl p-5 flex items-center gap-4 hover:border-gray-500 transition-colors">
          <div className="p-3 bg-gray-900 rounded-lg text-emerald-500"><TrendingUp size={24} /></div>
          <div>
            <p className="text-sm text-gray-500 font-medium">Weekly Income</p>
            <p className="text-xl font-bold text-white">+$0</p>
          </div>
        </div>
      </div>

      {/* 3. Инфраструктура / Ъпгрейди (Остава статично за сега) */}
      <div>
        <h2 className="text-xl font-bold text-white mb-4 flex items-center gap-2">
          <Target className="text-yellow-500" />
          Agency Infrastructure
        </h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {facilities.map((facility) => (
            <div key={facility.id} className="bg-gray-800 border border-gray-700 rounded-xl p-5 flex flex-col">
              <div className="flex justify-between items-start mb-2">
                <h3 className="text-lg font-bold text-white">{facility.name}</h3>
                <span className="text-xs font-mono bg-gray-900 text-gray-400 px-2 py-1 rounded border border-gray-700">
                  Lvl {facility.level}/{facility.maxLevel}
                </span>
              </div>
              <p className="text-gray-400 text-sm mb-6 flex-1">{facility.description}</p>
              
              <button 
                className={`w-full py-2 rounded-lg font-bold text-sm transition-colors ${
                  facility.level === facility.maxLevel 
                    ? 'bg-gray-900 text-gray-600 cursor-not-allowed' 
                    : 'bg-gray-700 text-white hover:bg-yellow-500 hover:text-black'
                }`}
                disabled={facility.level === facility.maxLevel}
              >
                {facility.level === facility.maxLevel ? 'MAX LEVEL' : 'UPGRADE ($50,000)'}
              </button>
            </div>
          ))}
        </div>
      </div>

    </div>
  );
}