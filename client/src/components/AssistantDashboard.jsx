// components/AssistantDashboard.jsx
"use client";

import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import {
  Calendar,
  Users,
  Clock,
  CheckCircle2,
  XCircle,
  LogOut,
  Loader2,
  UserCheck,
  Activity,
} from "lucide-react";
import { authService, api } from "../../services/authService";

const AssistantDashboard = () => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [pendingAppointments, setPendingAppointments] = useState([]);
  const [approvedAppointments, setApprovedAppointments] = useState([]); 
  const [assignedDoctors, setAssignedDoctors] = useState([]);
  const [stats, setStats] = useState({
    pendingAppointments: 0,
    approvedAppointments: 0,
  });
  const [approvedPage, setApprovedPage] = useState(1);
  const itemsPerPage = 5;
  const router = useRouter();

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const userData = await authService.getMe();
        
        const isAssistant = userData.type === 7; // Assistant
        if (!isAssistant) {
          toast.error("Access denied. Assistant privileges required.");
          router.push("/dashboard/dashboard");
          return;
        }

        setUser(userData);
        await loadAssistantData(userData.id);
      } catch (err) {
        console.error("Assistant dashboard error:", err);
        if (err.response?.status === 401) router.push("/login");
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [router]);

const loadAssistantData = async (assistantId) => {
  try {
    console.log("🟡 Loading assistant data for:", assistantId);

    const [pendingRes, doctorsRes, approvedRes] = await Promise.allSettled([
      api.get(`/appointments/assistant/${assistantId}/pending`),
      api.get(`/users/assistant/${assistantId}/doctors`),
      api.get(`/appointments/assistant/${assistantId}/approved`)
    ]);

    console.log("📊 Pending appointments response:", pendingRes);
    console.log("👨‍⚕️ Assigned doctors response:", doctorsRes);
    console.log("✅ Approved appointments response:", approvedRes);
    
    const pendingAppointments = pendingRes.status === 'fulfilled' ? pendingRes.value.data || [] : [];
    const assignedDoctors = doctorsRes.status === 'fulfilled' ? doctorsRes.value.data || [] : [];
    const approvedAppointments = approvedRes.status === 'fulfilled' ? approvedRes.value.data || [] : [];

    console.log("📈 Pending appointments count:", pendingAppointments.length);
    console.log("🏥 Assigned doctors count:", assignedDoctors.length);
    console.log("✅ Approved appointments count:", approvedAppointments.length);
    
    assignedDoctors.forEach(doctor => {
      console.log(`👨‍⚕️ Assigned doctor: ${doctor.name} (${doctor.email})`);
    });

    pendingAppointments.forEach(app => {
      console.log(`📅 Pending: ${app.doctorName} for ${app.userName} on ${app.appointmentDate}`);
    });

    if (pendingRes.status === 'rejected') {
      console.error("❌ Error fetching pending appointments:", pendingRes.reason);
      toast.warning("Cannot load pending appointments");
    }

    if (doctorsRes.status === 'rejected') {
      console.error("❌ Error fetching assigned doctors:", doctorsRes.reason);
      toast.warning("Cannot load assigned doctors");
    }

    if (approvedRes.status === 'rejected') {
      console.error("❌ Error fetching approved appointments:", approvedRes.reason);
    }

    setPendingAppointments(pendingAppointments);
    setAssignedDoctors(assignedDoctors);
    setApprovedAppointments(approvedAppointments);
    
    setStats({
      pendingAppointments: pendingAppointments.length,
      approvedAppointments: approvedAppointments.length,
      totalDoctors: assignedDoctors.length
    });

  } catch (err) {
    console.error("❌ Error loading assistant data:", err);
    toast.error("Failed to load dashboard data");
  }
};

  const paginate = (data, page) => {
    const start = (page - 1) * itemsPerPage;
    return data.slice(start, start + itemsPerPage);
  };

