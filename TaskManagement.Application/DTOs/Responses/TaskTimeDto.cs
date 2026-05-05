namespace TaskManagement.Application.DTOs.Responses
{
    public class TaskTimeDto
    {
        public Guid TaskId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public int TotalMinutes { get; set; }
        public int LogCount { get; set; }
    }
}
