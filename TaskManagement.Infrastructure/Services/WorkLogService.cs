using TaskManagement.Application.DTOs;
using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace TaskManagement.Infrastructure.Services
{
    public class WorkLogService : IWorkLogService
    {
        private readonly AppDbContext _dbContext;

        public WorkLogService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<WorkLogDto> StartWorkAsync(Guid taskId, Guid userId)
        {
            // Rule 1: Check if user already has an active work log
            var activeWorkLog = await _dbContext.WorkLogs
                .FirstOrDefaultAsync(wl => wl.UserId == userId && wl.EndTime == null);

            if (activeWorkLog != null)
            {
                throw new ConflictException(
                    $"User already has an active work log on task {activeWorkLog.TaskId}. " +
                    $"Stop that work log first before starting a new one.",
                    "active_worklog_exists");
            }

            // Rule 2: Check if task exists
            var task = await _dbContext.TaskItems
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                throw new NotFoundException($"Task with ID {taskId} not found.", "task_not_found");
            }

            // Rule 3: Check if task is assigned to the user (optional but recommended)
            if (task.AssignedUserId != userId)
            {
                throw new ConflictException(
                    $"Task {taskId} is not assigned to user {userId}. " +
                    $"Only assigned users can log work on this task.",
                    "task_not_assigned_to_user");
            }

            // Create new WorkLog
            var workLog = new WorkLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TaskId = taskId,
                StartTime = DateTime.UtcNow,
                EndTime = null,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.WorkLogs.Add(workLog);
            await _dbContext.SaveChangesAsync();

            return MapToDto(workLog);
        }

        public async Task<WorkLogDto> StopWorkAsync(Guid taskId, Guid userId)
        {
            // Find active work log for this task and user
            var workLog = await _dbContext.WorkLogs
                .FirstOrDefaultAsync(wl => 
                    wl.TaskId == taskId && 
                    wl.UserId == userId && 
                    wl.EndTime == null);

            if (workLog == null)
            {
                throw new NotFoundException(
                    $"No active work log found for task {taskId} and user {userId}.",
                    "worklog_not_found");
            }

            // Set end time
            workLog.EndTime = DateTime.UtcNow;

            _dbContext.WorkLogs.Update(workLog);
            await _dbContext.SaveChangesAsync();

            return MapToDto(workLog);
        }

        public async Task<WorkLogDto?> GetActiveWorkLogAsync(Guid userId)
        {
            var workLog = await _dbContext.WorkLogs
                .FirstOrDefaultAsync(wl => wl.UserId == userId && wl.EndTime == null);

            if (workLog == null)
                return null;

            return MapToDto(workLog);
        }

        private WorkLogDto MapToDto(WorkLog workLog)
        {
            return new WorkLogDto
            {
                Id = workLog.Id,
                UserId = workLog.UserId,
                TaskId = workLog.TaskId,
                StartTime = workLog.StartTime,
                EndTime = workLog.EndTime,
                DurationMinutes = workLog.DurationMinutes,
                CreatedAt = workLog.CreatedAt
            };
        }
    }
}
