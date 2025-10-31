// client/components/DoctorDashboard.jsx
"use client";
import ChatInbox from '../ChatInbox';
import NotificationCenter from '../NotificationCenter';
import { MessageCircle } from 'lucide-react';
import ReportCard from './ReportCard';
import ReportModal from './ReportModal';
import { generateReportPDFTemplate, downloadHtmlAsPdf } from './ReportPDFTemplate';
import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import {
  Pill,
  Calendar,
  Users,
  Eye,
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
import { authService, api } from "../../../services/authService";

const DoctorDashboard = () => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
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

const fetchAssistantReports = async (assistantId) => {
  try {
    console.log('🔄 Loading assistant reports for:', assistantId);
   
    const response = await api.get(`/api/appointments/reports`);
    console.log('📊 All reports for assistant:', response.data);
    setReports(response.data || []);
    
  } catch (error) {
    console.error('❌ Error loading assistant reports:', error);
    toast.error('Failed to load assistant reports');
    setReports([]);
  }
};

useEffect(() => {
  const initializeLocalStorage = () => {
    const existingReports = localStorage.getItem('doctorReports');
    if (!existingReports) {
      localStorage.setItem('doctorReports', JSON.stringify([]));
      console.log('📝 Initialized empty doctorReports in localStorage');
    }
  };

  initializeLocalStorage();
}, []);
  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const userData = await authService.getMe();
        
        if (!userData) {
          router.push("/login");
          return;
        }

        const isDoctor = userData.type === 4; // HealthcareProvider
        const isAssistant = userData.type === 7; // Assistant
        
        if (!isDoctor && !isAssistant) {
          toast.error("Access denied. Doctor or Assistant privileges required.");
          router.push("/dashboard/dashboard");
          return;
        }

        setUser(userData);
        console.log('👤 User loaded:', userData);
        await Promise.all([
          loadAppointments(userData, isDoctor),
          loadMedications(userData),
          loadReports(userData)
        ]);

      } catch (error) {
        console.error('❌ Dashboard error:', error);
        toast.error("Failed to load dashboard data");
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [router]);
 
const loadAppointments = async (userData, isDoctor) => {
  try {
    console.log('🔄 Loading appointments for:', userData.id, userData.firstName, userData.lastName);
    
    let filteredAppointments = [];

    if (isDoctor) {
      try {
       
        console.log('🔄 Using general appointments endpoint...');
        const response = await api.get("/appointments");
        const allAppointments = response.data || [];
        
        const doctorFullName = `Dr. ${userData.firstName} ${userData.lastName}`;
        console.log('🔍 Looking for doctor:', doctorFullName);
        
        filteredAppointments = allAppointments.filter(app => {
          const matchesDoctor = app.doctorName === doctorFullName;
          const isApproved = app.status === 2 || app.status === 'Approved';
          
          if (matchesDoctor) {
            console.log(`✅ Found appointment for ${doctorFullName}:`, {
              id: app.id,
              patient: app.userName,
              date: app.appointmentDate,
              status: app.status
            });
          }
          
          return matchesDoctor && isApproved;
        });
        
        console.log('👨‍⚕️ Final filtered appointments:', filteredAppointments.length);

      } catch (error) {
        console.error('❌ Error loading appointments:', error);
        
        filteredAppointments = [];
      }
    } else {
    
      try {
        const pendingResponse = await api.get(`/appointments/assistant/${userData.id}/pending`);
        filteredAppointments = pendingResponse.data || [];
      } catch (pendingError) {
        console.error('❌ Error loading assistant appointments:', pendingError);
        throw pendingError;
      }
    }

    console.log('📋 Final appointments count:', filteredAppointments.length);
    setAppointments(filteredAppointments);
    
    const approvedApps = filteredAppointments.filter(app => app.status === 2 || app.status === 'Approved');
    const today = new Date().toISOString().split('T')[0];
    const todayApps = approvedApps.filter(app => {
      try {
        const appointmentDate = new Date(app.appointmentDate).toISOString().split('T')[0];
        return appointmentDate === today;
      } catch {
        return false;
      }
    });
    const pendingApps = filteredAppointments.filter(app => app.status === 8 || app.status === 'Pending');
    
    const uniquePatients = [...new Set(approvedApps.map(app => app.userId))];

    setStats({
      totalAppointments: approvedApps.length,
      todayAppointments: todayApps.length,
      pendingAppointments: pendingApps.length,
      totalPatients: uniquePatients.length,
    });

  } catch (error) {
    console.error('❌ Error in loadAppointments:', error);
    toast.error("Failed to load appointments");
    setAppointments([]);
  }
};
const loadMedications = async (userData) => {
  try {

    const response = await api.get("/api/medications");
    const allMeds = response.data || [];
  
    const filteredMeds = allMeds.filter(med => 
      med.prescribedBy === `${userData.firstName} ${userData.lastName}` ||
      med.prescribedBy === `Dr. ${userData.firstName} ${userData.lastName}` ||
      med.doctorId === userData.id
    );
    
    setMedications(filteredMeds);
    console.log('💊 Medications loaded:', filteredMeds.length);
  } catch (error) {
    console.error('Error loading medications:', error);
    
    try {
      const altResponse = await api.get("/medications");
      const allMeds = altResponse.data || [];
      
      const filteredMeds = allMeds.filter(med => 
        med.prescribedBy === `${userData.firstName} ${userData.lastName}` ||
        med.prescribedBy === `Dr. ${userData.firstName} ${userData.lastName}` ||
        med.doctorId === userData.id
      );
      
      setMedications(filteredMeds);
      console.log('💊 Medications loaded via alternative endpoint:', filteredMeds.length);
    } catch (altError) {
      console.error('Both medication endpoints failed');
      setMedications([]);
    }
  }
};

