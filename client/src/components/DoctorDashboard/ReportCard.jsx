// src/components/DoctorDashboard/ReportCard.jsx
import React from 'react';
import { FileText, Download, Edit, Trash2, Eye } from 'lucide-react';

const ReportCard = ({ 
  report, 
  onEdit, 
  onDelete, 
  onView, 
  onDownload 
}) => {
  return (
    <div className="bg-gradient-to-br from-slate-700/50 to-slate-800/50 rounded-2xl p-6 border border-slate-600/50 hover:border-purple-500/50 transition-all duration-300 hover:shadow-2xl hover:shadow-purple-500/20 group">
    
      <div className="flex items-start justify-between mb-4">
        <div className="flex-1">
          <div className="flex items-center gap-2 mb-2">
            <div className="w-3 h-3 bg-purple-500 rounded-full"></div>
            <span className="text-purple-400 text-xs font-medium uppercase tracking-wide">
              Medicine Report
            </span>
          </div>
          <h3 className="text-white font-bold text-lg mb-1 group-hover:text-purple-300 transition-colors">
            {report.userName || 'Pacient i panjohur'}
          </h3>
          <p className="text-slate-400 text-sm">
            {report.doctorName || 'Mjek i panjohur'}
          </p>
        </div>
        <div className="bg-purple-500/20 rounded-full p-2 group-hover:bg-purple-500/30 transition-colors">
          <FileText className="w-5 h-5 text-purple-400" />
        </div>
      </div>

      {/* Report Overview */}
      <div className="space-y-3 mb-4">
        <div className="flex items-center gap-2 text-slate-300">
          <i className="fas fa-calendar text-blue-400 text-sm"></i>
          <span className="text-sm">
            {new Date(report.reportDate).toLocaleDateString('sq-AL', {
              day: 'numeric',
              month: 'long',
              year: 'numeric'
            })}
          </span>
        </div>
        
        {report.diagnosis && (
          <div className="bg-slate-600/30 rounded-lg p-3">
            <p className="text-slate-300 text-sm font-medium mb-1">Diagnose</p>
            <p className="text-white text-sm line-clamp-2">{report.diagnosis}</p>
          </div>
        )}

        {report.treatment && (
          <div className="bg-slate-600/30 rounded-lg p-3">
            <p className="text-slate-300 text-sm font-medium mb-1">Treatment</p>
            <p className="text-white text-sm line-clamp-2">{report.treatment}</p>
          </div>
        )}
      </div>

      {/* Action Button */}
      <div className="flex justify-between items-center pt-4 border-t border-slate-600/50">
        <div className="flex gap-1">
          <button
            onClick={() => onEdit(report)}
            className="p-2 bg-blue-500/20 hover:bg-blue-500/40 text-blue-400 rounded-lg transition-all duration-200 hover:scale-110"
            title="Modifiko Raportin"
          >
            <Edit className="w-4 h-4" />
          </button>
          <button
            onClick={() => onView(report)}
            className="p-2 bg-indigo-500/20 hover:bg-indigo-500/40 text-indigo-400 rounded-lg transition-all duration-200 hover:scale-110"
            title="Shiko Raportin"
          >
            <Eye className="w-4 h-4" />
          </button>
        </div>
        
        <div className="flex gap-1">
          <button
            onClick={() => onDownload(report.id)}
            className="p-2 bg-green-500/20 hover:bg-green-500/40 text-green-400 rounded-lg transition-all duration-200 hover:scale-110"
            title="Shkarko PDF"
          >
            <Download className="w-4 h-4" />
          </button>
          <button
            onClick={() => onDelete(report.id)}
            className="p-2 bg-red-500/20 hover:bg-red-500/40 text-red-400 rounded-lg transition-all duration-200 hover:scale-110"
            title="Fshi Raportin"
          >
            <Trash2 className="w-4 h-4" />
          </button>
        </div>
      </div>
    </div>
  );
};

export default ReportCard;