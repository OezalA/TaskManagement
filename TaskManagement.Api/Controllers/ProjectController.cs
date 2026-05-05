using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Api.Controllers
{
    [Authorize(Policy = "ApiAccess")]
    [ApiController]
    [Route("api/projects")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IWorkLogQueryService _workLogQueryService;
        private readonly ICurrentUserService _currentUserService;

        public ProjectController(IProjectService projectService, IWorkLogQueryService workLogQueryService, ICurrentUserService currentUserService)
        {
            _projectService = projectService;
            _workLogQueryService = workLogQueryService;
            _currentUserService = currentUserService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProjectRequest request)
        {
            var project = new Project
            {
                Name = request.Name,
                Description = request.Description
            };

            var createdProject = await _projectService.CreateAsync(project);

            return CreatedAtAction(
                nameof(GetById),
                new { id = createdProject.Id },
                createdProject
            );
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var projects = await _projectService.GetAllAsync();
            return Ok(projects);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var project = await _projectService.GetByIdAsync(id);
            return Ok(project);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, Project project)
        {
            if (id != project.Id)
                return BadRequest("Project ID mismatch");

            await _projectService.UpdateAsync(project);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _projectService.DeleteAsync(id);
            return NoContent();
        }

        // Time tracking endpoints
        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id:guid}/total-time")]
        public async Task<IActionResult> GetProjectTotalTime(Guid id)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            var projectTime = await _workLogQueryService.GetProjectTotalTimeAsync(id, currentUser.Id);
            
            if (projectTime == null)
                return NotFound("No work logs found for this project");

            return Ok(projectTime);
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id:guid}/total-time-all-users")]
        public async Task<IActionResult> GetProjectTotalTimeAllUsers(Guid id)
        {
            var projectTime = await _workLogQueryService.GetProjectTotalTimeAllUsersAsync(id);
            
            if (projectTime == null)
                return NotFound("No work logs found for this project");

            return Ok(projectTime);
        }
    }
}