const handleApproveAppointment = async (appointmentId) => {
  try {
    console.log("🟡 Attempting to approve appointment:", appointmentId);
    console.log("🟡 Current user ID:", user?.id);
    
    const appointmentToApprove = pendingAppointments.find(app => app.id === appointmentId);
    console.log("📋 Appointment details:", appointmentToApprove);
    
    if (!appointmentToApprove) {
      console.error("❌ Appointment not found in local state");
      toast.error("Appointment not found");
      return;
    }

    const response = await api.put(`/appointments/${appointmentId}/assistant-approve`);
    
    console.log("✅ Approval response status:", response.status);
    console.log("✅ Approval response data:", response.data);

    setPendingAppointments(prev => 
      prev.filter(app => app.id !== appointmentId)
    );

    setApprovedAppointments(prev => [...prev, { 
      ...appointmentToApprove, 
      status: 'Approved' 
    }]);
    
    setStats(prev => ({
      ...prev,
      pendingAppointments: prev.pendingAppointments - 1,
      approvedAppointments: prev.approvedAppointments + 1
    }));
    
    toast.success("Appointment approved successfully!");
  } catch (err) {
    console.error("❌ Error approving appointment:", err);
    console.error("❌ Error response:", err.response);
    
    if (err.response?.status === 403) {
      toast.error("You are not authorized to approve this appointment");
    } else if (err.response?.status === 404) {
      toast.error("Appointment not found");
    } else {
      toast.error(err.response?.data?.message || "Failed to approve appointment");
    }
  }
};
const handleRejectAppointment = async (appointmentId, rejectionReason) => {
  try {
    console.log("🟡 Attempting to reject appointment:", appointmentId);
    
    if (!rejectionReason.trim()) {
      toast.error("Please provide a reason for rejection");
      return;
    }

    const response = await api.put(`/appointments/${appointmentId}/assistant-reject`, {
      rejectionReason: rejectionReason
    });

    console.log("✅ Rejection response status:", response.status);

    // Update local state
    setPendingAppointments(prev => 
      prev.filter(app => app.id !== appointmentId)
    );
    
    setStats(prev => ({
      ...prev,
      pendingAppointments: prev.pendingAppointments - 1
    }));
    
    toast.success("Appointment rejected successfully!");
  } catch (err) {
    console.error("❌ Error rejecting appointment:", err);
    toast.error(err.response?.data?.message || "Failed to reject appointment");
  }
};

  const handleLogout = async () => {
    await authService.logout();
    router.push("/");
  };

  if (loading) return (
    <div className="min-h-screen flex items-center justify-center">
      <Loader2 className="w-16 h-16 animate-spin text-blue-500" />
    </div>
  );

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900 relative">
      <ToastContainer theme="dark" />
      
      <header className="bg-slate-900/80 backdrop-blur-xl border-b border-slate-700/50 sticky top-0 z-50">
        <div className="max-w-7xl mx-auto px-6 py-5">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4">
              <div className="w-14 h-14 bg-gradient-to-br from-purple-600 to-pink-600 rounded-2xl flex items-center justify-center shadow-xl">
                <UserCheck className="w-7 h-7 text-white" />
              </div>
              <div>
                <h1 className="text-3xl font-bold bg-gradient-to-r from-slate-100 to-slate-300 bg-clip-text text-transparent">
                  Assistant Dashboard
                </h1>
                <p className="text-slate-400">Medical Assistant Portal</p>
              </div>
            </div>
            <button
              onClick={handleLogout}
              className="flex items-center space-x-2 px-6 py-3 bg-gradient-to-r from-red-600 to-red-700 text-white rounded-xl font-bold shadow-xl hover:from-red-700 hover:to-red-800 transition-colors"
            >
              <LogOut className="w-5 h-5" />
              <span>Logout</span>
            </button>
          </div>
        </div>
      </header>

      <main className="max-w-7xl mx-auto px-6 py-12 relative z-10">
     {/* Stats Grid */}
