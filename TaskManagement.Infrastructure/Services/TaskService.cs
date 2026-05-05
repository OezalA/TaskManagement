using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Persistence;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Infrastructure.Services
{
    public class TaskService : ITaskService
    {
        private AppDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;
        public TaskService(AppDbContext dbContext, ICurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
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

            if (request.Title != null)
                task.Title = request.Title;

            if (request.Description != null)
                task.Description = request.Description;

            if (request.DueDate.HasValue)
                task.DueDate = DateTime.SpecifyKind(request.DueDate.Value, DateTimeKind.Utc);

            if (request.Status.HasValue)
                task.Status = request.Status.Value;

            if (request.AssignedUserId.HasValue)
            {
                var userExists = await _dbContext.Users.AnyAsync(u => u.Id == request.AssignedUserId.Value);
                if (!userExists)
                    throw new NotFoundException("User not found", "UserNotFound");

                task.AssignedUserId = request.AssignedUserId;
            }

            if (request.ProjectId.HasValue)
            {
                var projectExists = await _dbContext.Projects.AnyAsync(p => p.Id == request.ProjectId.Value);
                if (!projectExists)
                    throw new NotFoundException("Project not found", "ProjectNotFound");

                task.ProjectId = request.ProjectId.Value;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task MarkAsDoneAsync(Guid taskId)
        {
            var task = await _dbContext.TaskItems.FindAsync(taskId);
            if (task == null)
                throw new NotFoundException("Task not found", "TaskNotFound");

            if (task.Status == TaskItemStatus.Done)
                throw new ConflictException(
                    "Task is already completed",
                    "TaskAlreadyCompleted"
                );

            task.Status = TaskItemStatus.Done;
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

        public async Task CompleteAsync(Guid taskId)
        {
            var task = await _dbContext.TaskItems.FindAsync(taskId);
            if (task == null)
                throw new NotFoundException("Task not found", "TaskNotFound");

            if (task.Status == TaskItemStatus.Done)
                throw new ConflictException(
                    "Task is already completed",
                    "TaskAlreadyCompleted"
                );

            // Rule: Cannot complete if there's an active worklog
            var activeWorkLog = await _dbContext.WorkLogs
                .AnyAsync(wl => wl.TaskId == taskId && wl.EndTime == null);

            if (activeWorkLog)
                throw new ConflictException(
                    "Cannot complete task while work is still in progress. " +
                    "Stop the active worklog first.",
                    "ActiveWorkLogExists"
                );

            task.Status = TaskItemStatus.Done;
            task.DueDate = DateTime.UtcNow; 
            await _dbContext.SaveChangesAsync();
        }
        public async Task AssignToMeAsync(Guid taskId)
        {
            var task = await _dbContext.TaskItems.FindAsync(taskId);
            if (task == null)
                throw new NotFoundException("Task not found", "TaskNotFound");

            var currentUser = await _currentUserService.GetCurrentUserAsync();

            task.AssignedUserId = currentUser.Id;
            await _dbContext.SaveChangesAsync();
        }

    }
}