const loadReports = async (userData) => {
  try {
    console.log('📊 Loading reports from localStorage...');
    
    const localReports = localStorage.getItem('doctorReports');
    
    if (localReports) {
      const parsedReports = JSON.parse(localReports);
      
      if (userData?.type === 4) { // Doctor
        const doctorReports = parsedReports.filter(report => 
          report.doctorId === userData.id
        );
        setReports(doctorReports);
        console.log('📊 Doctor reports loaded from localStorage:', doctorReports.length);
      } else {
    
        setReports(parsedReports);
        console.log('📊 All reports loaded from localStorage:', parsedReports.length);
      }
    } else {
      setReports([]);
      console.log('📊 No reports found in localStorage');
    }
    
  } catch (error) {
    console.error('Error loading reports from localStorage:', error);
    setReports([]);
  }
};


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
    console.log('🔄 Fetching doctor patients...');
    
    try {
      const patientsRes = await api.get(`/users/doctor/${user.id}/patients`);
      setPatients(patientsRes.data || []);
      console.log('👥 Patients from API:', patientsRes.data);
      return;
    } catch (endpointError) {
      console.log('❌ Specific patients endpoint failed:', endpointError);
    }

    const approvedAppointments = appointments.filter(app => 
      app.status === 2 || app.status === 'Approved'
    );
    
    const uniquePatientMap = new Map();
    
    approvedAppointments.forEach(app => {
      if (app.userId && !uniquePatientMap.has(app.userId)) {
        uniquePatientMap.set(app.userId, {
          id: app.userId,
          firstName: app.userFirstName || 'Patient',
          lastName: app.userLastName || '',
          email: app.userEmail || `patient${uniquePatientMap.size + 1}@email.com`,
          phoneNumber: app.userPhone || '',
          lastAppointment: app.appointmentDate || new Date().toISOString()
        });
      }
    });
    
    const uniquePatients = Array.from(uniquePatientMap.values());
    setPatients(uniquePatients);
    console.log('👥 Patients from appointments:', uniquePatients);
    
  } catch (err) {
    console.error("Error fetching patients:", err);
  
    const approvedAppointments = appointments.filter(app => 
      app.status === 2 || app.status === 'Approved'
    );
    
    const uniquePatients = approvedAppointments.map((app, index) => ({
      id: app.userId || `patient-${index}`,
      firstName: app.userFirstName || `Patient ${index + 1}`,
      lastName: app.userLastName || '',
      email: app.userEmail || `patient${index + 1}@email.com`,
      lastAppointment: app.appointmentDate || new Date().toISOString()
    }));
    
    setPatients(uniquePatients);
  }
};
 const handleAddMedicationSubmit = async (e) => {
  e.preventDefault();
  try {
 
    await api.post("/api/medications", {
      ...medicationForm,
      userId: user.id,
      doctorId: user.id,
      prescribedBy: `${user.firstName} ${user.lastName}`,
    });
    
    setShowAddMedicationForm(false);
    setMedicationForm({
      name: "", dosage: "", instructions: "", frequency: "Daily", duration: ""
    });

    await loadMedications(user);
    toast.success("Medication added successfully!");
  } catch (err) {
    console.error("Error adding medication:", err);
    
    try {
      await api.post("/medications", {
        ...medicationForm,
        userId: user.id,
        doctorId: user.id,
        prescribedBy: `${user.firstName} ${user.lastName}`,
      });
      
      setShowAddMedicationForm(false);
      setMedicationForm({
        name: "", dosage: "", instructions: "", frequency: "Daily", duration: ""
      });
      
      await loadMedications(user);
      toast.success("Medication added successfully!");
    } catch (altError) {
      console.error("Both medication endpoints failed");
      toast.error("Failed to add medication - service unavailable");
    }
  }
};
 
  const handleCreateReport = async (appointment) => {
  console.log('🎯 Create Report clicked for appointment:', appointment);
  
  if (!appointment) {
    console.error('❌ No appointment provided');
    toast.error("No appointment selected");
    return;
  }
  
  if (appointment.status !== 2 && appointment.status !== 'Approved') {
    console.error('❌ Appointment not approved:', appointment.status);
    toast.error("You can only create reports for approved appointments");
    return;
  }
  
  console.log('✅ Setting selected appointment and opening modal');
  
  const userFirstName = appointment.userFirstName || appointment.userName?.split(' ')[0] || 'Patient';
  const userLastName = appointment.userLastName || appointment.userName?.split(' ')[1] || '';
  const fullUserName = `${userFirstName} ${userLastName}`.trim();

  const safeAppointment = {
    id: appointment.id || '',
    userId: appointment.userId || '',
    userFirstName: userFirstName,
    userLastName: userLastName,
    userName: fullUserName,
    doctorName: appointment.doctorName || `Dr. ${user?.firstName} ${user?.lastName}`,
    appointmentDate: appointment.appointmentDate || new Date().toISOString(),
    purpose: appointment.purpose || 'No purpose specified',
    status: appointment.status,
    specialty: appointment.specialty || 'General Medicine'
  };
  
  console.log('👤 Patient details:', {
    original: {
      userFirstName: appointment.userFirstName,
      userLastName: appointment.userLastName, 
      userName: appointment.userName
    },
    processed: {
      userFirstName: userFirstName,
      userLastName: userLastName,
      fullUserName: fullUserName
    }
  });
  
  setSelectedAppointment(safeAppointment);
  setEditingReport(null);
  setShowReportModal(true);
  
  console.log('✅ Modal should be visible now with patient:', fullUserName);
};

 const handleSubmitReport = async (formData) => {
  try {
    console.log('📝 Starting report submission...');

    if (!selectedAppointment) {
      toast.error("No appointment selected");
      return;
    }

    if (!user) {
      toast.error("User not authenticated");
      return;
    }

    const patientFirstName = selectedAppointment.userFirstName || 'Patient';
    const patientLastName = selectedAppointment.userLastName || '';
    const patientFullName = `${patientFirstName} ${patientLastName}`.trim();

    const reportData = {
      appointmentId: selectedAppointment.id,
      userId: selectedAppointment.userId,
      doctorId: user.id,
      reportDate: new Date().toISOString(),
      diagnosis: formData.diagnosis || '',
      symptoms: formData.symptoms || '',
      treatment: formData.treatment || '',
      medications: formData.medications || '',
      notes: formData.notes || '',
      recommendations: formData.recommendations || '',
      userName: patientFullName, 
      doctorName: `Dr. ${user.firstName} ${user.lastName}`,
      specialty: selectedAppointment.specialty || 'General Medicine',
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString()
    };

    console.log('📋 Report data prepared for patient:', patientFullName);
    console.log('📋 Full report data:', reportData);

    const newReport = {
      id: editingReport ? editingReport.id : `report-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
      ...reportData
    };

    console.log('💾 Saving report locally for patient:', patientFullName);

    let updatedReports;
    if (editingReport) {
     
      updatedReports = reports.map(r => r.id === editingReport.id ? newReport : r);
      toast.success('✅ Report is updated succesfully!');
    } else {
     
      updatedReports = [newReport, ...reports];
      toast.success('✅ Report succesfully created!');
    }
    
    setReports(updatedReports);
    localStorage.setItem('doctorReports', JSON.stringify(updatedReports));
    
    console.log('💯 Report saved successfully for patient:', patientFullName);

    setShowReportModal(false);
    setSelectedAppointment(null);
    setEditingReport(null);
    
  } catch (error) {
    console.error('❌ Error saving report:', error);
    toast.error('Gabim në ruajtjen e raportit');
  }
};


const handleEditReport = (report) => {
  console.log('✏️ Editing report:', report);
  
  setEditingReport(report);
  setSelectedAppointment({
    id: report.appointmentId,
    userId: report.userId,
    userFirstName: report.userName?.split(' ')[0] || 'Patient',
    userLastName: report.userName?.split(' ')[1] || '',
    doctorName: report.doctorName,
    appointmentDate: report.reportDate,
    purpose: 'Follow-up consultation',
    status: 2 // Approved
  });
  setShowReportModal(true);
};

const handleDeleteReport = async (reportId) => {
  if (!window.confirm('Jeni i sigurt që dëshironi të fshini këtë raport?')) return;
  
  try {
    console.log('🗑️ Deleting report:', reportId);
    
    const updatedReports = reports.filter(report => report.id !== reportId);
    setReports(updatedReports);
    localStorage.setItem('doctorReports', JSON.stringify(updatedReports));
    
    toast.success('✅ Raporti u fshi me sukses!');
    
  } catch (error) {
    console.error('❌ Error deleting report:', error);
    toast.error('Gabim në fshirjen e raportit');
  }
};

const handleDownloadPdf = async (reportId) => {
  try {
    const report = reports.find(r => r.id === reportId);
    if (!report) {
      toast.error('Raporti nuk u gjet!');
      return;
    }

    const htmlContent = generateReportPDFTemplate(report);
    const filename = `medicine-report-${report.userName ? report.userName.replace(/\s+/g, '-') : 'pacient'}-${new Date().toISOString().split('T')[0]}.pdf`;
    
    await downloadHtmlAsPdf(htmlContent, filename);
    toast.success('✅ PDF is opened for download');
    
  } catch (error) {
    console.error('❌ Error while downloading the pdf:', error);
    toast.error('Error in downloading the pdf!');
  }
};

const viewReportQuick = (report) => {
  const reportDetails = `
    Detailed Report
    ================
    
    Patient: ${report.userName}
    Doctor: ${report.doctorName}
    DATE: ${new Date(report.reportDate).toLocaleDateString('sq-AL')}
    
    Diagnose:
    ${report.diagnosis || 'No Diagnose'}
    
    Symptoms:
    ${report.symptoms || 'No Symptoms'}
    
    Treatment:
    ${report.treatment || 'No Treatment'}
    
    Medications:
    ${report.medications || 'No Medications'}
    
    Notes:
    ${report.notes || 'No Notes'}
    
    Recomendations:
    ${report.recommendations || 'No Recomendations'}
  `;
  
  const modal = window.open('', 'Report', 'width=600,height=700');
  modal.document.write(`
    <html>
      <head><title>Raporti Mjekësor</title></head>
      <body style="font-family: Arial; padding: 20px;">
        <h2 style="color: #2c5aa0;">Raport Mjekësor</h2>
        <pre style="white-space: pre-wrap; font-size: 14px;">${reportDetails}</pre>
        <button onclick="window.print()" style="margin-top: 20px; padding: 10px; background: #2c5aa0; color: white; border: none; border-radius: 5px;">🖨️ Printo</button>
      </body>
    </html>
  `);
  modal.document.close();
};
const handleApproveAppointment = async (appointmentId) => {
  if (user?.type === 7) { 
    try {
      await api.put(`/api/appointments/${appointmentId}/assistant-approve`);
       setAppointments(prev => prev.filter(app => app.id !== appointmentId));
      toast.success("Appointment approved successfully!");
    } catch (error) {
      console.error('Error approving appointment:', error);
      toast.error("Failed to approve appointment");
    }
  }
};

const handleRejectAppointment = async (appointmentId) => {
  if (user?.type === 7) { 
    const reason = prompt("Please provide a reason for rejection:");
    if (reason) {
      try {
        await api.put(`/api/appointments/${appointmentId}/assistant-reject`, {
          rejectionReason: reason
        });
        
        setAppointments(prev => prev.filter(app => app.id !== appointmentId));
        toast.success("Appointment rejected successfully!");
      } catch (error) {
        console.error('Error rejecting appointment:', error);
        toast.error("Failed to reject appointment");
      }
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
            },
            {
              label: "Today's Appointments", 
              value: stats.todayAppointments,
              icon: Clock,
              color: "from-emerald-500 to-teal-500",
            },
            {
              label: user?.type === 4 ? "Pending Appointments" : "Pending Approval",
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

       
{/* Appointments Section */}
<div className="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-12">
  <div className="bg-slate-800/50 backdrop-blur-xl rounded-3xl border border-slate-700/50 p-8">
    <div className="flex items-center justify-between mb-8">
      <h2 className="text-2xl font-bold text-white flex items-center gap-3">
        <Calendar className="w-7 h-7 text-blue-400" />
        {user?.type === 4 ? "My Approved Appointments" : "Pending Appointments"} 
        ({user?.type === 4 
          ? appointments.filter(app => app.status === 2 || app.status === 'Approved').length 
          : appointments.filter(app => app.status === 8 || app.status === 'Pending').length
        })
      </h2>
    </div>
    <div className="space-y-4 max-h-96 overflow-y-auto">
      {appointments.filter(app => 
        user?.type === 4 
          ? (app.status === 2 || app.status === 'Approved')
          : (app.status === 8 || app.status === 'Pending')
      ).length > 0 ? (
        appointments
          .filter(app => user?.type === 4 
            ? (app.status === 2 || app.status === 'Approved')
            : (app.status === 8 || app.status === 'Pending')
          )
          .map((appointment) => (
            <div
              key={appointment.id}
              className="bg-slate-700/30 rounded-xl p-4 border border-slate-600/50 hover:border-blue-500/50 transition-all"
            >
              <div className="flex justify-between items-start mb-3">
                <div>
                  <h3 className="text-white font-semibold">
                    {appointment.userFirstName || appointment.userName?.split(' ')[0] || 'Patient'} 
                    {appointment.userLastName || appointment.userName?.split(' ')[1] || ''}
                  </h3>
                  <p className="text-slate-400 text-sm">{appointment.purpose || 'No purpose specified'}</p>
                </div>
                <span
                  className={`px-2 py-1 rounded-full text-xs font-medium ${
                    appointment.status === 2 || appointment.status === 'Approved'
                      ? "bg-emerald-500/20 text-emerald-400"
                      : appointment.status === 8 || appointment.status === 'Pending'
                      ? "bg-amber-500/20 text-amber-400"
                      : "bg-slate-500/20 text-slate-400"
                  }`}
                >
                  {appointment.status === 2 || appointment.status === 'Approved' ? "Approved" : 
                   appointment.status === 8 || appointment.status === 'Pending' ? "Pending" : 
                   appointment.status || "Unknown"}
                </span>
              </div>
              
              <div className="text-slate-300 text-sm mb-3">
                <p>
                  📅 {appointment.appointmentDate ? new Date(appointment.appointmentDate).toLocaleDateString() : 'No date'} 
                  at ⏰ {appointment.startTime ? (typeof appointment.startTime === 'string' ? appointment.startTime.substring(0, 5) : appointment.startTime) : 'No time'}
                </p>
                {appointment.notes && (
                  <p className="text-slate-400 text-sm mt-1">📝 {appointment.notes}</p>
                )}
              </div>

            {/* Buton for creating report*/}
{(appointment.status === 2 || appointment.status === 'Approved') && user?.type === 4 && (
  <div className="flex justify-end mt-4">
    <button
      onClick={(e) => {
        e.stopPropagation();
        console.log('🖱️ Create Report button clicked');
        handleCreateReport(appointment);
      }}
      className="flex items-center gap-2 px-4 py-2 bg-gradient-to-r from-purple-600 to-indigo-600 hover:from-purple-700 hover:to-indigo-700 text-white rounded-lg font-semibold transition-all duration-200 hover:scale-105 active:scale-95 shadow-lg hover:shadow-purple-500/25 group"
    >
      <FileText className="w-4 h-4 group-hover:scale-110 transition-transform" />
      Create Report
    </button>
  </div>
)}
              
              {/* Butonat për approve/reject for assistants */}
              {(appointment.status === 8 || appointment.status === 'Pending') && user?.type === 7 && (
                <div className="flex justify-end gap-2 mt-3">
                  <button
                    onClick={() => handleApproveAppointment(appointment.id)}
                    className="flex items-center gap-2 px-3 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg font-semibold transition-all"
                  >
                    <CheckCircle2 className="w-4 h-4" />
                    Approve
                  </button>
                  <button
                    onClick={() => handleRejectAppointment(appointment.id)}
                    className="flex items-center gap-2 px-3 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg font-semibold transition-all"
                  >
                    <XCircle className="w-4 h-4" />
                    Reject
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
              : "No pending appointments"
            }
          </p>
          <p className="text-slate-500 text-sm mt-2">
            {user?.type === 4 
              ? "Approved appointments will appear here after assistant approval" 
              : "Pending appointments will appear here for approval"
            }
          </p>
        </div>
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
     
{/* Seksioni i Raporteve */}
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
          onDownload={handleDownloadPdf}
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
{/* Report Modal */}
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
{showChatInbox && (
  <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
    <div className="bg-slate-800/95 backdrop-blur-xl rounded-3xl w-full max-w-6xl h-[90vh] overflow-hidden border border-slate-700/50 flex flex-col">
      
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