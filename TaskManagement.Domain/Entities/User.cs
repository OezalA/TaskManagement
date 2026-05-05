
namespace TaskManagement.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string? Email { get; set; }

        // Entra ID mapping
        public string EntraObjectId { get; set; } = string.Empty;

        // Navigation
        public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
        public ICollection<TeamUser> Teams { get; set; } = new List<TeamUser>();
        public ICollection<WorkLog> WorkLogs { get; set; } = new List<WorkLog>();
    }
}
