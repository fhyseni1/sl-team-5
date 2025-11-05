// client/components/ReportCardMini.jsx
import {
  Edit,
  Trash2,
  Download,
  Eye,
  Pill,
  Calendar,
  Activity,
} from "lucide-react";

const ReportCardMini = ({ report, onEdit, onDelete, onView, onDownload }) => {
  const date = new Date(report.reportDate).toLocaleDateString("sq-AL");

  // Extract medications count (split by comma, semicolon, or newline)
  const medsList = report.medications
    ?.split(/[,;\n]+/)
    .map((m) => m.trim())
    .filter(Boolean);
  const medsCount = medsList?.length || 0;

  // Estimate treatment duration from notes or treatment field
  const durationMatch =
    report.treatment?.match(/(\d+)\s*(day|week|month)s?/i) ||
    report.notes?.match(/(\d+)\s*(day|week|month)s?/i);
  const duration = durationMatch
    ? `${durationMatch[1]} ${durationMatch[2]}${
        durationMatch[1] > 1 ? "s" : ""
      }`
    : null;

  return (
    <div className="bg-slate-800/60 backdrop-blur-sm rounded-xl p-5 border border-slate-700 hover:border-slate-600 transition-all duration-200 hover:shadow-lg group relative overflow-hidden">
      {/* Gradient accent */}
      <div className="absolute top-0 right-0 w-24 h-24 bg-gradient-to-bl from-amber-500/10 to-transparent rounded-full blur-xl -mr-12 -mt-12" />

      <div className="flex justify-between items-start mb-4">
        <div className="flex-1">
          <h4 className="text-white font-semibold text-base line-clamp-1">
            {report.diagnosis || "No Diagnosis"}
          </h4>
          <p className="text-slate-400 text-xs mt-1">{date}</p>
        </div>

        {/* Action buttons */}
        <div className="flex gap-1.5 ml-3 opacity-70 group-hover:opacity-100 transition-opacity">
          <button
            onClick={() => onView(report)}
            className="p-2 hover:bg-slate-700 rounded-lg transition-colors"
            title="Quick view"
          >
            <Eye className="w-4 h-4 text-slate-400 hover:text-sky-400" />
          </button>
          <button
            onClick={() => onEdit(report)}
            className="p-2 hover:bg-slate-700 rounded-lg transition-colors"
            title="Edit"
          >
            <Edit className="w-4 h-4 text-slate-400 hover:text-blue-400" />
          </button>
          <button
            onClick={() => onDownload(report.id)}
            className="p-2 hover:bg-slate-700 rounded-lg transition-colors"
            title="Download PDF"
          >
            <Download className="w-4 h-4 text-slate-400 hover:text-emerald-400" />
          </button>
          <button
            onClick={() => onDelete(report.id)}
            className="p-2 hover:bg-red-900/30 rounded-lg transition-colors"
            title="Delete"
          >
            <Trash2 className="w-4 h-4 text-slate-400 hover:text-red-400" />
          </button>
        </div>
      </div>

      {/* Symptoms preview */}
      <p className="text-slate-300 text-sm line-clamp-2 leading-relaxed mb-4">
        {report.symptoms || "No symptoms recorded"}
      </p>

      {/* Report Stats Row */}
      <div className="flex items-center gap-4 text-xs">
        {/* Medications */}
        <div className="flex items-center gap-1.5 text-emerald-400">
          <Pill className="w-3.5 h-3.5" />
          <span className="font-medium">
            {medsCount} med{medsCount !== 1 ? "s" : ""}
          </span>
        </div>

        {/* Duration */}
        {duration && (
          <div className="flex items-center gap-1.5 text-amber-400">
            <Calendar className="w-3.5 h-3.5" />
            <span className="font-medium">{duration}</span>
          </div>
        )}

        {/* Severity / Follow-up hint */}
        {report.recommendations?.toLowerCase().includes("follow") && (
          <div className="flex items-center gap-1.5 text-pink-400">
            <Activity className="w-3.5 h-3.5" />
            <span className="font-medium">Follow-up</span>
          </div>
        )}
      </div>

      {/* Optional: Show first med name if available */}
      {medsCount > 0 && medsList?.[0] && (
        <p className="text-slate-500 text-xs mt-2 truncate">
          e.g. {medsList[0]}
        </p>
      )}
    </div>
  );
};

export default ReportCardMini;
