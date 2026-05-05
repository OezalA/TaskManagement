using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Services;

namespace TaskManagement.Api.Controllers
{
    [Authorize(Policy = "ApiAccess")]
    [ApiController]
    [Route("api/tasks")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly IWorkLogService _workLogService;
        private readonly IWorkLogQueryService _workLogQueryService;
        private readonly ICurrentUserService _currentUserService;

        public TaskController(ITaskService taskService, IWorkLogService workLogService, IWorkLogQueryService workLogQueryService, ICurrentUserService currentUserService)
        {
            _taskService = taskService;
            _workLogService = workLogService;
            _workLogQueryService = workLogQueryService;
            _currentUserService = currentUserService;
        }

        [Authorize(Roles = "Admin,User")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
        {
            var task = new TaskItem
            {
                Title = request.Title,
                Description = request.Description,
                ProjectId = request.ProjectId,
                DueDate = request.DueDate.HasValue
        ? DateTime.SpecifyKind(request.DueDate.Value, DateTimeKind.Utc)
        : null
            };

            var createdTask = await _taskService.CreateAsync(task);

            return CreatedAtAction(
                nameof(GetById),
                new { id = createdTask.Id },
                createdTask
            );
        }
        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var task = await _taskService.GetByIdAsync(id);
            return Ok(task);
        }

        [HttpGet("by-project/{projectId:guid}")]
        public async Task<IActionResult> GetByProject(Guid projectId)
        {
            var tasks = await _taskService.GetByProjectAsync(projectId);
            return Ok(tasks);
        }
        [Authorize(Roles = "Admin,User")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, TaskItem task)
        {
            if (id != task.Id)
                return BadRequest("Task ID mismatch");

            await _taskService.UpdateAsync(task);
            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [Authorize(Policy = "ApiAccess")]
        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> Patch(Guid id, [FromBody] UpdateTaskRequest request)
        {
            await _taskService.UpdatePartialAsync(id, request);
            return NoContent();
        }

        [HttpPost("{id:guid}/complete")]
        public async Task<IActionResult> Complete(Guid id)
        {
            await _taskService.CompleteAsync(id);
            return NoContent();
        }
        [HttpPost("{id:guid}/assign-to-me")]
        public async Task<IActionResult> AssignToMe(Guid id)
        {
            await _taskService.AssignToMeAsync(id);
            return NoContent();
        }

        [Authorize(Roles = "Admin,User")]
        [HttpPut("{id:guid}/assign/{userId:guid}")]
        public async Task<IActionResult> AssignUser(Guid id, Guid userId)
        {
            await _taskService.AssignUserAsync(id, userId);
            return NoContent();
        }
        [Authorize(Roles = "Admin,User")]
        [HttpPut("{id:guid}/done")]
        public async Task<IActionResult> MarkAsDone(Guid id)
        {
            await _taskService.MarkAsDoneAsync(id);
            return NoContent();
        }
        [Authorize(Roles = "Admin,User")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _taskService.DeleteAsync(id);
            return NoContent();
        }

        // WorkLog endpoints
        [Authorize(Roles = "Admin,User")]
        [HttpPost("{id:guid}/start-work")]
        public async Task<IActionResult> StartWork(Guid id, [FromBody] StartWorkRequest request)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            var workLog = await _workLogService.StartWorkAsync(id, currentUser.Id);
            return CreatedAtAction(nameof(StartWork), new { id }, workLog);
        }

        [Authorize(Roles = "Admin,User")]
        [HttpPost("{id:guid}/stop-work")]
        public async Task<IActionResult> StopWork(Guid id, [FromBody] StopWorkRequest request)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            var workLog = await _workLogService.StopWorkAsync(id, currentUser.Id);
            return Ok(workLog);
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet("active-work")]
        public async Task<IActionResult> GetActiveWork()
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            var workLog = await _workLogService.GetActiveWorkLogAsync(currentUser.Id);
            
            if (workLog == null)
                return NotFound("No active work log found");

            return Ok(workLog);
        }

        // Query endpoints for time tracking
        [Authorize(Roles = "Admin,User")]
        [HttpGet("this-week")]
        public async Task<IActionResult> GetThisWeekTasks()
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            var weeklyTasks = await _workLogQueryService.GetThisWeekTasksAsync(currentUser.Id);
            return Ok(weeklyTasks);
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id:guid}/total-time")]
        public async Task<IActionResult> GetTaskTotalTime(Guid id)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            var taskTime = await _workLogQueryService.GetTaskTotalTimeAsync(id, currentUser.Id);
            
            if (taskTime == null)
                return NotFound("No work logs found for this task");

            return Ok(taskTime);
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id:guid}/work-logs")]
        public async Task<IActionResult> GetTaskWorkLogs(Guid id)
        {
            var workLogs = await _workLogQueryService.GetTaskWorkLogsAsync(id);
            return Ok(workLogs);
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id:guid}/total-time-all-users")]
        public async Task<IActionResult> GetTaskTotalTimeAllUsers(Guid id)
        {
            var taskTime = await _workLogQueryService.GetTaskTotalTimeAllUsersAsync(id);
            
            if (taskTime == null)
                return NotFound("No work logs found for this task");

            return Ok(taskTime);
        }
    }
}
