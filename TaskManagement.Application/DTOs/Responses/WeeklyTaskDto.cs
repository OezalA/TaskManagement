namespace TaskManagement.Application.DTOs.Responses
{
    public class WeeklyTaskDto
    {
        public Guid TaskId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int TotalMinutes { get; set; }
        public int LogCount { get; set; }
        public DateTime FirstLogDate { get; set; }
        public DateTime LastLogDate { get; set; }
    }
}
