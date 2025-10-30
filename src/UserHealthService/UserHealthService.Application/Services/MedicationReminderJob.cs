using UserHealthService.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace UserHealthService.Application.Services
{
    public class MedicationReminderJob : IMedicationReminderJob
    {
        public async Task ProcessReminders()
        {
            // Simple console output for testing
            Console.WriteLine($" MEDICATION REMINDER JOB STARTED: {DateTime.UtcNow}");
            
            // Simulate work
            await Task.Delay(100);
            
            Console.WriteLine($" MEDICATION REMINDER JOB COMPLETED: {DateTime.UtcNow}");
        }
    }
}