
using TaskManagement.Domain.Enums;

namespace TaskManagement.Domain.Entities
{
    public class TaskItem : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
        public TaskItemStatus Status { get; set; }

        // Foreign
        public Guid? AssignedUserId { get; set; }
        public  User? AssignedUser { get; set; }
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = default!;

        
    }

    
}