<div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-12">
  <div className="bg-gradient-to-br from-amber-500/10 to-orange-500/10 backdrop-blur-xl rounded-2xl p-8 border border-amber-500/30">
    <Activity className="w-12 h-12 text-amber-400 mx-auto mb-4" />
    <p className="text-3xl font-bold text-white mb-2">{stats.pendingAppointments}</p>
    <p className="text-amber-400 font-semibold">Pending Appointments</p>
  </div>
  
  <div className="bg-gradient-to-br from-emerald-500/10 to-teal-500/10 backdrop-blur-xl rounded-2xl p-8 border border-emerald-500/30">
    <CheckCircle2 className="w-12 h-12 text-emerald-400 mx-auto mb-4" />
    <p className="text-3xl font-bold text-white mb-2">{stats.approvedAppointments}</p>
    <p className="text-emerald-400 font-semibold">Approved Appointments</p>
  </div>
  
  <div className="bg-gradient-to-br from-blue-500/10 to-cyan-500/10 backdrop-blur-xl rounded-2xl p-8 border border-blue-500/30">
    <Users className="w-12 h-12 text-blue-400 mx-auto mb-4" />
    <p className="text-3xl font-bold text-white mb-2">{stats.totalDoctors}</p>
    <p className="text-blue-400 font-semibold">Assigned Doctors</p>
  </div>
