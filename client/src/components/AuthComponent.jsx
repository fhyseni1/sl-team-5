import React, { useState, useEffect } from "react";
import { useRouter } from "next/router";
import {
  Cross,
  Mail,
  Lock,
  User,
  Phone,
  ArrowRight,
  Loader2,
  Heart,
  Shield,
  Activity,
  Sparkles,
} from "lucide-react";
import { authService } from "../../services/authService";

const AuthComponent = () => {
  const [isLogin, setIsLogin] = useState(true);
  const [formData, setFormData] = useState({
    email: "",
    password: "",
    firstName: "",
    lastName: "",
    phoneNumber: "",
  });
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [startTransition, setStartTransition] = useState(false);
  const [destination, setDestination] = useState(null);
  const [showWelcome, setShowWelcome] = useState(true);
  const router = useRouter();

  useEffect(() => {
    const timer = setTimeout(() => {
      setShowWelcome(false);
    }, 3000);
    return () => clearTimeout(timer);
  }, []);

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      let response;
      if (isLogin) {
        response = await authService.login({
          email: formData.email,
          password: formData.password,
        });
      } else {
        response = await authService.register({
          email: formData.email,
          password: formData.password,
          firstName: formData.firstName,
          lastName: formData.lastName,
          phoneNumber: formData.phoneNumber,
        });
      }

      const userData = response;
      console.log("Login Response:", userData);

      // Determine destination based on user type
      let targetRoute;
      if (userData.type === 5 || userData.type === "Admin") {
        targetRoute = "/dashboard/admin-dashboard";
      } else if (
        userData.type === 4 ||
        userData.type === "HealthcareProvider"
      ) {
        targetRoute = "/dashboard/doctor-dashboard";
      } else if (userData.type === 7 || userData.type === "Assistant") {
        targetRoute = "/dashboard/assistant-dashboard";
      } else if (userData.type === 8 || userData.type === "ClinicAdmin") {
        targetRoute = "/dashboard/clinic-dashboard";
      } else {
        targetRoute = "/dashboard/dashboard";
      }

      // Trigger transition animation
      console.log("Starting sliding door animation to:", targetRoute);
      setDestination(targetRoute);
      setStartTransition(true);
    } catch (err) {
      setError(err.response?.data?.message || "Authentication failed");
      console.error("Authentication error:", err);
      setLoading(false);
    }
  };

  // Navigate after animation completes
  useEffect(() => {
    if (startTransition && destination) {
      console.log("Animation triggered, navigating in 1200ms");
      const timer = setTimeout(() => {
        console.log("Navigating to:", destination);
        router.push(destination);
      }, 1200);
      return () => clearTimeout(timer);
    }
  }, [startTransition, destination, router]);

  return (
    <div className="min-h-screen bg-gradient-to-br from-emerald-50 via-teal-50 to-cyan-50 flex items-center justify-center p-4 relative overflow-hidden">
      {/* Embedded CSS */}
      <style jsx>{`
        @keyframes slide-left {
          0% {
            transform: translateX(0);
          }
          100% {
            transform: translateX(-100%);
          }
        }
        @keyframes slide-right {
          0% {
            transform: translateX(0);
          }
          100% {
            transform: translateX(100%);
          }
        }
        @keyframes zoom-in {
          0% {
            transform: scale(1);
            opacity: 1;
          }
          100% {
            transform: scale(1.5);
            opacity: 0;
          }
        }
        @keyframes welcome-fade-out {
          0% {
            opacity: 1;
            transform: scale(1);
          }
          80% {
            opacity: 1;
            transform: scale(1.02);
          }
          100% {
            opacity: 0;
            transform: scale(1.1);
          }
        }
        @keyframes welcome-fade-in {
          0% {
            opacity: 0;
            transform: translateY(-20px);
          }
          100% {
            opacity: 1;
            transform: translateY(0);
          }
        }
        @keyframes pulse-glow {
          0%,
          100% {
            box-shadow: 0 0 20px rgba(16, 185, 129, 0.4);
          }
          50% {
            box-shadow: 0 0 40px rgba(16, 185, 129, 0.6);
          }
        }
        @keyframes float {
          0%,
          100% {
            transform: translateY(0px);
          }
          50% {
            transform: translateY(-10px);
          }
        }
        @keyframes sparkle {
          0%,
          100% {
            opacity: 0.3;
            transform: scale(0.8);
          }
          50% {
            opacity: 1;
            transform: scale(1.2);
          }
        }
        .animate-slide-left {
          animation: slide-left 800ms ease-out forwards;
        }
        .animate-slide-right {
          animation: slide-right 800ms ease-out forwards;
        }
        .animate-zoom-in {
          animation: zoom-in 400ms ease-in 800ms forwards;
        }
        .animate-welcome-fade-out {
          animation: welcome-fade-out 800ms ease-out forwards;
        }
        .animate-welcome-fade-in {
          animation: welcome-fade-in 600ms ease-out forwards;
        }
        .animate-pulse-glow {
          animation: pulse-glow 2s ease-in-out infinite;
        }
        .animate-float {
          animation: float 3s ease-in-out infinite;
        }
        .animate-sparkle {
          animation: sparkle 2s ease-in-out infinite;
        }
        .door {
          background: rgba(0, 128, 128, 0.9);
          border: 2px solid #0f766e;
          box-shadow: 0 0 10px rgba(0, 0, 0, 0.3);
        }
      `}</style>

      {/* Animated Background Elements */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        <div className="absolute top-20 left-10 w-2 h-2 bg-emerald-400 rounded-full animate-sparkle"></div>
        <div
          className="absolute top-40 right-20 w-3 h-3 bg-teal-400 rounded-full animate-sparkle"
          style={{ animationDelay: "0.5s" }}
        ></div>
        <div
          className="absolute bottom-32 left-1/4 w-2 h-2 bg-cyan-400 rounded-full animate-sparkle"
          style={{ animationDelay: "1s" }}
        ></div>
        <div
          className="absolute top-1/3 right-1/3 w-2 h-2 bg-emerald-300 rounded-full animate-sparkle"
          style={{ animationDelay: "1.5s" }}
        ></div>
        <div className="absolute -top-40 -right-40 w-96 h-96 bg-emerald-200/30 rounded-full blur-3xl"></div>
        <div className="absolute -bottom-40 -left-40 w-96 h-96 bg-cyan-200/30 rounded-full blur-3xl"></div>
      </div>

      {/* Welcome Screen */}
      {showWelcome && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-gradient-to-br from-emerald-500 via-teal-500 to-cyan-500 animate-welcome-fade-out">
          <div className="text-center animate-welcome-fade-in">
            <div className="mb-8 relative">
              <div className="inline-flex items-center justify-center w-32 h-32 bg-white/20 backdrop-blur-xl rounded-3xl animate-pulse-glow border-4 border-white/30">
                <Cross
                  className="w-16 h-16 text-white animate-float"
                  strokeWidth={2.5}
                />
              </div>
              <Sparkles
                className="absolute -top-2 -right-2 w-8 h-8 text-yellow-300 animate-spin"
                style={{ animationDuration: "3s" }}
              />
            </div>
            <h1 className="text-6xl font-bold text-white mb-4 tracking-tight">
              MedicTrack
            </h1>
            <p className="text-2xl text-white/90 font-light">
              Your Health, Simplified
            </p>
          </div>
        </div>
      )}

      {/* Transition Overlay */}
      {startTransition && (
        <div className="fixed inset-0 z-50 bg-gray-900/80">
          <div
            className="absolute left-0 w-1/2 h-full door animate-slide-left"
            style={{ zIndex: 60 }}
          >
            <div className="absolute inset-y-0 right-0 w-3 bg-teal-800" />
          </div>
          <div
            className="absolute right-0 w-1/2 h-full door animate-slide-right"
            style={{ zIndex: 60 }}
          >
            <div className="absolute inset-y-0 left-0 w-3 bg-teal-800" />
          </div>
          <div
            className="absolute inset-0 bg-gradient-to-br from-emerald-50 via-teal-50 to-cyan-50 animate-zoom-in"
            style={{ zIndex: 55 }}
          />
        </div>
      )}

      {/* Main Content */}
      <div
        className={`w-full max-w-6xl mx-auto grid lg:grid-cols-2 gap-8 items-center transition-all duration-700 ${
          showWelcome ? "opacity-0" : "opacity-100"
        }`}
      >
        {/* Left Side - Hero Section */}
        <div className="hidden lg:flex flex-col justify-center space-y-8 px-8">
          <div className="space-y-4">
            <div className="inline-flex items-center gap-2 px-4 py-2 bg-emerald-100 rounded-full">
              <Sparkles className="w-4 h-4 text-emerald-600" />
              <span className="text-sm font-semibold text-emerald-700">
                Healthcare Made Simple
              </span>
            </div>
            <h1 className="text-5xl font-bold text-gray-900 leading-tight">
              Welcome to
              <br />
              <span className="text-transparent bg-clip-text bg-gradient-to-r from-emerald-600 via-teal-600 to-cyan-600">
                MedicTrack
              </span>
            </h1>
            <p className="text-xl text-gray-600 leading-relaxed">
              Your complete healthcare companion. Track appointments, manage
              medications, and stay connected with your healthcare providers.
            </p>
          </div>

          <div className="space-y-4">
            <div className="flex items-start gap-4 p-4 bg-white rounded-2xl shadow-lg">
              <div className="p-3 bg-emerald-100 rounded-xl">
                <Heart className="w-6 h-6 text-emerald-600" />
              </div>
              <div>
                <h3 className="font-semibold text-gray-900 mb-1">
                  Personalized Care
                </h3>
                <p className="text-sm text-gray-600">
                  Tailored health tracking for your unique needs
                </p>
              </div>
            </div>
            <div className="flex items-start gap-4 p-4 bg-white rounded-2xl shadow-lg">
              <div className="p-3 bg-teal-100 rounded-xl">
                <Shield className="w-6 h-6 text-teal-600" />
              </div>
              <div>
                <h3 className="font-semibold text-gray-900 mb-1">
                  Secure & Private
                </h3>
                <p className="text-sm text-gray-600">
                  Your health data protected with enterprise-grade security
                </p>
              </div>
            </div>
            <div className="flex items-start gap-4 p-4 bg-white rounded-2xl shadow-lg">
              <div className="p-3 bg-cyan-100 rounded-xl">
                <Activity className="w-6 h-6 text-cyan-600" />
              </div>
              <div>
                <h3 className="font-semibold text-gray-900 mb-1">
                  Real-time Updates
                </h3>
                <p className="text-sm text-gray-600">
                  Stay informed with instant health insights
                </p>
              </div>
            </div>
          </div>
        </div>

        {/* Right Side - Form */}
        <div
          className={`transition-all duration-300 ${
            startTransition ? "opacity-0 scale-95" : "opacity-100 scale-100"
          }`}
        >
          <div className="bg-white rounded-3xl shadow-2xl p-8 sm:p-10 border border-gray-100">
            <div className="text-center mb-8">
              <div className="inline-flex items-center justify-center w-16 h-16 bg-gradient-to-br from-emerald-500 to-teal-500 rounded-2xl mb-4 shadow-lg">
                <Cross className="w-8 h-8 text-white" strokeWidth={2.5} />
              </div>
              <h2 ව className="text-3xl font-bold text-gray-900 mb-2">
                {isLogin ? "Welcome Back" : "Get Started"}
              </h2>
              <p className="text-gray-600">
                {isLogin
                  ? "Sign in to continue your health journey"
                  : "Create your account to begin"}
              </p>
            </div>

            <form onSubmit={handleSubmit} className="space-y-5">
              {!isLogin && (
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label
                      htmlFor="firstName"
                      className="block text-sm font-semibold text-gray-700 mb-2"
                    >
                      First Name
                    </label>
                    <div className="relative group">
                      <User className="absolute left-3.5 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 group-focus-within:text-emerald-500 transition-colors" />
                      <input
                        type="text"
                        id="firstName"
                        name="firstName"
                        placeholder="John"
                        value={formData.firstName}
                        onChange={handleChange}
                        required={!isLogin}
                        className="w-full pl-11 pr-4 py-3 bg-gray-50 border border-gray-200 rounded-xl text-gray-900 placeholder:text-gray-400 focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 focus:bg-white transition-all outline-none"
                      />
                    </div>
                  </div>
                  <div>
                    <label
                      htmlFor="lastName"
                      className="block text-sm font-semibold text-gray-700 mb-2"
                    >
                      Last Name
                    </label>
                    <div className="relative group">
                      <User className="absolute left-3.5 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 group-focus-within:text-emerald-500 transition-colors" />
                      <input
                        type="text"
                        id="lastName"
                        name="lastName"
                        placeholder="Doe"
                        value={formData.lastName}
                        onChange={handleChange}
                        required={!isLogin}
                        className="w-full pl-11 pr-4 py-3 bg-gray-50 border border-gray-200 rounded-xl text-gray-900 placeholder:text-gray-400 focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 focus:bg-white transition-all outline-none"
                      />
                    </div>
                  </div>
                </div>
              )}

              {!isLogin && (
                <div>
                  <label
                    htmlFor="phoneNumber"
                    className="block text-sm font-semibold text-gray-700 mb-2"
                  >
                    Phone Number
                  </label>
                  <div className="relative group">
                    <Phone className="absolute left-3.5 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 group-focus-within:text-emerald-500 transition-colors" />
                    <input
                      type="tel"
                      id="phoneNumber"
                      name="phoneNumber"
                      placeholder="+1 (555) 000-0000"
                      value={formData.phoneNumber}
                      onChange={handleChange}
                      required={!isLogin}
                      className="w-full pl-11 pr-4 py-3 bg-gray-50 border border-gray-200 rounded-xl text-gray-900 placeholder:text-gray-400 focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 focus:bg-white transition-all outline-none"
                    />
                  </div>
                </div>
              )}

              <div>
                <label
                  htmlFor="email"
                  className="block text-sm font-semibold text-gray-700 mb-2"
                >
                  Email Address
                </label>
                <div className="relative group">
                  <Mail className="absolute left-3.5 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 group-focus-within:text-emerald-500 transition-colors" />
                  <input
                    type="email"
                    id="email"
                    name="email"
                    placeholder="you@example.com"
                    value={formData.email}
                    onChange={handleChange}
                    required
                    className="w-full pl-11 pr-4 py-3 bg-gray-50 border border-gray-200 rounded-xl text-gray-900 placeholder:text-gray-400 focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 focus:bg-white transition-all outline-none"
                  />
                </div>
              </div>

              <div>
                <label
                  htmlFor="password"
                  className="block text-sm font-semibold text-gray-700 mb-2"
                >
                  Password
                </label>
                <div className="relative group">
                  <Lock className="absolute left-3.5 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 group-focus-within:text-emerald-500 transition-colors" />
                  <input
                    type="password"
                    id="password"
                    name="password"
                    placeholder="••••••••"
                    value={formData.password}
                    onChange={handleChange}
                    required
                    className="w-full pl-11 pr-4 py-3 bg-gray-50 border border-gray-200 rounded-xl text-gray-900 placeholder:text-gray-400 focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 focus:bg-white transition-all outline-none"
                  />
                </div>
              </div>

              {error && (
                <div className="bg-red-50 border border-red-200 rounded-xl p-4">
                  <p className="text-sm text-red-700 font-medium">{error}</p>
                </div>
              )}

              <button
                type="submit"
                disabled={loading}
                className="w-full bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-700 hover:to-teal-700 text-white font-semibold py-3.5 px-6 rounded-xl transition-all duration-200 flex items-center justify-center disabled:opacity-60 disabled:cursor-not-allowed shadow-lg shadow-emerald-500/30 hover:shadow-xl hover:shadow-emerald-500/40 hover:-translate-y-0.5"
              >
                {loading ? (
                  <>
                    <Loader2 className="w-5 h-5 mr-2 animate-spin" />
                    {isLogin ? "Signing in..." : "Creating account..."}
                  </>
                ) : (
                  <>
                    {isLogin ? "Sign In" : "Create Account"}
                    <ArrowRight className="w-5 h-5 ml-2" />
                  </>
                )}
              </button>
            </form>

            <div className="mt-8 text-center">
              <p className="text-gray-600">
                {isLogin
                  ? "Don't have an account?"
                  : "Already have an account?"}{" "}
                <button
                  onClick={() => setIsLogin(!isLogin)}
                  className="text-emerald-600 hover:text-emerald-700 font-semibold transition-colors"
                >
                  {isLogin ? "Sign Up" : "Sign In"}
                </button>
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AuthComponent;
