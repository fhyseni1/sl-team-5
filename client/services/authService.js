import axios from "axios";

const API_URL = process.env.NEXT_PUBLIC_API_URL;

const api = axios.create({
  baseURL: API_URL,
  withCredentials: true,
});

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401 && !error.config._retry) {
      error.config._retry = true;
      try {
        await api.post("/auth/refresh-token", { refreshToken: "" });
        return api(error.config);
      } catch (refreshError) {
        window.location.href = "/login";
        return Promise.reject(refreshError);
      }
    }
    return Promise.reject(error);
  }
);

export const authService = {
  async register(userData) {
    const response = await api.post("/auth/register", userData);
    return response.data;
  },

  async login(credentials) {
    const response = await api.post("/auth/login", credentials);
    return response.data;
  },

  async logout() {
    await api.post("auth/logout");
  },

  async getMe() {
    const response = await api.get("/auth/me");
    return response.data;
  },

  async changePassword(passwordData) {
    const response = await api.post("/auth/change-password", passwordData);
    return response.data;
  },
  async getActiveUsersCount() {
    const response = await api.get("users/stats/active-count");
    return response.data;
  },
};

export default authService;
