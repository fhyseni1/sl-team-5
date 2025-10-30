// src/components/DoctorDashboard/ReportPDFTemplate.js

export const generateReportPDFTemplate = (report) => {
  return `
    <!DOCTYPE html>
    <html>
    <head>
      <title>Medicine Report - ${report.id}</title>
      <meta charset="UTF-8">
      <style>
        @import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap');
        @import url('https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css');
        
        * {
          margin: 0;
          padding: 0;
          box-sizing: border-box;
        }
        
        body {
          font-family: 'Inter', sans-serif;
          line-height: 1.6;
          color: #1f2937;
          background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
          min-height: 100vh;
          padding: 40px 20px;
        }
        
        .container {
          max-width: 800px;
          margin: 0 auto;
          background: white;
          border-radius: 20px;
          box-shadow: 0 20px 60px rgba(0, 0, 0, 0.1);
          overflow: hidden;
        }
        
        /* Header */
        .header {
          background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%);
          color: white;
          padding: 40px;
          text-align: center;
          position: relative;
        }
        
        .header::before {
          content: '';
          position: absolute;
          top: 0;
          left: 0;
          right: 0;
          bottom: 0;
          background: url('data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" fill="white" opacity="0.1"><circle cx="50" cy="50" r="2"/></svg>');
        }
        
        .hospital-logo {
          font-size: 14px;
          opacity: 0.9;
          margin-bottom: 10px;
          font-weight: 300;
          letter-spacing: 1px;
        }
        
        .header h1 {
          font-size: 32px;
          font-weight: 700;
          margin-bottom: 8px;
          letter-spacing: -0.5px;
        }
        
        .header-subtitle {
          font-size: 16px;
          opacity: 0.9;
          font-weight: 400;
        }
        
        /* Patient Info */
        .patient-info {
          padding: 30px 40px;
          background: #f8fafc;
          border-bottom: 1px solid #e2e8f0;
        }
        
        .info-grid {
          display: grid;
          grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
          gap: 20px;
        }
        
        .info-card {
          background: white;
          padding: 20px;
          border-radius: 12px;
          border: 1px solid #e2e8f0;
          box-shadow: 0 2px 10px rgba(0, 0, 0, 0.04);
        }
        
        .info-card h3 {
          color: #4f46e5;
          font-size: 14px;
          font-weight: 600;
          margin-bottom: 12px;
          text-transform: uppercase;
          letter-spacing: 0.5px;
          display: flex;
          align-items: center;
          gap: 8px;
        }
        
        .info-item {
          display: flex;
          justify-content: space-between;
          margin-bottom: 8px;
          padding-bottom: 8px;
          border-bottom: 1px solid #f1f5f9;
        }
        
        .info-item:last-child {
          border-bottom: none;
          margin-bottom: 0;
        }
        
        .info-label {
          font-weight: 500;
          color: #64748b;
          min-width: 120px;
        }
        
        .info-value {
          font-weight: 600;
          color: #1e293b;
          text-align: right;
        }
        
        /* Medical Content */
        .medical-content {
          padding: 40px;
        }
        
        .section {
          margin-bottom: 32px;
          page-break-inside: avoid;
        }
        
        .section:last-child {
          margin-bottom: 0;
        }
        
        .section-header {
          display: flex;
          align-items: center;
          margin-bottom: 20px;
          padding-bottom: 12px;
          border-bottom: 2px solid #e2e8f0;
        }
        
        .section-icon {
          width: 40px;
          height: 40px;
          background: #4f46e5;
          border-radius: 10px;
          display: flex;
          align-items: center;
          justify-content: center;
          margin-right: 16px;
          color: white;
          font-size: 16px;
        }
        
        .section-title {
          font-size: 20px;
          font-weight: 600;
          color: #1e293b;
        }
        
        .content-box {
          background: #f8fafc;
          padding: 24px;
          border-radius: 12px;
          border-left: 4px solid #4f46e5;
        }
        
        .content-text {
          line-height: 1.7;
          color: #374151;
          white-space: pre-wrap;
          font-size: 14px;
        }
        
        .empty-content {
          color: #94a3b8;
          font-style: italic;
        }
        
        /* Footer */
        .footer {
          background: #1e293b;
          color: white;
          padding: 30px 40px;
          text-align: center;
        }
        
        .footer-content {
          display: grid;
          grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
          gap: 20px;
          margin-bottom: 20px;
        }
        
        .footer-item {
          text-align: center;
        }
        
        .footer-label {
          font-size: 12px;
          opacity: 0.7;
          margin-bottom: 4px;
        }
        
        .footer-value {
          font-size: 14px;
          font-weight: 500;
        }
        
        .signature-area {
          margin-top: 30px;
          padding-top: 20px;
          border-top: 1px solid #334155;
        }
        
        .signature-line {
          width: 300px;
          height: 1px;
          background: #475569;
          margin: 40px auto 10px;
        }
        
        .signature-text {
          font-size: 12px;
          opacity: 0.7;
        }
        
        .contact-info {
          margin-top: 20px;
          font-size: 12px;
          opacity: 0.7;
          line-height: 1.6;
        }
        
        /* Print Styles */
        @media print {
          body {
            background: white;
            padding: 0;
          }
          
          .container {
            box-shadow: none;
            border-radius: 0;
          }
          
          .header {
            background: #4f46e5 !important;
            -webkit-print-color-adjust: exact;
          }
          
          .section {
            page-break-inside: avoid;
          }
          
          .section-icon {
            background: #4f46e5 !important;
            -webkit-print-color-adjust: exact;
          }
        }
      </style>
    </head>
    <body>
      <div class="container">
        <!-- Header -->
        <div class="header">
          <div class="hospital-logo">Center Hospital"</div>
          <h1>Medicine Report</h1>
          <div class="header-subtitle">Official Medicine Document</div>
        </div>
        
        <!-- Patient Information -->
        <div class="patient-info">
          <div class="info-grid">
            <div class="info-card">
              <h3><i class="fas fa-user-injured"></i> Patient Information</h3>
              <div class="info-item">
                <span class="info-label">Full Name:</span>
                <span class="info-value">${report.userName || 'Undefined'}</span>
              </div>
              <div class="info-item">
                <span class="info-label">Patiend ID:</span>
                <span class="info-value">${report.userId || 'N/A'}</span>
              </div>
            </div>
            
            <div class="info-card">
              <h3><i class="fas fa-user-md"></i> Medicine Information</h3>
              <div class="info-item">
                <span class="info-label">Doctor:</span>
                <span class="info-value">${report.doctorName || 'Undefined'}</span>
              </div>
              <div class="info-item">
                <span class="info-label">Specialization:</span>
                <span class="info-value">${report.specialty || 'Undefined'}</span>
              </div>
            </div>
            
            <div class="info-card">
              <h3><i class="fas fa-calendar-alt"></i> Appointment Details</h3>
              <div class="info-item">
                <span class="info-label">Report Date:</span>
                <span class="info-value">${new Date(report.reportDate).toLocaleDateString('sq-AL', {
                  year: 'numeric',
                  month: 'long',
                  day: 'numeric'
                })}</span>
              </div>
              <div class="info-item">
                <span class="info-label"> Appointment ID:</span>
                <span class="info-value">${report.appointmentId || 'N/A'}</span>
              </div>
            </div>
          </div>
        </div>
        
        <!-- Medical Content -->
        <div class="medical-content">
          <!-- Diagnosis -->
          <div class="section">
            <div class="section-header">
              <div class="section-icon">
                <i class="fas fa-stethoscope"></i>
              </div>
              <h2 class="section-title">Diagnozë</h2>
            </div>
            <div class="content-box">
              <div class="content-text">
                ${report.diagnosis || '<span class="empty-content">No registered diagnose</span>'}
              </div>
            </div>
          </div>
          
          <!-- Symptoms -->
          <div class="section">
            <div class="section-header">
              <div class="section-icon">
                <i class="fas fa-thermometer-half"></i>
              </div>
              <h2 class="section-title">Symptoms</h2>
            </div>
            <div class="content-box">
              <div class="content-text">
                ${report.symptoms || '<span class="empty-content">No registered symptoms</span>'}
              </div>
            </div>
          </div>
          
          <!-- Treatment -->
          <div class="section">
            <div class="section-header">
              <div class="section-icon">
                <i class="fas fa-heartbeat"></i>
              </div>
              <h2 class="section-title">Treatment</h2>
            </div>
            <div class="content-box">
              <div class="content-text">
                ${report.treatment || '<span class="empty-content">No specific treatment</span>'}
              </div>
            </div>
          </div>
          
          <!-- Medications -->
          <div class="section">
            <div class="section-header">
              <div class="section-icon">
                <i class="fas fa-pills"></i>
              </div>
              <h2 class="section-title">Prescribed Medications</h2>
            </div>
            <div class="content-box">
              <div class="content-text">
                ${report.medications || '<span class="empty-content">No prescribed medications</span>'}
              </div>
            </div>
          </div>
          
          <!-- Notes -->
          <div class="section">
            <div class="section-header">
              <div class="section-icon">
                <i class="fas fa-clipboard-list"></i>
              </div>
              <h2 class="section-title">Clinic Notes</h2>
            </div>
            <div class="content-box">
              <div class="content-text">
                ${report.notes || '<span class="empty-content">No extra notes</span>'}
              </div>
            </div>
          </div>
          
          <!-- Recommendations -->
          <div class="section">
            <div class="section-header">
              <div class="section-icon">
                <i class="fas fa-calendar-check"></i>
              </div>
              <h2 class="section-title">Recommendations</h2>
            </div>
            <div class="content-box">
              <div class="content-text">
                ${report.recommendations || '<span class="empty-content">No specific recommendations</span>'}
              </div>
            </div>
          </div>
        </div>
        
        <!-- Footer -->
        <div class="footer">
          <div class="footer-content">
            <div class="footer-item">
              <div class="footer-label">Created Date</div>
              <div class="footer-value">${new Date(report.createdAt).toLocaleDateString('sq-AL')}</div>
            </div>
            <div class="footer-item">
              <div class="footer-label">Updated Date</div>
              <div class="footer-value">${new Date(report.updatedAt).toLocaleDateString('sq-AL')}</div>
            </div>
            <div class="footer-item">
              <div class="footer-label">Report Number</div>
              <div class="footer-value">${report.id}</div>
            </div>
          </div>
          
          <div class="signature-area">
            <div class="signature-line"></div>
            <div class="signature-text">Doctor Signature</div>
            <div class="signature-text" style="margin-top: 5px;">${report.doctorName || 'Head Doctor'}</div>
          </div>
          
          <div class="contact-info">
            <p><i class="fas fa-phone"></i> +383 49 000 000 | <i class="fas fa-globe"></i> www.center-hospital.com</p>
            <p><i class="fas fa-map-marker-alt"></i> Prishtinë, Kosovë</p>
            <p style="margin-top: 10px;"><em>This is official medicine report. Save it carefully.</em></p>
          </div>
        </div>
      </div>
      
      <script>
        //Add buttons for printing
        document.addEventListener('DOMContentLoaded', function() {
          const buttonContainer = document.createElement('div');
          buttonContainer.innerHTML = \`
            <div style="position: fixed; top: 20px; right: 20px; z-index: 10000; background: white; padding: 20px; border-radius: 12px; box-shadow: 0 8px 30px rgba(0,0,0,0.3); font-family: 'Inter', sans-serif; border: 1px solid #e2e8f0;">
              <h3 style="margin: 0 0 12px 0; color: #1e293b; font-size: 16px; font-weight: 600;">
                <i class="fas fa-print"></i> Shkarko Raportin
              </h3>
              <p style="margin: 0 0 16px 0; color: #64748b; font-size: 13px; line-height: 1.4;">
                For downloading as PDF, select "Save as PDF" in printing dialogue.
              </p>
              <div style="display: flex; gap: 10px;">
                <button onclick="window.print()" style="background: #4f46e5; color: white; border: none; padding: 10px 20px; border-radius: 8px; cursor: pointer; font-size: 13px; font-weight: 500; flex: 1; display: flex; align-items: center; justify-content: center; gap: 6px;">
                  <i class="fas fa-print"></i> Open for Printing
                </button>
                <button onclick="window.close()" style="background: #64748b; color: white; border: none; padding: 10px 16px; border-radius: 8px; cursor: pointer; font-size: 13px; font-weight: 500; display: flex; align-items: center; justify-content: center; gap: 6px;">
                  <i class="fas fa-times"></i> Close
                </button>
              </div>
            </div>
          \`;
          document.body.appendChild(buttonContainer);
        });
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
        alert('Please allow pop ups for downloading PDF');
        reject(new Error('Popup blocked'));
        return;
      }
      
      printWindow.document.write(htmlContent);
      printWindow.document.close();
     
      printWindow.onload = function() {
        setTimeout(() => {
          try {
            printWindow.focus();
            printWindow.print();
           
            printWindow.onafterprint = function() {
              setTimeout(() => {
                printWindow.close();
              }, 1000);
            };
            
            resolve();
            
          } catch (error) {
            console.error('Error in opening for printing:', error);
            alert('Error in opening the printing');
            reject(error);
          }
        }, 1000);
      };
      
      printWindow.onerror = function() {
        alert('Gabim në ngarkimin e PDF-së');
        reject(new Error('Window loading error'));
      };
      
    } catch (error) {
      console.error('Error in creation of PDF:', error);
      alert('Error in creation of PDF');
      reject(error);
    }
  });
};