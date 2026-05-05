using TaskManagement.Application.DTOs.Responses;

namespace TaskManagement.Application.Interfaces
{
    public interface IWorkLogQueryService
    {
        /// <summary>
        /// Gets all tasks the user worked on this week with total time spent.
        /// </summary>
        Task<List<WeeklyTaskDto>> GetThisWeekTasksAsync(Guid userId);

        /// <summary>
        /// Gets total time spent on a specific task by the user.
        /// </summary>
        Task<TaskTimeDto?> GetTaskTotalTimeAsync(Guid taskId, Guid userId);

        /// <summary>
        /// Gets total time spent on all tasks in a project by the user.
        /// </summary>
        Task<ProjectTimeDto?> GetProjectTotalTimeAsync(Guid projectId, Guid userId);

        /// <summary>
        /// Gets all work logs for a task.
        /// </summary>
        Task<List<WorkLogDto>> GetTaskWorkLogsAsync(Guid taskId);

        /// <summary>
        /// Gets total time spent on a task (all users combined).
        /// </summary>
        Task<TaskTimeDto?> GetTaskTotalTimeAllUsersAsync(Guid taskId);

        /// <summary>
        /// Gets total time spent on a project (all users combined).
        /// </summary>
        Task<ProjectTimeDto?> GetProjectTotalTimeAllUsersAsync(Guid projectId);
    }
}
