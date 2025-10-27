import { api } from './authService';

const notificationService = {
  async getAllNotifications() {
    try {
      const response = await api.get('/notifications');
      return response.data || [];
    } catch (error) {
      console.error('Error fetching notifications:', error);
      return [];
    }
  },

  async getNotificationById(id) {
    try {
      const response = await api.get(`/notifications/${id}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching notification:', error);
      return null;
    }
  },

  async getNotificationsByUserId(userId) {
    try {
      const response = await api.get(`/notifications/user/${userId}`);
      return response.data || [];
    } catch (error) {
      console.error('Error fetching user notifications:', error);
      return [];
    }
  },

  async getUnreadNotifications(userId) {
    try {
      const response = await api.get(`/notifications/user/${userId}/unread`);
      return response.data || [];
    } catch (error) {
      console.error('Error fetching unread notifications:', error);
      return [];
    }
  },

  async getCaregiverNotifications(caregiverId) {
    try {
      const response = await api.get(`/notifications/caregiver/${caregiverId}`);
      return response.data || [];
    } catch (error) {
      console.error('Error fetching caregiver notifications:', error);
      return [];
    }
  },

  async createNotification(notificationData) {
    try {
      const response = await api.post('/notifications', notificationData);
      return response.data;
    } catch (error) {
      console.error('Error creating notification:', error);
      throw error;
    }
  },

  async markAsRead(notificationId) {
    try {
      await api.put(`/notifications/${notificationId}/read`);
      return true;
    } catch (error) {
      console.error('Error marking notification as read:', error);
      return false;
    }
  },

  async deleteNotification(notificationId) {
    try {
      await api.delete(`/notifications/${notificationId}`);
      return true;
    } catch (error) {
      console.error('Error deleting notification:', error);
      return false;
    }
  },

  parsePatientNameFromTitle(title) {
    const match = title.match(/^\[([^\]]+)\]/);
    return match ? match[1] : null;
  },

  isPatientNotification(title) {
    return title && title.startsWith('[') && title.includes(']');
  }
};

export default notificationService;

