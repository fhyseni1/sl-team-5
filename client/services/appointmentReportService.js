import api from './api';

const appointmentReportService = {
 createReport: async (reportData) => {
    try {
      const response = await api.post('/api/appointmentreports', reportData);
      return response.data;
    } catch (error) {
      throw error.response?.data || error.message;
    }
  },

  getReport: async (id) => {
    try {
      const response = await api.get(`/api/appointmentreports/${id}`);
      return response.data;
    } catch (error) {
      throw error.response?.data || error.message;
    }
  },

  getReportByAppointment: async (appointmentId) => {
    try {
      const response = await api.get(`/api/appointmentreports/appointment/${appointmentId}`);
      return response.data;
    } catch (error) {
      throw error.response?.data || error.message;
    }
  },

  getDoctorReports: async (doctorId) => {
    try {
      const response = await api.get(`/api/appointmentreports/doctor/${doctorId}`);
      return response.data;
    } catch (error) {
      throw error.response?.data || error.message;
    }
  },

  updateReport: async (id, reportData) => {
    try {
      const response = await api.put(`/api/appointmentreports/${id}`, reportData);
      return response.data;
    } catch (error) {
      throw error.response?.data || error.message;
    }
  },

  deleteReport: async (id) => {
    try {
      await api.delete(`/api/appointmentreports/${id}`);
      return true;
    } catch (error) {
      throw error.response?.data || error.message;
    }
  },

  downloadReportPdf: async (id) => {
    try {
      const response = await api.get(`/api/appointmentreports/${id}/download`, {
        responseType: 'blob'
      });
      return response.data;
    } catch (error) {
      throw error.response?.data || error.message;
    }
  }
};

export default appointmentReportService;