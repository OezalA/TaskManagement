using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/projects")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Project project)
        {
            var createdProject = await _projectService.CreateAsync(project);
            return CreatedAtAction(nameof(GetById), new { id = createdProject.Id }, createdProject);
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
            var project = await _projectService.GeByIdAsync(id);
            if (project == null) return NotFound();

            return Ok(project);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, Project project)
        {
            if(id != project.Id) return BadRequest("Project ID mismatch");

            var updated = await _projectService.UpdateAsync(project);

            if (!updated) return NotFound();

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete (Guid id)
        {
            var deleted = await _projectService.DeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }


    }
}
