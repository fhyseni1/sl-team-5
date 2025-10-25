"use client";
import ChatInbox from '../components/ChatInbox';
import { MessageCircle } from 'lucide-react';
import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
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
  User,
  AlertTriangle,
  FileText,
  UserPlus,
} from "lucide-react";
import { authService, api } from "../../services/authService";
import { userService } from "../../services/userService";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import AppointmentCalendar from "../components/AppointmentCalendar";
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
  const [userMedications, setUserMedications] = useState([]);
  const [medicationsLoading, setMedicationsLoading] = useState(false);
  const [userAllergies, setUserAllergies] = useState([]);
  const [showAllergyWarning, setShowAllergyWarning] = useState(false);
  const [allergyWarningMedication, setAllergyWarningMedication] = useState("");
  const [acknowledgeWarning, setAcknowledgeWarning] = useState(false);
  const [showChatInbox, setShowChatInbox] = useState(false);
  const [medicationTypes, setMedicationTypes] = useState({
    prescription: [],
    overTheCounter: [],
    selfAdded: []
  });
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
    dosageUnit: "mg",
    schedule: "",
    frequency: "Daily",
    type: "Prescription"
  });

  // Fetch user data, appointments, and medications
  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        setError("");
        console.log("Fetching user data...");

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

        // Load allergies from user profile
        const savedProfile = localStorage.getItem(`userProfile_${userId}`);
        if (savedProfile) {
          const parsedProfile = JSON.parse(savedProfile);
          setUserAllergies(parsedProfile.allergies || []);
          console.log("Loaded allergies:", parsedProfile.allergies);
        }

        const results = await Promise.allSettled([
          userService.getActiveUsersCount(),
          userService.getUpcomingAppointmentsCount(userId),
          api
            .get("/users")
            .then((res) => {
              const healthcareProviders = res.data.filter(
                (user) => user.type === 4
              );
              return healthcareProviders;
            })
            .catch(() => []),
          api.get(`/appointments/user/${userId}`),
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

  useEffect(() => {
    const fetchUserMedications = async () => {
      if (!user?.id) return;
      
      try {
        setMedicationsLoading(true);
        console.log("🟡 Fetching user medications...");
   
        let allMedications = [];
        try {
          const response = await api.get(`/medications/user/${user.id}`);
          allMedications = response.data || [];
          console.log("✅ Medications loaded from backend:", allMedications);
        } catch (backendError) {
          console.log("🔵 Backend not available, using localStorage");
   
          const localMedications = localStorage.getItem(`userMedications_${user.id}`);
          if (localMedications) {
            allMedications = JSON.parse(localMedications);
            console.log("📋 Medications loaded from localStorage:", allMedications);
          }
        }

        const prescriptionMeds = allMedications.filter(med => 
          med.isPrescription || med.requiresPrescription || med.prescribedBy || med.type === 'Prescription'
        );
        const overTheCounterMeds = allMedications.filter(med => 
          med.type === 'OverTheCounter' || med.type === 'OTC' || med.type === 'Over the Counter'
        );
        const selfAddedMeds = allMedications.filter(med => 
          !med.isPrescription && !med.requiresPrescription && 
          !med.prescribedBy && med.type !== 'OverTheCounter' && 
          med.type !== 'OTC' && med.type !== 'Over the Counter' &&
          med.type !== 'Prescription'
        );
        
        setMedicationTypes({
          prescription: prescriptionMeds,
          overTheCounter: overTheCounterMeds,
          selfAdded: selfAddedMeds
        });
        
        setUserMedications(allMedications);
        
      } catch (error) {
        console.error("❌ Error fetching medications:", error);
        setUserMedications([]);
      } finally {
        setMedicationsLoading(false);
      }
    };

    if (user?.id) {
      fetchUserMedications();
    }
  }, [user?.id]);

  // Medication Card Component
  const MedicationCard = ({ medication, onDelete, type }) => {
    const typeColor = getTypeColor(medication.type);
    const isPrescription = type === 'prescription';
    
    return (
      <div
        className={`bg-gradient-to-br ${
          isPrescription 
            ? 'from-purple-500/10 to-pink-500/10 border-purple-500/20' 
            : type === 'otc'
            ? 'from-blue-500/10 to-cyan-500/10 border-blue-500/20'
            : 'from-emerald-500/10 to-teal-500/10 border-emerald-500/20'
        } backdrop-blur-xl rounded-xl p-6 border hover:scale-105 cursor-pointer relative transition-all`}
      >
        {/* Delete Button for self-added medications */}
        {type === 'self' && (
          <button
            onClick={() => onDelete(medication.id)}
            className="absolute top-3 right-3 p-1 bg-red-500/20 hover:bg-red-500/30 rounded-lg transition-colors"
            title="Delete medication"
          >
            <X className="w-4 h-4 text-red-400" />
          </button>
        )}

        {/* Prescription Badge */}
        {isPrescription && (
          <div className="absolute top-3 right-3">
            <Shield className="w-5 h-5 text-purple-400" />
          </div>
        )}

        <div className="flex items-start justify-between mb-3">
          <h4 className="text-white font-semibold text-lg">
            {medication.name}
          </h4>
          <span className={`px-2 py-1 rounded-full text-xs font-medium ${
            isPrescription 
              ? "bg-purple-500/20 text-purple-400"
              : typeColor === "blue" ? "bg-blue-500/20 text-blue-400" :
                typeColor === "purple" ? "bg-purple-500/20 text-purple-400" :
                typeColor === "amber" ? "bg-amber-500/20 text-amber-400" :
                typeColor === "green" ? "bg-green-500/20 text-green-400" :
                "bg-gray-500/20 text-gray-400"
          }`}>
            {isPrescription ? "Prescription" : getTypeText(medication.type)}
          </span>
        </div>
        
        <div className="space-y-2">
          <div className="flex items-center gap-2">
            <Pill className="w-4 h-4 text-emerald-400" />
            <p className="text-slate-300 text-sm">
              {medication.dosage} {getDosageUnitText(medication.dosageUnit)}
            </p>
          </div>
          
          <div className="flex items-center gap-2">
            <Clock className="w-4 h-4 text-blue-400" />
            <p className="text-slate-300 text-sm">
              {getFrequencyText(medication.frequency)}
            </p>
          </div>
          
          {medication.schedule && (
            <div className="flex items-center gap-2">
              <Bell className="w-4 h-4 text-amber-400" />
              <p className="text-slate-300 text-sm">
                {medication.schedule.split(':').slice(0, 2).join(':')}
              </p>
            </div>
          )}
          
          {isPrescription && medication.prescribedBy && (
            <div className="flex items-center gap-2">
              <User className="w-4 h-4 text-purple-400" />
              <p className="text-slate-300 text-sm">
                Prescribed by Dr. {medication.prescribedBy}
              </p>
            </div>
          )}
          
          {medication.instructions && (
            <p className="text-slate-400 text-xs mt-2">
              {medication.instructions}
            </p>
          )}
        </div>
        
        <div className="flex items-center justify-between mt-4 pt-3 border-t border-slate-500/20">
          <span className={`text-sm font-medium ${
            isPrescription ? "text-purple-400" : "text-emerald-400"
          }`}>
            {isPrescription ? "Prescribed" : "Active"}
          </span>
          <div className="flex items-center gap-1">
            <div className={`w-2 h-2 rounded-full animate-pulse ${
              isPrescription ? "bg-purple-400" : "bg-emerald-400"
            }`}></div>
            <span className={`text-xs ${
              isPrescription ? "text-purple-400" : "text-emerald-400"
            }`}>Now</span>
          </div>
        </div>
      </div>
    );
  };

  // Allergy warning check function
  const checkForAllergyWarnings = (medicationName) => {
    if (!medicationName || userAllergies.length === 0) {
      return { hasWarning: false };
    }

    const commonAllergyKeywords = {
      'Penicillin': ['penicillin', 'amoxicillin', 'ampicillin', 'oxacillin', 'amoxil', 'augmentin'],
      'Sulfa': ['sulfa', 'sulfamethoxazole', 'sulfasalazine', 'bactrim', 'septra'],
      'Aspirin': ['aspirin', 'salicylate', 'asa'],
      'Ibuprofen': ['ibuprofen', 'advil', 'motrin', 'nuprin'],
      'Codeine': ['codeine', 'hydrocodone', 'oxycodone', 'vicodin', 'percocet'],
      'Cephalosporins': ['cephalexin', 'ceftriaxone', 'cefuroxime', 'cefdinir']
    };

    const userAllergyNames = userAllergies.map(allergy => allergy.toLowerCase());
    const medNameLower = medicationName.toLowerCase();
    
    for (const [allergyType, medications] of Object.entries(commonAllergyKeywords)) {
      if (userAllergyNames.includes(allergyType.toLowerCase())) {
        for (const medKeyword of medications) {
          if (medNameLower.includes(medKeyword)) {
            return {
              hasWarning: true,
              allergyType: allergyType,
              medicationName: medicationName
            };
          }
        }
      }
    }
    
    return { hasWarning: false };
  };

  // Helper functions for medication display
  const getDosageUnitText = (unitCode) => {
    if (!unitCode) return "mg";
    if (typeof unitCode === 'string') {
      return unitCode;
    }
    
    const units = {
      1: "mg",
      2: "g", 
      3: "ml",
      5: "tablets",
      6: "capsules",
      7: "drops",
      "mg": "mg",
      "g": "g",
      "ml": "ml",
      "tablets": "tablets",
      "capsules": "capsules",
      "drops": "drops"
    };
    return units[unitCode] || "units";
  };

  const getFrequencyText = (frequencyCode) => {
    if (!frequencyCode) return "Once Daily";
    if (typeof frequencyCode === 'string') {
      return frequencyCode;
    }
    
    const frequencies = {
      1: "Once Daily",
      2: "Twice Daily", 
      3: "Three Times Daily",
      4: "Four Times Daily",
      7: "Weekly",
      8: "Monthly",
      6: "As Needed",
      "Daily": "Once Daily",
      "Twice Daily": "Twice Daily",
      "Three Times Daily": "Three Times Daily",
      "Four Times Daily": "Four Times Daily",
      "Weekly": "Weekly",
      "Monthly": "Monthly",
      "As Needed": "As Needed"
    };
    return frequencies[frequencyCode] || "Daily";
  };

  const getTypeText = (typeCode) => {
    if (!typeCode) return "Medication";
    if (typeof typeCode === 'string') {
      return typeCode;
    }
    
    const types = {
      1: "Prescription",
      2: "OTC",
      3: "Supplement", 
      4: "Vitamin",
      5: "Herbal",
      "Prescription": "Prescription",
      "OverTheCounter": "OTC",
      "Over the Counter": "OTC",
      "Supplement": "Supplement",
      "Vitamin": "Vitamin",
      "Herbal": "Herbal"
    };
    return types[typeCode] || "Medication";
  };

  const getTypeColor = (typeCode) => {
    if (!typeCode) return "gray";
    const colors = {
      1: "blue",
      2: "purple", 
      3: "amber",
      4: "green",
      5: "gray",
      "Prescription": "blue",
      "OverTheCounter": "purple",
      "Over the Counter": "purple",
      "Supplement": "amber",
      "Vitamin": "green",
      "Herbal": "gray"
    };
    return colors[typeCode] || "gray";
  };

  // Event handlers
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
        throw new Error("User ID is not available");
      }

      if (!appointmentForm.doctorId) {
        throw new Error("Please select a doctor");
      }

      const appointmentDate = new Date(appointmentForm.appointmentDate);
      if (isNaN(appointmentDate.getTime())) {
         toast.error("Invalid appointment date");
        return;
      }
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
        status: 8,
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
      console.log("🟡 Adding medication...");
      
      // VALIDATION
      if (!medicationForm.name || !medicationForm.dosage || !medicationForm.schedule) {
        alert("❌ Please fill all required fields!");
        return;
      }

      // Final allergy check
      if (showAllergyWarning && !acknowledgeWarning) {
        alert("❌ Please acknowledge the allergy warning before proceeding");
        return;
      }

      const dosageValue = parseFloat(medicationForm.dosage);
      if (isNaN(dosageValue)) {
        alert("❌ Please enter a valid dosage number");
        return;
      }
      
      // Create new medication
      const newMedication = {
        id: "med-" + Date.now(), // Generate unique ID
        name: medicationForm.name.trim(),
        dosage: dosageValue,
        dosageUnit: medicationForm.dosageUnit,
        frequency: medicationForm.frequency,
        schedule: medicationForm.schedule + ":00",
        type: medicationForm.type,
        instructions: `Take ${medicationForm.dosage} ${medicationForm.dosageUnit} ${medicationForm.frequency.toLowerCase()}`,
        status: "Active",
        userId: user.id,
        isPrescription: medicationForm.type === "Prescription",
        requiresPrescription: medicationForm.type === "Prescription",
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      };

      console.log("📋 New medication:", newMedication);

      let savedMedication = newMedication;
      try {
        const response = await api.post("/medications", newMedication);
        if (response.data) {
          savedMedication = { ...newMedication, id: response.data.id || newMedication.id };
          console.log("✅ Medication saved to backend:", savedMedication);
        }
      } catch (backendError) {
        console.log("🔵 Backend save failed, using localStorage only:", backendError);
      }

      // Always save to localStorage
      const updatedMedications = [savedMedication, ...userMedications];
      setUserMedications(updatedMedications);
 
      setMedicationTypes(prev => ({
        ...prev,
        selfAdded: [savedMedication, ...prev.selfAdded]
      }));
      
      // Save to localStorage
      localStorage.setItem(`userMedications_${user.id}`, JSON.stringify(updatedMedications));
      
      // Reset form and close modal
      setShowMedicationForm(false);
      setMedicationForm({
        name: "",
        dosage: "",
        dosageUnit: "mg",
        schedule: "",
        frequency: "Daily",
        type: "Prescription"
      });
      setShowAllergyWarning(false);
      setAcknowledgeWarning(false);
      
      alert(`✅ Medication "${medicationForm.name}" added successfully!`);

    } catch (err) {
      console.error("❌ Error adding medication:", err);
      alert("❌ Technical problem. Please try again.");
    }
  };

  const handleDeleteMedication = async (medicationId) => {
    if (!window.confirm("Are you sure you want to delete this medication?")) {
      return;
    }

    try {
    
      if (!medicationId.startsWith('local-') && !medicationId.startsWith('med-')) {
        try {
          await api.delete(`/medications/${medicationId}`);
          console.log("✅ Medication deleted from backend");
        } catch (deleteError) {
          console.error("❌ Backend delete failed, continuing locally:", deleteError);
        }
      }
   
      const updatedMedications = userMedications.filter(med => med.id !== medicationId);
      setUserMedications(updatedMedications);
      
      setMedicationTypes(prev => ({
        prescription: prev.prescription.filter(med => med.id !== medicationId),
        overTheCounter: prev.overTheCounter.filter(med => med.id !== medicationId),
        selfAdded: prev.selfAdded.filter(med => med.id !== medicationId)
      }));
      
      // Update localStorage
      localStorage.setItem(`userMedications_${user.id}`, JSON.stringify(updatedMedications));
      
      alert("✅ Medication deleted successfully!");
    } catch (error) {
      console.error("❌ Error deleting medication:", error);
      alert("❌ Failed to delete medication");
    }
  };

  const handleMedicationNameChange = (e) => {
    const { name, value } = e.target;
    setMedicationForm(prev => ({ ...prev, [name]: value }));

    if (value.trim().length > 2) {
      const warning = checkForAllergyWarnings(value);
      if (warning.hasWarning) {
        setShowAllergyWarning(true);
        setAllergyWarningMedication(warning.allergyType);
      } else {
        setShowAllergyWarning(false);
        setAcknowledgeWarning(false);
      }
    } else {
      setShowAllergyWarning(false);
      setAcknowledgeWarning(false);
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
            
            {/* Profile and Logout Buttons */}
            <div className="flex items-center space-x-4">
              <button
                onClick={() => router.push("/profile")}
                className="flex items-center space-x-2 px-6 py-3 bg-gradient-to-r from-blue-600 to-indigo-600 text-white rounded-xl font-bold shadow-xl hover:shadow-blue-500/50 transition-all"
              >
                <User className="w-5 h-5" />
                <span>Profile</span>
              </button>

              <button
                onClick={handleLogout}
                className="flex items-center space-x-2 px-6 py-3 bg-gradient-to-r from-red-600 to-red-700 text-white rounded-xl font-bold shadow-xl hover:shadow-red-500/50 transition-all"
              >
                <LogOut className="w-5 h-5" />
                <span>Logout</span>
              </button>
            </div>
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
            <p className="text-3xl font-bold text-white mb-1">
              {userMedications.length}
            </p>
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

        {/* Your Medications Section */}
        <div className="mb-12">
          <div className="bg-slate-800/50 backdrop-blur-xl rounded-2xl p-8 border border-slate-700/50">
            <h3 className="text-2xl font-bold text-white mb-6 flex items-center gap-3">
              <Pill className="w-7 h-7 text-emerald-400" />
              Your Medications
            </h3>
            
            {medicationsLoading ? (
              <div className="flex justify-center py-8">
                <Loader2 className="w-8 h-8 animate-spin text-emerald-400" />
              </div>
            ) : userMedications.length > 0 ? (
              <div className="space-y-8">
                {/* Prescription Medications */}
                {medicationTypes.prescription.length > 0 && (
                  <div>
                    <h4 className="text-lg font-semibold text-purple-400 mb-4 flex items-center gap-2">
                      <FileText className="w-5 h-5" />
                      Prescription Medications ({medicationTypes.prescription.length})
                    </h4>
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                      {medicationTypes.prescription.map((medication) => (
                        <MedicationCard 
                          key={medication.id} 
                          medication={medication} 
                          onDelete={handleDeleteMedication}
                          type="prescription"
                        />
                      ))}
                    </div>
                  </div>
                )}
                
                {/* Over-the-Counter Medications */}
                {medicationTypes.overTheCounter.length > 0 && (
                  <div>
                    <h4 className="text-lg font-semibold text-blue-400 mb-4 flex items-center gap-2">
                      <Pill className="w-5 h-5" />
                      Over-the-Counter ({medicationTypes.overTheCounter.length})
                    </h4>
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                      {medicationTypes.overTheCounter.map((medication) => (
                        <MedicationCard 
                          key={medication.id} 
                          medication={medication} 
                          onDelete={handleDeleteMedication}
                          type="otc"
                        />
                      ))}
                    </div>
                  </div>
                )}
                
                {/* Self-Added Medications */}
                {medicationTypes.selfAdded.length > 0 && (
                  <div>
                    <h4 className="text-lg font-semibold text-amber-400 mb-4 flex items-center gap-2">
                      <UserPlus className="w-5 h-5" />
                      Self-Added Medications ({medicationTypes.selfAdded.length})
                    </h4>
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                      {medicationTypes.selfAdded.map((medication) => (
                        <MedicationCard 
                          key={medication.id} 
                          medication={medication} 
                          onDelete={handleDeleteMedication}
                          type="self"
                        />
                      ))}
                    </div>
                  </div>
                )}
              </div>
            ) : (
              <div className="text-center py-8">
                <Pill className="w-16 h-16 text-slate-600 mx-auto mb-4" />
                <p className="text-slate-400 text-lg mb-2">No medications found</p>
                <p className="text-slate-500 text-sm">
                  Add your first medication to get started with tracking
                </p>
                <button
                  onClick={() => setShowMedicationForm(true)}
                  className="mt-4 px-6 py-2 bg-gradient-to-r from-emerald-600 to-teal-600 text-white rounded-lg font-semibold hover:shadow-emerald-500/50 transition-all"
                >
                  Add First Medication
                </button>
              </div>
            )}
          </div>
        </div>

        {/* Your Appointments Section */}
        <div className="mb-12">
          <div className="bg-slate-800/50 backdrop-blur-xl rounded-2xl p-8 border border-slate-700/50">
            <h3 className="text-2xl font-bold text-white mb-6 flex items-center gap-3">
              <Calendar className="w-7 h-7 text-blue-400" />
              Your Appointments
            </h3>
                    <AppointmentCalendar
          appointments={userAppointments}
          doctors={doctors}
        />
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
                            ).toLocaleDateString()}
                            , {appointment.startTime} - {appointment.endTime}
                          </p>
                        </div>
                      </div>
                      <div>
                        <div className="flex flex-col items-end gap-1">
                          <span
                            className={`px-3 py-1 rounded-full text-sm font-medium ${
                              String(appointment.status || "").toLowerCase() ===
                              "approved"
                                ? "bg-emerald-500/20 text-emerald-400"
                                : String(
                                    appointment.status || ""
                                  ).toLowerCase() === "pending"
                                ? "bg-amber-500/20 text-amber-400"
                                : "bg-red-500/20 text-red-400"
                            }`}
                          >
                            {appointment.status || "Unknown"}
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

        {/* Quick Actions Section */}
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
                <button
  onClick={() => setShowChatInbox(true)}
  className="w-full flex items-center space-x-3 p-5 bg-gradient-to-r from-purple-600 to-pink-600 text-white rounded-xl font-semibold hover:shadow-purple-500/50 transition-all hover:scale-105 group"
