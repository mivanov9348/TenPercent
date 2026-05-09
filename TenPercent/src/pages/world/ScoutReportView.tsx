import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, FileText, AlertCircle, TrendingUp, TrendingDown, BrainCircuit, Loader2, Info } from 'lucide-react';
import { API_URL } from '../../config';

export default function ScoutReportView() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [report, setReport] = useState<any>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchReport = async () => {
      try {
        const userId = localStorage.getItem('userId');
        if (!userId) return;

        const response = await fetch(`${API_URL}/scouting/report/${userId}/${id}`);
        const data = await response.json();

        if (response.ok) {
          setReport(data);
        } else {
          setError(data.message || "Failed to load report.");
        }
      } catch (err: any) {
        setError("Network error. Could not reach server.");
      } finally {
        setIsLoading(false);
      }
    };

    fetchReport();
  }, [id]);

  const formatMoney = (amount: number) => {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(amount);
  };

  const getGradeColor = (grade: string) => {
    if (grade.includes('A')) return 'bg-emerald-500/20 text-emerald-400 border-emerald-500/50';
    if (grade.includes('B')) return 'bg-blue-500/20 text-blue-400 border-blue-500/50';
    if (grade.includes('C')) return 'bg-yellow-500/20 text-yellow-500 border-yellow-500/50';
    return 'bg-red-500/20 text-red-400 border-red-500/50';
  };

  if (isLoading) {
    return <div className="flex justify-center items-center h-64"><Loader2 className="animate-spin text-yellow-500" size={40} /></div>;
  }

  if (error || !report) {
    return (
      <div className="max-w-3xl mx-auto mt-10 p-6 bg-gray-900 border border-gray-800 rounded-2xl text-center">
        <AlertCircle size={48} className="text-red-500 mx-auto mb-4" />
        <h2 className="text-xl font-bold text-white mb-2">Report Not Found</h2>
        <p className="text-gray-400 mb-6">{error}</p>
        <button onClick={() => navigate(-1)} className="px-6 py-2 bg-gray-800 hover:bg-gray-700 text-white font-bold rounded-xl transition-colors">
          Go Back
        </button>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto space-y-6 pb-12">
      <div className="flex justify-between items-center">
        <button onClick={() => navigate(-1)} className="flex items-center gap-2 text-gray-400 hover:text-white transition-colors font-bold text-sm">
          <ArrowLeft size={16} /> Back to Player
        </button>
        <div className="flex items-center gap-2 bg-gray-900 border border-gray-800 px-3 py-1.5 rounded-lg text-xs font-bold text-gray-400">
          <Info size={14} /> Scout Knowledge Level: <span className="text-yellow-500">{report.knowledgeLevel}/5</span>
        </div>
      </div>

      <div className="bg-gray-900 border border-gray-800 rounded-3xl p-8 shadow-2xl relative overflow-hidden">
        {/* Header */}
        <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-6 border-b border-gray-800 pb-6 mb-6">
          <div className="flex items-center gap-4">
            <div className="w-16 h-16 bg-blue-600/20 border border-blue-500/30 rounded-2xl flex items-center justify-center text-blue-500">
              <FileText size={32} />
            </div>
            <div>
              <p className="text-sm font-bold text-gray-500 uppercase tracking-widest mb-1">Confidential Scout Report</p>
              <h1 className="text-3xl font-black text-white">{report.playerName}</h1>
              <p className="text-xs text-gray-500 mt-1">Generated: {new Date(report.generatedAt).toLocaleDateString()}</p>
            </div>
          </div>
          
          <div className="text-center md:text-right w-full md:w-auto">
            <p className="text-xs font-bold text-gray-500 uppercase tracking-widest mb-2">Final Recommendation</p>
            <div className={`inline-block px-6 py-2 border rounded-xl font-black text-lg shadow-lg ${getGradeColor(report.recommendationGrade)}`}>
              {report.recommendationGrade}
            </div>
          </div>
        </div>

        {/* Stats Estimates */}
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
          <div className="bg-gray-800/50 rounded-xl p-4 text-center border border-gray-800">
            <p className="text-gray-500 text-[10px] font-bold uppercase tracking-widest mb-1">Est. Ability (OVR)</p>
            <p className="text-2xl font-black text-white font-mono">
              {report.minOVR === report.maxOVR ? report.minOVR : `${report.minOVR}-${report.maxOVR}`}
            </p>
          </div>
          <div className="bg-gray-800/50 rounded-xl p-4 text-center border border-gray-800">
            <p className="text-gray-500 text-[10px] font-bold uppercase tracking-widest mb-1">Est. Potential</p>
            <p className="text-2xl font-black text-green-400 font-mono">
              {report.minPOT === report.maxPOT ? report.minPOT : `${report.minPOT}-${report.maxPOT}`}
            </p>
          </div>
          <div className="bg-gray-800/50 rounded-xl p-4 text-center border border-gray-800">
            <p className="text-gray-500 text-[10px] font-bold uppercase tracking-widest mb-1">Est. Market Value</p>
            <p className="text-xl font-black text-white font-mono">{formatMoney(report.estimatedValue)}</p>
          </div>
          <div className="bg-gray-800/50 rounded-xl p-4 text-center border border-gray-800">
            <p className="text-gray-500 text-[10px] font-bold uppercase tracking-widest mb-1">Est. Wage Demand</p>
            <p className="text-xl font-black text-white font-mono">{formatMoney(report.estimatedWage)}/wk</p>
          </div>
        </div>

        {/* Written Report */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
          <div className="space-y-6">
            <div>
              <h3 className="flex items-center gap-2 text-lg font-bold text-emerald-400 mb-3">
                <TrendingUp size={20} /> Pros / Strengths
              </h3>
              <p className="text-gray-300 leading-relaxed text-sm bg-gray-800/30 p-4 rounded-xl border border-gray-800/50 italic">
                "{report.strengths}"
              </p>
            </div>
            
            <div>
              <h3 className="flex items-center gap-2 text-lg font-bold text-red-400 mb-3">
                <TrendingDown size={20} /> Cons / Weaknesses
              </h3>
              <p className="text-gray-300 leading-relaxed text-sm bg-gray-800/30 p-4 rounded-xl border border-gray-800/50 italic">
                "{report.weaknesses}"
              </p>
            </div>
          </div>

          <div>
            <h3 className="flex items-center gap-2 text-lg font-bold text-purple-400 mb-3">
              <BrainCircuit size={20} /> Personality & Character
            </h3>
            <div className="bg-purple-900/10 border border-purple-500/20 p-5 rounded-xl h-full">
              <p className="text-gray-300 leading-relaxed text-sm">
                {report.personalityNotes}
              </p>
            </div>
          </div>
        </div>

      </div>
    </div>
  );
}