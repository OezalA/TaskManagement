using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{userId:guid}/tasks")]
        public async Task<IActionResult> GetUserTasks(Guid userId)
        {
            var tasks = await _userService.GetUserTasksAsync(userId);

            var response = tasks.Select(t => new UserTaskResponse
            {
                TaskId = t.Id,
                Title = t.Title,
                Status = t.Status.ToString(),
                ProjectId = t.Project.Id,
                ProjectName = t.Project.Name
            });

            return Ok(response);
        }

        [HttpGet("{userId:guid}/projects")]
        public async Task<IActionResult> GetUserProjects(Guid userId)
        {
            var projects = await _userService.GetUserProjectsAsync(userId);

            var response = projects.Select(p => new UserProjectResponse
            {
                ProjectId = p.Id,
                ProjectName = p.Name
            });

            return Ok(response);
        }

    }
}
