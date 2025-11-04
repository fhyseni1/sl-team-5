using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using UserHealthService.Application.DTOs.Appointments;
using UserHealthService.Application.Interfaces;

namespace UserHealthService.Application.Services
{
    public class PDFReportService : IPDFReportService
    {
        public PDFReportService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateAppointmentReportPDFAsync(AppointmentReportResponseDto report)
        {
            return await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        page.Header()
                            .AlignCenter()
                            .Text("Medical Visit Report")
                            .SemiBold().FontSize(24).FontColor(Colors.Blue.Medium);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(column =>
                            {
                        column.Spacing(20);

                        // Clinic Information
                        column.Item().Background(Colors.Grey.Lighten3).Padding(10).Column(clinicColumn =>
                        {
                            clinicColumn.Item().Text("Clinic Information").SemiBold().FontSize(14);
                            clinicColumn.Item().Text("HealthCare Medical Center");
                            clinicColumn.Item().Text("123 Medical Drive, Health City");
                            clinicColumn.Item().Text("Phone: +1 (555) 123-4567");
                            clinicColumn.Item().Text("Email: info@healthcaremedical.com");
                        });

                        // Patient Information
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Patient Information").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Doctor Information").FontColor(Colors.White).Bold();
                            });

                            table.Cell().Padding(5).Text($"Name: {report.UserName}");
                            table.Cell().Padding(5).Text($"Name: {report.DoctorName}");

                            table.Cell().Padding(5).Text($"Patient ID: {report.UserId}");
                            table.Cell().Padding(5).Text($"Specialty: {report.Specialty}");

                            table.Cell().Padding(5).Text($"Report Date: {report.ReportDate:dd/MM/yyyy}");
                            table.Cell().Padding(5).Text($"Doctor ID: {report.DoctorId}");
                        });

                        
                        column.Item().Background(Colors.Grey.Lighten4).Padding(15).Column(medicalColumn =>
                        {
                            medicalColumn.Spacing(10);

                            medicalColumn.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Diagnosis:").SemiBold();
                                row.RelativeItem(3).Text(string.IsNullOrEmpty(report.Diagnosis) ? "Not specified" : report.Diagnosis);
                            });

                            // Symptoms
                            medicalColumn.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Symptoms:").SemiBold();
                                row.RelativeItem(3).Text(string.IsNullOrEmpty(report.Symptoms) ? "Not specified" : report.Symptoms);
                            });

                            // Treatment
                            medicalColumn.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Treatment:").SemiBold();
                                row.RelativeItem(3).Text(string.IsNullOrEmpty(report.Treatment) ? "Not specified" : report.Treatment);
                            });

                            // Medications
                            medicalColumn.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Medications:").SemiBold();
                                row.RelativeItem(3).Text(string.IsNullOrEmpty(report.Medications) ? "Not specified" : report.Medications);
                            });
                        });

                        column.Item().Table(additionalTable =>
                        {
                            additionalTable.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            additionalTable.Cell().Column(notesColumn =>
                            {
                                notesColumn.Item().Text("Clinical Notes").SemiBold().FontSize(12);
                                notesColumn.Item().Background(Colors.White).Padding(8).MinHeight(50)
                                    .Text(string.IsNullOrEmpty(report.Notes) ? "No additional notes." : report.Notes);
                            });

                            additionalTable.Cell().Column(recommendationsColumn =>
                            {
                                recommendationsColumn.Item().Text("Recommendations").SemiBold().FontSize(12);
                                recommendationsColumn.Item().Background(Colors.White).Padding(8).MinHeight(50)
                                    .Text(string.IsNullOrEmpty(report.Recommendations) ? "No specific recommendations." : report.Recommendations);
                            });
                        });

                        column.Item().PaddingTop(20).AlignCenter().Text(text =>
                        {
                            text.Span("Report generated on: ").SemiBold();
                            text.Span($"{DateTime.Now:dd/MM/yyyy HH:mm}");
                            text.EmptyLine();
                            text.Span("Report created: ").SemiBold();
                            text.Span($"{report.CreatedAt:dd/MM/yyyy}");
                            text.EmptyLine();
                            text.Span("Last updated: ").SemiBold();
                            text.Span($"{report.UpdatedAt:dd/MM/yyyy}");
                        });
                });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Page ");
                                x.CurrentPageNumber();
                                x.Span(" of ");
                                x.TotalPages();
                                x.Span(" - Confidential Medical Document");
                            });
                            });
                        });

                        return document.GeneratePdf();
                    });
                }

                public string GetReportFileName(AppointmentReportResponseDto report)
                {
                    var cleanUserName = System.Text.RegularExpressions.Regex.Replace(report.UserName, @"[^a-zA-Z0-9_-]", "_");
                    var date = report.ReportDate.ToString("yyyyMMdd");
                    return $"Medical_Report_{cleanUserName}_{date}.pdf";
                }

                public async Task TestPDFGeneration()
                {
                    var sampleReport = new AppointmentReportResponseDto
                    {
                        Id = Guid.NewGuid(),
                        AppointmentId = Guid.NewGuid(),
                        UserId = Guid.NewGuid(),
                        UserName = "John Doe",
                        DoctorId = Guid.NewGuid(),
                        DoctorName = "Dr. Smith",
                        Specialty = "Cardiology",
                        ReportDate = DateTime.UtcNow,
                        Diagnosis = "Hypertension Stage 1",
                        Symptoms = "Elevated blood pressure, occasional headaches",
                        Treatment = "Lifestyle modifications and medication",
                        Medications = "Lisinopril 10mg daily",
                        Notes = "Patient advised to reduce sodium intake and exercise regularly",
                        Recommendations = "Follow up in 3 months for blood pressure check",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var pdfBytes = await GenerateAppointmentReportPDFAsync(sampleReport);
                    var fileName = GetReportFileName(sampleReport);
                    
                    await File.WriteAllBytesAsync(fileName, pdfBytes);
                    
                    Console.WriteLine($"PDF generated successfully: {fileName}");
                    Console.WriteLine($"File size: {pdfBytes.Length} bytes");
                }
            }
        }