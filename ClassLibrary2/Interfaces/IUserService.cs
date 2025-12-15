using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetAllAsync();
        Task<List<TaskItem>> GetUserTasksAsync(Guid userId);
        Task<List<Project>> GetUserProjectsAsync(Guid userId);
    }
}