>
  <MessageCircle className="w-6 h-6 group-hover:scale-110 transition-transform" />
  <span>Messages</span>
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
{showChatInbox && (
  <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
    <div className="bg-slate-800/95 backdrop-blur-xl rounded-3xl w-full max-w-6xl h-[90vh] overflow-hidden border border-slate-700/50 flex flex-col">
      {/* Header i përmirësuar */}
      <div className="flex items-center justify-between p-6 border-b border-slate-700/50 bg-slate-900/80">
        <div className="flex items-center gap-3">
          <MessageCircle className="w-8 h-8 text-purple-400" />
          <div>
            <h2 className="text-2xl font-bold text-white">Doctor Messages</h2>
            <p className="text-slate-400 text-sm">Chat with your healthcare providers</p>
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
        <ChatInbox currentUser={user} isDoctorView={false} />
      </div>
    </div>
  </div>
)}
      {/* Medication Form Modal */}
      {showMedicationForm && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div className="bg-slate-800/95 backdrop-blur-xl rounded-3xl max-w-md w-full p-8 border border-slate-700/50">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-2xl font-bold text-white">Add Medication</h2>
              <button
                onClick={() => {
                  setShowMedicationForm(false);
                  setShowAllergyWarning(false);
                  setAcknowledgeWarning(false);
                }}
                className="p-2 hover:bg-slate-700 rounded-xl"
              >
                <X className="w-6 h-6" />
              </button>
            </div>

            {/* ALLERGY WARNING */}
            {showAllergyWarning && (
              <div className="mb-6 p-4 bg-red-500/20 border border-red-500/50 rounded-xl">
                <div className="flex items-center gap-3 mb-2">
                  <AlertTriangle className="w-5 h-5 text-red-400" />
                  <span className="text-red-400 font-semibold">ALLERGY ALERT</span>
                </div>
                <p className="text-red-300 text-sm">
                  You are allergic to <strong>{allergyWarningMedication}</strong>. 
                  This medication may contain ingredients you're allergic to.
                </p>
                <div className="flex items-center gap-2 mt-3">
                  <input 
                    type="checkbox" 
                    id="acknowledgeWarning"
                    checked={acknowledgeWarning}
                    onChange={(e) => setAcknowledgeWarning(e.target.checked)}
                    className="w-4 h-4 text-red-600 bg-slate-700 border-slate-600 rounded focus:ring-red-500"
                  />
                  <label htmlFor="acknowledgeWarning" className="text-red-300 text-sm">
                    I understand the risks and want to proceed
                  </label>
                </div>
              </div>
            )}

            <form onSubmit={handleMedicationSubmit} className="space-y-4">
              <div>
                <input
                  name="name"
                  placeholder="Medication Name *"
                  value={medicationForm.name}
                  onChange={handleMedicationNameChange}
                  className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
                  required
                />
                {showAllergyWarning && (
                  <p className="text-amber-400 text-xs mt-2 flex items-center gap-1">
                    <AlertTriangle className="w-3 h-3" />
                    Warning: Potential allergy conflict detected
                  </p>
                )}
              </div>
              
              <div className="grid grid-cols-2 gap-4">
                <input
                  name="dosage"
                  type="number"
                  step="0.1"
                  placeholder="Dosage *"
                  value={medicationForm.dosage}
                  onChange={(e) => handleInputChange(e, setMedicationForm)}
                  className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
                  required
                />
                <select
                  name="dosageUnit"
                  value={medicationForm.dosageUnit}
                  onChange={(e) => handleInputChange(e, setMedicationForm)}
                  className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
                >
                  <option value="mg">mg</option>
                  <option value="g">g</option>
                  <option value="ml">ml</option>
                  <option value="tablets">tablets</option>
                  <option value="capsules">capsules</option>
                  <option value="drops">drops</option>
                </select>
              </div>

              <select
                name="frequency"
                value={medicationForm.frequency}
                onChange={(e) => handleInputChange(e, setMedicationForm)}
                className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
              >
                <option value="Daily">Once Daily</option>
                <option value="Twice Daily">Twice Daily</option>
                <option value="Three Times Daily">Three Times Daily</option>
                <option value="Four Times Daily">Four Times Daily</option>
                <option value="Weekly">Weekly</option>
                <option value="Monthly">Monthly</option>
                <option value="As Needed">As Needed</option>
              </select>

              <input
                name="schedule"
                type="time"
                value={medicationForm.schedule}
                onChange={(e) => handleInputChange(e, setMedicationForm)}
                className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
                required
              />

              <select
                name="type"
                value={medicationForm.type}
                onChange={(e) => handleInputChange(e, setMedicationForm)}
                className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
              >
                <option value="Prescription">Prescription</option>
                <option value="Over the Counter">Over the Counter</option>
                <option value="Supplement">Supplement</option>
                <option value="Vitamin">Vitamin</option>
                <option value="Herbal">Herbal</option>
              </select>

              <button
                type="submit"
                disabled={showAllergyWarning && !acknowledgeWarning}
                className="w-full bg-gradient-to-r from-emerald-600 to-teal-600 text-white py-4 rounded-xl font-bold hover:shadow-emerald-500/50 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {showAllergyWarning ? "Add Medication Anyway" : "Add Medication"}
              </button>
            </form>
          </div>
        </div>
      )}

      {/* Appointment Form Modal */}
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