using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Persistence;

namespace TaskManagement.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _dbContext;

        public UserService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _dbContext.Users
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<TaskItem>> GetUserTasksAsync(Guid userId)
        {
            if (!await _dbContext.Users.AnyAsync(u => u.Id == userId))
                throw new NotFoundException("User not found", "UserNotFound");

            return await _dbContext.TaskItems
                .Include(t => t.Project)
                .Where(t => t.AssignedUserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Project>> GetUserProjectsAsync(Guid userId)
        {
            if (!await _dbContext.Users.AnyAsync(u => u.Id == userId))
                throw new NotFoundException("User not found", "UserNotFound");

            return await _dbContext.TaskItems
                .Where(t => t.AssignedUserId == userId)
                .Select(t => t.Project)
                .Distinct()
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
