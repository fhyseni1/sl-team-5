namespace MedicationService.Domain.Enums
{
    public enum ReminderStatus
    {
        Scheduled = 1,
        Sent = 2,
        Acknowledged = 3,
        Missed = 4,
        Snoozed = 5,
        Cancelled = 6
    }
}

