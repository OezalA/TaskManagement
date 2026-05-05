using TaskManagement.Application.DTOs;
using TaskManagement.Application.DTOs.Responses;

namespace TaskManagement.Application.Interfaces
{
    public interface IWorkLogService
    {
        /// <summary>
        /// Starts work on a task for the current user.
        /// Rules:
        /// - User cannot have more than one active work log at a time
        /// - Cannot start work if task doesn't exist or is not assigned to user
        /// </summary>
        Task<WorkLogDto> StartWorkAsync(Guid taskId, Guid userId);

        /// <summary>
        /// Stops work on the current active work log for a task.
        /// Rules:
        /// - Task must have an active (unfinished) work log
        /// </summary>
        Task<WorkLogDto> StopWorkAsync(Guid taskId, Guid userId);

        /// <summary>
        /// Gets the current active work log for a user, if any.
        /// </summary>
        Task<WorkLogDto?> GetActiveWorkLogAsync(Guid userId);
    }
}
