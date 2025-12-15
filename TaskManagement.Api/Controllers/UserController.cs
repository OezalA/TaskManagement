using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize]
        [HttpGet("secure-test")]
        public IActionResult SecureTest()
        {
            return Ok("Authenticated");
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

        

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetMyUser()
        {
            var user = HttpContext.User;

            var response = new
            {
                // Core identity
                ObjectId = user.FindFirstValue("oid"),
                TenantId = user.FindFirstValue("tid"),
                UserPrincipalName = user.FindFirstValue("preferred_username"),
                Name = user.FindFirstValue("name"),
                Email =
                    user.FindFirstValue(ClaimTypes.Email)
                    ?? user.FindFirstValue("preferred_username"),

                // Authorization info
                Scopes = user.FindFirstValue("scp")?.Split(' '),

                // Raw claims (çok faydal? debug için)
                Claims = user.Claims.Select(c => new
                {
                    c.Type,
                    c.Value
                })
            };

            return Ok(response);
        }

    }
}
