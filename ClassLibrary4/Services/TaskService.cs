using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Exceptions;
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
            if (!await _dbContext.Projects.AnyAsync(p => p.Id == task.ProjectId))
                throw new NotFoundException("Project not found", "ProjectNotFound");
            
            _dbContext.TaskItems.Add(task);
           await _dbContext.SaveChangesAsync();

            return task;
        }

        public async Task<TaskItem> GetByIdAsync(Guid id)
        {
            var task =  await _dbContext.TaskItems
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);

            if (task == null)
                throw new NotFoundException("Task not found", "TaskNotFound");

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

        public async Task UpdateAsync(TaskItem taskItem)
        {
            var exists = await _dbContext.TaskItems
                .AnyAsync(t => t.Id == taskItem.Id);

            if (!exists)
                throw new NotFoundException("Task not found", "TaskNotFound");

            _dbContext.TaskItems.Update(taskItem);
            await _dbContext.SaveChangesAsync();
        }
        public async Task UpdatePartialAsync(Guid taskId, UpdateTaskRequest request)
        {
            var task = await _dbContext.TaskItems.FindAsync(taskId);
            if (task == null)
                throw new NotFoundException("Task not found", "TaskNotFound");

            var hasAnyChange = false;

            if (request.Title != null)
            {
                task.Title = request.Title;
                hasAnyChange = true;
            }

            if (request.Description != null)
            {
                task.Description = request.Description;
                hasAnyChange = true;
            }

            if (request.DueDate.HasValue)
            {
                task.DueDate = DateTime.SpecifyKind(
                    request.DueDate.Value,
                    DateTimeKind.Utc
                );
                hasAnyChange = true;
            }

            if (!hasAnyChange)
                throw new ValidationException(
                    "No fields provided for update",
                    "EmptyPatchRequest"
                );

            await _dbContext.SaveChangesAsync();
        }

        public async Task MarkAsDoneAsync(Guid taskId)
        {
            var task = await _dbContext.TaskItems.FindAsync(taskId);
            if (task == null)
                throw new NotFoundException("Task not found", "TaskNotFound");

            if (task.Status == TaskStatus.Done)
                throw new ConflictException(
                    "Task is already completed",
                    "TaskAlreadyCompleted"
                );

            task.Status = TaskStatus.Done;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var task = await _dbContext.TaskItems.FindAsync(id);
            if (task == null)
                throw new NotFoundException("Task not found", "TaskNotFound");

            _dbContext.TaskItems.Remove(task);
            await _dbContext.SaveChangesAsync();
        }
        public async Task AssignUserAsync(Guid taskId, Guid userId)
        {
            var task = await _dbContext.TaskItems.FindAsync(taskId);
            if (task == null)
                throw new NotFoundException("Task not found", "TaskNotFound");

            if (!await _dbContext.Users.AnyAsync(u => u.Id == userId))
                throw new NotFoundException("User not found", "UserNotFound");

            if (task.AssignedUserId == userId)
                throw new ConflictException(
                    "User is already assigned to this task",
                    "UserAlreadyAssigned"
                );

            task.AssignedUserId = userId;
            await _dbContext.SaveChangesAsync();
        }
    }
}
