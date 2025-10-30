using System.Net.Http.Headers;
using UserHealthService.Application.DTOs.Appointments;

namespace UserHealthService.Application.Interfaces
{
    public interface IPDFReportService
    {
        Task<byte[]> GenerateAppointmentReportPDFAsync(AppointmentReportResponseDto report);
        string GetReportFileName(AppointmentReportResponseDto report);
    }
}
