// src/components/DoctorDashboard/ReportModal.jsx
import React from 'react';
import { X, Calendar, Users, Stethoscope, FileText } from 'lucide-react';

const ReportModal = ({
  showModal,
  selectedAppointment,
  editingReport,
  user,
  onClose,
  onSubmit
}) => {
  if (!showModal) return null;

  const handleSubmit = (e) => {
    e.preventDefault();
    const formData = new FormData(e.target);
    onSubmit({
      diagnosis: formData.get('diagnosis'),
      symptoms: formData.get('symptoms'),
      treatment: formData.get('treatment'),
      medications: formData.get('medications'),
      notes: formData.get('notes'),
      recommendations: formData.get('recommendations')
    });
  };

  return (
    <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
      <div className="bg-slate-800/95 backdrop-blur-xl rounded-3xl max-w-4xl w-full max-h-[90vh] overflow-y-auto border border-slate-700/50 shadow-2xl">
        {/* Header */}
        <div className="bg-gradient-to-r from-purple-600/20 to-indigo-600/20 p-6 rounded-t-3xl border-b border-slate-700/50">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="bg-purple-500/20 rounded-xl p-2">
                <FileText className="w-6 h-6 text-purple-400" />
              </div>
              <div>
                <h2 className="text-2xl font-bold text-white">
                  {editingReport ? 'Modifiko Raportin' : 'Krijo Raport të Ri'}
                </h2>
                <p className="text-slate-300 text-sm">
                  {editingReport ? 'Përditëso detajet e raportit' : 'Plotëso informacionin mjekësor'}
                </p>
              </div>
            </div>
            <button
              onClick={onClose}
              className="p-2 hover:bg-slate-700/50 rounded-xl transition-all duration-200 hover:scale-110"
            >
              <X className="w-6 h-6 text-slate-300" />
            </button>
          </div>
        </div>

        <div className="p-6">
          {/* Appointment Information*/}
          {selectedAppointment && (
            <div className="bg-slate-700/30 rounded-2xl p-6 mb-6 border border-slate-600/50">
              <h3 className="text-white font-bold text-lg mb-4 flex items-center gap-2">
                <Calendar className="w-5 h-5 text-blue-400" />
                Appointment Information
              </h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <div className="flex items-center gap-2">
                    <Users className="w-4 h-4 text-slate-400" />
                    <span className="text-slate-300 text-sm">Patient:</span>
                    <span className="text-white font-medium">
                      {selectedAppointment.userFirstName} {selectedAppointment.userLastName}
                    </span>
                  </div>
                  <div className="flex items-center gap-2">
                    <Stethoscope className="w-4 h-4 text-slate-400" />
                    <span className="text-slate-300 text-sm">Doctor:</span>
                    <span className="text-white font-medium">
                      {selectedAppointment.doctorName || `Dr. ${user?.firstName} ${user?.lastName}`}
                    </span>
                  </div>
                </div>
                <div className="space-y-2">
                  <div className="flex items-center gap-2">
                    <Calendar className="w-4 h-4 text-slate-400" />
                    <span className="text-slate-300 text-sm">Date:</span>
                    <span className="text-white font-medium">
                      {selectedAppointment.appointmentDate ? 
                        new Date(selectedAppointment.appointmentDate).toLocaleDateString('sq-AL') : 
                        'Nuk është specifikuar'
                      }
                    </span>
                  </div>
                  <div className="flex items-center gap-2">
                    <FileText className="w-4 h-4 text-slate-400" />
                    <span className="text-slate-300 text-sm">Goal:</span>
                    <span className="text-white font-medium">
                      {selectedAppointment.purpose || 'Konsultim i përgjithshëm'}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          )}

          {/* Report Form */}
          <form onSubmit={handleSubmit} className="space-y-6">
            
            {/* Diagnosis */}
            <div className="bg-slate-700/30 rounded-2xl p-6 border border-slate-600/50">
              <label className="text-white font-bold text-lg mb-3 flex items-center gap-2">
                <i className="fas fa-stethoscope text-red-400"></i>
                Diagnose *
              </label>
              <textarea
                name="diagnosis"
                defaultValue={editingReport?.diagnosis || ''}
                placeholder="Shkruani diagnozën kryesore..."
                rows="3"
                className="w-full p-4 bg-slate-600/50 border border-slate-500/50 rounded-xl text-white resize-vertical focus:border-purple-500/50 focus:ring-2 focus:ring-purple-500/20 transition-all"
                required
              />
            </div>

            {/* Symptoms & Treatment - në një rresht */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
              <div className="bg-slate-700/30 rounded-2xl p-6 border border-slate-600/50">
                <label className="text-white font-bold text-lg mb-3 flex items-center gap-2">
                  <i className="fas fa-thermometer-half text-amber-400"></i>
                  Symptoms
                </label>
                <textarea
                  name="symptoms"
                  defaultValue={editingReport?.symptoms || ''}
                  placeholder="Describe the symptoms..."
                  rows="4"
                  className="w-full p-4 bg-slate-600/50 border border-slate-500/50 rounded-xl text-white resize-vertical focus:border-amber-500/50 focus:ring-2 focus:ring-amber-500/20 transition-all"
                />
              </div>

              <div className="bg-slate-700/30 rounded-2xl p-6 border border-slate-600/50">
                <label className="text-white font-bold text-lg mb-3 flex items-center gap-2">
                  <i className="fas fa-heartbeat text-green-400"></i>
                  Treatment
                </label>
                <textarea
                  name="treatment"
                  defaultValue={editingReport?.treatment || ''}
                  placeholder="Describe the treatment plan..."
                  rows="4"
                  className="w-full p-4 bg-slate-600/50 border border-slate-500/50 rounded-xl text-white resize-vertical focus:border-green-500/50 focus:ring-2 focus:ring-green-500/20 transition-all"
                />
              </div>
            </div>

            {/* Medications */}
            <div className="bg-slate-700/30 rounded-2xl p-6 border border-slate-600/50">
              <label className="text-white font-bold text-lg mb-3 flex items-center gap-2">
                <i className="fas fa-pills text-blue-400"></i>
                Perscribed Medications
              </label>
              <textarea
                name="medications"
                defaultValue={editingReport?.medications || ''}
                placeholder="List medications, dosages and instructions for use..."
                rows="3"
                className="w-full p-4 bg-slate-600/50 border border-slate-500/50 rounded-xl text-white resize-vertical focus:border-blue-500/50 focus:ring-2 focus:ring-blue-500/20 transition-all"
              />
            </div>

            {/* Notes & Recommendations */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
              <div className="bg-slate-700/30 rounded-2xl p-6 border border-slate-600/50">
                <label className="text-white font-bold text-lg mb-3 flex items-center gap-2">
                  <i className="fas fa-clipboard-list text-slate-400"></i>
                  Notes
                </label>
                <textarea
                  name="notes"
                  defaultValue={editingReport?.notes || ''}
                  placeholder="Extra medicine note..."
                  rows="3"
                  className="w-full p-4 bg-slate-600/50 border border-slate-500/50 rounded-xl text-white resize-vertical focus:border-slate-500/50 focus:ring-2 focus:ring-slate-500/20 transition-all"
                />
              </div>

              <div className="bg-slate-700/30 rounded-2xl p-6 border border-slate-600/50">
                <label className="text-white font-bold text-lg mb-3 flex items-center gap-2">
                  <i className="fas fa-calendar-check text-indigo-400"></i>
                  Recommendations
                </label>
                <textarea
                  name="recommendations"
                  defaultValue={editingReport?.recommendations || ''}
                  placeholder="Recommendations to follow..."
                  rows="3"
                  className="w-full p-4 bg-slate-600/50 border border-slate-500/50 rounded-xl text-white resize-vertical focus:border-indigo-500/50 focus:ring-2 focus:ring-indigo-500/20 transition-all"
                />
              </div>
            </div>

            {/*  Submit Button */}
            <div className="flex gap-4 pt-4">
              <button
                type="button"
                onClick={onClose}
                className="flex-1 px-6 py-4 bg-slate-600/50 hover:bg-slate-600/70 text-white rounded-xl font-bold border border-slate-500/50 transition-all duration-200 hover:scale-105"
              >
                Cancel
              </button>
              <button
                type="submit"
                className="flex-1 px-6 py-4 bg-gradient-to-r from-purple-600 to-indigo-600 hover:from-purple-700 hover:to-indigo-700 text-white rounded-xl font-bold shadow-lg hover:shadow-purple-500/25 transition-all duration-200 hover:scale-105 flex items-center justify-center gap-2"
              >
                <FileText className="w-5 h-5" />
                {editingReport ? 'Update Report' : 'Create Report'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default ReportModal;