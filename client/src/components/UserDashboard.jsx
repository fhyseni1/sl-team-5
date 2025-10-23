// client/src/components/UserDashboard.jsx
"use client";

import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import {
  Pill,
  Calendar,
  Bell,
  Users,
  Activity,
  Clock,
  AlertCircle,
  LogOut,
  Loader2,
  TrendingUp,
  Shield,
  CheckCircle2,
  Sparkles,
  ArrowUpRight,
  Plus,
  X,
  Camera,
} from "lucide-react";
import AppointmentCalendar from "../components/AppointmentCalendar";
import { authService, api } from "../../services/authService";
import { userService } from "../../services/userService";

const UserDashboard = () => {
  const [activeUsersCount, setActiveUsersCount] = useState(null);
  const [user, setUser] = useState(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const [upcomingAppointmentsCount, setUpcomingAppointmentsCount] = useState(0);
  const [userAppointments, setUserAppointments] = useState([]);
  const [doctors, setDoctors] = useState([]);
  const [showScheduleForm, setShowScheduleForm] = useState(false);
  const [showMedicationForm, setShowMedicationForm] = useState(false);
  const router = useRouter();

  const [appointmentForm, setAppointmentForm] = useState({
    doctorId: "",
    appointmentDate: "",
    startTime: "",
    endTime: "",
    purpose: "",
    notes: "",
  });

  const [medicationForm, setMedicationForm] = useState({
    name: "",
    dosage: "",
    schedule: "",
    frequency: "Daily",
  });

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        setError("");
        console.log("Fetching user appointments...");

        const userData = await authService.getMe();
        if (userData.type === 5) {
          router.push("/dashboard/admin-dashboard");
          return;
        }

        const userId = userData.id || userData.userId;
        if (!userId) {
          console.error("Missing user ID:", userData);
          setError("User ID is missing");
          return;
        }

        setUser({
          firstName: userData.firstName,
          lastName: userData.lastName,
          email: userData.email,
          id: userId,
          type: userData.type,
        });

        const results = await Promise.allSettled([
          userService.getActiveUsersCount(),
          userService.getUpcomingAppointmentsCount(
            userData.id || userData.userId
          ),
          api
            .get("/users")
            .then((res) => {
              const healthcareProviders = res.data.filter(
                (user) => user.type === 4
              );

              return healthcareProviders;
            })
            .catch(() => []),
          api.get(`/appointments/user/${userData.id || userData.userId}`),
        ]);

        setActiveUsersCount(
          results[0].status === "fulfilled" ? results[0].value : 0
        );
        setUpcomingAppointmentsCount(
          results[1].status === "fulfilled" ? results[1].value : 0
        );
        setDoctors(results[2].status === "fulfilled" ? results[2].value : []);
        const appointments =
          results[3].status === "fulfilled" ? results[3].value?.data || [] : [];
        if (appointments.length > 0) {
          console.log("First appointment status:", {
            status: appointments[0].status,
            type: typeof appointments[0].status,
            isNull: appointments[0].status === null,
            isUndefined: appointments[0].status === undefined,
          });
        }

        const sortedAppointments = appointments.sort((a, b) => {
          const dateA = new Date(a.appointmentDate);
          const dateB = new Date(b.appointmentDate);
          if (dateA > dateB) return -1;
          if (dateA < dateB) return 1;
          return 0;
        });

        setUserAppointments(sortedAppointments);
      } catch (err) {
        console.error("Dashboard fetch error:", err);
        setError("Failed to load dashboard data");
        if (err.response?.status === 401) {
          router.push("/login");
        }
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [router]);

  const handleLogout = async () => {
    try {
      await authService.logout();
      router.push("/");
    } catch (err) {
      router.push("/");
    }
  };

  const handleInputChange = (e, setter) => {
    const { name, value } = e.target;
    setter((prev) => ({ ...prev, [name]: value }));
  };

  const handleScheduleSubmit = async (e) => {
    e.preventDefault();
    try {
      if (!user?.id) {
        toast.error("User ID is not available");
        return;
      }

      if (!appointmentForm.doctorId) {
        toast.error("Please select a doctor");
        return;
      }

      const appointmentDate = new Date(appointmentForm.appointmentDate);
      if (isNaN(appointmentDate.getTime())) {
        toast.error("Invalid appointment date");
        return;
      }

      // Validate time slot
      const [startHour, startMinute] = appointmentForm.startTime
        .split(":")
        .map(Number);
      const [endHour, endMinute] = appointmentForm.endTime
        .split(":")
        .map(Number);
      const durationInMinutes =
        endHour * 60 + endMinute - (startHour * 60 + startMinute);

      if (durationInMinutes !== 30) {
        toast.error("Appointments must be exactly 30 minutes long");
        return;
      }

      // Check if the time is within business hours (8 AM to 8 PM)
      if (startHour < 8 || startHour >= 20 || endHour < 8 || endHour > 20) {
        toast.error("Appointments must be between 8:00 AM and 8:00 PM");
        return;
      }

      // Check if the appointment date is in the past
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      if (appointmentDate < today) {
        toast.error("Cannot schedule appointments in the past");
        return;
      }

      const formattedDate = appointmentDate.toISOString().split("T")[0];

      const selectedDoctor = doctors.find(
        (d) => d.id === appointmentForm.doctorId
      );
      const doctorName = selectedDoctor
        ? `${selectedDoctor.firstName} ${selectedDoctor.lastName}`
        : "";

      const appointmentData = {
        userId: user.id,
        doctorId: appointmentForm.doctorId,
        doctorName: doctorName,
        appointmentDate: formattedDate,
        startTime: appointmentForm.startTime.includes(":00")
          ? appointmentForm.startTime
          : appointmentForm.startTime + ":00",
        endTime: appointmentForm.endTime.includes(":00")
          ? appointmentForm.endTime
          : appointmentForm.endTime + ":00",
        purpose: appointmentForm.purpose?.trim() || "",
        notes: appointmentForm.notes?.trim() || "",
        status: 8, // Explicitly setting status to Pending (8)
      };

      const response = await api.post("/appointments", appointmentData);

      if (response.data) {
        setShowScheduleForm(false);
        setAppointmentForm({
          doctorId: "",
          appointmentDate: "",
          startTime: "",
          endTime: "",
          purpose: "",
          notes: "",
        });
        const [appointments, count] = await Promise.all([
          userService.getUserAppointments(user.id),
          userService.getUpcomingAppointmentsCount(user.id),
        ]);
        setUserAppointments(appointments);
        setUpcomingAppointmentsCount(count);
        toast.success("Appointment scheduled! Waiting for admin approval.");
      }
    } catch (err) {
      console.error("Appointment scheduling error:", err.response?.data || err);
      toast.error(
        err.response?.data?.message ||
          err.message ||
          "Failed to schedule appointment"
      );
    }
  };

  const handleMedicationSubmit = async (e) => {
    e.preventDefault();
    try {
      await api.post("/medications", {
        ...medicationForm,
        userId: user.id,
      });
      setShowMedicationForm(false);
      setMedicationForm({
        name: "",
        dosage: "",
        schedule: "",
        frequency: "Daily",
      });
      alert("✅ Medication added successfully!");
    } catch (err) {
      alert("❌ Failed to add medication");
    }
  };

  const handleScanTherapy = () => {
    alert("📷 Barcode scanning feature coming soon!");
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
                <Pill className="w-7 h-7 text-white" />
              </div>
              <div>
                <h1 className="text-3xl font-bold bg-gradient-to-r from-slate-100 to-slate-300 bg-clip-text text-transparent">
                  MediTrack
                </h1>
                <p className="text-slate-400">Your Health Companion</p>
              </div>
            </div>
            <button
              onClick={handleLogout}
              className="flex items-center space-x-2 px-6 py-3 bg-gradient-to-r from-red-600 to-red-700 text-white rounded-xl font-bold shadow-xl hover:shadow-red-500/50 transition-all"
            >
              <LogOut className="w-5 h-5" /> Logout
            </button>
          </div>
        </div>
      </header>

      <main className="max-w-7xl mx-auto px-6 py-12 relative z-10">
        <div className="mb-12">
          <h2 className="text-4xl font-bold bg-gradient-to-r from-slate-100 to-slate-300 bg-clip-text text-transparent mb-3">
            Welcome, {user.firstName}!
          </h2>
          <p className="text-slate-400 text-lg">Here's your health overview</p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-12">
          <div className="bg-gradient-to-br from-emerald-500/10 to-teal-500/10 backdrop-blur-xl rounded-2xl p-8 border border-emerald-500/30 shadow-xl">
            <Pill className="w-12 h-12 text-emerald-400 mx-auto mb-4" />
            <p className="text-3xl font-bold text-white mb-1">12</p>
            <p className="text-emerald-400 font-semibold">Active Medications</p>
          </div>
          <div className="bg-gradient-to-br from-blue-500/10 to-cyan-500/10 backdrop-blur-xl rounded-2xl p-8 border border-blue-500/30 shadow-xl">
            <Calendar className="w-12 h-12 text-blue-400 mx-auto mb-4" />
            <p className="text-3xl font-bold text-white mb-1">
              {upcomingAppointmentsCount}
            </p>
            <p className="text-blue-400 font-semibold">Pending Appointments</p>
          </div>
          <div className="bg-gradient-to-br from-amber-500/10 to-orange-500/10 backdrop-blur-xl rounded-2xl p-8 border border-amber-500/30 shadow-xl">
            <Bell className="w-12 h-12 text-amber-400 mx-auto mb-4" />
            <p className="text-3xl font-bold text-white mb-1">5</p>
            <p className="text-amber-400 font-semibold">Pending Reminders</p>
          </div>
          <div className="bg-gradient-to-br from-violet-500/10 to-purple-500/10 backdrop-blur-xl rounded-2xl p-8 border border-violet-500/30 shadow-xl">
            <Users className="w-12 h-12 text-violet-400 mx-auto mb-4" />
            <p className="text-3xl font-bold text-white mb-1">
              {activeUsersCount || "..."}
            </p>
            <p className="text-violet-400 font-semibold">Active Users</p>
          </div>
        </div>

        <AppointmentCalendar
          appointments={userAppointments}
          doctors={doctors}
        />

        <div className="mb-12">
          <div className="bg-slate-800/50 backdrop-blur-xl rounded-2xl p-8 border border-slate-700/50">
            <h3 className="text-2xl font-bold text-white mb-6 flex items-center gap-3">
              <Calendar className="w-7 h-7 text-blue-400" />
              Your Appointments
            </h3>
            <div className="divide-y divide-slate-700">
              {userAppointments.length > 0 ? (
                userAppointments.map((appointment) => (
                  <div
                    key={appointment.id}
                    className="py-4 first:pt-0 last:pb-0"
                  >
                    <div className="flex items-center justify-between">
                      <div>
                        <h4 className="text-white font-semibold mb-1">
                          {doctors.find((d) => d.id === appointment.doctorId)
                            ? `Dr. ${
                                doctors.find(
                                  (d) => d.id === appointment.doctorId
                                ).firstName
                              } ${
                                doctors.find(
                                  (d) => d.id === appointment.doctorId
                                ).lastName
                              }`
                            : "Doctor"}
                        </h4>
                        <p className="text-slate-400 text-sm">
                          {appointment.purpose}
                        </p>
                        <div className="flex items-center gap-2 mt-2">
                          <Clock className="w-4 h-4 text-blue-400" />
                          <p className="text-slate-300 text-sm">
                            {new Date(
                              appointment.appointmentDate
                            ).toLocaleDateString("en-US", {
                              weekday: "long",
                              year: "numeric",
                              month: "long",
                              day: "numeric",
                            })}
                            {" at "}
                            {appointment.startTime
                              ? appointment.startTime.substring(0, 5)
                              : ""}{" "}
                            -
                            {appointment.endTime
                              ? appointment.endTime.substring(0, 5)
                              : ""}
                          </p>
                        </div>
                      </div>
                      <div>
                        <div className="flex flex-col items-end gap-1">
                          <span
                            className={`px-3 py-1 rounded-full text-sm font-medium ${
                              appointment.status === 2 // Approved
                                ? "bg-emerald-500/20 text-emerald-400"
                                : appointment.status === 8 // Pending
                                ? "bg-amber-500/20 text-amber-400"
                                : appointment.status === 9 // Rejected
                                ? "bg-red-500/20 text-red-400"
                                : "bg-slate-500/20 text-slate-400"
                            }`}
                          >
                            {appointment.status === 2
                              ? "Approved"
                              : appointment.status === 8
                              ? "Pending"
                              : appointment.status === 9
                              ? "Rejected"
                              : "Unknown"}
                          </span>
                          {String(appointment.status || "").toLowerCase() ===
                            "rejected" && (
                            <span className="text-xs text-red-400">
                              {appointment.rejectionReason
                                ? `Reason: ${appointment.rejectionReason}`
                                : "Rejected by admin"}
                            </span>
                          )}
                        </div>
                      </div>
                    </div>
                  </div>
                ))
              ) : (
                <p className="text-slate-400 text-center py-4">
                  No appointments found. Schedule one now!
                </p>
              )}
            </div>
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-12">
          <div className="bg-slate-800/50 backdrop-blur-xl rounded-2xl p-8 border border-slate-700/50">
            <h3 className="text-2xl font-bold text-white mb-6 flex items-center gap-3">
              <Activity className="w-7 h-7 text-blue-400" />
              Quick Actions
            </h3>
            <div className="space-y-4">
              <button
                onClick={() => setShowMedicationForm(true)}
                className="w-full flex items-center space-x-3 p-5 bg-gradient-to-r from-blue-600 to-indigo-600 text-white rounded-xl font-semibold hover:shadow-blue-500/50 transition-all hover:scale-105"
              >
                <Pill className="w-6 h-6" />
                <span>Add New Medication</span>
              </button>
              <button
                onClick={handleScanTherapy}
                className="w-full flex items-center space-x-3 p-5 bg-gradient-to-r from-purple-600 to-pink-600 text-white rounded-xl font-semibold hover:shadow-purple-500/50 transition-all hover:scale-105"
              >
                <Camera className="w-6 h-6" />
                <span>Scan Therapy</span>
              </button>
              <button
                onClick={() => setShowScheduleForm(true)}
                className="w-full flex items-center space-x-3 p-5 bg-gradient-to-r from-emerald-600 to-teal-600 text-white rounded-xl font-semibold hover:shadow-emerald-500/50 transition-all hover:scale-105"
              >
                <Calendar className="w-6 h-6" />
                <span>Schedule Appointment</span>
              </button>
            </div>
          </div>
          <div className="bg-gradient-to-br from-emerald-600 to-teal-700 text-white rounded-2xl p-8 shadow-2xl">
            <h3 className="text-2xl font-bold mb-4">Health Score</h3>
            <p className="text-lg mb-6">98% medication adherence</p>
            <div className="w-full bg-white/20 rounded-full h-3">
              <div className="bg-white h-3 rounded-full w-[98%]"></div>
            </div>
          </div>
        </div>
      </main>

      {showMedicationForm && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div className="bg-slate-800/95 backdrop-blur-xl rounded-3xl max-w-md w-full p-8 border border-slate-700/50">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-2xl font-bold text-white">Add Medication</h2>
              <button
                onClick={() => setShowMedicationForm(false)}
                className="p-2 hover:bg-slate-700 rounded-xl"
              >
                <X className="w-6 h-6" />
              </button>
            </div>
            <form onSubmit={handleMedicationSubmit} className="space-y-4">
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
              <input
                name="schedule"
                placeholder="Time (e.g., 8:00 AM) *"
                type="time"
                value={medicationForm.schedule}
                onChange={(e) => handleInputChange(e, setMedicationForm)}
                className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
                required
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
              </select>
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

      {showScheduleForm && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div className="bg-slate-800/95 backdrop-blur-xl rounded-3xl max-w-2xl w-full max-h-[90vh] overflow-y-auto p-8 border border-slate-700/50">
            <div className="flex items-center justify-between mb-8">
              <h2 className="text-2xl font-bold text-white">
                Schedule Appointment
              </h2>
              <button
                onClick={() => setShowScheduleForm(false)}
                className="p-2 hover:bg-slate-700 rounded-xl"
              >
                <X className="w-6 h-6" />
              </button>
            </div>
            <form onSubmit={handleScheduleSubmit} className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <select
                  name="doctorId"
                  value={appointmentForm.doctorId}
                  onChange={(e) => handleInputChange(e, setAppointmentForm)}
                  className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
                  required
                >
                  <option value="">Select Doctor</option>
                  {doctors.length > 0 ? (
                    doctors.map((doctor) => (
                      <option
                        key={doctor.id}
                        value={doctor.id || doctor.userId}
                      >
                        {`Dr. ${doctor.firstName} ${doctor.lastName}`}
                      </option>
                    ))
                  ) : (
                    <option value="" disabled>
                      No doctors available
                    </option>
                  )}
                </select>
                <input
                  name="appointmentDate"
                  type="date"
                  value={appointmentForm.appointmentDate}
                  onChange={(e) => handleInputChange(e, setAppointmentForm)}
                  className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
                  required
                />
              </div>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <label className="block text-slate-300 mb-2">
                    Select Time Slot
                  </label>
                  <select
                    name="startTime"
                    value={appointmentForm.startTime}
                    onChange={(e) => {
                      const startTime = e.target.value;
                      const [hours, minutes] = startTime.split(":");
                      const endTime = new Date(
                        2025,
                        0,
                        1,
                        parseInt(hours),
                        parseInt(minutes) + 30
                      )
                        .toTimeString()
                        .slice(0, 5);

                      handleInputChange(e, setAppointmentForm);
                      setAppointmentForm((prev) => ({
                        ...prev,
                        endTime: endTime,
                      }));
                    }}
                    className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
                    required
                  >
                    <option value="">Select time</option>
                    {Array.from({ length: 32 }, (_, i) => {
                      const hour = Math.floor(i / 2) + 8; // Start from 8 AM
                      const minutes = (i % 2) * 30;
                      const time = `${hour
                        .toString()
                        .padStart(2, "0")}:${minutes
                        .toString()
                        .padStart(2, "0")}`;
                      return (
                        <option key={time} value={time}>
                          {time}
                        </option>
                      );
                    })}
                  </select>
                </div>
                <div>
                  <label className="block text-slate-300 mb-2">
                    End Time (30min slot)
                  </label>
                  <input
                    name="endTime"
                    type="time"
                    value={appointmentForm.endTime}
                    className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white cursor-not-allowed"
                    disabled
                  />
                </div>
              </div>
              <textarea
                name="purpose"
                placeholder="Purpose of visit *"
                value={appointmentForm.purpose}
                onChange={(e) => handleInputChange(e, setAppointmentForm)}
                rows="3"
                className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white resize-vertical"
                required
              />
              <textarea
                name="notes"
                placeholder="Additional notes"
                value={appointmentForm.notes}
                onChange={(e) => handleInputChange(e, setAppointmentForm)}
                rows="3"
                className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white resize-vertical"
              />
              <button
                type="submit"
                className="w-full bg-gradient-to-r from-emerald-600 to-teal-600 text-white py-4 rounded-xl font-bold hover:shadow-emerald-500/50 transition-all"
              >
                Submit for Approval
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default UserDashboard;
