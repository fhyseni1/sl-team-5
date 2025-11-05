// client/src/components/AdminDashboard.jsx
"use client";
import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import {
  Users,
  Calendar,
  Pill,
  Activity,
  LogOut,
  Loader2,
  Shield,
  Search,
  Trash,
  Eye,
  BarChart3,
  AlertTriangle,
  Check,
  XCircle,
  UserCheck,
  Plus,
  UserPlus,
  X,
  Stethoscope,
} from "lucide-react";
import { userService } from "../../services/userService";
import { authService, api } from "../../services/authService";
import AppointmentCalendar from "./AppointmentCalendar";
import NotificationCenter from "./NotificationCenter";

const AdminDashboard = () => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [stats, setStats] = useState({});
  const [users, setUsers] = useState([]);
  const [pendingAppointments, setPendingAppointments] = useState([]);
  const [approvedAppointments, setApprovedAppointments] = useState([]);
  const [doctors, setDoctors] = useState([]);
  const [searchTerm, setSearchTerm] = useState("");
  const [showAddClinicForm, setShowAddClinicForm] = useState(false);
  const [showAddUserForm, setShowAddUserForm] = useState(false);
  const [doctorPage, setDoctorPage] = useState(1);
  const [pendingPage, setPendingPage] = useState(1);
  const [approvedPage, setApprovedPage] = useState(1);
  const [userAppointments, setUserAppointments] = useState([]);
  const [userPage, setUserPage] = useState(1);

  const [generatedPassword, setGeneratedPassword] = useState("");
  const [clinics, setClinics] = useState([]);
  const [registrationType, setRegistrationType] = useState(null);
  const [rejectedAppointments, setRejectedAppointments] = useState([]);
  const [rejectedPage, setRejectedPage] = useState(1);

  const [showPasswordModal, setShowPasswordModal] = useState(false);
  const [newClinicData, setNewClinicData] = useState({
    email: "",
    clinicName: "",
    password: "",
  });

  const [clinicForm, setClinicForm] = useState({
    name: "",
    email: "",
    password: "",
    clinicName: "",
    address: "",
    phoneNumber: "",
  });

  const [userForm, setUserForm] = useState({
    email: "",
    password: "",
    firstName: "",
    lastName: "",
    phoneNumber: "",
    type: "Patient",
  });
  const itemsPerPage = 5;
  const router = useRouter();

  // Combine pending and approved appointments for the calendar
  const allAppointments = [...pendingAppointments, ...approvedAppointments];

  const paginate = (data, page) => {
    const start = (page - 1) * itemsPerPage;
    return data.slice(start, start + itemsPerPage);
  };

  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState({
    id: null,
    type: null,
    name: "",
  });

  const handleDeleteDoctor = async (doctor) => {
    setDeleteTarget({
      id: doctor.id,
      type: "doctor",
      name: `Dr. ${doctor.firstName} ${doctor.lastName}`,
    });
    setShowDeleteModal(true);
  };

  const handleDeleteUser = async (user) => {
    setDeleteTarget({
      id: user.id,
      type: "user",
      name: `${user.firstName} ${user.lastName}`,
    });
    setShowDeleteModal(true);
  };

  const handleConfirmDelete = async () => {
    try {
      await api.delete(`/users/${deleteTarget.id}`);
      if (deleteTarget.type === "doctor") {
        setDoctors((prev) => prev.filter((d) => d.id !== deleteTarget.id));
        toast.success("Doctor deleted successfully!");
      } else {
        setUsers((prev) => prev.filter((u) => u.id !== deleteTarget.id));
        toast.success("User deleted successfully!");
      }
      setShowDeleteModal(false);
    } catch (err) {
      console.error("Error deleting:", err.response?.data || err);
      toast.error(
        err.response?.data?.message ||
          err.message ||
          `Failed to delete ${deleteTarget.type}`
      );
    }
  };

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const userData = await authService.getMe();
        const isAdmin = userData.type === 5;
        if (!isAdmin) {
          await authService.logout();
          router.push("/dashboard/dashboard");
          return;
        }
        setUser(userData);
        const results = await Promise.allSettled([
          api.get("/users/count"),
          api.get("/users"),
          api.get("/appointments/pending"),
          api.get("/appointments/approved"),
          api.get("/appointments/rejected"),
          api.get("/users").then((res) => {
            const allUsers = res.data || [];
            return allUsers.filter((user) => user.type === 4); // Doctors
          }),
          api.get("/clinics").then((res) => res.data || []), // Fetch clinics
        ]);

        const usersCount =
          results[0].status === "fulfilled" ? results[0].value.data : 0;
        const allUsers =
          results[1].status === "fulfilled" ? results[1].value.data : [];
        const pendingAppts =
          results[2].status === "fulfilled" ? results[2].value.data : [];
        const approvedAppts =
          results[3].status === "fulfilled" ? results[3].value.data : [];
        const rejectedAppts =
          results[4].status === "fulfilled" ? results[4].value.data : [];
        const healthcareProviders =
          results[5].status === "fulfilled" ? results[5].value : [];
        const clinicsData =
          results[6].status === "fulfilled" ? results[6].value : [];

        setUsers(allUsers);
        setDoctors(healthcareProviders);
        setPendingAppointments(pendingAppts);
        setApprovedAppointments(approvedAppts);
        setRejectedAppointments(rejectedAppts);
        setClinics(clinicsData);
        setStats({
          totalUsers: usersCount || 0,
          totalAppointments:
            (pendingAppts.length || 0) +
            (approvedAppts.length || 0) +
            (rejectedAppts.length || 0),
          pendingAppointments: pendingAppts.length || 0,
          approvedAppointments: approvedAppts.length || 0,
          rejectedAppointments: rejectedAppts.length || 0,
          totalDoctors: healthcareProviders.length || 0,
          totalAssistants: 0, // Vendos 0
          totalClinics: clinicsData.length || 0,
        });
      } catch (err) {
        console.error("Admin dashboard error:", err);
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

  const handleAddClinicSubmit = async (e) => {
    e.preventDefault();
    try {
      const nameParts = clinicForm.name.split(" ");
      const firstName = nameParts[0] || "Clinic";
      const lastName = nameParts.slice(1).join(" ") || "Admin";
      const clinicEmail =
        clinicForm.email ||
        `${clinicForm.clinicName
          .toLowerCase()
          .replace(/\s+/g, ".")}@meditrack.com`;
      const payload = {
        email: clinicEmail,
        password: clinicForm.password,
        firstName,
        lastName,
        phoneNumber: clinicForm.phoneNumber || "",
        type: 8,
        clinicName: clinicForm.clinicName,
        address: clinicForm.address,
      };
      console.log("Register clinic payload:", payload);
      console.log("Cookies:", document.cookie);
      const response = await api.post("/auth/register-clinic", payload);
      if (response.data) {
        setNewClinicData({
          email: clinicEmail,
          name: clinicForm.name,
          password: clinicForm.password,
        });
        setRegistrationType("clinic");
        setShowPasswordModal(true);
        setClinicForm({
          name: "",
          email: "",
          password: "",
          clinicName: "",
          address: "",
          phoneNumber: "",
        });
        const usersRes = await api.get("/users");
        const users = usersRes.data || [];
        const clinicAdmins = users.filter((user) => user.type === 8);
        setUsers(users);
        setClinics(clinicAdmins);
        setStats((prev) => ({
          ...prev,
          totalClinics: clinicAdmins.length,
        }));
      }
    } catch (err) {
      console.error("Error adding clinic:", {
        status: err.response?.status,
        data: err.response?.data,
        headers: err.response?.headers,
        error: err.message,
      });
      toast.error(
        err.response?.data?.message || err.message || "Failed to add clinic"
      );
    }
  };

  const handleAddUserSubmit = async (e) => {
    e.preventDefault();
    try {
      const response = await api.post("/auth/register", {
        email: userForm.email,
        password: userForm.password,
        firstName: userForm.firstName,
        lastName: userForm.lastName,
        phoneNumber: userForm.phoneNumber || "",
        type: userForm.type === "Admin" ? 5 : 1,
      });
      if (response.data) {
        setShowAddUserForm(false);
        setUserForm({
          email: "",
          password: "",
          firstName: "",
          lastName: "",
          phoneNumber: "",
          type: "Patient",
        });
        const usersRes = await api.get("/users");
        setUsers(usersRes.data || []);
        setStats((prev) => ({
          ...prev,
          totalUsers: (usersRes.data || []).length,
        }));
        toast.success("User created successfully!");
      }
    } catch (err) {
      console.error("Error creating user:", err.response?.data || err);
      toast.error(
        err.response?.data?.message ||
          err.response?.data?.error ||
          "Failed to create user"
      );
    }
  };

  const handleInputChange = (e, setter) => {
    const { name, value } = e.target;
    setter((prev) => ({ ...prev, [name]: value }));
  };

  if (loading)
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Loader2 className="w-16 h-16 animate-spin" />
      </div>
    );

  if (error)
    return (
      <div className="min-h-screen flex items-center justify-center text-red-500">
        {error}
      </div>
    );

  const filteredUsers = users.filter(
    (u) =>
      u.firstName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      u.lastName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      u.email?.toLowerCase().includes(searchTerm.toLowerCase())
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
      <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_right,_var(--tw-gradient-stops))] from-purple-900/20 via-transparent to-emerald-900/20"></div>
      <header className="bg-slate-900/80 backdrop-blur-xl border-b border-slate-700/50 sticky top-0 z-50">
        <div className="max-w-7xl mx-auto px-6 py-5">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4">
              <div className="w-14 h-14 bg-gradient-to-br from-purple-600 to-pink-600 rounded-2xl flex items-center justify-center shadow-xl">
                <Shield className="w-7 h-7 text-white" />
              </div>
              <div>
                <h1 className="text-3xl font-bold bg-gradient-to-r from-slate-100 to-slate-300 bg-clip-text text-transparent">
                  Admin Dashboard
                </h1>
                <p className="text-slate-400">MediTrack Management</p>
              </div>
            </div>
            <div className="flex items-center space-x-4">
              <NotificationCenter currentUser={user} />
              <button
                onClick={handleLogout}
                className="flex items-center space-x-2 px-6 py-3 bg-gradient-to-r from-red-600 to-red-700 text-white rounded-xl font-bold shadow-xl"
              >
                <LogOut className="w-5 h-5" /> Logout
              </button>
            </div>
          </div>
        </div>
      </header>
      <main className="max-w-7xl mx-auto px-6 py-12 relative z-10">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-12">
          {[
            {
              label: "Total Users",
              value: stats.totalUsers,
              icon: Users,
              color: "from-blue-500",
            },
            {
              label: "Pending Appointments",
              value: stats.pendingAppointments,
              icon: Calendar,
              color: "from-orange-500",
            },
            {
              label: "Approved Appointments",
              value: stats.approvedAppointments,
              icon: Check,
              color: "from-emerald-500",
            },
            {
              label: "Rejected Appointments",
              value: stats.rejectedAppointments,
              icon: XCircle,
              color: "from-red-500",
            },
            {
              label: "Total Doctors",
              value: stats.totalDoctors,
              icon: Stethoscope,
              color: "from-emerald-500",
            },

            {
              label: "Total Clinics",
              value: stats.totalClinics,
              icon: Shield,
              color: "from-teal-500",
            },
            {
              label: "Total Appointments",
              value: stats.totalAppointments,
              icon: Pill,
              color: "from-purple-500",
            },
          ].map((stat, i) => (
            <div
              key={i}
              className={`bg-gradient-to-br ${stat.color}/10 backdrop-blur-xl rounded-2xl p-8 border shadow-xl hover:scale-105 transition-all`}
            >
              <stat.icon className="w-12 h-12 text-blue-400 mx-auto mb-4" />
              <p className="text-3xl font-bold text-white mb-2">{stat.value}</p>
              <p className="text-slate-400 font-semibold">{stat.label}</p>
            </div>
          ))}
        </div>
        <div className="bg-slate-800/50 backdrop-blur-xl rounded-3xl border border-slate-700/50 p-8 mb-8">
          <div className="flex items-center justify-between mb-8">
            <h2 className="text-3xl font-bold text-white flex items-center gap-3">
              <Stethoscope className="w-8 h-8 text-emerald-400" />
              Registered Doctors ({doctors.length})
            </h2>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-left">
              <thead>
                <tr className="border-b border-slate-700/50">
                  <th className="py-4 px-6 font-semibold text-slate-300">
                    Name
                  </th>
                  <th className="py-4 px-6 font-semibold text-slate-300">
                    Email
                  </th>
                  <th className="py-4 px-6 font-semibold text-slate-300">
                    Phone
                  </th>
                </tr>
              </thead>
              <tbody>
                {paginate(doctors, doctorPage).map((doctor) => (
                  <tr
                    key={doctor.id}
                    className="border-b border-slate-700/30 hover:bg-slate-700/30"
                  >
                    <td className="py-4 px-6 font-medium text-white">
                      {doctor.firstName} {doctor.lastName}
                    </td>
                    <td className="py-4 px-6 text-slate-300">{doctor.email}</td>
                    <td className="py-4 px-6 text-slate-300">
                      {doctor.phoneNumber}
                    </td>
                    <td className="py-4 px-6">
                      <button
                        onClick={() => handleDeleteDoctor(doctor)}
                        className="p-2 bg-red-500/20 hover:bg-red-500/40 text-red-400 rounded-lg transition-all"
                        title="Delete Doctor"
                      >
                        <Trash className="w-5 h-5" />
                      </button>
                    </td>
                  </tr>
                ))}
                {doctors.length === 0 && (
                  <tr>
                    <td
                      colSpan={3}
                      className="py-12 text-center text-slate-400"
                    >
                      No doctors registered yet
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
            {doctors.length > itemsPerPage && (
              <div className="flex justify-center items-center mt-4 space-x-4">
                <button
                  onClick={() => setDoctorPage((p) => Math.max(p - 1, 1))}
                  className="px-4 py-2 bg-slate-700/50 rounded-lg text-white disabled:opacity-50"
                  disabled={doctorPage === 1}
                >
                  Prev
                </button>
                <span className="text-slate-300">
                  Page {doctorPage} of{" "}
                  {Math.ceil(doctors.length / itemsPerPage)}
                </span>
                <button
                  onClick={() =>
                    setDoctorPage((p) =>
                      p < Math.ceil(doctors.length / itemsPerPage) ? p + 1 : p
                    )
                  }
                  className="px-4 py-2 bg-slate-700/50 rounded-lg text-white disabled:opacity-50"
                  disabled={
                    doctorPage === Math.ceil(doctors.length / itemsPerPage)
                  }
                >
                  Next
                </button>
              </div>
            )}
          </div>
        </div>

        <div className="mb-12">
          <AppointmentCalendar
            appointments={allAppointments}
            doctors={doctors}
          />
        </div>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
          <div className="bg-slate-800/50 backdrop-blur-xl rounded-3xl border border-slate-700/50 p-8 mb-8">
            <div className="flex items-center justify-between mb-8">
              <h2 className="text-3xl font-bold text-white flex items-center gap-3">
                <Check className="w-8 h-8 text-emerald-400" />
                Approved Appointments ({approvedAppointments.length})
              </h2>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full text-left">
                <thead>
                  <tr className="border-b border-slate-700/50">
                    <th className="py-4 px-6 font-semibold text-slate-300">
                      Patient
                    </th>
                    <th className="py-4 px-6 font-semibold text-slate-300">
                      Doctor
                    </th>
                    <th className="py-4 px-6 font-semibold text-slate-300">
                      Date/Time
                    </th>
                    <th className="py-4 px-6 font-semibold text-slate-300">
                      Purpose
                    </th>
                    <th className="py-4 px-6 font-semibold text-slate-300">
                      Status
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {paginate(approvedAppointments, approvedPage).map((app) => (
                    <tr
                      key={app.id}
                      className="border-b border-slate-700/30 hover:bg-slate-700/30"
                    >
                      <td className="py-4 px-6 font-medium text-white">
                        {app.userFirstName} {app.userLastName}
                      </td>
                      <td className="py-4 px-6 text-slate-300">
                        {app.doctorName}
                      </td>
                      <td className="py-4 px-6 text-slate-300">
                        {new Date(app.appointmentDate).toLocaleDateString()} at{" "}
                        {app.startTime}
                      </td>
                      <td className="py-4 px-6 text-slate-400 max-w-xs truncate">
                        {app.purpose}
                      </td>
                      <td className="py-4 px-6">
                        <span className="px-3 py-1 bg-emerald-500/20 text-emerald-400 rounded-full text-sm font-medium">
                          Approved
                        </span>
                      </td>
                    </tr>
                  ))}
                  {approvedAppointments.length === 0 && (
                    <tr>
                      <td
                        colSpan={5}
                        className="py-12 text-center text-slate-400"
                      >
                        No approved appointments
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
              {approvedAppointments.length > itemsPerPage && (
                <div className="flex justify-center items-center mt-4 space-x-4">
                  <button
                    onClick={() => setApprovedPage((p) => Math.max(p - 1, 1))}
                    className="px-4 py-2 bg-slate-700/50 rounded-lg text-white disabled:opacity-50"
                    disabled={approvedPage === 1}
                  >
                    Prev
                  </button>
                  <span className="text-slate-300">
                    Page {approvedPage} of{" "}
                    {Math.ceil(approvedAppointments.length / itemsPerPage)}
                  </span>
                  <button
                    onClick={() =>
                      setApprovedPage((p) =>
                        p <
                        Math.ceil(approvedAppointments.length / itemsPerPage)
                          ? p + 1
                          : p
                      )
                    }
                    className="px-4 py-2 bg-slate-700/50 rounded-lg text-white disabled:opacity-50"
                    disabled={
                      approvedPage ===
                      Math.ceil(approvedAppointments.length / itemsPerPage)
                    }
                  >
                    Next
                  </button>
                </div>
              )}
            </div>
          </div>
          <div className="bg-slate-800/50 backdrop-blur-xl rounded-3xl border border-slate-700/50 p-8 mb-8">
            <div className="flex items-center justify-between mb-8">
              <h2 className="text-3xl font-bold text-white flex items-center gap-3">
                <XCircle className="w-8 h-8 text-red-400" />
                Rejected Appointments ({rejectedAppointments.length})
              </h2>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full text-left">
                <thead>
                  <tr className="border-b border-slate-700/50">
                    <th className="py-4 px-6 font-semibold text-slate-300">
                      Patient
                    </th>
                    <th className="py-4 px-6 font-semibold text-slate-300">
                      Doctor
                    </th>
                    <th className="py-4 px-6 font-semibold text-slate-300">
                      Date/Time
                    </th>
                    <th className="py-4 px-6 font-semibold text-slate-300">
                      Purpose
                    </th>
                    <th className="py-4 px-6 font-semibold text-slate-300">
                      Rejection Reason
                    </th>
                    <th className="py-4 px-6 font-semibold text-slate-300">
                      Status
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {paginate(rejectedAppointments, rejectedPage).map((app) => (
                    <tr
                      key={app.id}
                      className="border-b border-slate-700/30 hover:bg-slate-700/30"
                    >
                      <td className="py-4 px-6 font-medium text-white">
                        {app.userFirstName} {app.userLastName}
                      </td>
                      <td className="py-4 px-6 text-slate-300">
                        {app.doctorName}
                      </td>
                      <td className="py-4 px-6 text-slate-300">
                        {new Date(app.appointmentDate).toLocaleDateString()} at{" "}
                        {app.startTime}
                      </td>
                      <td className="py-4 px-6 text-slate-400 max-w-xs truncate">
                        {app.purpose}
                      </td>
                      <td className="py-4 px-6 text-slate-400 max-w-xs">
                        {app.rejectionReason || "No reason provided"}
                      </td>
                      <td className="py-4 px-6">
                        <span className="px-3 py-1 bg-red-500/20 text-red-400 rounded-full text-sm font-medium">
                          Rejected
                        </span>
                      </td>
                    </tr>
                  ))}
                  {rejectedAppointments.length === 0 && (
                    <tr>
                      <td
                        colSpan={6}
                        className="py-12 text-center text-slate-400"
                      >
                        No rejected appointments
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
              {rejectedAppointments.length > itemsPerPage && (
                <div className="flex justify-center items-center mt-4 space-x-4">
                  <button
                    onClick={() => setRejectedPage((p) => Math.max(p - 1, 1))}
                    className="px-4 py-2 bg-slate-700/50 rounded-lg text-white disabled:opacity-50"
                    disabled={rejectedPage === 1}
                  >
                    Prev
                  </button>
                  <span className="text-slate-300">
                    Page {rejectedPage} of{" "}
                    {Math.ceil(rejectedAppointments.length / itemsPerPage)}
                  </span>
                  <button
                    onClick={() =>
                      setRejectedPage((p) =>
                        p <
                        Math.ceil(rejectedAppointments.length / itemsPerPage)
                          ? p + 1
                          : p
                      )
                    }
                    className="px-4 py-2 bg-slate-700/50 rounded-lg text-white disabled:opacity-50"
                    disabled={
                      rejectedPage ===
                      Math.ceil(rejectedAppointments.length / itemsPerPage)
                    }
                  >
                    Next
                  </button>
                </div>
              )}
            </div>
          </div>
          <div className="bg-slate-800/50 backdrop-blur-xl rounded-3xl border border-slate-700/50 p-8">
            <div className="flex items-center justify-between mb-8">
              <h2 className="text-3xl font-bold text-white flex items-center gap-3">
                <Users className="w-8 h-8 text-blue-400" />
                Users ({filteredUsers.length})
              </h2>
              <button
                onClick={() => setShowAddUserForm(true)}
                className="bg-gradient-to-r from-blue-600 to-indigo-600 text-white px-6 py-3 rounded-xl font-bold hover:shadow-blue-500/50 transition-all"
              >
                + Add User
              </button>
            </div>
            <div className="relative mb-6">
              <Search className="w-5 h-5 text-slate-400 absolute left-4 top-1/2 -translate-y-1/2" />
              <input
                type="text"
                placeholder="Search users..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-12 pr-4 py-3 bg-slate-700/50 border border-slate-600 rounded-2xl text-white placeholder-slate-400"
              />
            </div>
            <div className="overflow-x-auto">
              <table className="w-full text-left">
                <thead>
                  <tr className="border-b border-slate-700/50">
                    <th className="py-3 px-4 text-slate-300 font-semibold">
                      Name
                    </th>
                    <th className="py-3 px-4 text-slate-300 font-semibold">
                      Email
                    </th>
                    <th className="py-3 px-4 text-slate-300 font-semibold">
                      Type
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {paginate(filteredUsers, userPage).map((u) => (
                    <tr
                      key={u.id}
                      className="border-b border-slate-700/30 hover:bg-slate-700/30"
                    >
                      <td className="py-3 px-4 font-medium text-white">
                        {u.firstName} {u.lastName}
                      </td>
                      <td className="py-3 px-4 text-slate-300">{u.email}</td>
                      <td className="py-3 px-4">
                        <span
                          className={`px-3 py-1 rounded-full text-xs font-bold ${
                            u.type === "Admin"
                              ? "bg-purple-500/20 text-purple-400"
                              : "bg-emerald-500/20 text-emerald-400"
                          }`}
                        >
                          {u.type}
                        </span>
                      </td>
                      <td className="py-3 px-4">
                        {u.type !== "Admin" && (
                          <button
                            onClick={() => handleDeleteUser(u)}
                            className="p-2 bg-red-500/20 hover:bg-red-500/40 text-red-400 rounded-lg transition-all"
                            title="Delete User"
                          >
                            <Trash className="w-5 h-5" />
                          </button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {filteredUsers.length > itemsPerPage && (
                <div className="flex justify-center items-center mt-4 space-x-4">
                  <button
                    onClick={() => setUserPage((p) => Math.max(p - 1, 1))}
                    className="px-4 py-2 bg-slate-700/50 rounded-lg text-white disabled:opacity-50"
                    disabled={userPage === 1}
                  >
                    Prev
                  </button>
                  <span className="text-slate-300">
                    Page {userPage} of{" "}
                    {Math.ceil(filteredUsers.length / itemsPerPage)}
                  </span>
                  <button
                    onClick={() =>
                      setUserPage((p) =>
                        p < Math.ceil(filteredUsers.length / itemsPerPage)
                          ? p + 1
                          : p
                      )
                    }
                    className="px-4 py-2 bg-slate-700/50 rounded-lg text-white disabled:opacity-50"
                    disabled={
                      userPage ===
                      Math.ceil(filteredUsers.length / itemsPerPage)
                    }
                  >
                    Next
                  </button>
                </div>
              )}
            </div>
          </div>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mt-12">
          <button
            onClick={() => setShowAddClinicForm(true)}
            className="group bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-700 hover:to-teal-700 text-white p-8 rounded-3xl font-bold shadow-xl hover:shadow-emerald-500/50 transition-all hover:scale-105 flex flex-col items-center gap-4"
          >
            <Shield className="w-12 h-12 group-hover:scale-110 transition-transform" />
            <span className="text-xl">Add Clinic</span>
          </button>

          <div className="bg-gradient-to-r from-blue-600 to-indigo-600 text-white p-8 rounded-3xl shadow-xl hover:shadow-blue-500/50 transition-all hover:scale-105">
            <BarChart3 className="w-12 h-12 mx-auto mb-4" />
            <h3 className="text-xl font-bold mb-2">Analytics</h3>
            <p className="text-blue-100">View full reports</p>
          </div>
          <div className="bg-gradient-to-r from-purple-600 to-pink-600 text-white p-8 rounded-3xl shadow-xl hover:shadow-purple-500/50 transition-all hover:scale-105">
            <Activity className="w-12 h-12 mx-auto mb-4" />
            <h3 className="text-xl font-bold mb-2">System Health</h3>
            <p className="text-purple-100">98% Uptime</p>
          </div>
        </div>
      </main>

      {showAddClinicForm && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div className="bg-slate-800/95 backdrop-blur-xl rounded-3xl max-w-md w-full p-8 border border-slate-700/50">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-2xl font-bold text-white">Add Clinic</h2>
              <button
                onClick={() => setShowAddClinicForm(false)}
                className="p-2 hover:bg-slate-700 rounded-xl"
              >
                <X className="w-6 h-6" />
              </button>
            </div>
            <form onSubmit={handleAddClinicSubmit} className="space-y-4">
              <div>
                <input
                  name="name"
                  placeholder="Clinic Admin Name *"
                  value={clinicForm.name}
                  onChange={(e) => handleInputChange(e, setClinicForm)}
                  className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
                  required
                />
                <p className="text-slate-400 text-xs mt-1">e.g., John Smith</p>
              </div>
              <div>
                <input
                  name="email"
                  type="email"
                  placeholder="Email Address *"
                  value={clinicForm.email}
                  onChange={(e) => handleInputChange(e, setClinicForm)}
                  className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
                  required
                />
                <p className="text-slate-400 text-xs mt-1">
                  e.g., john.smith@meditrack.com
                </p>
              </div>
              <div>
                <input
                  name="password"
                  type="password"
                  placeholder="Set Password *"
                  value={clinicForm.password}
                  onChange={(e) => handleInputChange(e, setClinicForm)}
                  className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
                  required
                  minLength="6"
                />
                <p className="text-slate-400 text-xs mt-1">
                  Minimum 6 characters
                </p>
              </div>
              <div>
                <input
                  name="clinicName"
                  placeholder="Clinic Name *"
                  value={clinicForm.clinicName}
                  onChange={(e) => handleInputChange(e, setClinicForm)}
                  className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
                  required
                />
              </div>
              <input
                name="phoneNumber"
                placeholder="Phone Number"
                value={clinicForm.phoneNumber}
                onChange={(e) => handleInputChange(e, setClinicForm)}
                className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
              />
              <textarea
                name="address"
                placeholder="Clinic Address"
                value={clinicForm.address}
                onChange={(e) => handleInputChange(e, setClinicForm)}
                rows="2"
                className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white resize-vertical"
              />
              <button
                type="submit"
                className="w-full bg-gradient-to-r from-emerald-600 to-teal-600 text-white py-4 rounded-xl font-bold hover:shadow-emerald-500/50 transition-all"
              >
                Add Clinic
              </button>
            </form>
          </div>
        </div>
      )}
      {showPasswordModal && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div className="bg-slate-800/95 backdrop-blur-xl rounded-3xl max-w-md w-full p-8 border border-slate-700/50">
            <div className="flex flex-col items-center gap-6">
              <div className="w-16 h-16 bg-emerald-500/20 rounded-full flex items-center justify-center">
                <Check className="w-8 h-8 text-emerald-500" />
              </div>
              <h2 className="text-2xl font-bold text-white text-center">
                Clinic Registered Successfully!
              </h2>
              <div className="w-full space-y-4">
                <p className="text-slate-300 text-center">
                  Clinic admin can now login with these credentials:
                </p>
                <div className="bg-slate-700/50 rounded-xl p-4 space-y-3">
                  <div className="flex justify-between items-center">
                    <span className="text-slate-400">Name:</span>
                    <span className="text-white font-medium">
                      {newClinicData.name}
                    </span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="text-slate-400">Email:</span>
                    <span className="text-white font-mono text-sm">
                      {newClinicData.email}
                    </span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="text-slate-400">Password:</span>
                    <span className="text-white font-mono bg-slate-600 px-3 py-1 rounded text-sm">
                      {newClinicData.password}
                    </span>
                  </div>
                </div>
                <p className="text-green-400 text-sm text-center">
                  ✅ Clinic admin can now login to their dashboard
                </p>
              </div>
              <div className="flex gap-3 w-full">
                <button
                  onClick={() => {
                    navigator.clipboard.writeText(
                      `Email: ${newClinicData.email}\nPassword: ${newClinicData.password}`
                    );
                    toast.success("Credentials copied to clipboard!");
                  }}
                  className="flex-1 py-3 bg-blue-600 hover:bg-blue-700 text-white rounded-xl font-bold transition-all"
                >
                  Copy Credentials
                </button>
                <button
                  onClick={() => {
                    setShowPasswordModal(false);
                    setRegistrationType(null);
                  }}
                  className="flex-1 py-3 bg-gradient-to-r from-emerald-600 to-teal-600 text-white rounded-xl font-bold hover:shadow-emerald-500/50 transition-all"
                >
                  Done
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
      {showDeleteModal && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div className="bg-slate-800/95 backdrop-blur-xl rounded-3xl max-w-md w-full p-8 border border-slate-700/50">
            <div className="flex flex-col items-center gap-6">
              <div className="w-16 h-16 bg-red-500/20 rounded-full flex items-center justify-center">
                <AlertTriangle className="w-8 h-8 text-red-500" />
              </div>
              <h2 className="text-2xl font-bold text-white text-center">
                Confirm Deletion
              </h2>
              <p className="text-slate-300 text-center">
                Are you sure you want to delete {deleteTarget.name}? This action
                cannot be undone.
              </p>
              <div className="flex gap-4 w-full">
                <button
                  onClick={() => setShowDeleteModal(false)}
                  className="flex-1 py-3 px-4 bg-slate-700/50 hover:bg-slate-700 text-white rounded-xl transition-all"
                >
                  Cancel
                </button>
                <button
                  onClick={handleConfirmDelete}
                  className="flex-1 py-3 px-4 bg-red-600 hover:bg-red-700 text-white rounded-xl transition-all"
                >
                  Delete
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
      {showAddUserForm && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div className="bg-slate-800/95 backdrop-blur-xl rounded-3xl max-w-md w-full p-8 border border-slate-700/50">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-2xl font-bold text-white">Create User</h2>
              <button
                onClick={() => setShowAddUserForm(false)}
                className="p-2 hover:bg-slate-700 rounded-xl"
              >
                <X className="w-6 h-6" />
              </button>
            </div>
            <form onSubmit={handleAddUserSubmit} className="space-y-4">
              <input
                name="firstName"
                placeholder="First Name *"
                value={userForm.firstName}
                onChange={(e) => handleInputChange(e, setUserForm)}
                className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
                required
              />
              <input
                name="lastName"
                placeholder="Last Name *"
                value={userForm.lastName}
                onChange={(e) => handleInputChange(e, setUserForm)}
                className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
                required
              />
              <input
                name="email"
                type="email"
                placeholder="Email *"
                value={userForm.email}
                onChange={(e) => handleInputChange(e, setUserForm)}
                className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
                required
              />
              <input
                name="password"
                type="password"
                placeholder="Password *"
                value={userForm.password}
                onChange={(e) => handleInputChange(e, setUserForm)}
                className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
                required
              />
              <input
                name="phoneNumber"
                placeholder="Phone (optional)"
                value={userForm.phoneNumber}
                onChange={(e) => handleInputChange(e, setUserForm)}
                className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
              />
              <select
                name="type"
                value={userForm.type}
                onChange={(e) => handleInputChange(e, setUserForm)}
                className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
              >
                <option value="Patient">Patient</option>
                <option value="Admin">Admin</option>
              </select>
              <button
                type="submit"
                className="w-full bg-gradient-to-r from-blue-600 to-indigo-600 text-white py-4 rounded-xl font-bold hover:shadow-blue-500/50 transition-all"
              >
                Create User
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default AdminDashboard;
