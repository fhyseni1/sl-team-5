using System.Threading.Tasks;

namespace UserHealthService.Application.Interfaces
{
    public interface IMedicationReminderJob
    {
        Task ProcessReminders();
    }
}