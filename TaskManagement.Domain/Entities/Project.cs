
namespace TaskManagement.Domain.Entities
{
    public class Project : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatTime { get; set; } = DateTime.UtcNow;

        public List<TaskItem> Tasks = new();
    }
}
