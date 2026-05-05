namespace TaskManagement.Application.DTOs.Responses
{
    public class WorkLogDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? DurationMinutes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
