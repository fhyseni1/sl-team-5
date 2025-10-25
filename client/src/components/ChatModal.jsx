import React, { useState, useEffect, useRef } from 'react';
import { X, Send, User, Clock, MessageCircle } from 'lucide-react';
import { chatService } from '../../services/ChatService';
import { toast } from 'react-toastify';

const ChatModal = ({ isOpen, onClose, currentUser, otherUser, isDoctorView = false }) => {
  const [messages, setMessages] = useState([]);
  const [newMessage, setNewMessage] = useState('');
  const [replyingTo, setReplyingTo] = useState(null);
  const [loading, setLoading] = useState(false);
  const messagesEndRef = useRef(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  useEffect(() => {
    if (isOpen && otherUser) {
      loadConversation();
      markAsRead();
    }
  }, [isOpen, otherUser]);

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const loadConversation = async () => {
    try {
      setLoading(true);
      const conversation = await chatService.getConversation(otherUser.id);
      setMessages(conversation);
    } catch (error) {
      console.error('Error loading conversation:', error);
      toast.error('Failed to load conversation');
    } finally {
      setLoading(false);
    }
  };

  const markAsRead = async () => {
    try {
      await chatService.markConversationAsRead(otherUser.id);
    } catch (error) {
      console.error('Error marking as read:', error);
    }
  };

  const sendMessage = async (e) => {
    e.preventDefault();
    if (!newMessage.trim()) return;

    try {
      const message = await chatService.sendMessage(
        otherUser.id, 
        newMessage, 
        replyingTo?.id
      );
      
      setMessages(prev => [...prev, message]);
      setNewMessage('');
      setReplyingTo(null);
    } catch (error) {
      console.error('Error sending message:', error);
      toast.error('Failed to send message');
    }
  };

  const formatTime = (dateString) => {
    return new Date(dateString).toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
      <div className="bg-slate-800/95 backdrop-blur-xl rounded-3xl max-w-2xl w-full h-[80vh] flex flex-col border border-slate-700/50">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-slate-700/50">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-gradient-to-br from-blue-600 to-emerald-600 rounded-full flex items-center justify-center">
              <User className="w-5 h-5 text-white" />
            </div>
            <div>
              <h2 className="text-xl font-bold text-white">
                {otherUser.firstName} {otherUser.lastName}
              </h2>
              <p className="text-slate-400 text-sm">
                {isDoctorView ? 'Patient' : 'Doctor'}
              </p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-2 hover:bg-slate-700 rounded-xl transition-colors"
          >
            <X className="w-6 h-6" />
          </button>
        </div>

        {/* Messages */}
        <div className="flex-1 overflow-y-auto p-6 space-y-4">
          {loading ? (
            <div className="flex justify-center py-8">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500"></div>
            </div>
          ) : messages.length === 0 ? (
            <div className="text-center py-8 text-slate-400">
              <MessageCircle className="w-12 h-12 mx-auto mb-4 opacity-50" />
              <p>No messages yet. Start the conversation!</p>
            </div>
          ) : (
            messages.map((message) => (
              <div
                key={message.id}
                className={`flex ${message.senderId === currentUser.id ? 'justify-end' : 'justify-start'}`}
              >
                <div
                  className={`max-w-xs lg:max-w-md rounded-2xl p-4 ${
                    message.senderId === currentUser.id
                      ? 'bg-gradient-to-br from-blue-600 to-indigo-600 text-white'
                      : 'bg-slate-700/50 text-slate-300'
                  }`}
                >
                  {/* Reply indicator */}
                  {message.parentMessage && (
                    <div className={`text-xs mb-2 p-2 rounded-lg ${
                      message.senderId === currentUser.id 
                        ? 'bg-blue-500/30' 
                        : 'bg-slate-600/50'
                    }`}>
                      <div className="font-medium">Replying to:</div>
                      <div className="opacity-80">{message.parentMessage}</div>
                    </div>
                  )}

                  <div className="text-sm">{message.message}</div>
                  
                  <div className="flex items-center justify-between mt-2 text-xs opacity-70">
                    <span>{formatTime(message.sentAt)}</span>
                    {message.senderId === currentUser.id && (
                      <span>{message.isRead ? 'Read' : 'Sent'}</span>
                    )}
                  </div>

                  {/* Reply button for received messages */}
                  {message.senderId !== currentUser.id && (
                    <button
                      onClick={() => setReplyingTo(message)}
                      className="text-xs mt-2 opacity-70 hover:opacity-100 transition-opacity"
                    >
                      Reply
                    </button>
                  )}
                </div>
              </div>
            ))
          )}
          <div ref={messagesEndRef} />
        </div>

        {/* Reply indicator */}
        {replyingTo && (
          <div className="px-6 py-3 bg-slate-700/30 border-t border-slate-600/50">
            <div className="flex items-center justify-between text-sm">
              <div>
                <span className="text-slate-400">Replying to: </span>
                <span className="text-slate-300">{replyingTo.message}</span>
              </div>
              <button
                onClick={() => setReplyingTo(null)}
                className="text-slate-400 hover:text-slate-300"
              >
                <X className="w-4 h-4" />
              </button>
            </div>
          </div>
        )}

        {/* Input */}
        <form onSubmit={sendMessage} className="p-6 border-t border-slate-700/50">
          <div className="flex gap-3">
            <input
              type="text"
              value={newMessage}
              onChange={(e) => setNewMessage(e.target.value)}
              placeholder="Type your message..."
              className="flex-1 p-4 bg-slate-700/80 border border-slate-600 rounded-xl text-white placeholder-slate-400"
            />
            <button
              type="submit"
              disabled={!newMessage.trim()}
              className="px-6 py-4 bg-gradient-to-r from-blue-600 to-indigo-600 text-white rounded-xl font-bold hover:shadow-blue-500/50 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <Send className="w-5 h-5" />
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default ChatModal;