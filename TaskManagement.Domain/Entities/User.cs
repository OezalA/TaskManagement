
namespace TaskManagement.Domain.Entities
{
    public class User : BaseEntity
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Entra ID mapping
        public string EntraObjectId { get; set; } = string.Empty;

        // Navigation
        public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
        public ICollection<TeamUser> Teams { get; set; } = new List<TeamUser>();
    }
}
