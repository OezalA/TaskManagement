using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Application.Interfaces;
using TaskManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace TaskManagement.Infrastructure.Services
{
    public class WorkLogQueryService : IWorkLogQueryService
    {
        private readonly AppDbContext _dbContext;

        public WorkLogQueryService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<WeeklyTaskDto>> GetThisWeekTasksAsync(Guid userId)
        {
            var today = DateTime.UtcNow;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var weekEnd = weekStart.AddDays(7);

            var weeklyTasks = await _dbContext.WorkLogs
                .Where(wl => wl.UserId == userId && 
                            wl.StartTime >= weekStart && 
                            wl.StartTime < weekEnd)
                .GroupBy(wl => new { wl.TaskId, wl.Task.Title, wl.Task.ProjectId, wl.Task.Project.Name })
                .Select(g => new WeeklyTaskDto
                {
                    TaskId = g.Key.TaskId,
                    TaskTitle = g.Key.Title,
                    ProjectId = g.Key.ProjectId,
                    ProjectName = g.Key.Name,
                    TotalMinutes = g.Sum(wl => wl.DurationMinutes ?? 0),
                    LogCount = g.Count(),
                    FirstLogDate = g.Min(wl => wl.StartTime),
                    LastLogDate = g.Max(wl => wl.StartTime)
                })
                .OrderByDescending(t => t.LastLogDate)
                .ToListAsync();

            return weeklyTasks;
        }

        public async Task<TaskTimeDto?> GetTaskTotalTimeAsync(Guid taskId, Guid userId)
        {
            var result = await _dbContext.WorkLogs
                .Where(wl => wl.TaskId == taskId && wl.UserId == userId)
                .GroupBy(wl => new { wl.TaskId, wl.Task.Title })
                .Select(g => new TaskTimeDto
                {
                    TaskId = g.Key.TaskId,
                    TaskTitle = g.Key.Title,
                    TotalMinutes = g.Sum(wl => wl.DurationMinutes ?? 0),
                    LogCount = g.Count()
                })
                .FirstOrDefaultAsync();

            return result;
        }

        public async Task<ProjectTimeDto?> GetProjectTotalTimeAsync(Guid projectId, Guid userId)
        {
            var result = await _dbContext.WorkLogs
                .Where(wl => wl.Task.ProjectId == projectId && wl.UserId == userId)
                .GroupBy(wl => new { wl.Task.ProjectId, wl.Task.Project.Name })
                .Select(g => new ProjectTimeDto
                {
                    ProjectId = g.Key.ProjectId,
                    ProjectName = g.Key.Name,
                    TotalMinutes = g.Sum(wl => wl.DurationMinutes ?? 0),
                    TaskCount = g.Select(wl => wl.TaskId).Distinct().Count(),
                    LogCount = g.Count()
                })
                .FirstOrDefaultAsync();

            return result;
        }

        public async Task<List<WorkLogDto>> GetTaskWorkLogsAsync(Guid taskId)
        {
            var workLogs = await _dbContext.WorkLogs
                .Where(wl => wl.TaskId == taskId)
                .OrderByDescending(wl => wl.StartTime)
                .Select(wl => new WorkLogDto
                {
                    Id = wl.Id,
                    UserId = wl.UserId,
                    TaskId = wl.TaskId,
                    StartTime = wl.StartTime,
                    EndTime = wl.EndTime,
                    DurationMinutes = wl.DurationMinutes,
                    CreatedAt = wl.CreatedAt
                })
                .ToListAsync();

            return workLogs;
        }

        public async Task<TaskTimeDto?> GetTaskTotalTimeAllUsersAsync(Guid taskId)
        {
            var result = await _dbContext.WorkLogs
                .Where(wl => wl.TaskId == taskId)
                .GroupBy(wl => new { wl.TaskId, wl.Task.Title })
                .Select(g => new TaskTimeDto
                {
                    TaskId = g.Key.TaskId,
                    TaskTitle = g.Key.Title,
                    TotalMinutes = g.Sum(wl => wl.DurationMinutes ?? 0),
                    LogCount = g.Count()
                })
                .FirstOrDefaultAsync();

            return result;
        }

        public async Task<ProjectTimeDto?> GetProjectTotalTimeAllUsersAsync(Guid projectId)
        {
            var result = await _dbContext.WorkLogs
                .Where(wl => wl.Task.ProjectId == projectId)
                .GroupBy(wl => new { wl.Task.ProjectId, wl.Task.Project.Name })
                .Select(g => new ProjectTimeDto
                {
                    ProjectId = g.Key.ProjectId,
                    ProjectName = g.Key.Name,
                    TotalMinutes = g.Sum(wl => wl.DurationMinutes ?? 0),
                    TaskCount = g.Select(wl => wl.TaskId).Distinct().Count(),
                    LogCount = g.Count()
                })
                .FirstOrDefaultAsync();

            return result;
        }
    }
}
