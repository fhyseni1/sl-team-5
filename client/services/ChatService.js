import { api } from './authService'; // Make sure this import works

const chatService = {
  // Get all conversations for the current user
  getConversations: async () => {
    try {
      console.log('🟡 Fetching conversations from API...');
      const response = await api.get('/chat/conversations');
      const conversations = response.data || [];
      
      if (conversations.length === 0) {
        console.log('ℹ️ No conversations found, returning empty array');
        return [];
      }
      
      console.log('✅ Conversations loaded:', conversations);
      return conversations;
    } catch (error) {
      console.error('❌ Error loading conversations:', error);
      return [];
    }
  },

  // Get conversation between current user and another user
  getConversation: async (otherUserId) => {
    try {
      const response = await api.get(`/chat/conversation/${otherUserId}`);
      return response.data || [];
    } catch (error) {
      console.error('Error loading conversation:', error);
      return [];
    }
  },



  // Mark conversation as read
  markConversationAsRead: async (otherUserId) => {
    try {
      await api.post(`/chat/conversation/${otherUserId}/read`);
    } catch (error) {
      console.error('Error marking conversation as read:', error);
    }
  },

  // Get unread messages count
  getUnreadCount: async () => {
    try {
      const response = await api.get('/chat/unread/count');
      return response.data;
    } catch (error) {
      console.error('Error getting unread count:', error);
      return 0;
    }
  },

  // Get available doctors for patient view
  getAvailableDoctors: async () => {
    try {
      const response = await api.get('/users/available-doctors');
      return response.data || [];
    } catch (error) {
      console.error('Error loading available doctors:', error);
      return [];
    }
  },


// Get doctor's patients for doctor view
getDoctorPatients: async (doctorId) => {
  try {
    console.log(`🟡 Calling REAL API: /users/doctor/${doctorId}/patients`);
    const response = await api.get(`/users/doctor/${doctorId}/patients`);
    console.log('✅ REAL Doctor patients response:', response.data);
    return response.data || [];
  } catch (error) {
    console.error('❌ REAL Error loading doctor patients:', error);
    console.error('❌ Error details:', error.response?.data);
    throw error; // Rethrow për ta kapur në component
  }
},

// Send a message - REAL API
sendMessage: async (receiverId, message, parentMessageId = null) => {
  try {
    const messageDto = {
      receiverId: receiverId,
      message: message,
      parentMessageId: parentMessageId
    };
    
    console.log('🟡 Sending REAL message:', messageDto);
    const response = await api.post('/chat/send', messageDto);
    console.log('✅ REAL Message sent response:', response.data);
    return response.data;
  } catch (error) {
    console.error('❌ REAL Error sending message:', error);
    console.error('❌ Error details:', error.response?.data);
    throw error;
  }
},
};

// Export as default
export default chatService;

// Also export as named export for flexibility
export { chatService };