// client/components/DoctorDashboard.jsx
"use client";
import ChatInbox from "../ChatInbox";
import NotificationCenter from "../NotificationCenter";
import { MessageCircle } from "lucide-react";
import ReportCard from "./ReportCard";
import ReportModal from "./ReportModal";
import { allergyService } from "../../../services/allergyService";
import {
  generateReportPDFTemplate,
  downloadHtmlAsPdf,
} from "./ReportPDFTemplate";
import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import {
  Pill,
  Calendar,
  Users,
  Eye,
  AlertTriangle,
  Clock,
  CheckCircle2,
  XCircle,
  LogOut,
  Loader2,
  Stethoscope,
  Plus,
  X,
  FileText,
  Download,
  Edit,
  Trash2,
  Activity,
  BarChart3,
} from "lucide-react";
import { authService } from "../../../services/authService";
import PatientAnalytics from "./PatientAnalytics";
import ReportCardMini from "./ReportCardMini";

// === API CLIENTS ===
const USERHEALTH_API_URL =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:5029/api";
const MEDICATION_API_URL =
  process.env.NEXT_PUBLIC_MEDICATION_API_URL || "http://localhost:5077/api";

// UserHealth API (Appointments, Reports, Auth)
const userHealthApi = {
  get: (endpoint) =>
    fetch(`${USERHEALTH_API_URL}${endpoint}`, {
      credentials: "include",
    }).then((r) => (r.ok ? r.json() : Promise.reject(r))),
  post: (endpoint, body) =>
    fetch(`${USERHEALTH_API_URL}${endpoint}`, {
      method: "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    }).then((r) => (r.ok ? r.json() : Promise.reject(r))),
  put: (endpoint, body) =>
    fetch(`${USERHEALTH_API_URL}${endpoint}`, {
      method: "PUT",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    }).then((r) => (r.ok ? r.json() : Promise.reject(r))),
  delete: (endpoint) =>
    fetch(`${USERHEALTH_API_URL}${endpoint}`, {
      method: "DELETE",
      credentials: "include",
    }).then((r) => (r.ok ? r : Promise.reject(r))),
};

// Medication API (Medications only)
const medicationApi = {
  get: (endpoint) =>
    fetch(`${MEDICATION_API_URL}${endpoint}`, {
      credentials: "include",
      headers: { "Content-Type": "application/json" },
    }).then((r) => (r.ok ? r.json() : Promise.reject(r))),
  post: (endpoint, body) =>
    fetch(`${MEDICATION_API_URL}${endpoint}`, {
      method: "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    }).then((r) => (r.ok ? r.json() : Promise.reject(r))),
};

// === ENUMS (same as backend) ===
const MedicationType = {
  Prescription: 1,
  OverTheCounter: 2,
  Supplement: 3,
  Vitamin: 4,
  Herbal: 5,
  Homeopathic: 6,
};

const DosageUnit = {
  Milligrams: 1,
  Grams: 2,
  Milliliters: 3,
  Liters: 4,
  Tablets: 5,
  Capsules: 6,
  Drops: 7,
  Puffs: 8,
  Patches: 9,
  Units: 10,
};

// FrequencyType enum matching backend
const FrequencyType = {
  OnceDaily: 1,
  TwiceDaily: 2,
  ThreeTimesDaily: 3,
  FourTimesDaily: 4,
  EveryFewHours: 5,
  AsNeeded: 6,
  Weekly: 7,
  Monthly: 8,
  Custom: 9,
};

// Map string frequency values to enum
const mapFrequencyToEnum = (frequencyString) => {
  const frequencyMap = {
    Daily: FrequencyType.OnceDaily,
    "Twice daily": FrequencyType.TwiceDaily,
    "Three times daily": FrequencyType.ThreeTimesDaily,
    "Four times daily": FrequencyType.FourTimesDaily,
    "Every few hours": FrequencyType.EveryFewHours,
    "As needed": FrequencyType.AsNeeded,
    Weekly: FrequencyType.Weekly,
    Monthly: FrequencyType.Monthly,
    Custom: FrequencyType.Custom,
  };
  return frequencyMap[frequencyString] || null;
};

const DoctorDashboard = () => {
  const [selectedPatient, setSelectedPatient] = useState(null);
  const [showPatientSelection, setShowPatientSelection] = useState(false);
  const [user, setUser] = useState(null);
  const [patientAllergies, setPatientAllergies] = useState([]);
  const [loading, setLoading] = useState(true);
  const [patients, setPatients] = useState([]);
  const [showChatInbox, setShowChatInbox] = useState(false);
  const [reports, setReports] = useState([]);
  const [showReportModal, setShowReportModal] = useState(false);
  const [editingReport, setEditingReport] = useState(null);
  const [selectedAppointment, setSelectedAppointment] = useState(null);
  const [showPatientsModal, setShowPatientsModal] = useState(false);
  const [appointments, setAppointments] = useState([]);
  const [medications, setMedications] = useState([]);
  const [showAddMedicationForm, setShowAddMedicationForm] = useState(false);
  const [showAnalytics, setShowAnalytics] = useState(false);
  const [selectedPatientForAnalytics, setSelectedPatientForAnalytics] =
    useState(null);
  const [stats, setStats] = useState({
    totalAppointments: 0,
    todayAppointments: 0,
    pendingAppointments: 0,
    totalPatients: 0,
  });

  const router = useRouter();

  const [medicationForm, setMedicationForm] = useState({
    name: "",
    genericName: "",
    dosage: "",
    dosageUnit: DosageUnit.Milligrams,
    instructions: "",
    frequency: "Daily",
    customFrequencyHours: null,
    daysOfWeek: "",
    monthlyDay: null,
    duration: "",
    description: "",
    medicationType: MedicationType.Prescription,
    pharmacyName: "Main Pharmacy",
    remainingRefills: 3,
    prescriptionNotes: "",
  });

  // === INITIAL LOAD ===
  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const userData = await authService.getMe();
        if (!userData) {
          router.push("/login");
          return;
        }

        const isDoctor = userData.type === 4;
        const isAssistant = userData.type === 7;
        if (!isDoctor && !isAssistant) {
          toast.error("Access denied");
          router.push("/dashboard");
          return;
        }

        setUser(userData);
        await Promise.all([
          loadAppointments(userData, isDoctor),
          loadMedications(userData),
          loadReports(userData),
        ]);
      } catch (error) {
        toast.error("Failed to load dashboard data");
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [router]);
useEffect(() => {
  if (selectedPatient) {
    fetchPatientAllergies(selectedPatient.id);
  } else {
    setPatientAllergies([]);
  }
}, [selectedPatient]);
  // === LOAD APPOINTMENTS (UserHealthService) ===
  const loadAppointments = async (userData, isDoctor) => {
    try {
      let filteredAppointments = [];
      if (isDoctor) {
        const response = await userHealthApi.get("/appointments");
        const allAppointments = response || [];
        const doctorFullName = `Dr. ${userData.firstName} ${userData.lastName}`;
        filteredAppointments = allAppointments.filter(
          (app) =>
            app.doctorName === doctorFullName &&
            (app.status === 2 || app.status === "Approved")
        );
      } else {
        const pendingResponse = await userHealthApi.get(
          `/appointments/assistant/${userData.id}/pending`
        );
        filteredAppointments = pendingResponse || [];
      }

      setAppointments(filteredAppointments);

      const approvedApps = filteredAppointments.filter(
        (app) => app.status === 2 || app.status === "Approved"
      );
      const today = new Date().toISOString().split("T")[0];
      const todayApps = approvedApps.filter(
        (app) =>
          new Date(app.appointmentDate).toISOString().split("T")[0] === today
      );
      const pendingApps = filteredAppointments.filter(
        (app) => app.status === 8 || app.status === "Pending"
      );
      const uniquePatients = [
        ...new Set(approvedApps.map((app) => app.userId)),
      ];

      setStats({
        totalAppointments: approvedApps.length,
        todayAppointments: todayApps.length,
        pendingAppointments: pendingApps.length,
        totalPatients: uniquePatients.length,
      });
    } catch (error) {
      toast.error("Failed to load appointments");
    }
  };

  const loadReports = async (userData) => {
    try {
      // Use ONLY the main endpoint - never the doctor-specific one
      const response = await userHealthApi.get("/appointmentreports");
      const allReports = response || [];

      // Filter on client side for doctors
      const filtered =
        userData.type === 4
          ? allReports.filter((report) => report.doctorId === userData.id)
          : allReports;

      setReports(filtered);
    } catch (error) {
      console.error("Failed to load reports:", error);
      toast.error("Failed to load reports");
      setReports([]);
    }
  };
  const handleAddMedicationSubmit = async (e) => {
    e.preventDefault();
    if (!selectedPatient) {
      toast.error("Please select a patient first");
      return;
    }

    try {
      console.log("🔄 Starting medication creation for doctor:", user.id);
      console.log("👤 Selected patient:", selectedPatient);

      if (!medicationForm.name || !medicationForm.dosage) {
        toast.error("Please fill all required fields");
        return;
      }

      const mappedFrequency = mapFrequencyToEnum(medicationForm.frequency);

      const medicationData = {
        userId: selectedPatient.id,
        doctorId: user.id,
        prescribedBy: `Dr. ${user.firstName} ${user.lastName}`,
        name: medicationForm.name.trim(),
        genericName: (medicationForm.genericName || medicationForm.name).trim(),
        manufacturer: "Unknown Manufacturer",
        type: parseInt(medicationForm.medicationType),
        dosage: parseFloat(medicationForm.dosage) || 0,
        dosageUnit: parseInt(medicationForm.dosageUnit),
        description: medicationForm.description || medicationForm.instructions,
        instructions: medicationForm.instructions,
        status: 1, // Active
        startDate: new Date().toISOString(),
        endDate: medicationForm.duration
          ? new Date(
              Date.now() + parseInt(medicationForm.duration) * 86400000
            ).toISOString()
          : null,
        frequency: mappedFrequency,
        customFrequencyHours:
          mappedFrequency === FrequencyType.EveryFewHours ||
          mappedFrequency === FrequencyType.Custom
            ? parseInt(medicationForm.customFrequencyHours || "0", 10) || null
            : null,
        daysOfWeek:
          mappedFrequency === FrequencyType.Weekly
            ? medicationForm.daysOfWeek || "Monday"
            : mappedFrequency === FrequencyType.Custom
            ? medicationForm.daysOfWeek || ""
            : null,
        monthlyDay:
          mappedFrequency === FrequencyType.Monthly &&
          medicationForm.monthlyDay
            ? parseInt(medicationForm.monthlyDay, 10)
            : null,
      };

      console.log("📦 Sending medication data:", medicationData);

      const createdMedication = await medicationApi.post(
        "/medications",
        medicationData
      );
      console.log("✅ Medication created:", createdMedication);

      console.log("🔍 Checking if doctor fields were saved:", {
        doctorIdInResponse: createdMedication.doctorId,
        prescribedByInResponse: createdMedication.prescribedBy,
        fullResponse: createdMedication,
      });

      try {
        const prescriptionData = {
          medicationId: createdMedication.id,
          prescriptionNumber: `RX-${Date.now()}-${Math.random()
            .toString(36)
            .substr(2, 5)
            .toUpperCase()}`,
          prescriberName: `Dr. ${user.firstName} ${user.lastName}`,
          prescriberContact: "",
          pharmacyName: medicationForm.pharmacyName || "Not specified",
          pharmacyContact: "",
          issueDate: new Date().toISOString(),
          expiryDate: new Date(Date.now() + 30 * 86400000).toISOString(),
          status: 1,
          notes:
            medicationForm.prescriptionNotes ||
            `Prescribed for ${selectedPatient.firstName} ${selectedPatient.lastName}`,
          remainingRefills: parseInt(medicationForm.remainingRefills) || 0,
        };

        console.log("📋 Sending prescription data:", prescriptionData);
        const createdPrescription = await medicationApi.post(
          "/prescriptions",
          prescriptionData
        );
        console.log("✅ Prescription created:", createdPrescription);
      } catch (prescriptionError) {
        console.log(
          "⚠️ Could not create prescription, but medication was saved:",
          prescriptionError
        );
      }

      await loadMedications(user);

      toast.success(
        `Medication "${medicationForm.name}" created for ${selectedPatient.firstName}!`
      );
    } catch (err) {
      console.error("❌ Error creating medication:", err);

      if (err.response) {
        try {
          const errorData = await err.response.json();
          console.error("Error details:", errorData);
          toast.error(
            `Failed to add medication: ${errorData.message || "Unknown error"}`
          );
        } catch {
          toast.error("Failed to add medication: Server error");
        }
      } else {
        toast.error("Failed to add medication to database");
      }
    } finally {
      setShowAddMedicationForm(false);
      setMedicationForm({
        name: "",
        genericName: "",
        dosage: "",
        dosageUnit: 1,
        instructions: "",
        frequency: "Daily",
        duration: "",
        description: "",
        medicationType: 1,
        pharmacyName: "",
        remainingRefills: 0,
        prescriptionNotes: "",
      });
      setSelectedPatient(null);
    }
  };
  const loadMedications = async (userData) => {
    try {
      console.log("🔄 Loading medications for doctor:", userData.id);

      const allMeds = await medicationApi.get("/medications");
      console.log("📦 ALL MEDICATIONS FROM DATABASE:", allMeds);

      if (!allMeds || allMeds.length === 0) {
        console.log("❌ No medications found in database");
        setMedications([]);
        return;
      }

      const doctorFullName = `Dr. ${userData.firstName} ${userData.lastName}`;
      console.log("🔍 Looking for medications with:", {
        doctorId: userData.id,
        doctorFullName: doctorFullName,
      });

      const filteredMeds = allMeds.filter((med) => {
        console.log("💊 Checking medication:", {
          id: med.id,
          name: med.name,
          doctorId: med.doctorId,
          prescribedBy: med.prescribedBy,
          userId: med.userId,
        });

        const matchesDoctorId = med.doctorId === userData.id;
        const matchesDoctorName = med.prescribedBy === doctorFullName;
        const matches = matchesDoctorId || matchesDoctorName;

        if (matches) {
          console.log("✅ MATCH FOUND for doctor!");
        }

        return matches;
      });

      console.log("🎯 FILTERED MEDICATIONS FOR DOCTOR:", filteredMeds);
      setMedications(filteredMeds);
    } catch (error) {
      console.error("❌ MedicationService error:", error);
      toast.error("Failed to load medications from database");
      setMedications([]);
    }
  };
const handleMedicationNameChangeForDoctor = async (e) => {
  const { name, value } = e.target;
  setMedicationForm((prev) => ({ ...prev, [name]: value }));

  if (value.trim().length > 2 && selectedPatient) {
    try {
      console.log("🟡 Doctor checking allergy conflicts for patient:", selectedPatient.id, value);
      const result = await allergyService.checkAllergyConflicts(selectedPatient.id, value);
      
      if (result.hasConflicts) {
        console.log("🔴 Allergy conflict detected for patient");
        toast.warning(`Allergy conflict detected for ${selectedPatient.firstName}!`, {
          autoClose: 10000,
        });
      }
    } catch (error) {
      console.error("❌ Error checking patient allergies:", error);
    }
  }
};
const fetchPatientAllergies = async (patientId) => {
  try {
    console.log("🟡 Fetching patient allergies from API:", patientId);
    const allergies = await allergyService.getUserAllergies(patientId);
    console.log("✅ Patient allergies:", allergies);
    setPatientAllergies(allergies);
  } catch (error) {
    console.error("❌ Error fetching patient allergies:", error);
    setPatientAllergies([]);
  }
};

  const openPatientAnalytics = (patient) => {
    setSelectedPatientForAnalytics(patient);
    setShowAnalytics(true);
  };

  const handleCreateReport = async (appointment) => {
    if (!appointment) {
      toast.error("No appointment selected");
      return;
    }
    if (appointment.status !== 2 && appointment.status !== "Approved") {
      toast.error("You can only create reports for approved appointments");
      return;
    }

    const safeAppointment = {
      id: appointment.id,
      userId: appointment.userId,
      doctorId: user.id,
      userName: `${appointment.userFirstName || ""} ${
        appointment.userLastName || ""
      }`.trim(),
      doctorName:
        appointment.doctorName || `Dr. ${user.firstName} ${user.lastName}`,
      appointmentDate: appointment.appointmentDate,
      purpose: appointment.purpose || "No purpose specified",
      status: appointment.status,
      specialty: appointment.specialty || "General Medicine",
    };

    setSelectedAppointment(safeAppointment);
    setEditingReport(null);
    setShowReportModal(true);
  };

  const handleSubmitReport = async (formData) => {
    try {
      const doctorId = user.id;

      const reportData = {
        appointmentId: selectedAppointment.id.toString(),
        userId: selectedAppointment.userId.toString(),
        doctorId: doctorId.toString(),
        reportDate: new Date().toISOString(),
        diagnosis: formData.diagnosis?.trim() || "",
        symptoms: formData.symptoms?.trim() || "",
        treatment: formData.treatment?.trim() || "",
        medications: formData.medications?.trim() || "",
        notes: formData.notes?.trim() || "",
        recommendations: formData.recommendations?.trim() || "",
      };

      let response;
      if (editingReport) {
        response = await fetch(
          `http://localhost:5029/api/appointmentreports/${editingReport.id}`,
          {
            method: "PUT",
            credentials: "include",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
              ...reportData,
              id: editingReport.id,
            }),
          }
        );
      } else {
        response = await fetch("http://localhost:5029/api/appointmentreports", {
          method: "POST",
          credentials: "include",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(reportData),
        });
      }

      console.log("Response status:", response.status);

      if (!response.ok) {
        const errorText = await response.text();
        console.error("Server error response:", errorText);
        throw new Error(`HTTP ${response.status}: ${errorText}`);
      }

      const result = await response.json();
      console.log("Server success response:", result);

      await loadReports(user);
      setShowReportModal(false);
      setEditingReport(null);
      setSelectedAppointment(null);
      toast.success(editingReport ? "Report updated!" : "Report created!");
    } catch (error) {
      console.error("Full error details:", error);
      toast.error(`Failed to save report: ${error.message}`);
    }
  };
  const handleEditReport = (report) => {
    console.log("Editing report:", report);
    setEditingReport(report);

    setSelectedAppointment({
      id: report.appointmentId,
      userId: report.userId,
      userFirstName: report.userName?.split(" ")[0] || "Patient",
      userLastName: report.userName?.split(" ")[1] || "",
      doctorName: report.doctorName,
      appointmentDate: report.reportDate,
      purpose: "Follow-up consultation",
      status: 2,
    });

    setShowReportModal(true);
  };

  const handleDeleteReport = async (reportId) => {
    try {
      if (!window.confirm("Are you sure you want to delete this report?"))
        return;

      console.log("🔄 Temporary delete workaround for report:", reportId);

      const response = await fetch(
        `${USERHEALTH_API_URL}/appointmentreports/${reportId}`,
        {
          method: "DELETE",
          credentials: "include",
        }
      );

      if (response.ok) {
        setReports((prev) => prev.filter((report) => report.id !== reportId));
        toast.success("Report deleted successfully!");
        return;
      }

      // If direct delete fails, try alternative approach
      console.log("Direct delete failed, trying alternative...");

      // Alternative: Use update to "soft delete" by clearing content
      const emptyReportData = {
        id: reportId,
        diagnosis: "[DELETED]",
        symptoms: "",
        treatment: "",
        medications: "",
        notes: "",
        recommendations: "",
      };

      try {
        const updateResponse = await userHealthApi.put(
          `/appointmentreports/${reportId}`,
          emptyReportData
        );
        if (updateResponse) {
          setReports((prev) => prev.filter((report) => report.id !== reportId));
          toast.success("Report cleared successfully!");
        }
      } catch (updateError) {
        console.error("Alternative also failed:", updateError);
        toast.error("Cannot delete report due to permission issues");
      }
    } catch (error) {
      console.error("Delete error:", error);
      toast.error("Error deleting report");
    }
  };

  const handleDownloadPdf = async (reportId) => {
    if (!reportId) {
      toast.error("Report ID missing – cannot download");
      return;
    }

    try {
      console.log("Starting PDF download for report:", reportId);

      // First, get the report data using the DTO endpoint
      const report = await userHealthApi.get(`/appointmentreports/${reportId}`);
      console.log("Report data received:", report);

      if (!report) {
        toast.error("Report data not found");
        return;
      }

      // Use your existing template
      const htmlContent = generateReportPDFTemplate(report);

      // Use your existing download function
      await downloadHtmlAsPdf(htmlContent, `medical-report-${reportId}.pdf`);

      toast.success(
        "PDF generated successfully! Use the print dialog to save as PDF."
      );
    } catch (error) {
      console.error("PDF download failed:", error);

      // Fallback: Show the report in a new window for manual printing
      try {
        const report = await userHealthApi.get(
          `/appointmentreports/${reportId}`
        );
        const htmlContent = generateReportPDFTemplate(report);

        const printWindow = window.open("", "_blank");
        printWindow.document.write(htmlContent);
        printWindow.document.close();

        toast.info(
          "Report opened in new window. Use browser print to save as PDF."
        );
      } catch (fallbackError) {
        console.error("Fallback also failed:", fallbackError);
        toast.error(
          "Failed to generate PDF. Please try the view option and use browser print."
        );
      }
    }
  };

  const viewReportQuick = (report) => {
    const reportDetails = `
      Detailed Report
      ================

      Patient: ${report.userName}
      Doctor: ${report.doctorName}
      DATE: ${new Date(report.reportDate).toLocaleDateString("sq-AL")}

      Diagnosis:
      ${report.diagnosis || "No Diagnosis"}

      Symptoms:
      ${report.symptoms || "No Symptoms"}

      Treatment:
      ${report.treatment || "No Treatment"}

      Medications:
      ${report.medications || "No Medications"}

      Notes:
      ${report.notes || "No Notes"}

      Recommendations:
      ${report.recommendations || "No Recommendations"}
    `;

    const modal = window.open("", "Report", "width=600,height=700");
    modal.document.write(`
      <html>
        <head><title>Medical Report</title></head>
        <body style="font-family: Arial; padding: 20px;">
          <h2 style="color: #2c5aa0;">Medical Report</h2>
          <pre style="white-space: pre-wrap; font-size: 14px;">${reportDetails}</pre>
          <button onclick="window.print()" style="margin-top: 20px; padding: 10px; background: #2c5aa0; color: white; border: none; border-radius: 5px;">Print</button>
        </body>
      </html>
    `);
    modal.document.close();
  };

  // === APPROVE / REJECT (Assistant) ===
  const handleApproveAppointment = async (appointmentId) => {
    if (user?.type === 7) {
      try {
        await userHealthApi.put(
          `/appointments/${appointmentId}/assistant-approve`
        );
        setAppointments((prev) =>
          prev.filter((app) => app.id !== appointmentId)
        );
        toast.success("Appointment approved successfully!");
      } catch (error) {
        toast.error("Failed to approve appointment");
      }
    }
  };

  const handleRejectAppointment = async (appointmentId) => {
    if (user?.type === 7) {
      const reason = prompt("Please provide a reason for rejection:");
      if (reason) {
        try {
          await userHealthApi.put(
            `/appointments/${appointmentId}/assistant-reject`,
            {
              rejectionReason: reason,
            }
          );
          setAppointments((prev) =>
            prev.filter((app) => app.id !== appointmentId)
          );
          toast.success("Appointment rejected successfully!");
        } catch (error) {
          toast.error("Failed to reject appointment");
        }
      }
    }
  };

  // === FETCH PATIENTS ===
  const fetchDoctorPatients = async () => {
    try {
      const patientsRes = await userHealthApi.get(
        `/users/doctor/${user.id}/patients`
      );
      setPatients(patientsRes || []);
    } catch {
      const approvedAppointments = appointments.filter(
        (app) => app.status === 2 || app.status === "Approved"
      );
      const uniquePatientMap = new Map();
      approvedAppointments.forEach((app) => {
        if (app.userId && !uniquePatientMap.has(app.userId)) {
          uniquePatientMap.set(app.userId, {
            id: app.userId,
            firstName: app.userFirstName || "Patient",
            lastName: app.userLastName || "",
            email:
              app.userEmail || `patient${uniquePatientMap.size + 1}@email.com`,
            phoneNumber: app.userPhone || "",
            lastAppointment: app.appointmentDate || new Date().toISOString(),
          });
        }
      });
      setPatients(Array.from(uniquePatientMap.values()));
    }
  };

  // === INPUT CHANGE ===
  const handleInputChange = (e, setter) => {
    const { name, value } = e.target;
    setter((prev) => ({ ...prev, [name]: value }));
  };

  // === LOGOUT ===
  const handleLogout = async () => {
    await authService.logout();
    router.push("/");
  };

  // === RENDER ===
  if (loading)
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Loader2 className="w-16 h-16 animate-spin text-blue-500" />
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
      <ToastContainer position="top-right" autoClose={5000} theme="dark" />
      <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_right,_var(--tw-gradient-stops))] from-blue-900/20 via-transparent to-emerald-900/20"></div>

      {/* HEADER */}
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
                <p className="text-slate-400">
                  Dr. {user.firstName} {user.lastName}
                </p>
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
          <p className="text-slate-400 text-lg">
            Manage your appointments and prescriptions
          </p>
        </div>

        {/* STATS */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-12">
          {[
            {
              label: "Total Appointments",
              value: stats.totalAppointments,
              icon: Calendar,
              color: "from-blue-500 to-cyan-500",
            },
            {
              label: "Today's Appointments",
              value: stats.todayAppointments,
              icon: Clock,
              color: "from-emerald-500 to-teal-500",
            },
            {
              label:
                user?.type === 4 ? "Pending Appointments" : "Pending Approval",
              value: stats.pendingAppointments,
              icon: user?.type === 4 ? Clock : Activity,
              color: "from-amber-500 to-orange-500",
            },
            {
              label: "Total Patients",
              value: stats.totalPatients,
              icon: Users,
              color: "from-purple-500 to-pink-500",
            },
          ].map((stat, i) => (
            <div
              key={i}
              className="bg-slate-800/50 backdrop-blur-xl rounded-2xl p-8 border border-slate-700/50 shadow-xl"
            >
              <stat.icon className="w-12 h-12 text-white mx-auto mb-4" />
              <p className="text-3xl font-bold text-white mb-1">{stat.value}</p>
              <p className="text-slate-300 font-semibold">{stat.label}</p>
            </div>
          ))}
        </div>

        {/* APPOINTMENTS & MEDICATIONS */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-12">
          {/* Appointments */}
          <div className="bg-slate-800/50 backdrop-blur-xl rounded-3xl border border-slate-700/50 p-8">
            <div className="flex items-center justify-between mb-8">
              <h2 className="text-2xl font-bold text-white flex items-center gap-3">
                <Calendar className="w-7 h-7 text-blue-400" />
                {user?.type === 4
                  ? "My Approved Appointments"
                  : "Pending Appointments"}
                (
                {user?.type === 4
                  ? appointments.filter(
                      (app) => app.status === 2 || app.status === "Approved"
                    ).length
                  : appointments.filter(
                      (app) => app.status === 8 || app.status === "Pending"
                    ).length}
                )
              </h2>
            </div>
            <div className="space-y-4 max-h-96 overflow-y-auto">
              {appointments.filter((app) =>
                user?.type === 4
                  ? app.status === 2 || app.status === "Approved"
                  : app.status === 8 || app.status === "Pending"
              ).length > 0 ? (
                appointments
                  .filter((app) =>
                    user?.type === 4
                      ? app.status === 2 || app.status === "Approved"
                      : app.status === 8 || app.status === "Pending"
                  )
                  .map((appointment) => (
                    <div
                      key={appointment.id}
                      className="bg-slate-700/30 rounded-xl p-4 border border-slate-600/50 hover:border-blue-500/50 transition-all"
                    >
                      <div className="flex justify-between items-start mb-3">
                        <div>
                          <h3 className="text-white font-semibold">
                            {appointment.userFirstName ||
                              appointment.userName?.split(" ")[0] ||
                              "Patient"}
                            {appointment.userLastName ||
                              appointment.userName?.split(" ")[1] ||
                              ""}
                          </h3>
                          <p className="text-slate-400 text-sm">
                            {appointment.purpose || "No purpose specified"}
                          </p>
                        </div>
                        <span
                          className={`px-2 py-1 rounded-full text-xs font-medium ${
                            appointment.status === 2 ||
                            appointment.status === "Approved"
                              ? "bg-emerald-500/20 text-emerald-400"
                              : "bg-amber-500/20 text-amber-400"
                          }`}
                        >
                          {appointment.status === 2 ||
                          appointment.status === "Approved"
                            ? "Approved"
                            : "Pending"}
                        </span>
                      </div>
                      <div className="text-slate-300 text-sm mb-3">
                        <p>
                          {appointment.appointmentDate
                            ? new Date(
                                appointment.appointmentDate
                              ).toLocaleDateString()
                            : "No date"}
                          at{" "}
                          {appointment.startTime
                            ? typeof appointment.startTime === "string"
                              ? appointment.startTime.substring(0, 5)
                              : appointment.startTime
                            : "No time"}
                        </p>
                        {appointment.notes && (
                          <p className="text-slate-400 text-sm mt-1">
                            {appointment.notes}
                          </p>
                        )}
                      </div>

                      {/* Create Report Button */}
                      {(appointment.status === 2 ||
                        appointment.status === "Approved") &&
                        user?.type === 4 && (
                          <div className="flex justify-end mt-4">
                            <button
                              onClick={(e) => {
                                e.stopPropagation();
                                handleCreateReport(appointment);
                              }}
                              className="flex items-center gap-2 px-4 py-2 bg-gradient-to-r from-purple-600 to-indigo-600 hover:from-purple-700 hover:to-indigo-700 text-white rounded-lg font-semibold transition-all duration-200 hover:scale-105 active:scale-95 shadow-lg hover:shadow-purple-500/25 group"
                            >
                              <FileText className="w-4 h-4 group-hover:scale-110 transition-transform" />
                              Create Report
                            </button>
                          </div>
                        )}

                      {/* Approve/Reject for Assistants */}
                      {(appointment.status === 8 ||
                        appointment.status === "Pending") &&
                        user?.type === 7 && (
                          <div className="flex justify-end gap-2 mt-3">
                            <button
                              onClick={() =>
                                handleApproveAppointment(appointment.id)
                              }
                              className="flex items-center gap-2 px-3 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg font-semibold transition-all"
                            >
                              <CheckCircle2 className="w-4 h-4" /> Approve
                            </button>
                            <button
                              onClick={() =>
                                handleRejectAppointment(appointment.id)
                              }
                              className="flex items-center gap-2 px-3 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg font-semibold transition-all"
                            >
                              <XCircle className="w-4 h-4" /> Reject
                            </button>
                          </div>
                        )}
                    </div>
                  ))
              ) : (
                <div className="text-center py-8">
                  <Calendar className="w-16 h-16 text-slate-600 mx-auto mb-4" />
                  <p className="text-slate-400 text-lg">
                    {user?.type === 4
                      ? "No approved appointments"
                      : "No pending appointments"}
                  </p>
                </div>
              )}
            </div>
          </div>

          {/* Medications */}
          <div className="bg-slate-800/50 backdrop-blur-xl rounded-3xl border border-slate-700/50 p-8">
            <div className="flex items-center justify-between mb-8">
              <h2 className="text-2xl font-bold text-white flex items-center gap-3">
                <Pill className="w-7 h-7 text-emerald-400" />
                Prescribed Medications ({medications ? medications.length : 0})
              </h2>
              <button
                onClick={() => setShowAddMedicationForm(true)}
                className="bg-gradient-to-r from-emerald-600 to-teal-600 text-white px-4 py-2 rounded-xl font-bold hover:shadow-emerald-500/50 transition-all"
              >
                <Plus className="w-5 h-5" />
              </button>
            </div>

            <div className="space-y-4 max-h-96 overflow-y-auto">
              {medications && medications.length > 0 ? (
                medications.map((medication) => (
                  <div
                    key={medication.id}
                    className="bg-slate-700/30 rounded-xl p-4 border border-slate-600/50 hover:border-emerald-500/50 transition-all"
                  >
                    <div className="flex justify-between items-start mb-3">
                      <h3 className="text-white font-semibold text-lg">
                        {medication.name || "Unnamed Medication"}
                      </h3>
                      <span className="text-emerald-400 text-sm bg-emerald-500/20 px-2 py-1 rounded-full">
                        {medication.frequency || "As needed"}
                      </span>
                    </div>

                    <div className="space-y-2">
                      <p className="text-slate-300 text-sm">
                        <span className="text-slate-400">Dosage:</span>{" "}
                        {medication.dosage || "No dosage specified"}
                      </p>

                      {medication.instructions && (
                        <p className="text-slate-400 text-sm">
                          <span className="text-slate-500">Instructions:</span>{" "}
                          {medication.instructions}
                        </p>
                      )}

                      {medication.patientName && (
                        <p className="text-slate-500 text-sm">
                          <span className="text-slate-600">Patient:</span>{" "}
                          {medication.patientName}
                        </p>
                      )}

                      <p className="text-slate-500 text-xs mt-2">
                        Prescribed on:{" "}
                        {medication.createdAt
                          ? new Date(medication.createdAt).toLocaleDateString()
                          : "Unknown date"}
                      </p>
                    </div>
                  </div>
                ))
              ) : (
                <div className="text-center py-8">
                  <Pill className="w-16 h-16 text-slate-600 mx-auto mb-4" />
                  <p className="text-slate-400 text-lg mb-2">
                    No medications prescribed yet
                  </p>
                  <p className="text-slate-500 text-sm">
                    Add medications using the "+" button above
                  </p>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* REPORTS SECTION */}
        <div className="bg-slate-800/50 backdrop-blur-xl rounded-3xl border border-slate-700/50 p-8 mb-12">
          <div className="flex items-center justify-between mb-8">
            <h2 className="text-2xl font-bold text-white flex items-center gap-3">
              <FileText className="w-7 h-7 text-purple-400" />
              Appointments Reports ({reports.length})
            </h2>
            {reports.length > 0 && (
              <div className="flex items-center gap-2 text-slate-400">
                <div className="w-2 h-2 bg-emerald-500 rounded-full"></div>
                <span className="text-sm">{reports.length} saved reports</span>
              </div>
            )}
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {reports.length > 0 ? (
              reports.map((report) => (
                <ReportCard
                  key={report.id}
                  report={report}
                  onEdit={handleEditReport}
                  onDelete={handleDeleteReport}
                  onView={viewReportQuick}
                  onDownload={() => handleDownloadPdf(report.id)}
                />
              ))
            ) : (
              <div className="col-span-3 text-center py-12">
                <div className="bg-slate-700/30 rounded-2xl p-8 max-w-md mx-auto">
                  <FileText className="w-20 h-20 text-slate-600 mx-auto mb-4" />
                  <h3 className="text-slate-300 text-xl font-bold mb-2">
                    No created reports
                  </h3>
                  <p className="text-slate-500 text-sm mb-6">
                    Create your first report from an approved appointment
                  </p>
                  <div className="flex items-center justify-center gap-2 text-slate-400 text-sm">
                    <Calendar className="w-4 h-4" />
                    <span>Go to "My Appointments" to start</span>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>

        {/* QUICK ACTIONS */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <button
            onClick={() => setShowAddMedicationForm(true)}
            className="group bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-700 hover:to-teal-700 text-white p-8 rounded-3xl font-bold shadow-xl hover:shadow-emerald-500/50 transition-all hover:scale-105 flex flex-col items-center gap-4"
          >
            <Pill className="w-12 h-12 group-hover:scale-110 transition-transform" />
            <span className="text-xl">Add Medication</span>
          </button>
          <div
            className="bg-gradient-to-r from-blue-600 to-indigo-600 text-white p-8 rounded-3xl shadow-xl hover:shadow-blue-500/50 transition-all hover:scale-105 cursor-pointer group"
            onClick={() => {
              fetchDoctorPatients().then(() => {
                if (patients.length > 0) {
                  setShowPatientSelection(true);
                }
              });
            }}
          >
            <BarChart3 className="w-12 h-12 mx-auto mb-4 group-hover:scale-110 transition-transform" />
            <h3 className="text-xl font-bold mb-2">Patient Analytics</h3>
            <p className="text-blue-100">View health reports</p>
          </div>
          <div
            className="bg-gradient-to-r from-indigo-600 to-purple-600 text-white p-8 rounded-3xl shadow-xl hover:shadow-purple-500/50 transition-all hover:scale-105 cursor-pointer group"
            onClick={() => setShowChatInbox(true)}
          >
            <MessageCircle className="w-12 h-12 mx-auto mb-4 group-hover:scale-110 transition-transform" />
            <h3 className="text-xl font-bold mb-2">Patient Messages</h3>
            <p className="text-indigo-100">Chat with your patients</p>
          </div>
          <div
            className="bg-gradient-to-r from-purple-600 to-pink-600 text-white p-8 rounded-3xl shadow-xl hover:shadow-purple-500/50 transition-all hover:scale-105 cursor-pointer"
            onClick={() => {
              fetchDoctorPatients();
              setShowPatientsModal(true);
            }}
          >
            <Users className="w-12 h-12 mx-auto mb-4" />
            <h3 className="text-xl font-bold mb-2">My Patients</h3>
            <p className="text-purple-100">{stats.totalPatients} patients</p>
          </div>
        </div>
      </main>

      {/* MODALS */}
      <ReportModal
        showModal={showReportModal}
        selectedAppointment={selectedAppointment}
        editingReport={editingReport}
        user={user}
        onClose={() => {
          setShowReportModal(false);
          setEditingReport(null);
          setSelectedAppointment(null);
        }}
        onSubmit={handleSubmitReport}
      />

      {showAddMedicationForm && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div className="bg-slate-800/95 backdrop-blur-xl rounded-3xl max-w-md w-full p-8 border border-slate-700/50">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-2xl font-bold text-white">
                {selectedPatient
                  ? `Add Medication for ${selectedPatient.firstName}`
                  : "Add Medication"}
              </h2>
              <button
                onClick={() => {
                  setShowAddMedicationForm(false);
                  setSelectedPatient(null);
                }}
                className="p-2 hover:bg-slate-700 rounded-xl"
              >
                <X className="w-6 h-6" />
              </button>
            </div>

            {!selectedPatient ? (
              <div className="space-y-4">
                <div className="text-center py-4">
                  <Users className="w-12 h-12 text-slate-400 mx-auto mb-3" />
                  <h3 className="text-white font-semibold mb-2">
                    Select Patient
                  </h3>
                  <p className="text-slate-400 text-sm mb-4">
                    Choose which patient to prescribe medication for
                  </p>
                </div>
                <button
                  onClick={() => {
                    fetchDoctorPatients();
                    setShowPatientSelection(true);
                  }}
                  className="w-full bg-gradient-to-r from-blue-600 to-indigo-600 text-white py-4 rounded-xl font-bold hover:shadow-blue-500/50 transition-all"
                >
                  Select from My Patients
                </button>
              </div>
            ) : (
              <form
                onSubmit={handleAddMedicationSubmit}
                className="w-full max-w-2xl bg-slate-800/70 rounded-2xl border border-slate-700 shadow-lg p-4 sm:p-6 space-y-3 max-h-[90vh] overflow-y-auto"
              >
                <div className="bg-slate-700/50 rounded-xl p-3 border border-slate-600 flex items-center justify-between">
                  <div>
                    <h4 className="text-white font-semibold text-lg sm:text-xl">
                      {selectedPatient.firstName} {selectedPatient.lastName}
                    </h4>
                    <p className="text-slate-400 text-sm sm:text-base">
                      {selectedPatient.email}
                    </p>
                  </div>
                  <button
                    type="button"
                    onClick={() => setSelectedPatient(null)}
                    className="text-slate-400 hover:text-white transition-colors"
                  >
                    <X className="w-5 h-5 sm:w-6 sm:h-6" />
                  </button>
                </div>
                {selectedPatient && patientAllergies.length > 0 && (
                  <div className="bg-amber-500/10 border border-amber-500/30 rounded-xl p-4">
                    <div className="flex items-center gap-2 mb-3">
                      <AlertTriangle className="w-5 h-5 text-amber-400" />
                      <h4 className="font-semibold text-amber-400">Patient Allergies</h4>
                    </div>
                    <div className="space-y-2">
                      {patientAllergies.map((allergy) => (
                        <div key={allergy.id} className="bg-amber-500/5 p-2 rounded-lg">
                          <p className="text-amber-300 text-sm">
                            <strong>{allergy.allergenName}</strong>
                            <span className={`ml-2 px-2 py-1 rounded-full text-xs ${
                              allergy.severity === 'LifeThreatening' ? 'bg-red-500/20 text-red-400' :
                              allergy.severity === 'Severe' ? 'bg-orange-500/20 text-orange-400' :
                              'bg-yellow-500/20 text-yellow-400'
                            }`}>
                              {allergy.severityDisplay}
                            </span>
                          </p>
                          {allergy.symptoms && (
                            <p className="text-amber-200 text-xs">Symptoms: {allergy.symptoms}</p>
                          )}
                        </div>
                      ))}
                    </div>
                    <p className="text-amber-300 text-xs mt-2">
                      ⚠️ Please check for potential conflicts before prescribing medication
                    </p>
                  </div>
                )}
                <select
                  name="medicationType"
                  value={medicationForm.medicationType}
                  onChange={(e) => handleInputChange(e, setMedicationForm)}
                  className="w-full p-3 sm:p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-emerald-500 transition"
                >
                  <option value={1}>Prescription</option>
                  <option value={2}>Over the Counter</option>
                  <option value={3}>Supplement</option>
                  <option value={4}>Vitamin</option>
                  <option value={5}>Herbal</option>
                  <option value={6}>Homeopathic</option>
                </select>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  <input
                    name="name"
                    placeholder="Brand Name *"
                    required
                    value={medicationForm.name}
                    onChange={(e) => handleInputChange(e, setMedicationForm)}
                    className="w-full p-3 sm:p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-emerald-500 transition"
                  />
                  <input
                    name="genericName"
                    placeholder="Generic Name"
                    value={medicationForm.genericName}
                    onChange={(e) => handleInputChange(e, setMedicationForm)}
                    className="w-full p-3 sm:p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-emerald-500 transition"
                  />
                </div>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  <input
                    name="pharmacyName"
                    placeholder="Pharmacy Name"
                    value={medicationForm.pharmacyName}
                    onChange={(e) => handleInputChange(e, setMedicationForm)}
                    className="w-full p-3 sm:p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
                  />
                  <input
                    name="remainingRefills"
                    placeholder="Refills"
                    type="number"
                    value={medicationForm.remainingRefills}
                    onChange={(e) => handleInputChange(e, setMedicationForm)}
                    className="w-full p-3 sm:p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
                  />
                </div>

                <textarea
                  name="prescriptionNotes"
                  placeholder="Prescription Notes"
                  value={medicationForm.prescriptionNotes}
                  onChange={(e) => handleInputChange(e, setMedicationForm)}
                  rows="2"
                  className="w-full p-3 sm:p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white"
                />
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  <input
                    name="dosage"
                    placeholder="Dosage *"
                    type="number"
                    step="0.1"
                    required
                    value={medicationForm.dosage}
                    onChange={(e) => handleInputChange(e, setMedicationForm)}
                    className="w-full p-3 sm:p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-emerald-500 transition"
                  />
                  <select
                    name="dosageUnit"
                    value={medicationForm.dosageUnit}
                    onChange={(e) => handleInputChange(e, setMedicationForm)}
                    className="w-full p-3 sm:p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-emerald-500 transition"
                  >
                    <option value={1}>Milligrams</option>
                    <option value={2}>Grams</option>
                    <option value={3}>Milliliters</option>
                    <option value={4}>Liters</option>
                    <option value={5}>Tablets</option>
                    <option value={6}>Capsules</option>
                    <option value={7}>Drops</option>
                    <option value={8}>Puffs</option>
                    <option value={9}>Patches</option>
                    <option value={10}>Units</option>
                  </select>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  <select
                    name="frequency"
                    value={medicationForm.frequency}
                    onChange={(e) => handleInputChange(e, setMedicationForm)}
                    className="w-full p-3 sm:p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white focus:outline-none focus:ring-2 focus:ring-emerald-500 transition"
                  >
                    <option value="Daily">Once Daily</option>
                    <option value="Twice daily">Twice Daily</option>
                    <option value="Three times daily">Three Times Daily</option>
                    <option value="Four times daily">Four Times Daily</option>
                    <option value="Every few hours">Every Few Hours</option>
                    <option value="As needed">As Needed</option>
                    <option value="Weekly">Weekly</option>
                    <option value="Monthly">Monthly</option>
                    <option value="Custom">Custom</option>
                  </select>
                  <input
                    name="duration"
                    placeholder="Duration in days"
                    type="number"
                    value={medicationForm.duration}
                    onChange={(e) => handleInputChange(e, setMedicationForm)}
                    className="w-full p-3 sm:p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-emerald-500 transition"
                  />
                </div>

                {/* Conditional fields based on frequency type */}
                {(medicationForm.frequency === "Every few hours" ||
                  medicationForm.frequency === "Custom") && (
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                    <input
                      name="customFrequencyHours"
                      placeholder="Frequency in hours (e.g., 6, 8, 12)"
                      type="number"
                      min="1"
                      value={medicationForm.customFrequencyHours || ""}
                      onChange={(e) =>
                        setMedicationForm({
                          ...medicationForm,
                          customFrequencyHours: e.target.value
                            ? parseInt(e.target.value)
                            : null,
                        })
                      }
                      className="w-full p-3 sm:p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-emerald-500 transition"
                      required={
                        medicationForm.frequency === "Every few hours" ||
                        medicationForm.frequency === "Custom"
                      }
                    />
                  </div>
                )}

                {medicationForm.frequency === "Weekly" && (
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                    <select
                      name="daysOfWeek"
                      value={medicationForm.daysOfWeek}
                      onChange={(e) =>
                        setMedicationForm({
                          ...medicationForm,
                          daysOfWeek: e.target.value,
                        })
                      }
                      className="w-full p-3 sm:p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white focus:outline-none focus:ring-2 focus:ring-emerald-500 transition"
                      required
                    >
                      <option value="">Select day</option>
                      <option value="Monday">Monday</option>
                      <option value="Tuesday">Tuesday</option>
                      <option value="Wednesday">Wednesday</option>
                      <option value="Thursday">Thursday</option>
                      <option value="Friday">Friday</option>
                      <option value="Saturday">Saturday</option>
                      <option value="Sunday">Sunday</option>
                    </select>
                  </div>
                )}

                {medicationForm.frequency === "Monthly" && (
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                    <input
                      name="monthlyDay"
                      placeholder="Day of month (1-31)"
                      type="number"
                      min="1"
                      max="31"
                      value={medicationForm.monthlyDay || ""}
                      onChange={(e) =>
                        setMedicationForm({
                          ...medicationForm,
                          monthlyDay: e.target.value
                            ? parseInt(e.target.value)
                            : null,
                        })
                      }
                      className="w-full p-3 sm:p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-emerald-500 transition"
                      required
                    />
                  </div>
                )}

                <textarea
                  name="description"
                  placeholder="Description (optional)"
                  value={medicationForm.description}
                  onChange={(e) => handleInputChange(e, setMedicationForm)}
                  rows="2"
                  className="w-full p-3 sm:p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white placeholder:text-slate-400 resize-vertical focus:outline-none focus:ring-2 focus:ring-emerald-500 transition"
                />
                <textarea
                  name="instructions"
                  placeholder="Instructions for use *"
                  required
                  rows="3"
                  value={medicationForm.instructions}
                  onChange={(e) => handleInputChange(e, setMedicationForm)}
                  className="w-full p-3 sm:p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white placeholder:text-slate-400 resize-vertical focus:outline-none focus:ring-2 focus:ring-emerald-500 transition"
                />

                <button
                  type="submit"
                  className="w-full bg-gradient-to-r from-emerald-600 to-teal-600 text-white py-3 sm:py-4 rounded-xl font-bold hover:scale-105 hover:shadow-lg transition-all duration-200"
                >
                  Prescribe Medication
                </button>
              </form>
            )}
          </div>
        </div>
      )}

      {showPatientSelection && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-[60] flex items-center justify-center p-4">
          <div className="bg-slate-800/95 backdrop-blur-xl rounded-3xl max-w-2xl w-full max-h-[80vh] overflow-hidden border border-slate-700/50">
            <div className="flex items-center justify-between p-6 border-b border-slate-700/50">
              <h2 className="text-2xl font-bold text-white">Select Patient</h2>
              <button
                onClick={() => setShowPatientSelection(false)}
                className="p-2 hover:bg-slate-700 rounded-xl"
              >
                <X className="w-6 h-6" />
              </button>
            </div>
            <div className="p-6 max-h-96 overflow-y-auto">
              {patients.length > 0 ? (
                <div className="grid grid-cols-1 gap-3">
                  {patients.map((patient) => (
                    <div
                      key={patient.id}
                      className="bg-slate-700/50 rounded-xl p-4 border border-slate-600/50 hover:border-blue-500/50 cursor-pointer transition-all hover:scale-105 flex justify-between items-center"
                    >
                      {/* Click whole card → prescribe medication */}
                      <div
                        onClick={() => {
                          setSelectedPatient(patient);
                          setShowPatientSelection(false);
                        }}
                      >
                        <h3 className="text-white font-semibold">
                          {patient.firstName} {patient.lastName}
                        </h3>
                        <p className="text-slate-300 text-sm">
                          {patient.email}
                        </p>
                      </div>

                      {/* Analytics button */}
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          openPatientAnalytics(patient);
                          setShowPatientSelection(false);
                        }}
                        className="ml-4 px-3 py-1.5 bg-gradient-to-r from-indigo-600 to-blue-600 text-white text-xs rounded-lg font-medium hover:shadow-lg transition-all"
                      >
                        <BarChart3 className="w-4 h-4 inline mr-1" />
                        Analytics
                      </button>
                    </div>
                  ))}
                </div>
              ) : (
                <div className="text-center py-8">
                  <Users className="w-16 h-16 text-slate-600 mx-auto mb-4" />
                  <p className="text-slate-400">No patients found</p>
                  <button
                    onClick={fetchDoctorPatients}
                    className="mt-4 text-blue-400 hover:text-blue-300"
                  >
                    Refresh List
                  </button>
                </div>
              )}
            </div>
          </div>
        </div>
      )}

      {/* ==== PATIENT ANALYTICS MODAL ==== */}
      {showAnalytics && selectedPatientForAnalytics && (
        <div className="fixed inset-0 bg-black/70 backdrop-blur-sm z-[70] flex items-center justify-center p-4">
          <div className="bg-slate-800/95 backdrop-blur-xl rounded-3xl w-full max-w-6xl max-h-[90vh] overflow-y-auto border border-slate-700/50">
            <PatientAnalytics
              patient={selectedPatientForAnalytics}
              doctor={user}
              onEditReport={(report) => {
                handleEditReport(report);
                setShowAnalytics(false);
              }}
              onClose={() => setShowAnalytics(false)}
            />
          </div>
        </div>
      )}
      {/* CHAT INBOX MODAL */}
      {showChatInbox && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div className="bg-slate-800/95 backdrop-blur-xl rounded-3xl w-full max-w-6xl h-[90vh] overflow-hidden border border-slate-700/50 flex flex-col">
            <div className="flex items-center justify-between p-6 border-b border-slate-700/50 bg-slate-900/80">
              <div className="flex items-center gap-3">
                <MessageCircle className="w-8 h-8 text-blue-400" />
                <div>
                  <h2 className="text-2xl font-bold text-white">
                    Patient Messages
                  </h2>
                  <p className="text-slate-400 text-sm">
                    Chat with your patients
                  </p>
                </div>
              </div>
              <button
                onClick={() => setShowChatInbox(false)}
                className="p-3 hover:bg-slate-700 rounded-xl transition-all hover:scale-105"
              >
                <X className="w-6 h-6 text-slate-300" />
              </button>
            </div>
            <div className="flex-1 p-0 overflow-hidden">
              <ChatInbox currentUser={user} isDoctorView={true} />
            </div>
          </div>
        </div>
      )}

      {/* PATIENTS MODAL */}
      {showPatientsModal && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div className="bg-slate-800/95 backdrop-blur-xl rounded-3xl max-w-4xl w-full max-h-[80vh] overflow-y-auto p-8 border border-slate-700/50">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-2xl font-bold text-white">
                My Patients ({patients.length})
              </h2>
              <button
                onClick={() => setShowPatientsModal(false)}
                className="p-2 hover:bg-slate-700 rounded-xl"
              >
                <X className="w-6 h-6" />
              </button>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {patients.map((patient) => (
                <div
                  key={patient.id}
                  className="bg-slate-700/50 rounded-xl p-4 border border-slate-600/50"
                >
                  <h3 className="text-white font-semibold text-lg">
                    {patient.firstName} {patient.lastName}
                  </h3>
                  <p className="text-slate-300 text-sm mt-1">{patient.email}</p>
                  {patient.phoneNumber && (
                    <p className="text-slate-400 text-sm">
                      Phone: {patient.phoneNumber}
                    </p>
                  )}
                  {patient.lastAppointment && (
                    <p className="text-slate-500 text-xs mt-2">
                      Last visit:{" "}
                      {new Date(patient.lastAppointment).toLocaleDateString()}
                    </p>
                  )}
                </div>
              ))}
            </div>
            {patients.length === 0 && (
              <p className="text-slate-400 text-center py-8">
                No patients found
              </p>
            )}
          </div>
        </div>
      )}
    </div>
  );
};

export default DoctorDashboard;
