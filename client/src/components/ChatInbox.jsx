import React, { useState, useEffect } from 'react';
import { MessageCircle, Search, User, Clock, X } from 'lucide-react';
import chatService from '../../services/ChatService';
import ChatModal from './ChatModal';

const ChatInbox = ({ currentUser, isDoctorView = false }) => {
  const [conversations, setConversations] = useState([]);
  const [selectedConversation, setSelectedConversation] = useState(null);
  const [showChatModal, setShowChatModal] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [loading, setLoading] = useState(true);
  const [availableDoctors, setAvailableDoctors] = useState([]);
  const [availablePatients, setAvailablePatients] = useState([]);
  const [showNewChatModal, setShowNewChatModal] = useState(false);
  const [loadingUsers, setLoadingUsers] = useState(false);

  useEffect(() => {
    loadConversations();
  }, []);

  const loadConversations = async () => {
    try {
      setLoading(true);
      console.log('🟡 Loading REAL conversations from API...');
      
      const convos = await chatService.getConversations();
      console.log('✅ REAL conversations loaded:', convos);
      
      if (convos && convos.length > 0) {
        setConversations(convos);
      } else {
        console.log('ℹ️ No conversations found in database');
        setConversations([]);
      }
    } catch (error) {
      console.error('❌ Error loading REAL conversations:', error);
      setConversations([]);
    } finally {
      setLoading(false);
    }
  };

  const loadAvailableUsers = async () => {
    try {
      setLoadingUsers(true);
      console.log('🟡 Loading available users...');
      console.log('👤 Current user ID:', currentUser.id);
      console.log('👨‍⚕️ Is doctor view:', isDoctorView);
      
      if (isDoctorView) {
        console.log('👨‍⚕️ Loading patients for doctor...');
        const patients = await chatService.getDoctorPatients(currentUser.id);
        console.log('✅ Patients response:', patients);
        setAvailablePatients(patients);
      } else {
        console.log('👤 Loading doctors for patient...');
        const doctors = await chatService.getAvailableDoctors();
        console.log('✅ Doctors response:', doctors);
        setAvailableDoctors(doctors);
      }
    } catch (error) {
      console.error('❌ Error loading available users:', error);
      console.error('Error details:', error.response?.data);
    } finally {
      setLoadingUsers(false);
    }
  };

const startNewConversation = async (otherUser) => {
  try {
    console.log('🟡 Starting new conversation with:', otherUser);
    
    // Kontrollo nëse otherUser.id ekziston
    if (!otherUser.id) {
      alert('Invalid user selected');
      return;
    }

    const initialMessage = `Hello, I'd like to start a conversation`;
    
    console.log('🟡 Sending initial message...');
    await chatService.sendMessage(otherUser.id, initialMessage);
    
    console.log('✅ Message sent successfully');
    
    setSelectedConversation({
      id: otherUser.id,
      firstName: otherUser.name.split(' ')[0],
      lastName: otherUser.name.split(' ').slice(1).join(' ')
    });
    setShowChatModal(true);
    setShowNewChatModal(false);
    

    await loadConversations();
    
  } catch (error) {
    console.error('❌ Error starting new conversation:', error);
    console.error('❌ Error details:', error.response?.data);
    
    if (error.response?.status === 500) {
      alert('Server error. Please try again later.');
    } else {
      alert('Failed to start conversation. Please try again.');
    }
  }
};

  const openNewChatModal = async () => {
    setShowNewChatModal(true);
    await loadAvailableUsers(); // Load users when opening the modal
  };

  const openChat = (conversation) => {
    setSelectedConversation({
      id: conversation.otherUserId,
      firstName: conversation.otherUserName.split(' ')[0],
      lastName: conversation.otherUserName.split(' ').slice(1).join(' ')
    });
    setShowChatModal(true);
  };

  const filteredConversations = conversations.filter(conv =>
    conv.otherUserName.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const formatTime = (dateString) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffInHours = (now - date) / (1000 * 60 * 60);

    if (diffInHours < 24) {
      return date.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
    } else {
      return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
    }
  };

  return (
    <>
      <div className="bg-slate-800/50 backdrop-blur-xl rounded-3xl border border-slate-700/50 p-6">
        {/* Header me butonin New Chat */}
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-2xl font-bold text-white flex items-center gap-3">
            <MessageCircle className="w-7 h-7 text-blue-400" />
            Messages
          </h2>
          <button
            onClick={openNewChatModal} // Use the new function
            className="bg-gradient-to-r from-emerald-600 to-teal-600 text-white px-4 py-2 rounded-xl font-bold hover:shadow-emerald-500/50 transition-all"
          >
            New Chat
          </button>
        </div>

        {showNewChatModal && (
          <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
            <div className="bg-slate-800/95 backdrop-blur-xl rounded-3xl max-w-md w-full p-6 border border-slate-700/50">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-xl font-bold text-white">
                  Start New Chat
                </h3>
                <button
                  onClick={() => setShowNewChatModal(false)}
                  className="p-2 hover:bg-slate-700 rounded-xl"
                >
                  <X className="w-5 h-5" />
                </button>
              </div>
              
              <div className="max-h-96 overflow-y-auto">
                {loadingUsers ? (
                  <div className="flex justify-center py-8">
                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500"></div>
                  </div>
                ) : isDoctorView ? (
                  // Lista e pacientëve për doktorë
                  availablePatients.length > 0 ? (
                    availablePatients.map(patient => (
                      <div
                        key={patient.id}
                        onClick={() => startNewConversation(patient)}
                        className="flex items-center gap-3 p-3 hover:bg-slate-700/50 rounded-lg cursor-pointer transition-colors"
                      >
                        <div className="w-10 h-10 bg-gradient-to-br from-blue-600 to-emerald-600 rounded-full flex items-center justify-center">
                          <User className="w-5 h-5 text-white" />
                        </div>
                        <div>
                          <h4 className="text-white font-semibold">{patient.name}</h4>
                          <p className="text-slate-400 text-sm">{patient.email}</p>
                          {patient.phoneNumber && (
                            <p className="text-slate-500 text-xs">📞 {patient.phoneNumber}</p>
                          )}
                        </div>
                      </div>
                    ))
                  ) : (
                    <div className="text-center py-8 text-slate-400">
                     <MessageCircle className="w-12 h-12 mx-auto mb-4 opacity-50" />
                      <p>No patients found</p>
                      <p className="text-sm mt-2">Patients will appear here once they schedule appointments with you.</p>
                    </div>
                  )
                ) : (
                  
                  availableDoctors.length > 0 ? (
                    availableDoctors.map(doctor => (
                      <div
                        key={doctor.id}
                        onClick={() => startNewConversation(doctor)}
                        className="flex items-center gap-3 p-3 hover:bg-slate-700/50 rounded-lg cursor-pointer transition-colors"
                      >
                        <div className="w-10 h-10 bg-gradient-to-br from-purple-600 to-pink-600 rounded-full flex items-center justify-center">
                          <User className="w-5 h-5 text-white" />
                        </div>
                        <div>
                          <h4 className="text-white font-semibold">{doctor.name}</h4>
                          <p className="text-slate-400 text-sm">{doctor.email}</p>
                          {doctor.specialty && (
                            <p className="text-slate-500 text-xs">{doctor.specialty}</p>
                          )}
                          {doctor.isActive !== undefined && (
                            <div className="flex items-center gap-1 mt-1">
                              <div className={`w-2 h-2 rounded-full ${doctor.isActive ? 'bg-green-500' : 'bg-gray-500'}`}></div>
                              <span className="text-xs text-slate-500">
                                {doctor.isActive ? 'Online' : 'Offline'}
                              </span>
                            </div>
                          )}
                        </div>
                      </div>
                    ))
                  ) : (
                    <div className="text-center py-8 text-slate-400">
                      <User className="w-12 h-12 mx-auto mb-4 opacity-50" />
                      <p>No doctors available</p>
                      <p className="text-sm mt-2">Please check back later or contact support.</p>
                    </div>
                  )
                )}
              </div>
            </div>
          </div>
        )}

        {/* Rest of your component remains the same */}
        {/* Search */}
        <div className="relative mb-6">
          <Search className="w-5 h-5 text-slate-400 absolute left-4 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            placeholder="Search conversations..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full pl-12 pr-4 py-3 bg-slate-700/50 border border-slate-600 rounded-2xl text-white placeholder-slate-400"
          />
        </div>

        {/* Conversations List */}
        <div className="space-y-3 max-h-96 overflow-y-auto">
          {loading ? (
            <div className="flex justify-center py-8">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500"></div>
            </div>
          ) : filteredConversations.length === 0 ? (
            <div className="text-center py-8 text-slate-400">
              <MessageCircle className="w-12 h-12 mx-auto mb-4 opacity-50" />
              <p>No conversations yet</p>
              <p className="text-sm mt-2">Start a new chat to begin messaging.</p>
            </div>
          ) : (
            filteredConversations.map((conversation) => (
              <div
                key={conversation.otherUserId}
                onClick={() => openChat(conversation)}
                className="flex items-center gap-4 p-4 bg-slate-700/30 rounded-xl border border-slate-600/50 hover:bg-slate-700/50 cursor-pointer transition-all"
              >
                <div className="relative">
                  <div className="w-12 h-12 bg-gradient-to-br from-blue-600 to-emerald-600 rounded-full flex items-center justify-center">
                    <User className="w-6 h-6 text-white" />
                  </div>
                  {conversation.unreadCount > 0 && (
                    <div className="absolute -top-1 -right-1 w-5 h-5 bg-red-500 rounded-full flex items-center justify-center text-xs font-bold">
                      {conversation.unreadCount}
                    </div>
                  )}
                </div>
                
                <div className="flex-1 min-w-0">
                  <div className="flex items-center justify-between mb-1">
                    <h3 className="text-white font-semibold truncate">
                      {conversation.otherUserName}
                    </h3>
                    <div className="flex items-center gap-1 text-slate-400 text-xs">
                      <Clock className="w-3 h-3" />
                      {formatTime(conversation.lastMessageTime)}
                    </div>
                  </div>
                  <p className="text-slate-400 text-sm truncate">
                    {conversation.lastMessage}
                  </p>
                  <div className="flex items-center gap-2 mt-1">
                    <span className={`text-xs px-2 py-1 rounded-full ${
                      conversation.isDoctor 
                        ? 'bg-purple-500/20 text-purple-400' 
                        : 'bg-emerald-500/20 text-emerald-400'
                    }`}>
                      {conversation.isDoctor ? 'Doctor' : 'Patient'}
                    </span>
                    {conversation.unreadCount > 0 && (
                      <span className="text-xs text-blue-400 font-medium">
                        {conversation.unreadCount} new
                      </span>
                    )}
                  </div>
                </div>
              </div>
            ))
          )}
        </div>
      </div>

      {/* Chat Modal */}
      {showChatModal && selectedConversation && (
        <ChatModal
          isOpen={showChatModal}
          onClose={() => {
            setShowChatModal(false);
            loadConversations(); // Refresh conversations when closing chat
          }}
          currentUser={currentUser}
          otherUser={selectedConversation}
          isDoctorView={isDoctorView}
        />
      )}
    </>
  );
};

export default ChatInbox;