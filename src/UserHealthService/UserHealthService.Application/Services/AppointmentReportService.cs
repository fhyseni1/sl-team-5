using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace UserHealthService.Application.Services
{
    public class AppointmentReportService : IAppointmentReportService
    {
        private readonly IAppointmentReportRepository _repository;

        public AppointmentReportService(IAppointmentReportRepository repository)
        {
            _repository = repository;
        }

        public async Task<AppointmentReport> CreateAsync(AppointmentReport report)
        {
            // Set creation timestamp and generate ID if not provided
            if (report.Id == Guid.Empty)
                report.Id = Guid.NewGuid();
                
            report.ReportDate = DateTime.UtcNow;
            report.CreatedAt = DateTime.UtcNow;
            report.UpdatedAt = DateTime.UtcNow;

            return await _repository.AddAsync(report);
        }

        public async Task<AppointmentReport?> GetByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<AppointmentReport?> GetByAppointmentIdAsync(Guid appointmentId)
        {
            return await _repository.GetByAppointmentIdAsync(appointmentId);
        }

        public async Task<IEnumerable<AppointmentReport>> GetByUserIdAsync(Guid userId)
        {
            return await _repository.GetByUserIdAsync(userId);
        }

        public async Task<IEnumerable<AppointmentReport>> GetByDoctorIdAsync(Guid doctorId)
        {
            return await _repository.GetByDoctorIdAsync(doctorId);
        }

        public async Task<AppointmentReport> UpdateAsync(AppointmentReport report)
        {
            report.UpdatedAt = DateTime.UtcNow;
            return await _repository.UpdateAsync(report);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<byte[]> GeneratePdfAsync(Guid reportId)
        {
            var report = await _repository.GetByIdAsync(reportId);
            if (report == null)
                throw new ArgumentException("Report not found");

            return GenerateSimplePdf(report);
        }

        private byte[] GenerateSimplePdf(AppointmentReport report)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream);
            
            // Generate PDF content with all the new fields
            writer.WriteLine("=== APPOINTMENT REPORT ===");
            writer.WriteLine($"Report ID: {report.Id}");
            writer.WriteLine($"Appointment ID: {report.AppointmentId}");
            writer.WriteLine($"Patient ID: {report.UserId}");
            writer.WriteLine($"Doctor ID: {report.DoctorId}");
            writer.WriteLine($"Report Date: {report.ReportDate:yyyy-MM-dd HH:mm}");
            writer.WriteLine();
            writer.WriteLine("=== DIAGNOSIS ===");
            writer.WriteLine(report.Diagnosis);
            writer.WriteLine();
            writer.WriteLine("=== SYMPTOMS ===");
            writer.WriteLine(report.Symptoms);
            writer.WriteLine();
            writer.WriteLine("=== TREATMENT ===");
            writer.WriteLine(report.Treatment);
            writer.WriteLine();
            writer.WriteLine("=== MEDICATIONS ===");
            writer.WriteLine(report.Medications);
            writer.WriteLine();
            writer.WriteLine("=== NOTES ===");
            writer.WriteLine(report.Notes);
            writer.WriteLine();
            writer.WriteLine("=== RECOMMENDATIONS ===");
            writer.WriteLine(report.Recommendations);
            writer.WriteLine();
            writer.WriteLine($"Created: {report.CreatedAt:yyyy-MM-dd HH:mm}");
            writer.WriteLine($"Last Updated: {report.UpdatedAt:yyyy-MM-dd HH:mm}");
            
            writer.Flush();
            return memoryStream.ToArray();
        }
    }
}