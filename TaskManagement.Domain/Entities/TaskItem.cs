using System;
using System.Collections.Generic;
using System.Linq;

namespace TaskManagement.Domain.Entities
{
    public class TaskItem : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
        public TaskStatus Status { get; set; }

        // Foreign
        public Guid? AssignedUserId { get; set; }
        public  User? AssignedUser { get; set; }
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = default!;

        
    }

    public enum TaskStatus
    {
        Todo = 0,
        InProgress = 1,
        Done = 2
    }
}
