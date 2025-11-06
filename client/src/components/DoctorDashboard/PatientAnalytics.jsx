// client/components/PatientAnalytics.jsx
"use client";

import React, { useEffect, useState } from "react";
import { Bar } from "react-chartjs-2";
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
} from "chart.js";
import { FileText, Calendar, Pill, Activity, X, BarChart3 } from "lucide-react";

import ReportCardMini from "./ReportCardMini";
import {
  generateReportPDFTemplate,
  downloadHtmlAsPdf,
} from "./ReportPDFTemplate";

ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend
);

/* ------------------------------------------------------------------ */
/* API helpers (unchanged)                                            */
/* ------------------------------------------------------------------ */
const USERHEALTH_API_URL =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:5029/api";

const userHealthApi = {
  get: (endpoint) =>
    fetch(`${USERHEALTH_API_URL}${endpoint}`, {
      credentials: "include",
    }).then((r) => (r.ok ? r.json() : Promise.reject(r))),
};

/* ------------------------------------------------------------------ */
/* PatientAnalytics component                                         */
/* ------------------------------------------------------------------ */
const PatientAnalytics = ({ patient, doctor, onEditReport, onClose }) => {
  const [analytics, setAnalytics] = useState({
    totalAppointments: 0,
    totalMedications: 0,
    totalReports: 0,
    lastAppointment: null,
    appointmentMonths: [],
    diagnosisCounts: [],
    patientReports: [],
  });

  /* -------------------------------------------------------------- */
  /* Load all data for the selected patient                         */
  /* -------------------------------------------------------------- */
  useEffect(() => {
    const fetchPatientAnalytics = async () => {
      try {
        const [appointmentsRes, medsRes, reportsRes] = await Promise.all([
          userHealthApi.get("/appointments"),
          fetch(
            `${
              process.env.NEXT_PUBLIC_MEDICATION_API_URL ||
              "http://localhost:5077/api"
            }/medications`,
            { credentials: "include" }
          ).then((r) => (r.ok ? r.json() : [])),
          userHealthApi.get("/appointmentreports"),
        ]);

        const patientAppointments = appointmentsRes.filter(
          (a) => a.userId === patient.id && a.doctorId === doctor.id
        );
        const patientMeds = medsRes.filter(
          (m) => m.userId === patient.id && m.doctorId === doctor.id
        );
        const patientReports = reportsRes.filter(
          (r) => r.userId === patient.id && r.doctorId === doctor.id
        );

        /* ---------- appointments per month (last 6) ---------- */
        const monthMap = {};
        patientAppointments.forEach((a) => {
          const d = new Date(a.appointmentDate);
          const key = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(
            2,
            "0"
          )}`;
          monthMap[key] = (monthMap[key] || 0) + 1;
        });
        const last6Months = Array.from({ length: 6 }, (_, i) => {
          const d = new Date();
          d.setMonth(d.getMonth() - i);
          return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(
            2,
            "0"
          )}`;
        }).reverse();
        const appointmentMonths = last6Months.map((m) => monthMap[m] || 0);

        /* ---------- top 5 diagnoses ---------- */
        const diagMap = {};
        patientReports.forEach((r) => {
          if (r.diagnosis)
            diagMap[r.diagnosis] = (diagMap[r.diagnosis] || 0) + 1;
        });
        const diagnosisCounts = Object.entries(diagMap)
          .sort((a, b) => b[1] - a[1])
          .slice(0, 5);

        /* ---------- last appointment date ---------- */
        let lastAppointment = null;
        if (patientAppointments.length) {
          lastAppointment = [...patientAppointments].sort(
            (a, b) => new Date(b.appointmentDate) - new Date(a.appointmentDate)
          )[0].appointmentDate;
        } else if (patientReports.length) {
          lastAppointment = [...patientReports].sort(
            (a, b) => new Date(b.reportDate) - new Date(a.reportDate)
          )[0].reportDate;
        }

        setAnalytics({
          totalAppointments: patientAppointments.length,
          totalMedications: patientMeds.length,
          totalReports: patientReports.length,
          lastAppointment,
          appointmentMonths,
          diagnosisCounts,
          patientReports,
        });
      } catch (e) {
        console.error("PatientAnalytics fetch error:", e);
      }
    };

    fetchPatientAnalytics();
  }, [patient, doctor]);

  /* -------------------------------------------------------------- */
  /* Report callbacks                                               */
  /* -------------------------------------------------------------- */
  const handleEditReport = (report) => onEditReport?.(report);

  const handleDeleteReport = async (reportId) => {
    if (!window.confirm("Delete this report?")) return;
    try {
      await fetch(`${USERHEALTH_API_URL}/appointmentreports/${reportId}`, {
        method: "DELETE",
        credentials: "include",
      });
      setAnalytics((prev) => ({
        ...prev,
        patientReports: prev.patientReports.filter((r) => r.id !== reportId),
        totalReports: prev.totalReports - 1,
      }));
    } catch {
      alert("Failed to delete report");
    }
  };

  const viewReportQuick = (report) => {
    const details = `
Detailed Report
===============
Patient: ${patient.firstName} ${patient.lastName}
Doctor: Dr. ${doctor.firstName} ${doctor.lastName}
DATE: ${new Date(report.reportDate).toLocaleDateString("sq-AL")}

Diagnosis: ${report.diagnosis || "—"}
Symptoms: ${report.symptoms || "—"}
Treatment: ${report.treatment || "—"}
Medications: ${report.medications || "—"}
Notes: ${report.notes || "—"}
Recommendations: ${report.recommendations || "—"}
    `.trim();

    const win = window.open("", "Report", "width=620,height=720");
    win.document.write(`
      <html><head><title>Report</title></head>
      <body style="font-family:Arial;padding:24px;">
        <h2 style="color:#2c5aa0;">Medical Report</h2>
        <pre style="white-space:pre-wrap;">${details}</pre>
        <button onclick="window.print()" style="margin-top:20px;padding:10px;background:#2c5aa0;color:white;border:none;border-radius:5px;">Print</button>
      </body></html>
    `);
    win.document.close();
  };

  const handleDownloadPdf = async (reportId) => {
    try {
      const report = await userHealthApi.get(`/appointmentreports/${reportId}`);
      const html = generateReportPDFTemplate(report);
      await downloadHtmlAsPdf(html, `report-${reportId}.pdf`);
    } catch {
      alert("PDF generation failed");
    }
  };

  /* -------------------------------------------------------------- */
  /* Chart data & options                                           */
  /* -------------------------------------------------------------- */
  const appointmentChartData = {
    labels: [
      "6 mo ago",
      "5 mo ago",
      "4 mo ago",
      "3 mo ago",
      "2 mo ago",
      "Last month",
    ],
    datasets: [
      {
        label: "Appointments",
        data: analytics.appointmentMonths,
        backgroundColor: "rgba(14, 165, 233, 0.7)",
        borderColor: "rgb(14, 165, 233)",
        borderWidth: 2,
        borderRadius: 8,
      },
    ],
  };

  const diagnosisChartData = {
    labels: analytics.diagnosisCounts.map((d) => d[0]),
    datasets: [
      {
        label: "Occurrences",
        data: analytics.diagnosisCounts.map((d) => d[1]),
        backgroundColor: "rgba(34, 197, 94, 0.7)",
        borderColor: "rgb(34, 197, 94)",
        borderWidth: 2,
        borderRadius: 8,
      },
    ],
  };

  const chartOptions = {
    responsive: true,
    maintainAspectRatio: true,
    plugins: {
      legend: { display: false },
      tooltip: {
        backgroundColor: "rgba(15, 23, 42, 0.95)",
        padding: 12,
        borderColor: "rgba(148, 163, 184, 0.2)",
        borderWidth: 1,
        titleColor: "#f1f5f9",
        bodyColor: "#cbd5e1",
        cornerRadius: 8,
      },
    },
    scales: {
      x: {
        grid: { display: false },
        ticks: { color: "#94a3b8", font: { size: 11 } },
      },
      y: {
        grid: { color: "rgba(148, 163, 184, 0.1)" },
        ticks: { color: "#94a3b8", font: { size: 11 } },
      },
    },
  };

  /* -------------------------------------------------------------- */
  /* Render                                                         */
  /* -------------------------------------------------------------- */
  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900 text-white p-6 md:p-8">
      <div className="max-w-7xl mx-auto">
        {/* ---------- Header ---------- */}
        <div className="flex items-start justify-between mb-8 pb-6 border-b border-slate-700/50">
          <div className="flex-1">
            <div className="flex items-center gap-3 mb-2">
              <div className="w-12 h-12 rounded-full bg-gradient-to-br from-sky-400 to-blue-500 flex items-center justify-center text-white font-bold text-lg shadow-lg">
                {patient.firstName?.[0]}
                {patient.lastName?.[0]}
              </div>
              <div>
                <h2 className="text-3xl font-bold bg-gradient-to-r from-white to-slate-300 bg-clip-text text-transparent">
                  {patient.firstName} {patient.lastName}
                </h2>
                <p className="text-slate-400 text-sm">{patient.email}</p>
              </div>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-2 hover:bg-slate-700/50 rounded-lg transition-all duration-200 hover:scale-105"
            aria-label="Close"
          >
            <X className="w-6 h-6 text-slate-400" />
          </button>
        </div>

        {/* ---------- Stats Cards ---------- */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
          {[
            {
              icon: Calendar,
              label: "Appointments",
              value: analytics.totalAppointments,
              gradient: "from-blue-500 to-cyan-500",
              bgGradient: "from-blue-500/10 to-cyan-500/10",
            },
            {
              icon: Pill,
              label: "Medications",
              value: analytics.totalMedications,
              gradient: "from-emerald-500 to-teal-500",
              bgGradient: "from-emerald-500/10 to-teal-500/10",
            },
            {
              icon: FileText,
              label: "Reports",
              value: analytics.totalReports,
              gradient: "from-amber-500 to-orange-500",
              bgGradient: "from-amber-500/10 to-orange-500/10",
            },
            {
              icon: Activity,
              label: "Last Visit",
              value: analytics.lastAppointment
                ? new Date(analytics.lastAppointment).toLocaleDateString(
                    "sq-AL"
                  )
                : "—",
              gradient: "from-pink-500 to-rose-500",
              bgGradient: "from-pink-500/10 to-rose-500/10",
            },
          ].map((s, i) => (
            <div
              key={i}
              className={`relative overflow-hidden bg-gradient-to-br ${s.bgGradient} backdrop-blur-xl rounded-2xl p-6 border border-white/10 shadow-xl hover:shadow-2xl transition-all duration-300 hover:-translate-y-1`}
            >
              <div
                className={`inline-flex p-3 rounded-xl bg-gradient-to-br ${s.gradient} shadow-lg mb-4`}
              >
                <s.icon className="w-6 h-6 text-white" />
              </div>
              <p className="text-3xl font-bold mb-1 text-white">{s.value}</p>
              <p className="text-slate-400 text-sm font-medium">{s.label}</p>
              <div
                className={`absolute -right-6 -bottom-6 w-24 h-24 bg-gradient-to-br ${s.gradient} opacity-10 rounded-full blur-2xl`}
              />
            </div>
          ))}
        </div>

        {/* ---------- Charts ---------- */}
        <div className="grid grid-cols-1 xl:grid-cols-2 gap-6 mb-8">
          {/* Appointments Trend */}
          <div className="bg-slate-800/40 backdrop-blur-xl rounded-2xl p-6 border border-slate-700/50 shadow-xl">
            <div className="flex items-center gap-3 mb-6">
              <div className="p-2 rounded-lg bg-sky-500/20">
                <BarChart3 className="w-5 h-5 text-sky-400" />
              </div>
              <h3 className="text-xl font-semibold text-white">
                Appointments Trend
              </h3>
            </div>
            <div className="h-64">
              <Bar data={appointmentChartData} options={chartOptions} />
            </div>
          </div>

          {/* Common Diagnoses */}
          <div className="bg-slate-800/40 backdrop-blur-xl rounded-2xl p-6 border border-slate-700/50 shadow-xl">
            <div className="flex items-center gap-3 mb-6">
              <div className="p-2 rounded-lg bg-emerald-500/20">
                <Activity className="w-5 h-5 text-emerald-400" />
              </div>
              <h3 className="text-xl font-semibold text-white">
                Common Diagnoses
              </h3>
            </div>
            {analytics.diagnosisCounts.length ? (
              <div className="h-64">
                <Bar data={diagnosisChartData} options={chartOptions} />
              </div>
            ) : (
              <div className="h-64 flex items-center justify-center">
                <div className="text-center">
                  <Activity className="w-12 h-12 text-slate-600 mx-auto mb-3 opacity-50" />
                  <p className="text-slate-500">No diagnosis data available</p>
                </div>
              </div>
            )}
          </div>
        </div>

        {/* ---------- Reports (ReportCardMini) ---------- */}
        <div className="bg-slate-800/40 backdrop-blur-xl rounded-2xl p-6 border border-slate-700/50 shadow-xl">
          <div className="flex items-center gap-3 mb-6">
            <div className="p-2 rounded-lg bg-amber-500/20">
              <FileText className="w-6 h-6 text-amber-400" />
            </div>
            <h4 className="text-xl font-semibold text-white">
              Medical Reports
              <span className="ml-2 text-sm font-normal text-slate-400">
                ({analytics.totalReports})
              </span>
            </h4>
          </div>

          {analytics.patientReports?.length > 0 ? (
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
              {analytics.patientReports.map((report) => (
                <ReportCardMini
                  key={report.id}
                  report={report}
                  onEdit={handleEditReport}
                  onDelete={handleDeleteReport}
                  onView={viewReportQuick}
                  onDownload={handleDownloadPdf}
                />
              ))}
            </div>
          ) : (
            <div className="text-center py-12">
              <div className="inline-flex p-4 rounded-full bg-slate-700/30 mb-4">
                <FileText className="w-12 h-12 text-slate-600" />
              </div>
              <p className="text-slate-400 text-lg">
                No reports for this patient yet
              </p>
              <p className="text-slate-500 text-sm mt-2">
                Reports will appear here once created
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default PatientAnalytics;
