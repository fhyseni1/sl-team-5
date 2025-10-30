// client/src/components/ClinicDashboard.jsx
"use client";
import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import {
  Users,
  Stethoscope,
  LogOut,
  Loader2,
  Plus,
  X,
  UserCheck,
  Check,
  Search,
  Trash,
  AlertTriangle,
} from "lucide-react";
import { authService, api } from "../../services/authService";

const ClinicDashboard = () => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [doctors, setDoctors] = useState([]);
  const [clinic, setClinic] = useState(null);
  const [showAddDoctorForm, setShowAddDoctorForm] = useState(false);
  const [showPasswordModal, setShowPasswordModal] = useState(false);
  const [newDoctorData, setNewDoctorData] = useState({
    email: "",
    name: "",
    password: "",
  });
  const [doctorForm, setDoctorForm] = useState({
    name: "",
    email: "",
    password: "",
    specialty: "",
    phoneNumber: "",
  });
  const [assistantRefresh, setAssistantRefresh] = useState(0);
  const [showAddAssistantForm, setShowAddAssistantForm] = useState(false);
const [showAssignAssistantModal, setShowAssignAssistantModal] = useState(false);
const [assistantForm, setAssistantForm] = useState({
  firstName: "",
  lastName: "",
  email: "",
  password: "",
  phoneNumber: "",
});
const [assignForm, setAssignForm] = useState({
  doctorId: "",
  assistantId: "",
});
const [assistants, setAssistants] = useState([]);
const [assistantPage, setAssistantPage] = useState(1);
  const [doctorPage, setDoctorPage] = useState(1);
  const [searchTerm, setSearchTerm] = useState("");
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState({ id: null, name: "",type: "" });
  const itemsPerPage = 5;
  const router = useRouter();

  const paginate = (data, page) => {
    const start = (page - 1) * itemsPerPage;
    return data.slice(start, start + itemsPerPage);
  };

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const userData = await authService.getMe();
        console.log("User Data:", userData); // Debug log
        if (userData.type !== 8) {
          console.warn("Unauthorized user type:", userData.type);
          await authService.logout();
          router.push("/dashboard/dashboard");
          return;
        }
        setUser(userData);

        // Fetch clinic associated with the admin user
        try {
          const clinicRes = await api.get(`/clinics/admin/${userData.id}`);
          console.log("Clinic Response:", clinicRes.data); // Debug log
          setClinic(clinicRes.data);
        } catch (clinicErr) {
          console.error("Clinic fetch error:", clinicErr);
          setError("Failed to load clinic data. Please try again later.");
          setClinic(null);
          return;
        }
      } catch (err) {
        console.error("Clinic dashboard error:", err);
        setError(err.response?.data?.message || "Failed to load dashboard");
        if (err.response?.status === 401) {
          await authService.logout();
          router.push("/login");
        }
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [router]);

  useEffect(() => {
    const fetchDoctors = async () => {
      if (clinic?.id) {
        try {
          const doctorsRes = await api.get(`/doctors/clinic/${clinic.id}`);
          console.log("Doctors Response:", doctorsRes.data); // Debug log
          setDoctors(doctorsRes.data || []);
        } catch (doctorErr) {
          console.error(
            "Doctor fetch error:",
            doctorErr.response?.data || doctorErr
          );
          setDoctors([]);
          toast.error("Failed to load doctors. Please try again.");
        }
      } else {
        console.warn("No clinic ID available, skipping doctors fetch");
        setDoctors([]);
      }
    };
    fetchDoctors();
  }, [clinic]);

