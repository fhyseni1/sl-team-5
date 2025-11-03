using System.ComponentModel.DataAnnotations;

public class AppointmentReportCreateDto
{
    [Required]
    public string AppointmentId { get; set; }
    [Required]
    public string UserId { get; set; }
    [Required]
    public string DoctorId { get; set; }
    public string ReportDate { get; set; } = string.Empty;
    public Guid? Id { get; set; }

    [Required]
    public string Diagnosis { get; set; } = string.Empty;
    public string Symptoms { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public string Medications { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string Recommendations { get; set; } = string.Empty;
}