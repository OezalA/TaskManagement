using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Persistence;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Infrastructure.Services
{
    public class TaskService : ITaskService
    {
        private AppDbContext _dbContext;

        public TaskService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TaskItem> CreateAsync(TaskItem task)
        {
            _dbContext.TaskItems.Add(task);
           await _dbContext.SaveChangesAsync();

            return task;
        }

        public async Task<TaskItem?> GeByIdAsync(Guid id)
        {
            var task =  await _dbContext.TaskItems
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);

            return task;


        }

        public async Task<List<TaskItem>> GetByProjectAsync(Guid projectID)
        {
            var task = await _dbContext.TaskItems
                .Where(t => t.ProjectId == projectID)
                .AsNoTracking()
                .ToListAsync();

            return task;
        }

        public async Task<bool> UpdateAsync(TaskItem taskItem)
        {
            var exist = await _dbContext.TaskItems.AnyAsync(t => t.Id == taskItem.Id);
            if(!exist)
                return false;

            _dbContext.TaskItems.Update(taskItem);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> MarkAsDoneAsync(Guid taskId)
        {
            var task = await _dbContext.TaskItems.FindAsync(taskId);
            if(task == null) return false;

            task.Status = TaskStatus.Done;
            await _dbContext.SaveChangesAsync();

            return true;

        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var task = await _dbContext.TaskItems.FindAsync(id);
            if (task == null) return false;

            _dbContext.TaskItems.Remove(task);
            await _dbContext.SaveChangesAsync();

            return true;

            
        }

        public async Task<bool> AssignUserAsync(Guid taskId, Guid userId)
        {
            var task = await _dbContext.TaskItems.FindAsync(taskId);
            if (task == null) return false;

            var userExist = await _dbContext.Users.AnyAsync(u => u.Id == userId);
            if(!userExist) return false;

            task.AssignedUserId = userId;
            await _dbContext.SaveChangesAsync();

            return true;

        }
    }
}
