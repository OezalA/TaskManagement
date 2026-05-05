namespace TaskManagement.Domain.Entities
{
    public class WorkLog : BaseEntity
    {
        // Foreign Keys
        public Guid UserId { get; set; }
        public Guid TaskId { get; set; }

        // Time tracking
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        // Calculated duration (in minutes) - read-only property
        public int? DurationMinutes
        {
            get
            {
                if (EndTime.HasValue)
                {
                    return (int)(EndTime.Value - StartTime).TotalMinutes;
                }
                return null;
            }
        }

        // Navigation
        public User User { get; set; } = default!;
        public TaskItem Task { get; set; } = default!;
    }
}
