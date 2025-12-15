using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Services;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

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

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, TaskItem task)
        {
            if (id != task.Id)
                return BadRequest("Task ID mismatch");

            await _taskService.UpdateAsync(task);
            return NoContent();
        }

        [HttpPut("{id:guid}/assign/{userId:guid}")]
        public async Task<IActionResult> AssignUser(Guid id, Guid userId)
        {
            await _taskService.AssignUserAsync(id, userId);
            return NoContent();
        }

        [HttpPut("{id:guid}/done")]
        public async Task<IActionResult> MarkAsDone(Guid id)
        {
            await _taskService.MarkAsDoneAsync(id);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _taskService.DeleteAsync(id);
            return NoContent();
        }
    }
}