</div>

        {/* Assigned Doctors Section */}
        {assignedDoctors.length > 0 && (
          <div className="bg-slate-800/50 backdrop-blur-xl rounded-3xl border border-slate-700/50 p-8 mb-8">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-2xl font-bold text-white flex items-center gap-3">
                <Users className="w-7 h-7 text-blue-400" />
                Your Assigned Doctors ({assignedDoctors.length})
              </h2>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {assignedDoctors.map((doctor) => (
                <div key={doctor.id} className="bg-slate-700/30 rounded-xl p-4 border border-slate-600/50">
                  <h3 className="text-white font-semibold text-lg">{doctor.name}</h3>
                  <p className="text-slate-300 text-sm">{doctor.email}</p>
                  {doctor.phoneNumber && (
                    <p className="text-slate-400 text-sm">📞 {doctor.phoneNumber}</p>
                  )}
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Pending Appointments Section */}
        <div className="bg-slate-800/50 backdrop-blur-xl rounded-3xl border border-slate-700/50 p-8">
          <div className="flex items-center justify-between mb-8">
            <h2 className="text-3xl font-bold text-white flex items-center gap-3">
              <Calendar className="w-8 h-8 text-amber-400" />
              Pending Appointments ({pendingAppointments.length})
            </h2>
          </div>

          <div className="space-y-4">
            {pendingAppointments.map((appointment) => (
              <div key={appointment.id} className="bg-slate-700/30 rounded-xl p-6 border border-slate-600/50">
                <div className="flex justify-between items-start mb-4">
                  <div>
                    <h3 className="text-white font-semibold text-lg">
                      {appointment.userName || "Unknown User"}
                    </h3>
                    <p className="text-slate-300">With Dr. {appointment.doctorName || "Unknown Doctor"}</p>
                    <p className="text-slate-400 text-sm mt-1">{appointment.purpose || "No purpose specified"}</p>
                  </div>
                  <div className="flex gap-2">
  <button
    onClick={() => handleApproveAppointment(appointment.id)}
    className="bg-gradient-to-r from-emerald-600 to-teal-600 text-white px-4 py-2 rounded-xl font-semibold hover:shadow-lg hover:shadow-emerald-500/25 transition-all duration-200"
  >
    Approve
  </button>
  <button
    onClick={() => {
      const reason = prompt("Please provide a reason for rejection:");
      if (reason) {
        handleRejectAppointment(appointment.id, reason);
      }
    }}
    className="bg-gradient-to-r from-red-600 to-red-700 text-white px-4 py-2 rounded-xl font-semibold hover:shadow-lg hover:shadow-red-500/25 transition-all duration-200"
  >
    Reject
  </button>
</div>
                </div>
                
                <div className="flex items-center gap-4 text-slate-300">
                  <div className="flex items-center gap-2">
                    <Calendar className="w-4 h-4" />
                    <span>
                      {appointment.appointmentDate 
                        ? new Date(appointment.appointmentDate).toLocaleDateString() 
                        : "Date not set"}
                    </span>
                  </div>
                  <div className="flex items-center gap-2">
                    <Clock className="w-4 h-4" />
                    <span>
                      {appointment.startTime || "N/A"} - {appointment.endTime || "N/A"}
                    </span>
                  </div>
                </div>
              </div>
            ))}
            
            {pendingAppointments.length === 0 && (
              <div className="text-center py-12">
                <Calendar className="w-16 h-16 text-slate-600 mx-auto mb-4" />
                <p className="text-slate-400 text-lg">No pending appointments</p>
                <p className="text-slate-500">All appointments are processed</p>
              </div>
            )}
          </div>
        </div>
        {/* Approved Appointments Section */}
<div className="bg-slate-800/50 backdrop-blur-xl rounded-3xl border border-slate-700/50 p-8 mt-8">
  <div className="flex items-center justify-between mb-8">
    <h2 className="text-3xl font-bold text-white flex items-center gap-3">
      <CheckCircle2 className="w-8 h-8 text-emerald-400" />
      Approved Appointments ({approvedAppointments.length})
    </h2>
  </div>

  <div className="space-y-4">
    {paginate(approvedAppointments, approvedPage).map((appointment) => (
      <div key={appointment.id} className="bg-slate-700/30 rounded-xl p-6 border border-slate-600/50">
        <div className="flex justify-between items-start mb-4">
          <div>
            <h3 className="text-white font-semibold text-lg">
              {appointment.userName || "Unknown User"}
            </h3>
            <p className="text-slate-300">With Dr. {appointment.doctorName || "Unknown Doctor"}</p>
            <p className="text-slate-400 text-sm mt-1">{appointment.purpose || "No purpose specified"}</p>
          </div>
          <div className="bg-emerald-500/20 text-emerald-400 px-4 py-2 rounded-xl font-semibold">
            Approved
          </div>
        </div>
        
        <div className="flex items-center gap-4 text-slate-300">
          <div className="flex items-center gap-2">
            <Calendar className="w-4 h-4" />
            <span>
              {appointment.appointmentDate 
                ? new Date(appointment.appointmentDate).toLocaleDateString() 
                : "Date not set"}
            </span>
          </div>
          <div className="flex items-center gap-2">
            <Clock className="w-4 h-4" />
            <span>
              {appointment.startTime || "N/A"} - {appointment.endTime || "N/A"}
            </span>
          </div>
        </div>
      </div>
    ))}
    
    {approvedAppointments.length === 0 && (
      <div className="text-center py-12">
        <CheckCircle2 className="w-16 h-16 text-slate-600 mx-auto mb-4" />
        <p className="text-slate-400 text-lg">No approved appointments</p>
        <p className="text-slate-500">Approved appointments will appear here</p>
      </div>
    )}
  </div>

  {/* Pagination for approved appointments */}
  {approvedAppointments.length > itemsPerPage && (
    <div className="flex justify-center items-center mt-6 space-x-4">
      <button
        onClick={() => setApprovedPage((p) => Math.max(p - 1, 1))}
        className="px-4 py-2 bg-slate-700/50 rounded-lg text-white disabled:opacity-50"
        disabled={approvedPage === 1}
      >
        Prev
      </button>
      <span className="text-slate-300">
        Page {approvedPage} of {Math.ceil(approvedAppointments.length / itemsPerPage)}
      </span>
      <button
        onClick={() =>
          setApprovedPage((p) =>
            p < Math.ceil(approvedAppointments.length / itemsPerPage) ? p + 1 : p
          )
        }
        className="px-4 py-2 bg-slate-700/50 rounded-lg text-white disabled:opacity-50"
        disabled={approvedPage === Math.ceil(approvedAppointments.length / itemsPerPage)}
      >
        Next
      </button>
    </div>
  )}
</div>
      </main>
    </div>
  );
};

export default AssistantDashboard;