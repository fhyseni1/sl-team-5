// client/src/services/authService.js
import axios from "axios";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5029";

const api = axios.create({
  baseURL: API_URL,
  withCredentials: true,
  headers: {
    "Content-Type": "application/json",
    Accept: "application/json",
  },
});

api.interceptors.request.use((config) => {
  const token = document.cookie
    .split("; ")
    .find((row) => row.startsWith("access_token="))
    ?.split("=")[1];
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  console.log("Request headers:", config.headers);
  return config;
});

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    // Skip retry for refresh-token endpoint
    if (
      error.response?.status === 401 &&
      !originalRequest._retry &&
      originalRequest.url !== `${API_URL}/auth/refresh-token`
    ) {
      originalRequest._retry = true;
      try {
        console.log("Attempting to refresh token");
        const response = await api.post(
          "/auth/refresh-token",
          {},
          { withCredentials: true }
        );
        console.log("Token refreshed:", response.data);
        // Note: Cookies are set by the backend, no need to set them here
        return api(originalRequest);
      } catch (refreshError) {
        console.error(
          "Refresh token failed:",
          refreshError.response?.data || refreshError.message
        );
        document.cookie =
          "access_token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
        document.cookie =
          "refresh_token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
        window.location.href = "/login";
        return Promise.reject(refreshError);
      }
    }
    console.error("Request failed:", error.response?.data || error.message);
    return Promise.reject(error);
  }
);

export const authService = {
  async register(userData) {
    const res = await api.post("/auth/register", userData);
    return res.data;
  },

  async login(credentials) {
    try {
      const res = await api.post("/auth/login", credentials);
      if (res.data && res.data.type) {
        localStorage.setItem("userType", res.data.type);
      }
      return res.data;
    } catch (error) {
      console.error("Login error:", error.response?.data || error.message);
      throw error;
    }
  },

  async logout() {
    try {
      await api.post("/auth/logout");
    } catch (e) {
      console.log("Logout failed", e);
    }
    document.cookie =
      "access_token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
    document.cookie =
      "refresh_token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
    localStorage.removeItem("userType");
  },

  async getMe() {
    const res = await api.get("/auth/me");
    return res.data;
  },

  async changePassword(data) {
    const res = await api.post("/auth/change-password", data);
    return res.data;
  },

  async refreshToken() {
    const response = await api.post(
      "/auth/refresh-token",
      {},
      { withCredentials: true }
    );
    return response.data;
  },
};

export default authService;
export { api };
