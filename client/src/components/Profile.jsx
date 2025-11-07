// components/Profile.jsx
"use client";

import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import {
  User,
  Bell,
  Shield,
  AlertTriangle,
  Plus,
  X,
  Save,
  Phone,
  Mail,
  Calendar,
  Home,
  MapPin,
  Loader2,
  Pill,
  Stethoscope,
  Heart
} from "lucide-react";
import { authService, api } from "../../services/authService";
import { allergyService } from "../../services/allergyService"; 
import { toast, ToastContainer } from "react-toastify"; 
import "react-toastify/dist/ReactToastify.css";
const Profile = () => {
  const [user, setUser] = useState(null);
    const [userAllergies, setUserAllergies] = useState([]);

const [profile, setProfile] = useState({
  firstName: "",
  lastName: "",
  email: "",
  phone: "",
  dateOfBirth: "",
  allergies: [], 
  conditions: [],
  emergencyContact: {
    name: "",
    phone: "",
    relationship: ""
  },
  address: {
    street: "",
    city: "",
    state: "",
    zipCode: "",
    country: ""
  },
  notifications: {
    medication: true,
    appointments: true,
    refills: true
  }
});

const [newAllergy, setNewAllergy] = useState({
  allergenName: "",
  severity: "Mild",
  symptoms: "",
  diagnosedDate: "",
  diagnosedBy: ""
});
  const [newCondition, setNewCondition] = useState("");
  const [isEditing, setIsEditing] = useState(false);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const router = useRouter();

  useEffect(() => {
    loadUserData();
  }, []);

const loadUserData = async () => {
  try {
    setLoading(true);
    
    const userData = await authService.getMe();
    
    if (!userData || !userData.id) {
      throw new Error("User data not available");
    }

    console.log("Loading profile for user:", userData.id);
    setUser(userData);

    setProfile(prev => ({
      ...prev,
      firstName: userData.firstName || "",
      lastName: userData.lastName || "",
      email: userData.email || "",
      phone: userData.phoneNumber || "",
      dateOfBirth: userData.dateOfBirth || ""
    }));

    try {
      console.log("🟡 Fetching allergies from API for user:", userData.id);
      const allergies = await allergyService.getUserAllergies(userData.id);
      console.log("✅ Allergies from API:", allergies);
      
      setUserAllergies(allergies);
      setProfile(prev => ({
        ...prev,
        allergies: allergies
      }));
    } catch (allergyError) {
      console.error("Error loading allergies:", allergyError);
     
      setUserAllergies([]);
      setProfile(prev => ({
        ...prev,
        allergies: []
      }));
    }

  } catch (error) {
    console.error("Error loading user data:", error);
   
    try {
      const savedProfile = localStorage.getItem(`userProfile_${userData?.id}`);
      if (savedProfile) {
        const parsedProfile = JSON.parse(savedProfile);
        setUserAllergies(parsedProfile.allergies || []);
        setProfile(prev => ({
          ...prev,
          ...parsedProfile
        }));
      }
    } catch (fallbackError) {
      console.error("Fallback also failed:", fallbackError);
    }
  } finally {
    setLoading(false);
  }
};

  const saveAllChanges = async () => {
    try {
      setSaving(true);
      
      if (!user || !user.id) {
        throw new Error("User ID not available");
      }
      localStorage.setItem(`userProfile_${user.id}`, JSON.stringify(profile));
      console.log("Profile saved for user:", user.id, profile);
      
      setIsEditing(false);
      alert("✅ All changes saved successfully!");
    } catch (error) {
      console.error("Error saving profile:", error);
      alert("❌ Failed to save changes");
    } finally {
      setSaving(false);
    }
  };

const addAllergy = async () => {
  if (newAllergy.allergenName?.trim() && user?.id) {
    try {
      
      let diagnosedDate = null;
      if (newAllergy.diagnosedDate) {
       
        diagnosedDate = new Date(newAllergy.diagnosedDate + 'T00:00:00Z').toISOString();
      }

      const allergyData = {
        userId: user.id,
        allergenName: newAllergy.allergenName.trim(),
        description: newAllergy.symptoms?.trim() || "",
        severity: mapSeverityToEnum(newAllergy.severity || "Mild"),
        symptoms: newAllergy.symptoms?.trim() || "",
        treatment: "",
        diagnosedDate: diagnosedDate, 
        diagnosedBy: newAllergy.diagnosedBy?.trim() || ""
      };

      console.log("🟡 Sending allergy data to backend:", allergyData);
      const createdAllergy = await allergyService.addAllergy(allergyData);
      console.log("✅ Allergy added successfully:", createdAllergy);

      const updatedAllergies = [...userAllergies, createdAllergy];
      setUserAllergies(updatedAllergies);
      setProfile(prev => ({
        ...prev,
        allergies: updatedAllergies
      }));

      setNewAllergy({
        allergenName: "",
        severity: "Mild",
        symptoms: "",
        diagnosedDate: "",
        diagnosedBy: ""
      });

      toast.success(`Allergy "${allergyData.allergenName}" added successfully!`);
      
    } catch (error) {
      console.error("❌ Error adding allergy:", error);
      toast.error(`Failed to add allergy: ${error.message}`);
    }
  } else {
    toast.error("Please enter an allergy name");
  }
};

const mapSeverityToEnum = (severity) => {
  const severityMap = {
    "Mild": 1,
    "Moderate": 2,
    "Severe": 3,
    "LifeThreatening": 4
  };
  return severityMap[severity] || 1;
};

const removeAllergy = async (allergyId) => {
  try {
    console.log("🟡 Deleting allergy:", allergyId);
    const success = await allergyService.deleteAllergy(allergyId);
    
    if (success) {
     
      const updatedAllergies = userAllergies.filter(allergy => allergy.id !== allergyId);
      setUserAllergies(updatedAllergies);
      setProfile(prev => ({
        ...prev,
        allergies: updatedAllergies
      }));

      toast.success("Allergy deleted successfully!");
    } else {
      toast.error("Failed to delete allergy");
    }
  } catch (error) {
    console.error("❌ Error deleting allergy:", error);
    toast.error("Error deleting allergy");
  }
};

const addCondition = () => {
  if (newCondition.trim()) {
    setProfile(prev => ({
      ...prev,
      conditions: [...prev.conditions, newCondition.trim()]
    }));
    setNewCondition("");
  }
};

const removeCondition = (index) => {
  setProfile(prev => ({
    ...prev,
    conditions: prev.conditions.filter((_, i) => i !== index)
  }));
};
  const updateNotificationSetting = (key, value) => {
    setProfile({
      ...profile,
      notifications: {
        ...profile.notifications,
        [key]: value
      }
    });
  };

  const handleInputChange = (e, section, field = null) => {
    const { name, value } = e.target;
    
    if (section === 'emergencyContact') {
      setProfile({
        ...profile,
        emergencyContact: {
          ...profile.emergencyContact,
          [name]: value
        }
      });
    } else if (section === 'address') {
      setProfile({
        ...profile,
        address: {
          ...profile.address,
          [name]: value
        }
      });
    } else if (section === 'main') {
      setProfile({
        ...profile,
        [name]: value
      });
    }
  };

const handleKeyPress = (e, action) => {
  if (e.key === 'Enter') {
    e.preventDefault();
    if (action === addAllergy && newAllergy.allergenName?.trim()) {
      addAllergy();
    } else if (action === addCondition && newCondition.trim()) {
      addCondition();
    }
  }
};

  // Funksion për të shfaqur ID-në e user-it për debug
  const getUserIdDisplay = () => {
    if (!user || !user.id) return "Unknown User";
    return `User ID: ${user.id}`;
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900 flex items-center justify-center">
        <Loader2 className="w-16 h-16 animate-spin text-blue-500" />
      </div>
    );
  }

  if (!user) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900 flex items-center justify-center">
        <div className="text-center">
          <p className="text-red-400 mb-4">Please log in to view your profile</p>
          <button 
            onClick={() => router.push("/login")}
            className="px-6 py-3 bg-blue-600 text-white rounded-xl hover:bg-blue-700 transition-colors"
          >
            Go to Login
          </button>
        </div>
      </div>
    );
  }

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
                <User className="w-7 h-7 text-white" />
              </div>
              <div>
                <h1 className="text-3xl font-bold bg-gradient-to-r from-slate-100 to-slate-300 bg-clip-text text-transparent">
                  Profile & Settings
                </h1>
                <p className="text-slate-400">
                  {user.firstName} {user.lastName} • {getUserIdDisplay()}
                </p>
              </div>
            </div>
            
            <div className="flex items-center space-x-4">
              <button
                onClick={() => router.push("/dashboard/dashboard")}
                className="flex items-center space-x-2 px-6 py-3 bg-gradient-to-r from-gray-600 to-gray-700 text-white rounded-xl font-bold shadow-xl hover:shadow-gray-500/50 transition-all"
              >
                Back to Dashboard
              </button>
              
              {isEditing && (
                <button
                  onClick={saveAllChanges}
                  disabled={saving}
                  className="flex items-center space-x-2 px-6 py-3 bg-gradient-to-r from-emerald-600 to-teal-600 text-white rounded-xl font-bold shadow-xl hover:shadow-emerald-500/50 transition-all disabled:opacity-50"
                >
                  {saving ? (
                    <Loader2 className="w-5 h-5 animate-spin" />
                  ) : (
                    <Save className="w-5 h-5" />
                  )}
                  <span>Save All Changes</span>
                </button>
              )}
              
              <button
                onClick={() => setIsEditing(!isEditing)}
                className="flex items-center space-x-2 px-6 py-3 bg-gradient-to-r from-blue-600 to-indigo-600 text-white rounded-xl font-bold shadow-xl hover:shadow-blue-500/50 transition-all"
              >
                <span>{isEditing ? "Cancel Editing" : "Edit Profile"}</span>
              </button>
            </div>
          </div>
        </div>
      </header>

      <main className="max-w-6xl mx-auto px-6 py-8 relative z-10">
        {/* Personal Information Card */}
        <div className="bg-slate-800/50 backdrop-blur-xl rounded-2xl p-8 border border-slate-700/50 mb-6">
          <div className="flex items-center gap-3 mb-6">
            <User className="w-7 h-7 text-blue-400" />
            <h2 className="text-2xl font-bold text-white">Personal Information</h2>
          </div>
          <p className="text-slate-400 mb-6">Your basic health profile</p>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label className="block text-sm font-medium text-slate-300 mb-2">
                First Name
              </label>
              {isEditing ? (
                <input
                  type="text"
                  name="firstName"
                  value={profile.firstName}
                  onChange={(e) => handleInputChange(e, 'main')}
                  className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  placeholder="First Name"
                />
              ) : (
                <p className="p-4 bg-slate-700/50 rounded-xl text-white border border-slate-600">
                  {profile.firstName || "Not set"}
                </p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-slate-300 mb-2">
                Last Name
              </label>
              {isEditing ? (
                <input
                  type="text"
                  name="lastName"
                  value={profile.lastName}
                  onChange={(e) => handleInputChange(e, 'main')}
                  className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  placeholder="Last Name"
                />
              ) : (
                <p className="p-4 bg-slate-700/50 rounded-xl text-white border border-slate-600">
                  {profile.lastName || "Not set"}
                </p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-slate-300 mb-2">
                Email
              </label>
              {isEditing ? (
                <input
                  type="email"
                  name="email"
                  value={profile.email}
                  onChange={(e) => handleInputChange(e, 'main')}
                  className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  placeholder="Email"
                />
              ) : (
                <p className="p-4 bg-slate-700/50 rounded-xl text-white border border-slate-600 flex items-center gap-2">
                  <Mail className="w-4 h-4 text-slate-400" />
                  {profile.email || "Not set"}
                </p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-slate-300 mb-2">
                Date of Birth
              </label>
              {isEditing ? (
                <input
                  type="date"
                  name="dateOfBirth"
                  value={profile.dateOfBirth}
                  onChange={(e) => handleInputChange(e, 'main')}
                  className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              ) : (
                <p className="p-4 bg-slate-700/50 rounded-xl text-white border border-slate-600 flex items-center gap-2">
                  <Calendar className="w-4 h-4 text-slate-400" />
                  {profile.dateOfBirth ? new Date(profile.dateOfBirth).toLocaleDateString() : "Not set"}
                </p>
              )}
            </div>

            <div className="md:col-span-2">
              <label className="block text-sm font-medium text-slate-300 mb-2">
                Phone
              </label>
              {isEditing ? (
                <input
                  type="tel"
                  name="phone"
                  value={profile.phone}
                  onChange={(e) => handleInputChange(e, 'main')}
                  className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  placeholder="(555) 123-4567"
                />
              ) : (
                <p className="p-4 bg-slate-700/50 rounded-xl text-white border border-slate-600 flex items-center gap-2">
                  <Phone className="w-4 h-4 text-slate-400" />
                  {profile.phone || "Not set"}
                </p>
              )}
            </div>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
                    {/* Allergies Card */}
            <div className="bg-slate-800/50 backdrop-blur-xl rounded-2xl p-8 border border-slate-700/50">
              <div className="flex items-center gap-3 mb-6">
                <AlertTriangle className="w-7 h-7 text-amber-400" />
                <h2 className="text-2xl font-bold text-white">Allergies</h2>
              </div>
              <p className="text-slate-400 mb-4">
                List any allergies for medication safety checks
              </p>

              {isEditing && (
                <div className="space-y-4 mb-4">
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-slate-300 mb-2">
                        Allergy Name *
                      </label>
                      <input
                        type="text"
                        value={newAllergy.allergenName || ""}
                        onChange={(e) => setNewAllergy(prev => ({...prev, allergenName: e.target.value}))}
                        placeholder="e.g., Penicillin, Sulfa, Aspirin"
                        className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-slate-300 mb-2">
                        Severity
                      </label>
                      <select
                        value={newAllergy.severity || "Mild"}
                        onChange={(e) => setNewAllergy(prev => ({...prev, severity: e.target.value}))}
                        className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      >
                        <option value="Mild">Mild</option>
                        <option value="Moderate">Moderate</option>
                        <option value="Severe">Severe</option>
                        <option value="LifeThreatening">Life Threatening</option>
                      </select>
                    </div>
                  </div>
                  
                  <div>
                    <label className="block text-sm font-medium text-slate-300 mb-2">
                      Symptoms
                    </label>
                    <input
                      type="text"
                      value={newAllergy.symptoms || ""}
                      onChange={(e) => setNewAllergy(prev => ({...prev, symptoms: e.target.value}))}
                      placeholder="e.g., Rash, Difficulty breathing, Swelling"
                      className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    />
                  </div>

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-slate-300 mb-2">
                        Diagnosed Date
                      </label>
                      <input
                        type="date"
                        value={newAllergy.diagnosedDate || ""}
                        onChange={(e) => setNewAllergy(prev => ({...prev, diagnosedDate: e.target.value}))}
                        className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-slate-300 mb-2">
                        Diagnosed By
                      </label>
                      <input
                        type="text"
                        value={newAllergy.diagnosedBy || ""}
                        onChange={(e) => setNewAllergy(prev => ({...prev, diagnosedBy: e.target.value}))}
                        placeholder="e.g., Dr. Smith"
                        className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      />
                    </div>
                  </div>

                  <div className="flex gap-2">
                    <button
                      onClick={addAllergy}
                      disabled={!newAllergy.allergenName?.trim()}
                      className="flex items-center gap-2 px-4 py-3 bg-emerald-600 text-white rounded-xl hover:bg-emerald-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      <Plus className="w-4 h-4" />
                      Add Allergy
                    </button>
                    
                    <button
                      onClick={() => setNewAllergy({})}
                      className="px-4 py-3 bg-slate-600 text-white rounded-xl hover:bg-slate-700 transition-colors"
                    >
                      Clear
                    </button>
                  </div>
                </div>
              )}

              {profile.allergies.length === 0 ? (
                <p className="text-slate-400 text-center py-8 bg-slate-700/30 rounded-xl">
                  No allergies added
                </p>
              ) : (
                <div className="space-y-3">
                  {profile.allergies.map((allergy, index) => (
              <div
                key={allergy.id || index}
                className="bg-red-500/10 border border-red-500/20 rounded-xl p-4"
              >
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-center gap-3 mb-2">
                      <AlertTriangle className="w-5 h-5 text-red-400" />
                      <h4 className="font-semibold text-white text-lg">
                        {allergy.allergenName}
                      </h4>
                      <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                        allergy.severity === 'LifeThreatening' ? 'bg-red-500/20 text-red-400' :
                        allergy.severity === 'Severe' ? 'bg-orange-500/20 text-orange-400' :
                        allergy.severity === 'Moderate' ? 'bg-yellow-500/20 text-yellow-400' :
                        'bg-blue-500/20 text-blue-400'
                      }`}>
                        {allergy.severity || 'Mild'}
                      </span>
                    </div>
                    
                    {allergy.symptoms && (
                      <p className="text-slate-300 text-sm mb-2">
                        <strong>Symptoms:</strong> {allergy.symptoms}
                      </p>
                    )}
                    
                    <div className="flex items-center gap-4 text-slate-400 text-sm">
                      {allergy.diagnosedDate && (
                        <span>
                          <Calendar className="w-3 h-3 inline mr-1" />
                          {new Date(allergy.diagnosedDate).toLocaleDateString()}
                        </span>
                      )}
                      {allergy.diagnosedBy && (
                        <span>Diagnosed by: {allergy.diagnosedBy}</span>
                      )}
                    </div>
                  </div>
                  
                  {isEditing && (
                    <button
                      onClick={() => removeAllergy(allergy.id)} 
                      className="ml-4 p-2 hover:bg-red-500/20 rounded-lg transition-colors"
                      title="Remove allergy"
                    >
                      <X className="w-4 h-4 text-red-400" />
                    </button>
                  )}
                </div>
              </div>
            ))}
                </div>
              )}
            </div>

          {/* Medical Conditions Card */}
          <div className="bg-slate-800/50 backdrop-blur-xl rounded-2xl p-8 border border-slate-700/50">
            <div className="flex items-center gap-3 mb-6">
              <Stethoscope className="w-7 h-7 text-purple-400" />
              <h2 className="text-2xl font-bold text-white">Medical Conditions</h2>
            </div>
            <p className="text-slate-400 mb-4">
              Track your current health conditions
            </p>

            {isEditing && (
              <div className="flex gap-2 mb-4">
                <input
                  type="text"
                  value={newCondition}
                  onChange={(e) => setNewCondition(e.target.value)}
                  onKeyPress={(e) => handleKeyPress(e, addCondition)}
                  placeholder="e.g., Hypertension"
                  className="flex-1 p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
                <button
                  onClick={addCondition}
                  className="px-4 py-4 bg-emerald-600 text-white rounded-xl hover:bg-emerald-700 transition-colors"
                >
                  <Plus className="w-5 h-5" />
                </button>
              </div>
            )}

            {profile.conditions.length === 0 ? (
              <p className="text-slate-400 text-center py-8 bg-slate-700/30 rounded-xl">
                No conditions added
              </p>
            ) : (
              <div className="flex flex-wrap gap-2">
                {profile.conditions.map((condition, index) => (
                  <div
                    key={index}
                    className="bg-purple-500/20 text-purple-400 px-4 py-3 rounded-xl flex items-center gap-2 border border-purple-500/30"
                  >
                    <Heart className="w-4 h-4" />
                    <span>{condition}</span>
                    {isEditing && (
                      <button
                        onClick={() => removeCondition(index)}
                        className="ml-2 hover:text-purple-300 transition-colors"
                      >
                        <X className="w-4 h-4" />
                      </button>
                    )}
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
          {/* Emergency Contact Card */}
          <div className="bg-slate-800/50 backdrop-blur-xl rounded-2xl p-8 border border-slate-700/50">
            <h2 className="text-2xl font-bold text-white mb-6">Emergency Contact</h2>
            <p className="text-slate-400 mb-6">
              Person to contact in case of emergency
            </p>

            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-slate-300 mb-2">
                  Name
                </label>
                {isEditing ? (
                  <input
                    type="text"
                    name="name"
                    value={profile.emergencyContact.name}
                    onChange={(e) => handleInputChange(e, 'emergencyContact')}
                    className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    placeholder="Jane Doe"
                  />
                ) : (
                  <p className="p-4 bg-slate-700/50 rounded-xl text-white border border-slate-600">
                    {profile.emergencyContact.name || "Not set"}
                  </p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-slate-300 mb-2">
                  Relationship
                </label>
                {isEditing ? (
                  <input
                    type="text"
                    name="relationship"
                    value={profile.emergencyContact.relationship}
                    onChange={(e) => handleInputChange(e, 'emergencyContact')}
                    className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    placeholder="Spouse"
                  />
                ) : (
                  <p className="p-4 bg-slate-700/50 rounded-xl text-white border border-slate-600">
                    {profile.emergencyContact.relationship || "Not set"}
                  </p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-slate-300 mb-2">
                  Phone
                </label>
                {isEditing ? (
                  <input
                    type="tel"
                    name="phone"
                    value={profile.emergencyContact.phone}
                    onChange={(e) => handleInputChange(e, 'emergencyContact')}
                    className="w-full p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    placeholder="(555) 987-6543"
                  />
                ) : (
                  <p className="p-4 bg-slate-700/50 rounded-xl text-white border border-slate-600 flex items-center gap-2">
                    <Phone className="w-4 h-4 text-slate-400" />
                    {profile.emergencyContact.phone || "Not set"}
                  </p>
                )}
              </div>
            </div>
          </div>

          {/* Notification Preferences Card */}
          <div className="bg-slate-800/50 backdrop-blur-xl rounded-2xl p-8 border border-slate-700/50">
            <div className="flex items-center gap-3 mb-6">
              <Bell className="w-7 h-7 text-green-400" />
              <h2 className="text-2xl font-bold text-white">Notification Preferences</h2>
            </div>
            <p className="text-slate-400 mb-6">
              Choose which reminders you want to receive
            </p>

            <div className="space-y-4">
              {[
                {
                  key: "medication",
                  label: "Medication Reminders",
                  description: "Get notified when it's time to take medication",
                  icon: Pill
                },
                {
                  key: "appointments",
                  label: "Appointment Reminders",
                  description: "Reminders before upcoming appointments",
                  icon: Calendar
                },
                {
                  key: "refills",
                  label: "Refill Reminders",
                  description: "Get notified when prescriptions need refills",
                  icon: Bell
                },
              ].map((item) => (
                <div
                  key={item.key}
                  className="flex items-center justify-between p-4 bg-slate-700/30 rounded-xl border border-slate-600/50"
                >
                  <div className="flex items-center gap-3">
                    <item.icon className="w-5 h-5 text-slate-400" />
                    <div>
                      <p className="font-medium text-white">{item.label}</p>
                      <p className="text-sm text-slate-400">{item.description}</p>
                    </div>
                  </div>
                  <label className="relative inline-flex items-center cursor-pointer">
                    <input
                      type="checkbox"
                      checked={profile.notifications[item.key]}
                      onChange={(e) =>
                        updateNotificationSetting(item.key, e.target.checked)
                      }
                      className="sr-only peer"
                      disabled={!isEditing}
                    />
                    <div className="w-11 h-6 bg-slate-600 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-slate-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-emerald-600"></div>
                  </label>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Data Privacy Notice */}
        <div className="bg-blue-500/10 backdrop-blur-xl rounded-2xl p-6 border border-blue-500/20">
          <div className="flex items-start gap-3">
            <Shield className="w-5 h-5 text-blue-400 mt-0.5" />
            <div className="text-sm text-blue-300">
              <p className="font-medium text-white mb-1">Data Privacy</p>
              <p>
                Your profile data is stored specifically for your account. 
                Each user has their own separate profile that cannot be accessed by other users.
              </p>
            </div>
          </div>
        </div>

        {/* Debug Info (mund ta fshish në production) */}
        <div className="mt-6 p-4 bg-slate-800/30 rounded-xl border border-slate-700/50">
          <p className="text-sm text-slate-400">
            <strong>Debug Info:</strong> User ID: {user?.id || "Unknown"} | 
            Profile loaded from: {localStorage.getItem(`userProfile_${user?.id}`) ? "localStorage" : "default"}
          </p>
        </div>
      </main>
    </div>
  );
};

export default Profile;