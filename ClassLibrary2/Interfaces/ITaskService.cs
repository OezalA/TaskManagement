using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces
{
    public interface ITaskService
    {
        Task<TaskItem> CreateAsync(TaskItem task);
        Task<TaskItem?> GetByIdAsync(Guid id);
        //Task<List<TaskItem>> GetAllAsync();
        Task<List<TaskItem>>GetByProjectAsync(Guid projectID);
        Task<bool> UpdateAsync(TaskItem taskItem);
        Task<bool> MarkAsDoneAsync(Guid taskId);
        Task<bool> DeleteAsync(Guid Id);
        Task<bool> AssignUserAsync (Guid taskId, Guid userId);
        
    }
}
