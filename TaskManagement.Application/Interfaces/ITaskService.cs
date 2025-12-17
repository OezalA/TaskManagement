using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces
{
    public interface ITaskService
    {
        Task<TaskItem> CreateAsync(TaskItem task);
        Task<TaskItem?> GetByIdAsync(Guid id);
        //Task<List<TaskItem>> GetAllAsync();
        Task<List<TaskItem>>GetByProjectAsync(Guid projectID);
        Task UpdateAsync(TaskItem taskItem);
        Task UpdatePartialAsync(Guid taskId, UpdateTaskRequest request);

        Task MarkAsDoneAsync(Guid taskId);
        Task DeleteAsync(Guid Id);
        Task AssignUserAsync (Guid taskId, Guid userId);
        
    }
}
