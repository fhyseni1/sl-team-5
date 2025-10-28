"use client";
import React, { useState, useEffect } from 'react';
import {
  Bell,
  X,
  Check,
  AlertCircle,
  Calendar,
  Pill,
  Clock,
  ChevronRight,
  Loader2
} from 'lucide-react';
import notificationService from '../../services/notificationService';

const NotificationCenter = ({ currentUser }) => {
  const [notifications, setNotifications] = useState([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [isOpen, setIsOpen] = useState(false);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (currentUser?.id) {
      fetchNotifications();
      
      // Poll for new notifications every 30 seconds
      const interval = setInterval(fetchNotifications, 30000);
      return () => clearInterval(interval);
    }
  }, [currentUser?.id]);

  const fetchNotifications = async () => {
    if (!currentUser?.id) return;

    try {
      setLoading(true);
      
      // Fetch caregiver notifications if user is a caregiver
      let userNotifications = [];
      
      // Check if this user has caregiver relationships
      const allNotifications = await notificationService.getNotificationsByUserId(currentUser.id);
      const caregiverNotifications = await notificationService.getCaregiverNotifications(currentUser.id);
      
      // Combine both, with caregiver notifications prioritized
      userNotifications = [...caregiverNotifications, ...allNotifications]
        .filter((notif, index, self) => 
          index === self.findIndex(n => n.id === notif.id)
        );
      
      setNotifications(userNotifications);
      
      // Count unread
      const unread = userNotifications.filter(n => !n.isRead).length;
      setUnreadCount(unread);
    } catch (error) {
      console.error('Error fetching notifications:', error);
    } finally {
      setLoading(false);
    }
  };

  const markAsRead = async (notificationId) => {
    try {
      await notificationService.markAsRead(notificationId);
      
      // Update local state
      setNotifications(prev =>
        prev.map(n => n.id === notificationId ? { ...n, isRead: true } : n)
      );
      setUnreadCount(prev => Math.max(0, prev - 1));
    } catch (error) {
      console.error('Error marking notification as read:', error);
    }
  };

  const markAllAsRead = async () => {
    try {
      const unreadNotifications = notifications.filter(n => !n.isRead);
      
      await Promise.all(
        unreadNotifications.map(n => notificationService.markAsRead(n.id))
      );
      
      setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
      setUnreadCount(0);
    } catch (error) {
      console.error('Error marking all as read:', error);
    }
  };

  const deleteNotification = async (notificationId) => {
    try {
      await notificationService.deleteNotification(notificationId);
      setNotifications(prev => prev.filter(n => n.id !== notificationId));
      if (notifications.find(n => n.id === notificationId && !n.isRead)) {
        setUnreadCount(prev => Math.max(0, prev - 1));
      }
    } catch (error) {
      console.error('Error deleting notification:', error);
    }
  };

  const getNotificationIcon = (type) => {
    switch (type) {
      case 'MedicationReminder':
        return <Pill className="w-5 h-5 text-emerald-400" />;
      case 'AppointmentReminder':
        return <Calendar className="w-5 h-5 text-blue-400" />;
      case 'Urgent':
        return <AlertCircle className="w-5 h-5 text-red-400" />;
      default:
        return <Bell className="w-5 h-5 text-amber-400" />;
    }
  };

  const parsePatientName = (title) => {
    const match = title.match(/^\[([^\]]+)\]/);
    return match ? match[1] : null;
  };

  return (
    <div className="relative">
      {/* Bell Icon Button */}
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="relative p-2 bg-slate-800/50 hover:bg-slate-700/50 rounded-xl transition-all hover:scale-105"
      >
        <Bell className="w-6 h-6 text-slate-300" />
        
        {/* Unread Count Badge */}
        {unreadCount > 0 && (
          <span className="absolute -top-1 -right-1 w-5 h-5 bg-red-500 text-white text-xs font-bold rounded-full flex items-center justify-center animate-pulse">
            {unreadCount > 9 ? '9+' : unreadCount}
          </span>
        )}
      </button>

      {/* Dropdown Panel */}
      {isOpen && (
        <>
          <div
            className="fixed inset-0 z-40"
            onClick={() => setIsOpen(false)}
          />
          <div className="absolute right-0 top-full mt-2 w-96 bg-slate-800/95 backdrop-blur-xl rounded-2xl border border-slate-700/50 shadow-2xl z-50 max-h-[600px] flex flex-col">
            {/* Header */}
            <div className="flex items-center justify-between p-4 border-b border-slate-700/50">
              <div className="flex items-center gap-2">
                <Bell className="w-6 h-6 text-blue-400" />
                <h3 className="text-lg font-bold text-white">Notifications</h3>
                {unreadCount > 0 && (
                  <span className="px-2 py-0.5 bg-red-500/20 text-red-400 text-xs font-medium rounded-full">
                    {unreadCount} new
                  </span>
                )}
              </div>
              <button
                onClick={() => setIsOpen(false)}
                className="p-2 hover:bg-slate-700/50 rounded-lg transition-all"
              >
                <X className="w-5 h-5 text-slate-400" />
              </button>
            </div>

            {/* Action Buttons */}
            {notifications.length > 0 && unreadCount > 0 && (
              <div className="p-2 border-b border-slate-700/50">
                <button
                  onClick={markAllAsRead}
                  className="w-full flex items-center gap-2 px-4 py-2 text-sm text-blue-400 hover:bg-slate-700/30 rounded-lg transition-all"
                >
                  <Check className="w-4 h-4" />
                  Mark all as read
                </button>
              </div>
            )}

            {/* Notifications List */}
            <div className="flex-1 overflow-y-auto">
              {loading ? (
                <div className="flex items-center justify-center p-8">
                  <Loader2 className="w-8 h-8 animate-spin text-blue-400" />
                </div>
              ) : notifications.length > 0 ? (
                <div className="p-2">
                  {notifications
                    .sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt))
                    .map((notification) => {
                      const patientName = parsePatientName(notification.title);
                      const isCaregiverNotification = patientName !== null;

                      return (
                        <div
                          key={notification.id}
                          className={`mb-2 p-3 rounded-xl border transition-all ${
                            notification.isRead
                              ? 'bg-slate-700/30 border-slate-600/30'
                              : 'bg-blue-500/10 border-blue-500/30'
                          }`}
                        >
                          <div className="flex items-start gap-3">
                            {/* Icon */}
                            <div className="mt-1">
                              {getNotificationIcon(notification.type)}
                            </div>

                            {/* Content */}
                            <div className="flex-1 min-w-0">
                              <div className="flex items-start justify-between gap-2">
                                <h4 className="text-sm font-semibold text-white">
                                  {notification.title}
                                </h4>
                                {!notification.isRead && (
                                  <div className="w-2 h-2 bg-blue-500 rounded-full mt-1.5 flex-shrink-0" />
                                )}
                              </div>

                              <p className="text-xs text-slate-300 mt-1 line-clamp-2">
                                {notification.message}
                              </p>

                              {/* Patient Name Badge for Caregiver Notifications */}
                              {isCaregiverNotification && (
                                <div className="mt-2 inline-flex items-center gap-1 px-2 py-0.5 bg-purple-500/20 text-purple-400 text-xs rounded-full">
                                  <Pill className="w-3 h-3" />
                                  Patient: {patientName}
                                </div>
                              )}

                              {/* Time */}
                              <div className="flex items-center gap-2 mt-2 text-xs text-slate-400">
                                <Clock className="w-3 h-3" />
                                {new Date(notification.createdAt).toLocaleTimeString()}
                              </div>
                            </div>

                            {/* Actions */}
                            <div className="flex flex-col gap-1">
                              {!notification.isRead && (
                                <button
                                  onClick={() => markAsRead(notification.id)}
                                  className="p-1.5 hover:bg-blue-500/20 text-blue-400 rounded-lg transition-all"
                                  title="Mark as read"
                                >
                                  <Check className="w-4 h-4" />
                                </button>
                              )}
                              <button
                                onClick={() => deleteNotification(notification.id)}
                                className="p-1.5 hover:bg-red-500/20 text-red-400 rounded-lg transition-all"
                                title="Delete"
                              >
                                <X className="w-4 h-4" />
                              </button>
                            </div>
                          </div>
                        </div>
                      );
                    })}
                </div>
              ) : (
                <div className="flex flex-col items-center justify-center p-8 text-center">
                  <Bell className="w-16 h-16 text-slate-600 mb-4" />
                  <p className="text-slate-400 text-sm">No notifications yet</p>
                </div>
              )}
            </div>

            {/* Footer */}
            {notifications.length > 0 && (
              <div className="p-3 border-t border-slate-700/50">
                <button
                  onClick={() => {
                    // Navigate to full notifications page
                    window.location.href = '/notifications';
                  }}
                  className="w-full flex items-center justify-center gap-2 text-sm text-blue-400 hover:text-blue-300 font-medium"
                >
                  View all notifications
                  <ChevronRight className="w-4 h-4" />
                </button>
              </div>
            )}
          </div>
        </>
      )}
    </div>
  );
};

export default NotificationCenter;

