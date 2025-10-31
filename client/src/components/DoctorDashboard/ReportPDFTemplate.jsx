// src/components/DoctorDashboard/ReportPDFTemplate.jsx

export const generateReportPDFTemplate = (report) => {
  return `
    <!DOCTYPE html>
    <html>
    <head>
      <title>Medical Report - ${report.id}</title>
      <meta charset="UTF-8">
      <script src="https://cdn.tailwindcss.com"></script>
      <style>
        @import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap');
        
        * {
          margin: 0;
          padding: 0;
          box-sizing: border-box;
        }
        
        body {
          font-family: 'Inter', sans-serif;
        }
        
        @media print {
          body {
            padding: 0 !important;
            margin: 0 !important;
          }
          
          .print-controls {
            display: none !important;
          }
          
          .container {
            box-shadow: none !important;
            border-radius: 0 !important;
            margin: 0 !important;
            max-width: none !important;
          }
        }
        
        .print-controls {
          font-family: 'Inter', sans-serif;
        }
      </style>
    </head>
    <body class="bg-gradient-to-br from-blue-900 via-purple-900 to-blue-900 min-h-screen p-8">
      <!-- Print Controls -->
      <div class="print-controls fixed top-6 right-6 z-50 bg-white rounded-xl shadow-2xl border border-gray-200 p-6 max-w-xs">
        <div class="flex items-center gap-3 mb-3">
          <div class="w-10 h-10 bg-blue-500 rounded-lg flex items-center justify-center">
            <span class="text-white text-lg">📄</span>
          </div>
          <div>
            <h3 class="text-gray-900 font-bold text-lg">Download Report</h3>
            <p class="text-gray-600 text-sm">Save as PDF via print dialog</p>
          </div>
        </div>
        <div class="flex gap-3">
          <button onclick="window.print()" class="flex-1 bg-blue-500 hover:bg-blue-600 text-white font-semibold py-3 px-4 rounded-lg transition-all duration-200 hover:scale-105 flex items-center justify-center gap-2">
            <span>🖨️</span>
            Print
          </button>
          <button onclick="window.close()" class="bg-gray-500 hover:bg-gray-600 text-white font-semibold py-3 px-4 rounded-lg transition-all duration-200 hover:scale-105 flex items-center justify-center gap-2">
            <span>✕</span>
            Close
          </button>
        </div>
      </div>

      <!-- Main Report Container -->
      <div class="container max-w-4xl mx-auto bg-white rounded-2xl shadow-2xl overflow-hidden">
        <!-- Header Section -->
        <div class="bg-gradient-to-r from-blue-600 to-purple-600 text-white p-8 text-center">
          <div class="mb-2">
            <span class="text-blue-200 text-sm font-medium tracking-wider">MEDICAL CENTER</span>
          </div>
          <h1 class="text-4xl font-bold mb-2">Medical Examination Report</h1>
          <p class="text-blue-100 text-lg">Official Healthcare Document</p>
        </div>

        <!-- Patient Information -->
        <div class="bg-gray-50 p-8 border-b border-gray-200">
          <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
            <!-- Patient Info Card -->
            <div class="bg-white rounded-xl p-6 border border-gray-200 shadow-sm">
              <div class="flex items-center gap-3 mb-4">
                <div class="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                  <span class="text-blue-600 text-lg">👤</span>
                </div>
                <h3 class="text-blue-600 font-bold text-sm uppercase tracking-wide">Patient Information</h3>
              </div>
              <div class="space-y-3">
                <div class="flex justify-between items-center pb-2 border-b border-gray-100">
                  <span class="text-gray-600 font-medium text-sm">Full Name:</span>
                  <span class="text-gray-900 font-semibold">${report.userName || 'Not Specified'}</span>
                </div>
                <div class="flex justify-between items-center">
                  <span class="text-gray-600 font-medium text-sm">Patient ID:</span>
                  <span class="text-gray-900 font-semibold">${report.userId || 'N/A'}</span>
                </div>
              </div>
            </div>

            <!-- Medical Info Card -->
            <div class="bg-white rounded-xl p-6 border border-gray-200 shadow-sm">
              <div class="flex items-center gap-3 mb-4">
                <div class="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
                  <span class="text-green-600 text-lg">👨‍⚕️</span>
                </div>
                <h3 class="text-green-600 font-bold text-sm uppercase tracking-wide">Medical Information</h3>
              </div>
              <div class="space-y-3">
                <div class="flex justify-between items-center pb-2 border-b border-gray-100">
                  <span class="text-gray-600 font-medium text-sm">Doctor:</span>
                  <span class="text-gray-900 font-semibold">${report.doctorName || 'Not Specified'}</span>
                </div>
                <div class="flex justify-between items-center">
                  <span class="text-gray-600 font-medium text-sm">Specialty:</span>
                  <span class="text-gray-900 font-semibold">${report.specialty || 'General Medicine'}</span>
                </div>
              </div>
            </div>

            <!-- Appointment Info Card -->
            <div class="bg-white rounded-xl p-6 border border-gray-200 shadow-sm">
              <div class="flex items-center gap-3 mb-4">
                <div class="w-10 h-10 bg-purple-100 rounded-lg flex items-center justify-center">
                  <span class="text-purple-600 text-lg">📅</span>
                </div>
                <h3 class="text-purple-600 font-bold text-sm uppercase tracking-wide">Appointment Details</h3>
              </div>
              <div class="space-y-3">
                <div class="flex justify-between items-center pb-2 border-b border-gray-100">
                  <span class="text-gray-600 font-medium text-sm">Report Date:</span>
                  <span class="text-gray-900 font-semibold">${new Date(report.reportDate).toLocaleDateString('en-US', {
                    year: 'numeric',
                    month: 'long',
                    day: 'numeric'
                  })}</span>
                </div>
                <div class="flex justify-between items-center">
                  <span class="text-gray-600 font-medium text-sm">Appointment ID:</span>
                  <span class="text-gray-900 font-semibold">${report.appointmentId || 'N/A'}</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Medical Content -->
        <div class="p-8">
          <!-- Diagnosis Section -->
          <div class="mb-8">
            <div class="flex items-center gap-4 mb-6 pb-4 border-b border-gray-200">
              <div class="w-12 h-12 bg-red-500 rounded-xl flex items-center justify-center">
                <span class="text-white text-xl">🩺</span>
              </div>
              <h2 class="text-2xl font-bold text-gray-900">Medical Diagnosis</h2>
            </div>
            <div class="bg-gray-50 rounded-xl p-6 border-l-4 border-red-500">
              <p class="text-gray-700 leading-relaxed whitespace-pre-wrap">${report.diagnosis || '<span class="text-gray-500 italic">No diagnosis recorded</span>'}</p>
            </div>
          </div>

          <!-- Symptoms & Treatment Grid -->
          <div class="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-8">
            <!-- Symptoms -->
            <div>
              <div class="flex items-center gap-4 mb-6 pb-4 border-b border-gray-200">
                <div class="w-12 h-12 bg-orange-500 rounded-xl flex items-center justify-center">
                  <span class="text-white text-xl">🌡️</span>
                </div>
                <h2 class="text-2xl font-bold text-gray-900">Symptoms</h2>
              </div>
              <div class="bg-gray-50 rounded-xl p-6 border-l-4 border-orange-500">
                <p class="text-gray-700 leading-relaxed whitespace-pre-wrap">${report.symptoms || '<span class="text-gray-500 italic">No symptoms recorded</span>'}</p>
              </div>
            </div>

            <!-- Treatment -->
            <div>
              <div class="flex items-center gap-4 mb-6 pb-4 border-b border-gray-200">
                <div class="w-12 h-12 bg-green-500 rounded-xl flex items-center justify-center">
                  <span class="text-white text-xl">💗</span>
                </div>
                <h2 class="text-2xl font-bold text-gray-900">Treatment Plan</h2>
              </div>
              <div class="bg-gray-50 rounded-xl p-6 border-l-4 border-green-500">
                <p class="text-gray-700 leading-relaxed whitespace-pre-wrap">${report.treatment || '<span class="text-gray-500 italic">No treatment plan specified</span>'}</p>
              </div>
            </div>
          </div>

          <!-- Medications -->
          <div class="mb-8">
            <div class="flex items-center gap-4 mb-6 pb-4 border-b border-gray-200">
              <div class="w-12 h-12 bg-blue-500 rounded-xl flex items-center justify-center">
                <span class="text-white text-xl">💊</span>
              </div>
              <h2 class="text-2xl font-bold text-gray-900">Prescribed Medications</h2>
            </div>
            <div class="bg-gray-50 rounded-xl p-6 border-l-4 border-blue-500">
              <p class="text-gray-700 leading-relaxed whitespace-pre-wrap">${report.medications || '<span class="text-gray-500 italic">No medications prescribed</span>'}</p>
            </div>
          </div>

          <!-- Notes & Recommendations Grid -->
          <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
            <!-- Clinical Notes -->
            <div>
              <div class="flex items-center gap-4 mb-6 pb-4 border-b border-gray-200">
                <div class="w-12 h-12 bg-gray-500 rounded-xl flex items-center justify-center">
                  <span class="text-white text-xl">📝</span>
                </div>
                <h2 class="text-2xl font-bold text-gray-900">Clinical Notes</h2>
              </div>
              <div class="bg-gray-50 rounded-xl p-6 border-l-4 border-gray-500">
                <p class="text-gray-700 leading-relaxed whitespace-pre-wrap">${report.notes || '<span class="text-gray-500 italic">No additional notes</span>'}</p>
              </div>
            </div>

            <!-- Recommendations -->
            <div>
              <div class="flex items-center gap-4 mb-6 pb-4 border-b border-gray-200">
                <div class="w-12 h-12 bg-purple-500 rounded-xl flex items-center justify-center">
                  <span class="text-white text-xl">✅</span>
                </div>
                <h2 class="text-2xl font-bold text-gray-900">Recommendations</h2>
              </div>
              <div class="bg-gray-50 rounded-xl p-6 border-l-4 border-purple-500">
                <p class="text-gray-700 leading-relaxed whitespace-pre-wrap">${report.recommendations || '<span class="text-gray-500 italic">No specific recommendations</span>'}</p>
              </div>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="bg-gray-900 text-white p-8">
          <!-- Report Metadata -->
          <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
            <div class="text-center">
              <div class="text-gray-400 text-sm font-medium mb-1">Created Date</div>
              <div class="font-semibold">${new Date(report.createdAt).toLocaleDateString('en-US')}</div>
            </div>
            <div class="text-center">
              <div class="text-gray-400 text-sm font-medium mb-1">Last Updated</div>
              <div class="font-semibold">${new Date(report.updatedAt).toLocaleDateString('en-US')}</div>
            </div>
            <div class="text-center">
              <div class="text-gray-400 text-sm font-medium mb-1">Report ID</div>
              <div class="font-semibold">${report.id}</div>
            </div>
          </div>

          <!-- Signature Area -->
          <div class="border-t border-gray-700 pt-8 text-center">
            <div class="max-w-md mx-auto">
              <div class="border-t border-gray-600 mt-8 mb-4"></div>
              <div class="text-gray-400 text-sm mb-1">Doctor's Signature</div>
              <div class="font-semibold text-lg">${report.doctorName || 'Medical Practitioner'}</div>
            </div>
          </div>

          <!-- Contact Information -->
          <div class="text-center mt-8 pt-6 border-t border-gray-700">
            <div class="text-gray-400 text-sm space-y-1">
              <p>📞 +1 (555) 123-4567 | 🌐 www.medicalcenter.com</p>
              <p>📍 123 Healthcare Street, Medical City, MC 12345</p>
              <p class="mt-3 text-gray-500 italic">This document contains sensitive medical information. Please store it securely.</p>
            </div>
          </div>
        </div>
      </div>

      <script>
        // Optional: Auto-print after delay
        // setTimeout(() => {
        //   window.print();
        // }, 1500);
      </script>
    </body>
    </html>
  `;
};

export const downloadHtmlAsPdf = (htmlContent, filename) => {
  return new Promise((resolve, reject) => {
    try {
      const printWindow = window.open('', '_blank');
      
      if (!printWindow) {
        alert('Please allow popups to download PDF');
        reject(new Error('Popup blocked'));
        return;
      }
      
      printWindow.document.write(htmlContent);
      printWindow.document.close();
     
      printWindow.onload = function() {
        setTimeout(() => {
          try {
            printWindow.focus();
           
            printWindow.onafterprint = function() {
              setTimeout(() => {
                printWindow.close();
                resolve();
              }, 1000);
            };
            
          } catch (error) {
            console.error('Error in print preparation:', error);
            reject(error);
          }
        }, 500);
      };
      
      printWindow.onerror = function() {
        alert('Error loading PDF content');
        reject(new Error('Window loading error'));
      };
      
    } catch (error) {
      console.error('Error creating PDF window:', error);
      alert('Error creating PDF document');
      reject(error);
    }
  });
};