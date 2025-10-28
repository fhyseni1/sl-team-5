// client/components/DoctorDashboard.jsx
"use client";
import ChatInbox from '../components/ChatInbox';
import NotificationCenter from '../components/NotificationCenter';
import { MessageCircle } from 'lucide-react';

import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import {
  Pill,
  Calendar,
  Users,
  Clock,
  CheckCircle2,
  XCircle,
  LogOut,
  Loader2,
  Stethoscope,
  Plus,
  X,
  Activity,
  BarChart3,
} from "lucide-react";
import { authService, api } from "../../services/authService";

const DoctorDashboard = () => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [patients, setPatients] = useState([]);
  const [showChatInbox, setShowChatInbox] = useState(false);
const [showPatientsModal, setShowPatientsModal] = useState(false);
  const [appointments, setAppointments] = useState([]);
  const [medications, setMedications] = useState([]);
  const [showAddMedicationForm, setShowAddMedicationForm] = useState(false);
  const [stats, setStats] = useState({
    totalAppointments: 0,
    todayAppointments: 0,
    pendingAppointments: 0,
    totalPatients: 0,
  });

  const router = useRouter();

  const [medicationForm, setMedicationForm] = useState({
    name: "",
    dosage: "",
    instructions: "",
    frequency: "Daily",
    duration: "",
  });

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const userData = await authService.getMe();
        
        // Check if user is a doctor (type 4 = HealthcareProvider)
        const isDoctor = userData.type === 4;
        if (!isDoctor) {
          toast.error("Access denied. Doctor privileges required.");
          router.push("/dashboard/dashboard");
          return;
        }

        setUser(userData);

        try {
          // Fetch all appointments and filter by doctor ID
          const [allAppointmentsRes, allMedicationsRes] = await Promise.all([
            api.get("/appointments"),
            api.get("/medications")
          ]);

          // Filter appointments for this doctor
          const doctorAppointments = (allAppointmentsRes.data || [])
            .filter(app => app.doctorId === userData.id);
          
          // Filter medications prescribed by this doctor
          const doctorMedications = (allMedicationsRes.data || [])
            .filter(med => med.doctorId === userData.id || med.prescribedBy === `${userData.firstName} ${userData.lastName}`);

          setAppointments(doctorAppointments);
          setMedications(doctorMedications);

          // Calculate stats
          const today = new Date().toISOString().split('T')[0];
          const todayApps = doctorAppointments.filter(
            app => app.appointmentDate === today
          );
          const pendingApps = doctorAppointments.filter(
            app => app.status === 8 // Pending status
          );

          const uniquePatients = [...new Set(doctorAppointments.map(app => app.userId))];

          setStats({
            totalAppointments: doctorAppointments.length,
            todayAppointments: todayApps.length,
            pendingAppointments: pendingApps.length,
            totalPatients: uniquePatients.length,
          });

        } catch (apiError) {
          console.warn("API endpoints not available, using mock data:", apiError);
          
          // Use mock data as fallback
          const mockAppointments = [
            {
              id: 1,
              userId: '123',
              userFirstName: 'John',
              userLastName: 'Doe',
              doctorId: userData.id,
              appointmentDate: new Date().toISOString().split('T')[0],
              startTime: '10:00',
              endTime: '10:30',
              purpose: 'Regular checkup',
              status: 8, // Pending
              notes: 'Patient needs routine examination'
            },
            {
              id: 2,
              userId: '124',
              userFirstName: 'Jane',
              userLastName: 'Smith',
              doctorId: userData.id,
              appointmentDate: new Date().toISOString().split('T')[0],
              startTime: '11:00',
              endTime: '11:30',
              purpose: 'Follow-up visit',
              status: 2, // Approved
              notes: 'Post-surgery follow-up'
            }
          ];

          const mockMedications = [
            {
              id: 1,
              name: 'Amoxicillin',
              dosage: '500mg',
              instructions: 'Take twice daily after meals',
              frequency: 'Daily',
              duration: '7 days',
              prescribedBy: `${userData.firstName} ${userData.lastName}`,
              createdAt: new Date().toISOString()
            }
          ];

          setAppointments(mockAppointments);
          setMedications(mockMedications);

          // Calculate stats from mock data
          const today = new Date().toISOString().split('T')[0];
          const todayApps = mockAppointments.filter(app => app.appointmentDate === today);
          const pendingApps = mockAppointments.filter(app => app.status === 8);
          const uniquePatients = [...new Set(mockAppointments.map(app => app.userId))];

          setStats({
            totalAppointments: mockAppointments.length,
            todayAppointments: todayApps.length,
            pendingAppointments: pendingApps.length,
            totalPatients: uniquePatients.length,
          });
        }

      } catch (err) {
        console.error("Doctor dashboard error:", err);
        setError(err.response?.data?.message || "Failed to load dashboard");
        if (err.response?.status === 401) router.push("/login");
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [router]);

  const handleLogout = async () => {
    await authService.logout();
    router.push("/");
  };

  const handleInputChange = (e, setter) => {
    const { name, value } = e.target;
    setter((prev) => ({ ...prev, [name]: value }));
  };
 const fetchDoctorPatients = async () => {
  try {
    const patientsRes = await api.get(`/users/doctor/${user.id}/patients`);
    setPatients(patientsRes.data || []);
  } catch (err) {
    console.error("Error fetching patients:", err);
    // Fallback: përdor pacientët nga takimet
    const uniquePatientIds = [...new Set(appointments.map(app => app.userId))];
    const mockPatients = uniquePatientIds.map((id, index) => ({
      id: id || `patient-${index}`,
      firstName: `Patient ${index + 1}`,
      lastName: '',
      email: `patient${index + 1}@email.com`,
      lastAppointment: new Date().toISOString().split('T')[0]
    }));
    setPatients(mockPatients);
  }
};
  const handleAddMedicationSubmit = async (e) => {
    e.preventDefault();
    try {
      // Use the general medications endpoint
      await api.post("/medications", {
        ...medicationForm,
        userId: user.id, // Use the doctor's ID as userId for now
        doctorId: user.id,
        prescribedBy: `${user.firstName} ${user.lastName}`,
      });
      
      setShowAddMedicationForm(false);
      setMedicationForm({
        name: "",
        dosage: "",
        instructions: "",
        frequency: "Daily",
        duration: "",
      });
      
      // Refresh the page to show new medication
      window.location.reload();
      
      toast.success("Medication added successfully!");
    } catch (err) {
      console.error("Error adding medication:", err);
      toast.error(
        err.response?.data?.message || "Failed to add medication"
      );
    }
  };

  const handleAppointmentAction = async (appointmentId, action) => {
  try {
    if (action === 'approve') {
      await api.put(`/appointments/${appointmentId}/doctor-approve`);
      toast.success("Appointment approved!");
    } else if (action === 'reject') {
      await api.put(`/appointments/${appointmentId}/doctor-reject`, {
        rejectionReason: "Schedule conflict - please reschedule"
      });
      toast.success("Appointment rejected!");
    }

    // Refresh appointments list
    const allAppointmentsRes = await api.get("/appointments");
    const doctorAppointments = (allAppointmentsRes.data || [])
      .filter(app => app.doctorId === user.id);
    
    setAppointments(doctorAppointments);

    // Update stats
    const today = new Date().toISOString().split('T')[0];
    const todayApps = doctorAppointments.filter(app => app.appointmentDate === today);
    const pendingApps = doctorAppointments.filter(app => app.status === 8);
    const uniquePatients = [...new Set(doctorAppointments.map(app => app.userId))];

    setStats({
      totalAppointments: doctorAppointments.length,
      todayAppointments: todayApps.length,
      pendingAppointments: pendingApps.length,
      totalPatients: uniquePatients.length,
    });

  } catch (err) {
    console.error(`Error ${action}ing appointment:`, err);
    
    if (err.response?.status === 403) {
      toast.error("You can only manage your own appointments");
    } else if (err.response?.status === 404) {
      toast.error("Appointment not found");
    } else {
      toast.error(
        err.response?.data?.message || `Failed to ${action} appointment`
      );
    }
  }
};

  if (loading)
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Loader2 className="w-16 h-16 animate-spin text-blue-500" />
      </div>
    );
    
  if (error)
    return (
      <div className="min-h-screen flex items-center justify-center text-red-500">
        {error}
      </div>
    );
    
  if (!user)
    return (
      <div className="min-h-screen flex items-center justify-center">
        Please log in
      </div>
    );

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900 relative">
      <ToastContainer
        position="top-right"
        autoClose={5000}
        hideProgressBar={false}
        newestOnTop
        closeOnClick
        rtl={false}
        pauseOnFocusLoss
        draggable
        pauseOnHover
        theme="dark"
      />
      <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_right,_var(--tw-gradient-stops))] from-blue-900/20 via-transparent to-emerald-900/20"></div>

      <header className="bg-slate-900/80 backdrop-blur-xl border-b border-slate-700/50 sticky top-0 z-50">
        <div className="max-w-7xl mx-auto px-6 py-5">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4">
              <div className="w-14 h-14 bg-gradient-to-br from-blue-600 to-emerald-600 rounded-2xl flex items-center justify-center shadow-xl">
                <Stethoscope className="w-7 h-7 text-white" />
              </div>
              <div>
                <h1 className="text-3xl font-bold bg-gradient-to-r from-slate-100 to-slate-300 bg-clip-text text-transparent">
                  Doctor Dashboard
                </h1>
                <p className="text-slate-400">Dr. {user.firstName} {user.lastName}</p>
              </div>
            </div>
            <div className="flex items-center space-x-4">
              <NotificationCenter currentUser={user} />
              
              <button
                onClick={handleLogout}
                className="flex items-center space-x-2 px-6 py-3 bg-gradient-to-r from-red-600 to-red-700 text-white rounded-xl font-bold shadow-xl hover:shadow-red-500/50 transition-all"
              >
                <LogOut className="w-5 h-5" /> Logout
              </button>
            </div>
          </div>
        </div>
      </header>

      <main className="max-w-7xl mx-auto px-6 py-12 relative z-10">
        <div className="mb-12">
          <h2 className="text-4xl font-bold bg-gradient-to-r from-slate-100 to-slate-300 bg-clip-text text-transparent mb-3">
            Welcome, Dr. {user.firstName}!
          </h2>
          <p className="text-slate-400 text-lg">Manage your appointments and prescriptions</p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-12">
          {[
            {
              label: "Total Appointments",
              value: stats.totalAppointments,
              icon: Calendar,
              color: "from-blue-500 to-cyan-500",
              bgColor: "blue"
            },
            {
              label: "Today's Appointments",
              value: stats.todayAppointments,
              icon: Clock,
              color: "from-emerald-500 to-teal-500",
              bgColor: "emerald"
            },
            {
              label: "Pending Approvals",
              value: stats.pendingAppointments,
              icon: Activity,
              color: "from-amber-500 to-orange-500",
              bgColor: "amber"
            },
            {
              label: "Total Patients",
              value: stats.totalPatients,
              icon: Users,
              color: "from-purple-500 to-pink-500",
              bgColor: "purple"
            },
          ].map((stat, i) => (
            <div
              key={i}
              className={`bg-gradient-to-br ${stat.color}/10 backdrop-blur-xl rounded-2xl p-8 border border-${stat.bgColor}-500/30 shadow-xl`}
            >
              <stat.icon className="w-12 h-12 text-white mx-auto mb-4" />
              <p className="text-3xl font-bold text-white mb-1">{stat.value}</p>
              <p className="text-slate-300 font-semibold">{stat.label}</p>
            </div>
          ))}
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-12">
          {/* Appointments Section */}
          <div className="bg-slate-800/50 backdrop-blur-xl rounded-3xl border border-slate-700/50 p-8">
            <div className="flex items-center justify-between mb-8">
              <h2 className="text-2xl font-bold text-white flex items-center gap-3">
                <Calendar className="w-7 h-7 text-blue-400" />
                Upcoming Appointments ({appointments.length})
              </h2>
            </div>
            <div className="space-y-4 max-h-96 overflow-y-auto">
              {appointments.length > 0 ? (
                appointments.map((appointment) => (
                  <div
                    key={appointment.id}
                    className="bg-slate-700/30 rounded-xl p-4 border border-slate-600/50"
                  >
                    <div className="flex justify-between items-start mb-3">
                      <div>
                        <h3 className="text-white font-semibold">
                          {appointment.userFirstName || 'Patient'} {appointment.userLastName || ''}
                        </h3>
                        <p className="text-slate-400 text-sm">{appointment.purpose || 'No purpose specified'}</p>
                      </div>
                      <span
                        className={`px-2 py-1 rounded-full text-xs font-medium ${
                          appointment.status === 2
                            ? "bg-emerald-500/20 text-emerald-400"
                            : appointment.status === 8
                            ? "bg-amber-500/20 text-amber-400"
                            : "bg-slate-500/20 text-slate-400"
                        }`}
                      >
                        {appointment.status === 2 ? "Approved" : 
                         appointment.status === 8 ? "Pending" : "Unknown"}
                      </span>
                    </div>
                    <div className="flex justify-between items-center">
                      <div className="text-slate-300 text-sm">
                        <p>
                          {appointment.appointmentDate ? new Date(appointment.appointmentDate).toLocaleDateString() : 'No date'} at{" "}
                          {appointment.startTime ? appointment.startTime.substring(0, 5) : 'No time'}
                        </p>
                      </div>
                      {appointment.status === 8 && (
                        <div className="flex gap-2">
                          <button
                            onClick={() => handleAppointmentAction(appointment.id, 'approve')}
                            className="p-2 bg-emerald-500/20 hover:bg-emerald-500/40 text-emerald-400 rounded-lg transition-all"
                            title="Approve Appointment"
                          >
                            <CheckCircle2 className="w-4 h-4" />
                          </button>
                          <button
                            onClick={() => handleAppointmentAction(appointment.id, 'reject')}
                            className="p-2 bg-red-500/20 hover:bg-red-500/40 text-red-400 rounded-lg transition-all"
                            title="Reject Appointment"
                          >
                            <XCircle className="w-4 h-4" />
                          </button>
                        </div>
                      )}
                    </div>
                  </div>
                ))
              ) : (
                <p className="text-slate-400 text-center py-8">
                  No appointments scheduled
                </p>
              )}
            </div>
          </div>

          {/* Medications Section */}
          <div className="bg-slate-800/50 backdrop-blur-xl rounded-3xl border border-slate-700/50 p-8">
            <div className="flex items-center justify-between mb-8">
              <h2 className="text-2xl font-bold text-white flex items-center gap-3">
                <Pill className="w-7 h-7 text-emerald-400" />
                Prescribed Medications ({medications.length})
              </h2>
              <button
                onClick={() => setShowAddMedicationForm(true)}
                className="bg-gradient-to-r from-emerald-600 to-teal-600 text-white px-4 py-2 rounded-xl font-bold hover:shadow-emerald-500/50 transition-all"
              >
                <Plus className="w-5 h-5" />
              </button>
            </div>
            <div className="space-y-4 max-h-96 overflow-y-auto">
              {medications.length > 0 ? (
                medications.map((medication) => (
                  <div
                    key={medication.id}
                    className="bg-slate-700/30 rounded-xl p-4 border border-slate-600/50"
                  >
                    <div className="flex justify-between items-start mb-2">
                      <h3 className="text-white font-semibold">{medication.name || 'Unnamed Medication'}</h3>
                      <span className="text-slate-400 text-sm">{medication.frequency || 'No frequency'}</span>
                    </div>
                    <p className="text-slate-300 text-sm mb-2">
                      Dosage: {medication.dosage || 'No dosage'}
                    </p>
                    {medication.instructions && (
                      <p className="text-slate-400 text-sm">
                        Instructions: {medication.instructions}
                      </p>
                    )}
                    <p className="text-slate-500 text-xs mt-2">
                      Prescribed on: {medication.createdAt ? new Date(medication.createdAt).toLocaleDateString() : 'Unknown date'}
                    </p>
                  </div>
                ))
              ) : (
                <p className="text-slate-400 text-center py-8">
                  No medications prescribed yet
                </p>
              )}
            </div>
          </div>
        </div>

        {/* Quick Actions */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <button
            onClick={() => setShowAddMedicationForm(true)}
            className="group bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-700 hover:to-teal-700 text-white p-8 rounded-3xl font-bold shadow-xl hover:shadow-emerald-500/50 transition-all hover:scale-105 flex flex-col items-center gap-4"
          >
            <Pill className="w-12 h-12 group-hover:scale-110 transition-transform" />
            <span className="text-xl">Add Medication</span>
          </button>
          <div className="bg-gradient-to-r from-blue-600 to-indigo-600 text-white p-8 rounded-3xl shadow-xl hover:shadow-blue-500/50 transition-all hover:scale-105">
            <BarChart3 className="w-12 h-12 mx-auto mb-4" />
            <h3 className="text-xl font-bold mb-2">Patient Analytics</h3>
            <p className="text-blue-100">View health reports</p>
          </div>
      <div className="bg-gradient-to-r from-indigo-600 to-purple-600 text-white p-8 rounded-3xl shadow-xl hover:shadow-purple-500/50 transition-all hover:scale-105 cursor-pointer group"
    onClick={() => setShowChatInbox(true)}>
  <MessageCircle className="w-12 h-12 mx-auto mb-4 group-hover:scale-110 transition-transform" />
  <h3 className="text-xl font-bold mb-2">Patient Messages</h3>
  <p className="text-indigo-100">Chat with your patients</p>
</div>
   <div className="bg-gradient-to-r from-purple-600 to-pink-600 text-white p-8 rounded-3xl shadow-xl hover:shadow-purple-500/50 transition-all hover:scale-105 cursor-pointer"
     onClick={() => {
       fetchDoctorPatients();
       setShowPatientsModal(true);
     }}>
  <Users className="w-12 h-12 mx-auto mb-4" />
  <h3 className="text-xl font-bold mb-2">My Patients</h3>
  <p className="text-purple-100">{stats.totalPatients} patients</p>
</div>
        </div>
      </main>
      {/* Patients Modal */}
{showPatientsModal && (
  <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
    <div className="bg-slate-800/95 backdrop-blur-xl rounded-3xl max-w-4xl w-full max-h-[80vh] overflow-y-auto p-8 border border-slate-700/50">
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-2xl font-bold text-white">My Patients ({patients.length})</h2>
        <button
          onClick={() => setShowPatientsModal(false)}
          className="p-2 hover:bg-slate-700 rounded-xl"
        >
          <X className="w-6 h-6" />
        </button>
      </div>
      
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {patients.map((patient) => (
          <div key={patient.id} className="bg-slate-700/50 rounded-xl p-4 border border-slate-600/50">
            <h3 className="text-white font-semibold text-lg">
              {patient.firstName} {patient.lastName}
            </h3>
            <p className="text-slate-300 text-sm mt-1">{patient.email}</p>
            {patient.phoneNumber && (
              <p className="text-slate-400 text-sm">📞 {patient.phoneNumber}</p>
            )}
            {patient.lastAppointment && (
              <p className="text-slate-500 text-xs mt-2">
                Last visit: {new Date(patient.lastAppointment).toLocaleDateString()}
              </p>
            )}
          </div>
        ))}
      </div>
      
      {patients.length === 0 && (
        <p className="text-slate-400 text-center py-8">No patients found</p>
      )}
    </div>
  </div>
)}
{showChatInbox && (
  <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
    <div className="bg-slate-800/95 backdrop-blur-xl rounded-3xl w-full max-w-6xl h-[90vh] overflow-hidden border border-slate-700/50 flex flex-col">
      {/* Header i përmirësuar */}
      <div className="flex items-center justify-between p-6 border-b border-slate-700/50 bg-slate-900/80">
        <div className="flex items-center gap-3">
          <MessageCircle className="w-8 h-8 text-blue-400" />
          <div>
            <h2 className="text-2xl font-bold text-white">Patient Messages</h2>
            <p className="text-slate-400 text-sm">Chat with your patients</p>
          </div>
        </div>
        <button
          onClick={() => setShowChatInbox(false)}
          className="p-3 hover:bg-slate-700 rounded-xl transition-all hover:scale-105"
        >
          <X className="w-6 h-6 text-slate-300" />
        </button>
      </div>
      
      {/* Chat container i përmirësuar */}
      <div className="flex-1 p-0 overflow-hidden">
        <ChatInbox currentUser={user} isDoctorView={true} />
      </div>
    </div>
  </div>
)}
      {/* Add Medication Modal */}
      {showAddMedicationForm && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div className="bg-slate-800/95 backdrop-blur-xl rounded-3xl max-w-md w-full p-8 border border-slate-700/50">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-2xl font-bold text-white">Add Medication</h2>
              <button
                onClick={() => setShowAddMedicationForm(false)}
                className="p-2 hover:bg-slate-700 rounded-xl"
              >
                <X className="w-6 h-6" />
              </button>
            </div>
            <form onSubmit={handleAddMedicationSubmit} className="space-y-4">
              <input
                name="name"
                placeholder="Medication Name *"
                value={medicationForm.name}
                onChange={(e) => handleInputChange(e, setMedicationForm)}
                className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
                required
              />
              <input
                name="dosage"
                placeholder="Dosage (e.g., 100mg) *"
                value={medicationForm.dosage}
                onChange={(e) => handleInputChange(e, setMedicationForm)}
                className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
                required
              />
              <textarea
                name="instructions"
                placeholder="Instructions for use"
                value={medicationForm.instructions}
                onChange={(e) => handleInputChange(e, setMedicationForm)}
                rows="3"
                className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white resize-vertical"
              />
              <select
                name="frequency"
                value={medicationForm.frequency}
                onChange={(e) => handleInputChange(e, setMedicationForm)}
                className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
              >
                <option value="Daily">Daily</option>
                <option value="Weekly">Weekly</option>
                <option value="Monthly">Monthly</option>
                <option value="As needed">As needed</option>
              </select>
              <input
                name="duration"
                placeholder="Duration (e.g., 30 days)"
                value={medicationForm.duration}
                onChange={(e) => handleInputChange(e, setMedicationForm)}
                className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
              />
              <button
                type="submit"
                className="w-full bg-gradient-to-r from-emerald-600 to-teal-600 text-white py-4 rounded-xl font-bold hover:shadow-emerald-500/50 transition-all"
              >
                Add Medication
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default DoctorDashboard;