// client/services/medicationService.js
import axios from "axios";

const MEDICATION_API_URL = process.env.NEXT_PUBLIC_MEDICATION_API_URL || "http://localhost:5077/api";

const medicationApi = axios.create({
  baseURL: MEDICATION_API_URL,
  withCredentials: true,
  headers: {
    "Content-Type": "application/json",
    Accept: "application/json",
  },
});

// Interceptor to add auth token from cookies
medicationApi.interceptors.request.use((config) => {
  const token = document.cookie
    .split("; ")
    .find((row) => row.startsWith("access_token="))
    ?.split("=")[1];
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Error interceptor
medicationApi.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error("MedicationService API error:", error.response?.data || error.message);
    return Promise.reject(error);
  }
);

export const medicationService = {
  /**
   * Create a new medication
   * @param {Object} medicationData - Medication data
   * @param {string} medicationData.userId - User ID (Guid)
   * @param {string} medicationData.name - Medication name
   * @param {string} medicationData.genericName - Generic name
   * @param {string} medicationData.manufacturer - Manufacturer
   * @param {number} medicationData.type - Medication type (enum)
   * @param {number} medicationData.dosage - Dosage amount
   * @param {number} medicationData.dosageUnit - Dosage unit (enum)
   * @param {string} medicationData.description - Description
   * @param {string} medicationData.instructions - Instructions
   * @param {string} medicationData.startDate - Start date (ISO string)
   * @param {string} [medicationData.endDate] - End date (ISO string, optional)
   * @param {string} [medicationData.doctorId] - Doctor ID (Guid, optional)
   * @param {string} medicationData.prescribedBy - Prescribed by name
   * @returns {Promise<Object|null>} Created medication object or null on error
   */
  async createMedication(medicationData) {
    try {
      const response = await medicationApi.post("/medications", medicationData);
      return response.data;
    } catch (error) {
      console.error("Error creating medication:", error.response?.data || error.message);
      return null;
    }
  },

  /**
   * Get all medications for a user
   * @param {string} userId - User ID (Guid)
   * @returns {Promise<Array|null>} Array of medications or null on error
   */
  async getMedicationsByUserId(userId) {
    try {
      const response = await medicationApi.get(`/medications/user/${userId}`);
      return response.data;
    } catch (error) {
      console.error("Error fetching medications:", error.response?.data || error.message);
      return null;
    }
  },

  /**
   * Get active medications for a user
   * @param {string} userId - User ID (Guid)
   * @returns {Promise<Array|null>} Array of active medications or null on error
   */
  async getActiveMedicationsByUserId(userId) {
    try {
      const response = await medicationApi.get(`/medications/user/${userId}/active`);
      return response.data;
    } catch (error) {
      console.error("Error fetching active medications:", error.response?.data || error.message);
      return null;
    }
  },

  /**
   * Update a medication
   * @param {string} id - Medication ID (Guid)
   * @param {Object} medicationData - Updated medication data
   * @returns {Promise<Object|null>} Updated medication object or null on error
   */
  async updateMedication(id, medicationData) {
    try {
      const response = await medicationApi.put(`/medications/${id}`, medicationData);
      return response.data;
    } catch (error) {
      console.error("Error updating medication:", error.response?.data || error.message);
      return null;
    }
  },

  /**
   * Delete a medication
   * @param {string} id - Medication ID (Guid)
   * @returns {Promise<boolean>} True if successful, false otherwise
   */
  async deleteMedication(id) {
    try {
      await medicationApi.delete(`/medications/${id}`);
      return true;
    } catch (error) {
      console.error("Error deleting medication:", error.response?.data || error.message);
      return false;
    }
  },

  /**
   * Create a medication schedule
   * @param {Object} scheduleData - Schedule data
   * @param {string} scheduleData.medicationId - Medication ID (Guid)
   * @param {number} scheduleData.frequency - Frequency type (1=OnceDaily, 2=TwiceDaily, 3=ThreeTimesDaily, 4=FourTimesDaily, 5=EveryFewHours, 6=AsNeeded, 7=Weekly, 8=Monthly, 9=Custom)
   * @param {string} scheduleData.timeOfDay - Time of day (TimeSpan format: HH:mm:ss)
   * @param {string} scheduleData.daysOfWeek - Days of week (comma-separated string)
   * @param {number} [scheduleData.customFrequencyHours] - Custom frequency in hours (required if frequency is 9=Custom)
   * @returns {Promise<Object|null>} Created schedule object or null on error
   */
  async createSchedule(scheduleData) {
    try {
      const response = await medicationApi.post("/schedules", scheduleData);
      return response.data;
    } catch (error) {
      console.error("Error creating schedule:", error.response?.data || error.message);
      return null;
    }
  },

  /**
   * Create a medication reminder
   * @param {Object} reminderData - Reminder data
   * @param {string} reminderData.medicationId - Medication ID (Guid)
   * @param {string} [reminderData.scheduleId] - Schedule ID (Guid, optional)
   * @param {string} reminderData.scheduledTime - Scheduled time (ISO string)
   * @param {string} [reminderData.message] - Reminder message (optional)
   * @param {string} [reminderData.notificationChannel] - Notification channel (optional)
   * @returns {Promise<Object|null>} Created reminder object or null on error
   */
  async createReminder(reminderData) {
    try {
      const response = await medicationApi.post("/reminders", reminderData);
      return response.data;
    } catch (error) {
      console.error("Error creating reminder:", error.response?.data || error.message);
      return null;
    }
  },

  /**
   * Get reminders by medication ID
   * @param {string} medicationId - Medication ID (Guid)
   * @returns {Promise<Array|null>} Array of reminders or null on error
   */
  async getRemindersByMedicationId(medicationId) {
    try {
      const response = await medicationApi.get(`/reminders/medication/${medicationId}`);
      return response.data;
    } catch (error) {
      console.error("Error fetching reminders:", error.response?.data || error.message);
      return null;
    }
  },
};

export default medicationService;

