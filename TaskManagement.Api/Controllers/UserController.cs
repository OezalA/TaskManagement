using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.Api.Controllers
{
    [Authorize(Policy = "ApiAccess")]
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IWorkLogQueryService _workLogQueryService;

        public UserController(IUserService userService, ICurrentUserService currentUserService, IWorkLogQueryService workLogQueryService)
        {
            _userService = userService;
            _currentUserService = currentUserService;
            _workLogQueryService = workLogQueryService;
        }

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

        

        [HttpGet("me")]
        public async Task<IActionResult> GetMyUser()
        {
            
            var user = await _currentUserService.GetCurrentUserAsync();
            return Ok(new
            {
                user.Id,
                user.DisplayName,
                user.Email,
                user.CreatedAt,
                user.EntraObjectId


            });

        }
        // Test
        [HttpGet("debug/claims")]
        public IActionResult GetClaims()
        {
            return Ok(User.Claims.Select(c => new { c.Type, c.Value }));
        }

        // Work Log Reporting
        [Authorize(Roles = "Admin,User")]
        [HttpGet("{userId:guid}/worklogs")]
        public async Task<IActionResult> GetUserWorkLogs(Guid userId, [FromQuery] string? week = null)
        {
            var workLogs = await _workLogQueryService.GetUserWorkLogsAsync(userId, week);
            return Ok(new
            {
                userId,
                week = week ?? "all",
                count = workLogs.Count,
                totalMinutes = workLogs.Sum(wl => wl.DurationMinutes ?? 0),
                workLogs
            });
        }

    }
}
