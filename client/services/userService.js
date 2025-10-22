import { api } from "./authService";

export const userService = {
  async getActiveUsersCount() {
    const res = await api.get("/users/stats/active-count");
    return res.data;
  },

  async getUpcomingAppointmentsCount(userId) {
    const res = await api.get(`/appointments/user/${userId}/upcoming/count`);
    return res.data;
  },

  async getUserAppointments(userId) {
    const res = await api.get(`/appointments/user/${userId}`);
    return res.data;
  },

  async getAllAppointments() {
    const res = await api.get("/appointments");
    return res.data;
  },

  async getApprovedAppointments() {
    const res = await api.get("/appointments?status=approved");
    return res.data;
  },
};

export default userService;
