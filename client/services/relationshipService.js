import { api } from './authService';

const relationshipService = {
  /**
   * Get all relationships for the current user
   * @param {string} userId - User ID
   * @returns {Promise<Array>} Array of relationships
   */
  async getRelationshipsByUserId(userId) {
    try {
      const response = await api.get(`/relationships/user/${userId}`);
      return response.data || [];
    } catch (error) {
      console.error('Error fetching relationships:', error);
      return [];
    }
  },

  /**
   * Get caregivers for a user
   * @param {string} userId - User ID
   * @returns {Promise<Array>} Array of caregivers
   */
  async getCaregiversByUserId(userId) {
    try {
      const response = await api.get(`/relationships/user/${userId}/caregivers`);
      return response.data || [];
    } catch (error) {
      console.error('Error fetching caregivers:', error);
      return [];
    }
  },

  /**
   * Create a new relationship
   * @param {Object} relationshipData - Relationship data
   * @param {string} relationshipData.userId - User ID (patient)
   * @param {string} relationshipData.relatedUserId - Related user ID (family member/caregiver)
   * @param {number} relationshipData.relationshipType - Relationship type enum (1-9)
   * @param {boolean} relationshipData.canManageMedications - Can manage medications permission
   * @param {boolean} relationshipData.canViewHealthData - Can view health data permission
   * @param {boolean} relationshipData.canScheduleAppointments - Can schedule appointments permission
   * @returns {Promise<Object|null>} Created relationship or null on error
   */
  async createRelationship(relationshipData) {
    try {
      const response = await api.post('/relationships', relationshipData);
      return response.data;
    } catch (error) {
      console.error('Error creating relationship:', error);
      throw error;
    }
  },

  /**
   * Update a relationship
   * @param {string} relationshipId - Relationship ID
   * @param {Object} relationshipData - Updated relationship data
   * @returns {Promise<Object|null>} Updated relationship or null on error
   */
  async updateRelationship(relationshipId, relationshipData) {
    try {
      const response = await api.put(`/relationships/${relationshipId}`, relationshipData);
      return response.data;
    } catch (error) {
      console.error('Error updating relationship:', error);
      throw error;
    }
  },

  /**
   * Update relationship permissions
   * @param {string} relationshipId - Relationship ID
   * @param {string} permissions - Comma-separated permissions string (e.g., "CanManageMedications,CanViewHealthData")
   * @returns {Promise<Object|null>} Updated relationship or null on error
   */
  async updatePermissions(relationshipId, permissions) {
    try {
      const response = await api.put(`/relationships/${relationshipId}/permissions`, permissions);
      return response.data;
    } catch (error) {
      console.error('Error updating permissions:', error);
      throw error;
    }
  },

  /**
   * Delete a relationship
   * @param {string} relationshipId - Relationship ID
   * @returns {Promise<boolean>} True if successful, false otherwise
   */
  async deleteRelationship(relationshipId) {
    try {
      await api.delete(`/relationships/${relationshipId}`);
      return true;
    } catch (error) {
      console.error('Error deleting relationship:', error);
      return false;
    }
  },

  /**
   * Search users by email or name
   * @param {string} searchTerm - Search term
   * @returns {Promise<Array>} Array of users
   */
  async searchUsers(searchTerm) {
    try {
      // Get all users and filter client-side since there's no search endpoint
      const response = await api.get('/users');
      const allUsers = response.data || [];
      
      if (!searchTerm || searchTerm.length < 2) {
        return [];
      }
      
      const term = searchTerm.toLowerCase();
      return allUsers.filter(user => 
        user.email?.toLowerCase().includes(term) ||
        user.firstName?.toLowerCase().includes(term) ||
        user.lastName?.toLowerCase().includes(term) ||
        `${user.firstName || ''} ${user.lastName || ''}`.toLowerCase().includes(term)
      ).slice(0, 10); // Limit to 10 results
    } catch (error) {
      console.error('Error searching users:', error);
      return [];
    }
  }
};

export default relationshipService;

