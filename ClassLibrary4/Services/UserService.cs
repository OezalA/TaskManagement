using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using TaskManagement.Application.DTOs.Responses;
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
            var users = await _dbContext.Users.AsNoTracking().ToListAsync();

            return users;
        }

        public async Task<List<TaskItem>> GetUserTasksAsync(Guid userId)
        {
            return await _dbContext.TaskItems
                .Include(t => t.Project)
                .Where(t => t.AssignedUserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Project>> GetUserProjectsAsync(Guid userId)
        {
            return await _dbContext.TaskItems
                .Where(t => t.AssignedUserId == userId)
                .Select(t => t.Project)
                .Distinct()
                .AsNoTracking()
                .ToListAsync();

        }

        
    }
}