useEffect(() => {
  const fetchAssistants = async () => {
    try {
      console.log("🟡 Fetching assistants...");
  
      const allUsersRes = await api.get("/users");
      const allUsers = allUsersRes.data || [];
   
      const assistantsList = allUsers.filter(user => 
        user.type === 7 || user.type === 'Assistant' // 7 = Assistant type
      );
      
      console.log(`✅ Found ${assistantsList.length} assistants`);
      setAssistants(assistantsList);
      
    } catch (err) {
      console.error("❌ Error fetching assistants:", err);
   
      setAssistants([]);
    }
  };
  
  fetchAssistants();
}, [assistantRefresh]);
useEffect(() => {
  const fetchDoctorsWithUserIds = async () => {
    if (clinic?.id) {
      try {
        // Get doctors from your clinic
        const doctorsRes = await api.get(`/doctors/clinic/${clinic.id}`);
        const clinicDoctors = doctorsRes.data || [];
        
        // Get all users to find the corresponding user IDs for doctors
        const usersRes = await api.get("/users");
        const allUsers = usersRes.data || [];
        
        // Map doctors to include their user ID
        const doctorsWithUserIds = clinicDoctors.map(doctor => {
          // Find the user that corresponds to this doctor
          // This assumes doctors have users with matching names or emails
          const doctorUser = allUsers.find(user => 
            user.type === 4 && // HealthcareProvider type
            (
              user.email.toLowerCase().includes(doctor.name.toLowerCase().replace(' ', '.')) ||
              `${user.firstName} ${user.lastName}`.toLowerCase() === doctor.name.toLowerCase()
            )
          );
          
          return {
            ...doctor,
            userId: doctorUser?.id // This is what we need for assignment
          };
        });
        
        setDoctors(doctorsWithUserIds);
      } catch (err) {
        console.error("Error fetching doctors with user data:", err);
      }
    }
  };
  
  fetchDoctorsWithUserIds();
}, [clinic]);
  const handleLogout = async () => {
    await authService.logout();
    router.push("/");
  };

  const handleAddDoctorSubmit = async (e) => {
    e.preventDefault();
    if (!clinic?.id) {
      toast.error("No clinic data available. Cannot add doctor.");
      return;
    }
    try {
      const nameParts = doctorForm.name.split(" ");
      const firstName = nameParts[0] || "Doctor";
      const lastName = nameParts.slice(1).join(" ") || "Unknown";
      const doctorEmail =
        doctorForm.email ||
        `${firstName.toLowerCase()}.${lastName.toLowerCase()}@meditrack.com`;

      const payload = {
        email: doctorEmail,
        password: doctorForm.password,
        firstName,
        lastName,
        phoneNumber: doctorForm.phoneNumber || "",
        specialty: doctorForm.specialty,
        clinicId: clinic.id,
      };

      const response = await api.post("/auth/register-doctor", payload);
      if (response.data) {
        setNewDoctorData({
          email: doctorEmail,
          name: doctorForm.name,
          password: doctorForm.password,
        });
        setShowPasswordModal(true);
        setDoctorForm({
          name: "",
          email: "",
          password: "",
          specialty: "",
          phoneNumber: "",
        });
        setShowAddDoctorForm(false);
        toast.success("Doctor added successfully!");
        const doctorsRes = await api.get(`/doctors/clinic/${clinic.id}`);
        console.log("Updated Doctors Response:", doctorsRes.data); // Debug log
        setDoctors(doctorsRes.data || []);
      }
    } catch (err) {
      console.error("Error adding doctor:", err.response?.data || err);
      toast.error(err.response?.data?.message || "Failed to add doctor");
    }
  };

  const handleDeleteDoctor = async (doctor) => {
    setDeleteTarget({
      id: doctor.id,
      name: `Dr. ${doctor.name}`,
    });
    setShowDeleteModal(true);
  };

  const handleConfirmDelete = async () => {
    try {
      if (deleteTarget.type === 'assistant') {
      
        await api.delete(`/users/${deleteTarget.id}`);
        setAssistants(prev => prev.filter(a => a.id !== deleteTarget.id));
        toast.success("Assistant deleted successfully!");
      } else {
       
        await api.delete(`/doctors/${deleteTarget.id}`);
        try {
          await api.delete(`/users/${deleteTarget.id}`);
        } catch (userDeleteErr) {
          console.warn(
            "User delete failed, continuing with doctor deletion:",
            userDeleteErr
          );
        }
        setDoctors(prev => prev.filter(d => d.id !== deleteTarget.id));
        toast.success("Doctor deleted successfully!");
      }
      setShowDeleteModal(false);
    } catch (err) {
      console.error("Error deleting:", err.response?.data || err);
      if (err.response?.status === 403) {
        toast.error("Permission denied: You can only delete assistants");
      } else {
        toast.error(err.response?.data?.message || "Failed to delete");
      }
    }
  };

