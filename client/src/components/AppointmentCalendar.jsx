import React, { useState, useMemo } from "react";
import {
  ChevronLeft,
  ChevronRight,
  Calendar as CalendarIcon,
} from "lucide-react";

const AppointmentCalendar = ({ appointments, doctors }) => {
  const [currentDate, setCurrentDate] = useState(new Date());
  const [selectedDate, setSelectedDate] = useState(null);
  const [selectedAppointments, setSelectedAppointments] = useState([]);

  const appointmentsByStatus = useMemo(() => {
    return {
      approved: appointments.filter((apt) => apt.status === 2),
      pending: appointments.filter((apt) => apt.status === 1),
      rejected: appointments.filter((apt) => apt.status === 3)
    };
  }, [appointments]);

  const daysInMonth = (date) => {
    return new Date(date.getFullYear(), date.getMonth() + 1, 0).getDate();
  };

  const firstDayOfMonth = (date) => {
    return new Date(date.getFullYear(), date.getMonth(), 1).getDay();
  };

  const getAppointmentsForDate = (date) => {
    // Format the calendar date in YYYY-MM-DD format without timezone conversion
    const dateStr = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
    
    return appointments.filter((apt) => {
      // Get just the date part from the appointment date string
      const aptDate = apt.appointmentDate.split('T')[0];
      return aptDate === dateStr;
    });
  };

  const getDoctorName = (doctorId) => {
    const doctor = doctors.find((d) => d.id === doctorId);
    return doctor ? `Dr. ${doctor.firstName} ${doctor.lastName}` : "Doctor";
  };

  const getPatientName = (apt) => {
    if (apt.userFirstName && apt.userLastName) {
      return `${apt.userFirstName} ${apt.userLastName}`;
    }
    return "Patient";
  };

  const previousMonth = () => {
    setCurrentDate(
      new Date(currentDate.getFullYear(), currentDate.getMonth() - 1, 1)
    );
  };

  const nextMonth = () => {
    setCurrentDate(
      new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 1)
    );
  };

  const monthName = currentDate.toLocaleDateString("en-US", {
    month: "long",
    year: "numeric",
  });
  const daysCount = daysInMonth(currentDate);
  const startDay = firstDayOfMonth(currentDate);
  const today = new Date();
  today.setHours(0, 0, 0, 0);

  const calendarDays = [];
  for (let i = 0; i < startDay; i++) {
    calendarDays.push(null);
  }
  for (let day = 1; day <= daysCount; day++) {
    calendarDays.push(day);
  }

  return (
    <div className="bg-slate-800/50 backdrop-blur-xl rounded-2xl p-8 border border-slate-700/50">
      <div className="flex items-center justify-between mb-6">
        <h3 className="text-2xl font-bold text-white flex items-center gap-3">
          <CalendarIcon className="w-7 h-7 text-emerald-400" />
          Appointments Calendar
        </h3>
        <div className="flex items-center gap-2">
          <button
            onClick={previousMonth}
            className="p-2 bg-slate-700/50 hover:bg-slate-700 rounded-lg transition-colors"
          >
            <ChevronLeft className="w-5 h-5 text-slate-300" />
          </button>
          <span className="text-white font-semibold px-4">{monthName}</span>
          <button
            onClick={nextMonth}
            className="p-2 bg-slate-700/50 hover:bg-slate-700 rounded-lg transition-colors"
          >
            <ChevronRight className="w-5 h-5 text-slate-300" />
          </button>
        </div>
      </div>

      <div className="grid grid-cols-7 gap-2">
        {["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"].map((day) => (
          <div
            key={day}
            className="text-center text-slate-400 font-semibold text-sm py-2"
          >
            {day}
          </div>
        ))}

        {calendarDays.map((day, index) => {
          if (day === null) {
            return <div key={`empty-${index}`} className="aspect-square" />;
          }

          const date = new Date(
            currentDate.getFullYear(),
            currentDate.getMonth(),
            day
          );
          const dayAppointments = getAppointmentsForDate(date);
          const isToday = date.toDateString() === today.toDateString();
          const hasAppointments = dayAppointments.length > 0;

          return (
            <div
              key={day}
              onClick={() => {
                setSelectedDate(date);
                setSelectedAppointments(dayAppointments);
              }}
              className={`
                aspect-square p-2 rounded-lg border transition-all
                ${
                  isToday
                    ? "border-blue-500 bg-blue-500/10"
                    : "border-slate-700/50"
                }
                ${
                  hasAppointments
                    ? "bg-emerald-500/20 border-emerald-500/50"
                    : "bg-slate-700/20"
                }
                hover:bg-slate-700/40 cursor-pointer
              `}
            >
              <div className="flex flex-col h-full">
                <span
                  className={`text-sm font-semibold ${
                    isToday ? "text-blue-400" : "text-slate-300"
                  }`}
                >
                  {day}
                </span>
                {hasAppointments && (
                  <div className="mt-1 flex-1">
                    {dayAppointments.slice(0, 2).map((apt) => (
                      <div
                        key={apt.id}
                        className={`text-xs px-1 py-0.5 rounded mb-1 truncate ${
                          apt.status === 2
                            ? 'bg-emerald-600/80 text-white'
                            : apt.status === 1
                            ? 'bg-orange-500/80 text-white'
                            : 'bg-red-500/80 text-white'
                        }`}
                        title={`Patient: ${getPatientName(apt)}
Doctor: ${getDoctorName(apt.doctorId)}
Time: ${apt.startTime.substring(0, 5)}
Status: ${apt.status === 2 ? 'Approved' : apt.status === 1 ? 'Pending' : 'Rejected'}`}
                      >
                        {apt.startTime.substring(0, 5)} - {getPatientName(apt)}
                      </div>
                    ))}
                    {dayAppointments.length > 2 && (
                      <div className="text-xs text-emerald-400 font-semibold">
                        +{dayAppointments.length - 2} more
                      </div>
                    )}
                  </div>
                )}
              </div>
            </div>
          );
        })}
      </div>

      {appointments.length === 0 && (
        <div className="mt-6 text-center">
          <p className="text-slate-400">No appointments yet</p>
        </div>
      )}

      <div className="mt-6 flex items-center gap-4 text-sm flex-wrap">
        <div className="flex items-center gap-2">
          <div className="w-4 h-4 bg-emerald-600/80 rounded"></div>
          <span className="text-slate-300">Approved</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-4 h-4 bg-orange-500/80 rounded"></div>
          <span className="text-slate-300">Pending</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-4 h-4 bg-red-500/80 rounded"></div>
          <span className="text-slate-300">Rejected</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-4 h-4 bg-blue-500/10 border border-blue-500 rounded"></div>
          <span className="text-slate-300">Today</span>
        </div>
      </div>

      {/* Appointment Details Modal */}
      {selectedDate && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4" onClick={() => setSelectedDate(null)}>
          <div className="bg-slate-800/95 backdrop-blur-xl rounded-3xl max-w-2xl w-full p-8 border border-slate-700/50" onClick={e => e.stopPropagation()}>
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-2xl font-bold text-white">
                Appointments for {selectedDate.toLocaleDateString('en-US', { 
                  weekday: 'long',
                  year: 'numeric',
                  month: 'long',
                  day: 'numeric'
                })}
              </h2>
              <button
                onClick={() => setSelectedDate(null)}
                className="p-2 hover:bg-slate-700/50 rounded-xl text-slate-400 hover:text-white transition-colors"
              >
                <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
            
            <div className="space-y-4 max-h-[60vh] overflow-y-auto pr-2">
              {selectedAppointments.length === 0 ? (
                <p className="text-slate-400 text-center py-4">No appointments for this date</p>
              ) : (
                selectedAppointments.map((apt) => (
                  <div
                    key={apt.id}
                    className={`p-4 rounded-xl border ${
                      apt.status === 2
                        ? 'bg-emerald-500/10 border-emerald-500/30'
                        : apt.status === 1
                        ? 'bg-orange-500/10 border-orange-500/30'
                        : 'bg-red-500/10 border-red-500/30'
                    }`}
                  >
                    <div className="flex items-start justify-between">
                      <div>
                        <h3 className="text-white font-semibold mb-1">
                          Patient: {getPatientName(apt)}
                        </h3>
                        <p className="text-slate-300 text-sm">
                          Doctor: {getDoctorName(apt.doctorId)}
                        </p>
                        <p className="text-slate-300 text-sm mt-1">
                          Time: {apt.startTime.substring(0, 5)} - {apt.endTime.substring(0, 5)}
                        </p>
                        {apt.purpose && (
                          <p className="text-slate-400 text-sm mt-2">
                            Purpose: {apt.purpose}
                          </p>
                        )}
                      </div>
                      <span
                        className={`px-3 py-1 rounded-full text-xs font-semibold ${
                          apt.status === 2
                            ? 'bg-emerald-500/20 text-emerald-400'
                            : apt.status === 1
                            ? 'bg-orange-500/20 text-orange-400'
                            : 'bg-red-500/20 text-red-400'
                        }`}
                      >
                        {apt.status === 2 ? 'Approved' : apt.status === 1 ? 'Pending' : 'Rejected'}
                      </span>
                    </div>
                    {apt.rejectionReason && apt.status === 3 && (
                      <p className="text-red-400 text-sm mt-2 border-t border-red-500/30 pt-2">
                        Reason for rejection: {apt.rejectionReason}
                      </p>
                    )}
                  </div>
                ))
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default AppointmentCalendar;
