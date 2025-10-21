"use client";

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
} from "lucide-react";
import { authService } from "../../services/authService"; // Import authService

const Dashboard = () => {
  const [activeUsersCount, setActiveUsersCount] = useState(null);
  const [user, setUser] = useState(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const router = useRouter();

  useEffect(() => {
    const fetchData = async () => {
      try {
        // Fetch current user details
        const userData = await authService.getMe();
        setUser({
          firstName: userData.firstName,
          lastName: userData.lastName,
          email: userData.email,
        });

        // Fetch active users count
        const count = await authService.getActiveUsersCount();
        setActiveUsersCount(count);
      } catch (err) {
        setError(
          err.response?.data?.message || "Failed to load dashboard data"
        );
        if (err.response?.status === 401) {
          // The axios interceptor in authService should redirect to /login on 401
          router.push("/");
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
      setUser(null);
      router.push("/");
    } catch (err) {
      console.error("Logout error:", err);
      setUser(null);
      router.push("/");
    }
  };

  // Rest of the component remains unchanged
  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900 flex items-center justify-center relative overflow-hidden">
        <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_right,_var(--tw-gradient-stops))] from-blue-900/20 via-transparent to-emerald-900/20"></div>
        <div className="text-center relative z-10">
          <Loader2 className="w-16 h-16 text-blue-500 animate-spin mx-auto mb-6" />
          <p className="text-slate-300 font-semibold text-lg">
            Loading your dashboard...
          </p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900 flex items-center justify-center p-4 relative overflow-hidden">
        <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_right,_var(--tw-gradient-stops))] from-blue-900/20 via-transparent to-emerald-900/20"></div>
        <div className="bg-white/95 backdrop-blur-xl rounded-3xl shadow-2xl p-10 max-w-md w-full border border-slate-200/50 relative z-10">
          <AlertCircle className="w-16 h-16 text-red-500 mx-auto mb-6" />
          <h2 className="text-2xl font-bold text-slate-900 text-center mb-3">
            Error
          </h2>
          <p className="text-red-600 text-center font-medium">{error}</p>
        </div>
      </div>
    );
  }

  if (!user) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900 flex items-center justify-center">
        <div className="text-center">
          <p className="text-slate-300 font-medium">Please log in</p>
        </div>
      </div>
    );
  }

  const stats = [
    {
      icon: Pill,
      label: "Active Medications",
      value: "12",
      change: "+2 this week",
      gradient: "from-blue-500 to-cyan-500",
      bgGradient: "from-blue-500/10 to-cyan-500/10",
      iconBg: "bg-blue-500/20",
      iconColor: "text-blue-400",
    },
    {
      icon: Calendar,
      label: "Upcoming Appointments",
      value: "3",
      change: "Next: Tomorrow",
      gradient: "from-emerald-500 to-teal-500",
      bgGradient: "from-emerald-500/10 to-teal-500/10",
      iconBg: "bg-emerald-500/20",
      iconColor: "text-emerald-400",
    },
    {
      icon: Bell,
      label: "Pending Reminders",
      value: "5",
      change: "2 due today",
      gradient: "from-amber-500 to-orange-500",
      bgGradient: "from-amber-500/10 to-orange-500/10",
      iconBg: "bg-amber-500/20",
      iconColor: "text-amber-400",
    },
    {
      icon: Users,
      label: "Active Users",
      value: activeUsersCount !== null ? activeUsersCount.toString() : "...",
      change: "System-wide",
      gradient: "from-violet-500 to-purple-500",
      bgGradient: "from-violet-500/10 to-purple-500/10",
      iconBg: "bg-violet-500/20",
      iconColor: "text-violet-400",
    },
  ];

  const recentActivity = [
    {
      icon: CheckCircle2,
      title: "Medication Taken",
      description: "Aspirin 100mg - Morning dose",
      time: "2 hours ago",
      iconColor: "text-emerald-400",
      bgColor: "bg-emerald-500/10",
      borderColor: "border-emerald-500/20",
    },
    {
      icon: Bell,
      title: "Reminder Set",
      description: "Evening medication reminder",
      time: "4 hours ago",
      iconColor: "text-blue-400",
      bgColor: "bg-blue-500/10",
      borderColor: "border-blue-500/20",
    },
    {
      icon: Calendar,
      title: "Appointment Scheduled",
      description: "Dr. Smith - Cardiology checkup",
      time: "Yesterday",
      iconColor: "text-teal-400",
      bgColor: "bg-teal-500/10",
      borderColor: "border-teal-500/20",
    },
  ];

  const upcomingDoses = [
    { medication: "Aspirin 100mg", time: "14:00", status: "pending" },
    { medication: "Vitamin D 1000IU", time: "18:00", status: "pending" },
    { medication: "Omega-3 Fish Oil", time: "20:00", status: "pending" },
  ];

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900 relative">
      <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_right,_var(--tw-gradient-stops))] from-blue-900/20 via-transparent to-emerald-900/20"></div>
      <div className="absolute inset-0 bg-grid-white/[0.02] bg-[size:50px_50px]"></div>

      <header className="bg-slate-900/80 backdrop-blur-xl border-b border-slate-700/50 sticky top-0 z-50 shadow-2xl">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-5">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4">
              <div className="flex items-center justify-center w-14 h-14 bg-gradient-to-br from-blue-600 to-emerald-600 rounded-2xl shadow-lg shadow-blue-500/30 transform hover:scale-105 transition-transform duration-300">
                <Pill className="w-7 h-7 text-white" />
              </div>
              <div>
                <h1 className="text-2xl font-bold bg-gradient-to-r from-slate-100 to-slate-300 bg-clip-text text-transparent">
                  MediTrack
                </h1>
                <p className="text-sm text-slate-400 font-medium">
                  Your Health Companion
                </p>
              </div>
            </div>
            <button
              onClick={handleLogout}
              className="flex items-center space-x-2 px-5 py-2.5 bg-gradient-to-r from-red-600 to-red-700 hover:from-red-700 hover:to-red-800 text-white rounded-xl transition-all duration-300 shadow-xl shadow-red-500/30 hover:shadow-red-500/50 hover:scale-105 transform font-semibold"
            >
              <LogOut className="w-4 h-4" />
              <span>Logout</span>
            </button>
          </div>
        </div>
      </header>

      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10 relative z-10">
        <div className="mb-10">
          <div className="flex items-center space-x-3 mb-3">
            <h2 className="text-4xl font-bold bg-gradient-to-r from-slate-100 via-blue-100 to-slate-100 bg-clip-text text-transparent">
              Welcome back, {user.firstName}!
            </h2>
            <Sparkles className="w-8 h-8 text-yellow-400 animate-pulse" />
          </div>
          <p className="text-slate-400 flex items-center space-x-2 text-lg">
            <Activity className="w-5 h-5" />
            <span>Here's your health overview for today</span>
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-10">
          {stats.map((stat, index) => {
            const Icon = stat.icon;
            return (
              <div
                key={index}
                className={`bg-gradient-to-br ${stat.bgGradient} backdrop-blur-xl rounded-2xl shadow-xl p-6 border border-slate-700/50 hover:shadow-2xl transition-all duration-300 transform hover:-translate-y-2 hover:scale-105 group`}
              >
                <div className="flex items-center justify-between mb-5">
                  <div
                    className={`${stat.iconBg} p-3.5 rounded-xl backdrop-blur-sm border border-slate-600/30 group-hover:scale-110 transition-transform duration-300`}
                  >
                    <Icon className={`w-6 h-6 ${stat.iconColor}`} />
                  </div>
                  <TrendingUp className="w-4 h-4 text-emerald-400" />
                </div>
                <h3 className="text-sm font-semibold text-slate-400 mb-2 uppercase tracking-wider">
                  {stat.label}
                </h3>
                <p className="text-4xl font-bold bg-gradient-to-r from-slate-100 to-slate-300 bg-clip-text text-transparent mb-2">
                  {stat.value}
                </p>
                <p className="text-xs text-slate-500 font-medium">
                  {stat.change}
                </p>
              </div>
            );
          })}
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-10">
          <div className="lg:col-span-2 bg-slate-800/50 backdrop-blur-xl rounded-2xl shadow-2xl p-8 border border-slate-700/50">
            <div className="flex items-center justify-between mb-8">
              <h3 className="text-2xl font-bold text-slate-100 flex items-center space-x-3">
                <Activity className="w-6 h-6 text-teal-400" />
                <span>Recent Activity</span>
              </h3>
              <button className="text-sm text-blue-400 hover:text-blue-300 font-bold flex items-center space-x-1 group">
                <span>View All</span>
                <ArrowUpRight className="w-4 h-4 group-hover:translate-x-1 group-hover:-translate-y-1 transition-transform" />
              </button>
            </div>
            <div className="space-y-4">
              {recentActivity.map((activity, index) => {
                const Icon = activity.icon;
                return (
                  <div
                    key={index}
                    className={`flex items-start space-x-4 p-5 rounded-xl ${activity.bgColor} border ${activity.borderColor} hover:bg-slate-700/30 transition-all duration-300 transform hover:scale-[1.02] group`}
                  >
                    <div
                      className={`${activity.bgColor} p-3 rounded-xl border ${activity.borderColor} group-hover:scale-110 transition-transform`}
                    >
                      <Icon className={`w-5 h-5 ${activity.iconColor}`} />
                    </div>
                    <div className="flex-1">
                      <h4 className="font-bold text-slate-200 mb-1">
                        {activity.title}
                      </h4>
                      <p className="text-sm text-slate-400">
                        {activity.description}
                      </p>
                    </div>
                    <span className="text-xs text-slate-500 flex items-center space-x-1 font-medium">
                      <Clock className="w-3 h-3" />
                      <span>{activity.time}</span>
                    </span>
                  </div>
                );
              })}
            </div>
          </div>

          <div className="bg-slate-800/50 backdrop-blur-xl rounded-2xl shadow-2xl p-8 border border-slate-700/50">
            <div className="flex items-center justify-between mb-8">
              <h3 className="text-2xl font-bold text-slate-100 flex items-center space-x-3">
                <Clock className="w-6 h-6 text-blue-400" />
                <span>Today's Schedule</span>
              </h3>
            </div>
            <div className="space-y-4">
              {upcomingDoses.map((dose, index) => (
                <div
                  key={index}
                  className="flex items-center justify-between p-4 bg-gradient-to-r from-blue-500/10 to-teal-500/10 rounded-xl border border-blue-500/20 hover:border-blue-400/40 transition-all duration-300 transform hover:scale-105 group"
                >
                  <div className="flex-1">
                    <p className="font-bold text-slate-200 text-sm mb-1">
                      {dose.medication}
                    </p>
                    <p className="text-xs text-slate-400 flex items-center space-x-1">
                      <Clock className="w-3 h-3" />
                      <span>{dose.time}</span>
                    </p>
                  </div>
                  <div className="w-3 h-3 bg-amber-400 rounded-full animate-pulse shadow-lg shadow-amber-400/50"></div>
                </div>
              ))}
            </div>
            <button className="w-full mt-6 bg-gradient-to-r from-blue-600 to-emerald-600 hover:from-blue-700 hover:to-emerald-700 text-white font-bold py-4 px-6 rounded-xl transition-all duration-300 flex items-center justify-center space-x-2 shadow-xl shadow-blue-500/30 hover:shadow-blue-500/50 hover:scale-105 transform">
              <Bell className="w-5 h-5" />
              <span>Set New Reminder</span>
            </button>
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-10">
          <div className="bg-gradient-to-br from-emerald-600 to-teal-700 rounded-2xl shadow-2xl p-8 text-white relative overflow-hidden group hover:scale-105 transition-transform duration-300">
            <div className="absolute inset-0 bg-[radial-gradient(circle_at_top_right,_var(--tw-gradient-stops))] from-white/10 via-transparent to-transparent"></div>
            <div className="relative z-10">
              <div className="flex items-center space-x-3 mb-5">
                <div className="bg-white/20 p-4 rounded-2xl backdrop-blur-sm border border-white/30 group-hover:scale-110 transition-transform">
                  <Shield className="w-7 h-7" />
                </div>
                <h3 className="text-2xl font-bold">Health Insights</h3>
              </div>
              <p className="text-white/95 mb-6 leading-relaxed text-base">
                You're maintaining an excellent medication adherence rate of
                98%. Keep up the great work!
              </p>
              <button className="bg-white text-emerald-600 hover:bg-slate-50 font-bold py-3 px-6 rounded-xl transition-all duration-300 shadow-lg hover:shadow-xl hover:scale-105 transform">
                View Detailed Report
              </button>
            </div>
          </div>

          <div className="bg-slate-800/50 backdrop-blur-xl rounded-2xl shadow-2xl p-8 border border-slate-700/50">
            <div className="flex items-center space-x-3 mb-6">
              <div className="bg-amber-500/20 p-4 rounded-2xl border border-amber-500/30">
                <AlertCircle className="w-7 h-7 text-amber-400" />
              </div>
              <h3 className="text-2xl font-bold text-slate-100">
                Quick Actions
              </h3>
            </div>
            <div className="space-y-3">
              <button className="w-full text-left px-5 py-4 bg-slate-700/50 hover:bg-slate-600/50 rounded-xl transition-all duration-300 border border-slate-600/50 font-semibold text-slate-200 hover:scale-105 transform hover:shadow-lg">
                Add New Medication
              </button>
              <button className="w-full text-left px-5 py-4 bg-slate-700/50 hover:bg-slate-600/50 rounded-xl transition-all duration-300 border border-slate-600/50 font-semibold text-slate-200 hover:scale-105 transform hover:shadow-lg">
                Schedule Appointment
              </button>
              <button className="w-full text-left px-5 py-4 bg-slate-700/50 hover:bg-slate-600/50 rounded-xl transition-all duration-300 border border-slate-600/50 font-semibold text-slate-200 hover:scale-105 transform hover:shadow-lg">
                Check Drug Interactions
              </button>
            </div>
          </div>
        </div>

        <div className="bg-slate-800/50 backdrop-blur-xl rounded-2xl shadow-2xl p-8 border border-slate-700/50">
          <div className="flex flex-col md:flex-row md:items-center md:justify-between">
            <div className="mb-6 md:mb-0">
              <h3 className="text-xl font-bold text-slate-100 mb-2">
                Account Information
              </h3>
              <p className="text-sm text-slate-400">
                Manage your personal details and preferences
              </p>
            </div>
            <div className="space-y-3 md:space-y-0 md:space-x-4 md:flex">
              <div className="bg-blue-500/10 px-6 py-3 rounded-xl border border-blue-500/20">
                <p className="text-xs text-slate-400 mb-1 font-semibold uppercase tracking-wider">
                  Email
                </p>
                <p className="font-bold text-slate-200">{user.email}</p>
              </div>
              <div className="bg-emerald-500/10 px-6 py-3 rounded-xl border border-emerald-500/20">
                <p className="text-xs text-slate-400 mb-1 font-semibold uppercase tracking-wider">
                  Name
                </p>
                <p className="font-bold text-slate-200">
                  {user.firstName} {user.lastName}
                </p>
              </div>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
};

export default Dashboard;