const handleDeleteAssistant = async (assistant) => {
  setDeleteTarget({
    id: assistant.id,
    name: `${assistant.firstName} ${assistant.lastName}`,
    type: 'assistant' 
  });
  setShowDeleteModal(true);
};



  const filteredDoctors = doctors.filter((d) =>
    d.name?.toLowerCase().includes(searchTerm.toLowerCase())
  );
const handleAddAssistantSubmit = async (e) => {
  e.preventDefault();
  try {
    console.log("🟡 === STARTING ASSISTANT REGISTRATION ===");
    
    const payload = {
      email: assistantForm.email,
      password: assistantForm.password,
      firstName: assistantForm.firstName,
      lastName: assistantForm.lastName,
      phoneNumber: assistantForm.phoneNumber || "",
    };

    console.log("🟡 Sending to /auth/register-assistant:", payload);
  
    const response = await api.post("/auth/register-assistant", payload);
    
    console.log("✅ Assistant registered:", response.data);

    setShowAddAssistantForm(false);
    setAssistantForm({
      firstName: "", lastName: "", email: "", password: "", phoneNumber: ""
    });
    
    setAssistantRefresh(prev => prev + 1);
    toast.success("Assistant created successfully!");
    
  } catch (err) {
    console.error("❌ Error:", err.response?.data || err);
    toast.error(err.response?.data?.message || "Failed to create assistant");
  }
};
const handleAssignAssistant = async (e) => {
  e.preventDefault();
  try {
    console.log("🟡 ===== STARTING ASSISTANT ASSIGNMENT =====");
    
    const selectedDoctor = doctors.find(d => d.id === assignForm.doctorId);
    
    if (!selectedDoctor || !selectedDoctor.userId) {
      toast.error("Doctor user information not found. Please make sure the doctor has a corresponding user account.");
      return;
    }

    const payload = {
      doctorId: selectedDoctor.userId,
      assistantId: assignForm.assistantId
    };

    console.log("🟡 Sending payload with User ID:", payload);
    
    const response = await api.post("/users/assign-assistant", payload);
    
    console.log("✅ SUCCESS - Assistant assigned:", response.data);
    
    setShowAssignAssistantModal(false);
    setAssignForm({ doctorId: "", assistantId: "" });
    toast.success("Assistant assigned to doctor successfully!");
    
  } catch (err) {
    console.error("❌ Error:", err.response?.data);
    
    if (err.response?.status === 400) {
      const errorMsg = err.response?.data?.message || "Invalid data";
      toast.error(`Assignment failed: ${errorMsg}`);
    } else {
      toast.error(err.response?.data?.message || "Failed to assign assistant");
    }
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
        <button
          onClick={handleLogout}
          className="ml-4 px-4 py-2 bg-red-600 text-white rounded-xl"
        >
          Logout
        </button>
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
      <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_right,_var(--tw-gradient-stops))] from-purple-900/20 via-transparent to-emerald-900/20"></div>
      <header className="bg-slate-900/80 backdrop-blur-xl border-b border-slate-700/50 sticky top-0 z-50">
        <div className="max-w-7xl mx-auto px-6 py-5">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4">
              <div className="w-14 h-14 bg-gradient-to-br from-emerald-600 to-teal-600 rounded-2xl flex items-center justify-center shadow-xl">
                <Stethoscope className="w-7 h-7 text-white" />
              </div>
              <div>
                <h1 className="text-3xl font-bold bg-gradient-to-r from-slate-100 to-slate-300 bg-clip-text text-transparent">
                  Clinic Dashboard
                </h1>
                <p className="text-slate-400">
                  {clinic?.clinicName || "No Clinic Data"}
                </p>
              </div>
            </div>
            <button
              onClick={handleLogout}
              className="flex items-center space-x-2 px-6 py-3 bg-gradient-to-r from-red-600 to-red-700 text-white rounded-xl font-bold shadow-xl"
            >
              <LogOut className="w-5 h-5" /> Logout
            </button>
          </div>
        </div>
      </header>
      <main className="max-w-7xl mx-auto px-6 py-12 relative z-10">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-12">
          <div className="bg-gradient-to-br from-emerald-500/10 backdrop-blur-xl rounded-2xl p-8 border shadow-xl hover:scale-105 transition-all">
            <Stethoscope className="w-12 h-12 text-emerald-400 mx-auto mb-4" />
            <p className="text-3xl font-bold text-white mb-2">
              {doctors.length}
            </p>
            <p className="text-slate-400 font-semibold">Registered Doctors</p>
          </div>
          <div className="bg-gradient-to-br from-blue-500/10 backdrop-blur-xl rounded-2xl p-8 border shadow-xl hover:scale-105 transition-all">
            <Users className="w-12 h-12 text-blue-400 mx-auto mb-4" />
            <p className="text-3xl font-bold text-white mb-2">
              {clinic?.id || "N/A"}
            </p>
            <p className="text-slate-400 font-semibold">Clinic ID</p>
          </div>
        </div>
        <div className="bg-slate-800/50 backdrop-blur-xl rounded-3xl border border-slate-700/50 p-8 mb-8">
          <div className="flex items-center justify-between mb-8">
            <h2 className="text-3xl font-bold text-white flex items-center gap-3">
              <Stethoscope className="w-8 h-8 text-emerald-400" />
              Registered Doctors ({filteredDoctors.length})
            </h2>
            <button
              onClick={() => setShowAddDoctorForm(true)}
              disabled={!clinic}
              className="bg-gradient-to-r from-emerald-600 to-teal-600 text-white px-6 py-3 rounded-xl font-bold hover:shadow-emerald-500/50 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <Plus className="w-5 h-5 inline-block mr-2" /> Add Doctor
            </button>
          </div>
          <div className="relative mb-6">
            <Search className="w-5 h-5 text-slate-400 absolute left-4 top-1/2 -translate-y-1/2" />
            <input
              type="text"
              placeholder="Search doctors..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full pl-12 pr-4 py-3 bg-slate-700/50 border border-slate-600 rounded-2xl text-white placeholder-slate-400"
            />
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-left">
              <thead>
                <tr className="border-b border-slate-700/50">
                  <th className="py-4 px-6 font-semibold text-slate-300">
                    Name
                  </th>
                  <th className="py-4 px-6 font-semibold text-slate-300">
                    Specialty
                  </th>
                  <th className="py-4 px-6 font-semibold text-slate-300">
                    Phone
                  </th>
                  <th className="py-4 px-6 font-semibold text-slate-300">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody>
                {paginate(filteredDoctors, doctorPage).map((doctor) => (
                  <tr
                    key={doctor.id}
                    className="border-b border-slate-700/30 hover:bg-slate-700/30"
                  >
                    <td className="py-4 px-6 font-medium text-white">
                      {doctor.name}
                    </td>
                    <td className="py-4 px-6 text-slate-300">
                      {doctor.specialty}
                    </td>
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
                {filteredDoctors.length === 0 && (
                  <tr>
                    <td
                      colSpan={4}
                      className="py-12 text-center text-slate-400"
                    >
                      No doctors registered yet
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
            {filteredDoctors.length > itemsPerPage && (
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
                  {Math.ceil(filteredDoctors.length / itemsPerPage)}
                </span>
                <button
                  onClick={() =>
                    setDoctorPage((p) =>
                      p < Math.ceil(filteredDoctors.length / itemsPerPage)
                        ? p + 1
                        : p
                    )
                  }
                  className="px-4 py-2 bg-slate-700/50 rounded-lg text-white disabled:opacity-50"
                  disabled={
                    doctorPage ===
                    Math.ceil(filteredDoctors.length / itemsPerPage)
                  }
                >
                  Next
                </button>
              </div>
            )}
          </div>
        </div>
      </main>
      {showAddDoctorForm && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div className="bg-slate-800/95 backdrop-blur-xl rounded-3xl max-w-md w-full p-8 border border-slate-700/50">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-2xl font-bold text-white">Add Doctor</h2>
              <button
                onClick={() => setShowAddDoctorForm(false)}
                className="p-2 hover:bg-slate-700 rounded-xl"
              >
                <X className="w-6 h-6" />
              </button>
            </div>
            <form onSubmit={handleAddDoctorSubmit} className="space-y-4">
              <div>
                <input
                  name="clinicName"
                  value={clinic?.clinicName || "No Clinic Data"}
                  readOnly
                  className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white opacity-70 cursor-not-allowed"
                />
                <p className="text-slate-400 text-xs mt-1">
                  Doctor will be added to this clinic
                </p>
              </div>
              <div>
                <input
                  name="name"
                  placeholder="Doctor Name *"
                  value={doctorForm.name}
                  onChange={(e) => handleInputChange(e, setDoctorForm)}
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
                  value={doctorForm.email}
                  onChange={(e) => handleInputChange(e, setDoctorForm)}
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
                  value={doctorForm.password}
                  onChange={(e) => handleInputChange(e, setDoctorForm)}
                  className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
                  required
                  minLength="6"
                />
                <p className="text-slate-400 text-xs mt-1">
                  Minimum 6 characters
                </p>
              </div>
              <input
                name="specialty"
                placeholder="Specialty *"
                value={doctorForm.specialty}
                onChange={(e) => handleInputChange(e, setDoctorForm)}
                className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
                required
              />
              <input
                name="phoneNumber"
                placeholder="Phone Number (optional)"
                value={doctorForm.phoneNumber}
                onChange={(e) => handleInputChange(e, setDoctorForm)}
                className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
              />
              <button
                type="submit"
                disabled={!clinic}
                className="w-full bg-gradient-to-r from-emerald-600 to-teal-600 text-white py-4 rounded-xl font-bold hover:shadow-emerald-500/50 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Add Doctor
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
                Doctor Registered Successfully!
              </h2>
              <div className="w-full space-y-4">
                <p className="text-slate-300 text-center">
                  Doctor can now login with these credentials:
                </p>
                <div className="bg-slate-700/50 rounded-xl p-4 space-y-3">
                  <div className="flex justify-between items-center">
                    <span className="text-slate-400">Name:</span>
                    <span className="text-white font-medium">
                      {newDoctorData.name}
                    </span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="text-slate-400">Email:</span>
                    <span className="text-white font-mono text-sm">
                      {newDoctorData.email}
                    </span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="text-slate-400">Password:</span>
                    <span className="text-white font-mono bg-slate-600 px-3 py-1 rounded text-sm">
                      {newDoctorData.password}
                    </span>
                  </div>
                </div>
                <p className="text-green-400 text-sm text-center">
                  ✅ Doctor can now login to their dashboard
                </p>
              </div>
              <div className="flex gap-3 w-full">
                <button
                  onClick={() => {
                    navigator.clipboard.writeText(
                      `Email: ${newDoctorData.email}\nPassword: ${newDoctorData.password}`
                    );
                    toast.success("Credentials copied to clipboard!");
                  }}
                  className="flex-1 py-3 bg-blue-600 hover:bg-blue-700 text-white rounded-xl font-bold transition-all"
                >
                  Copy Credentials
                </button>
                <button
                  onClick={() => setShowPasswordModal(false)}
                  className="flex-1 py-3 bg-gradient-to-r from-emerald-600 to-teal-600 text-white rounded-xl font-bold hover:shadow-emerald-500/50 transition-all"
                >
                  Done
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
     

      {/* Assistants Section - NOW PROPERLY INSIDE MAIN */}
        <div className="bg-slate-800/50 backdrop-blur-xl rounded-3xl border border-slate-700/50 p-8 mb-8">
          <div className="flex items-center justify-between mb-8">
            <h2 className="text-3xl font-bold text-white flex items-center gap-3">
              <UserCheck className="w-8 h-8 text-purple-400" />
              Registered Assistants ({assistants.length})
            </h2>
            <div className="flex gap-3">
              <button
                onClick={() => setShowAssignAssistantModal(true)}
                className="bg-gradient-to-r from-orange-600 to-amber-600 text-white px-6 py-3 rounded-xl font-bold hover:shadow-orange-500/50 transition-all"
              >
                Assign Assistant
              </button>
 <button
  onClick={() => setShowAddAssistantForm(true)}
  className="bg-gradient-to-r from-purple-600 to-pink-600 text-white px-6 py-3 rounded-xl font-bold hover:shadow-purple-500/50 transition-all"
>
  + Add Assistant
</button>
            </div>
          </div>
          
          <div className="overflow-x-auto">
            <table className="w-full text-left">
  <thead>
    <tr className="border-b border-slate-700/50">
      <th className="py-4 px-6 font-semibold text-slate-300">Name</th>
      <th className="py-4 px-6 font-semibold text-slate-300">Email</th>
      <th className="py-4 px-6 font-semibold text-slate-300">Phone</th>
      <th className="py-4 px-6 font-semibold text-slate-300">Actions</th>
    </tr>
  </thead>
  <tbody>
    {paginate(assistants, assistantPage).map((assistant) => (
      <tr key={assistant.id} className="border-b border-slate-700/30 hover:bg-slate-700/30">
        <td className="py-4 px-6 font-medium text-white">
          {assistant.firstName} {assistant.lastName}
        </td>
        <td className="py-4 px-6 text-slate-300">{assistant.email}</td>
        <td className="py-4 px-6 text-slate-300">{assistant.phoneNumber}</td>
        <td className="py-4 px-6">
          <button
            onClick={() => handleDeleteAssistant(assistant)}
            className="p-2 bg-red-500/20 hover:bg-red-500/40 text-red-400 rounded-lg transition-all"
            title="Delete Assistant"
          >
            <Trash className="w-5 h-5" />
          </button>
        </td>
      </tr>
    ))}
    {assistants.length === 0 && (
      <tr>
        <td colSpan={4} className="py-12 text-center text-slate-400"> 
          No assistants registered yet
        </td>
      </tr>
    )}
  </tbody>
</table>
            
            {assistants.length > itemsPerPage && (
              <div className="flex justify-center items-center mt-4 space-x-4">
                <button
                  onClick={() => setAssistantPage((p) => Math.max(p - 1, 1))}
                  className="px-4 py-2 bg-slate-700/50 rounded-lg text-white disabled:opacity-50"
                  disabled={assistantPage === 1}
                >
                  Prev
                </button>
                <span className="text-slate-300">
                  Page {assistantPage} of {Math.ceil(assistants.length / itemsPerPage)}
                </span>
                <button
                  onClick={() => setAssistantPage((p) => p < Math.ceil(assistants.length / itemsPerPage) ? p + 1 : p)}
                  className="px-4 py-2 bg-slate-700/50 rounded-lg text-white disabled:opacity-50"
                  disabled={assistantPage === Math.ceil(assistants.length / itemsPerPage)}
                >
                  Next
                </button>
              </div>
            )}
          </div>
        </div>
{/* Add Assistant Modal */}
{showAddAssistantForm && (
  <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
    <div className="bg-slate-800/95 backdrop-blur-xl rounded-3xl max-w-md w-full p-8 border border-slate-700/50">
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-2xl font-bold text-white">Add Assistant</h2>
        <button
          onClick={() => setShowAddAssistantForm(false)}
          className="p-2 hover:bg-slate-700 rounded-xl"
        >
          <X className="w-6 h-6" />
        </button>
      </div>
      
      <form onSubmit={handleAddAssistantSubmit} className="space-y-4">
        <div className="grid grid-cols-2 gap-4">
          <input
            name="firstName"
            placeholder="First Name *"
            value={assistantForm.firstName}
            onChange={(e) => handleInputChange(e, setAssistantForm)}
            className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
            required
          />
          <input
            name="lastName"
            placeholder="Last Name *"
            value={assistantForm.lastName}
            onChange={(e) => handleInputChange(e, setAssistantForm)}
            className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
            required
          />
        </div>
        <input
          name="email"
          type="email"
          placeholder="Email *"
          value={assistantForm.email}
          onChange={(e) => handleInputChange(e, setAssistantForm)}
          className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
          required
        />
        <input
          name="password"
          type="password"
          placeholder="Password *"
          value={assistantForm.password}
          onChange={(e) => handleInputChange(e, setAssistantForm)}
          className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
          required
          minLength="6"
        />
        <input
          name="phoneNumber"
          placeholder="Phone Number (optional)"
          value={assistantForm.phoneNumber}
          onChange={(e) => handleInputChange(e, setAssistantForm)}
          className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
        />
        <button
          type="submit"
          className="w-full bg-gradient-to-r from-purple-600 to-pink-600 text-white py-4 rounded-xl font-bold hover:shadow-purple-500/50 transition-all"
        >
          Add Assistant
        </button>
      </form>
    </div>
  </div>
)}

{/* Assign Assistant Modal */}
{showAssignAssistantModal && (
  <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
    <div className="bg-slate-800/95 backdrop-blur-xl rounded-3xl max-w-md w-full p-8 border border-slate-700/50">
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-2xl font-bold text-white">
          Assign Assistant to Doctor
        </h2>
        <button
          onClick={() => setShowAssignAssistantModal(false)}
          className="p-2 hover:bg-slate-700 rounded-xl"
        >
          <X className="w-6 h-6" />
        </button>
      </div>
      <form onSubmit={handleAssignAssistant} className="space-y-4">
       {/* In the assign assistant modal */}
<select
  name="doctorId"
  value={assignForm.doctorId}
  onChange={(e) => handleInputChange(e, setAssignForm)}
  className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
  required
>
  <option value="">Select Doctor *</option>
  {Array.isArray(doctors) &&
    doctors.map((doctor) => (
      <option 
        key={doctor.id} 
        value={doctor.id}
        disabled={!doctor.userId} 
      >
        Dr. {doctor.name} {doctor.userId ? "" : "(No User Account)"}
      </option>
    ))}
</select>
        <select
          name="assistantId"
          value={assignForm.assistantId}
          onChange={(e) => handleInputChange(e, setAssignForm)}
          className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
          required
        >
          <option value="">Select Assistant *</option>
          {Array.isArray(assistants) && assistants.length > 0 ? (
            assistants.map((assistant) => (
              <option key={assistant.id} value={assistant.id}>
                {assistant.firstName} {assistant.lastName} ({assistant.email})
              </option>
            ))
          ) : (
            <option value="" disabled>
              No assistants available
            </option>
          )}
        </select>
        <button
          type="submit"
          className="w-full bg-gradient-to-r from-orange-600 to-amber-600 text-white py-4 rounded-xl font-bold hover:shadow-orange-500/50 transition-all"
        >
          Assign Assistant
        </button>
      </form>
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
                Are you sure you want to delete <strong>{deleteTarget.name}</strong>? 
                <br />
                {deleteTarget.type === 'assistant' 
                  ? 'This assistant will be permanently removed from the system.' 
                  : 'This doctor will be permanently removed from the system.'}
                <br />
                This action cannot be undone.
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
                  Delete {deleteTarget.type === 'assistant' ? 'Assistant' : 'Doctor'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ClinicDashboard;