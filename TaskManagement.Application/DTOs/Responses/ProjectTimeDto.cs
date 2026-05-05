namespace TaskManagement.Application.DTOs.Responses
{
    public class ProjectTimeDto
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int TotalMinutes { get; set; }
        public int TaskCount { get; set; }
        public int LogCount { get; set; }
    }
}
