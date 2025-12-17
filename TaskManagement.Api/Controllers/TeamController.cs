using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Api.Controllers
{
    [Authorize(Policy = "ApiAccess")]
    [ApiController]
    [Route("api/teams")]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;

        public TeamController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTeamRequest request)
        {
            var team = new Team
            {
                Name = request.Name
            };

            var createdTeam = await _teamService.CreateAsync(team);

            return CreatedAtAction(
                nameof(GetById),
                new { id = createdTeam.Id },
                createdTeam
            );
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var teams = await _teamService.GetAllAsync();

            var response = teams.Select(t => new TeamResponse
            {
                Id = t.Id,
                Name = t.Name
            });

            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var team = await _teamService.GetByIdAsync(id);

            var response = new TeamResponse
            {
                Id = team.Id,
                Name = team.Name
            };

            return Ok(response);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _teamService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("{teamId:guid}/users/{userId:guid}")]
        public async Task<IActionResult> AddUser(Guid teamId, Guid userId)
        {
            await _teamService.AddUserToTeamAsync(teamId, userId);
            return NoContent();
        }

        [HttpDelete("{teamId:guid}/users/{userId:guid}")]
        public async Task<IActionResult> RemoveUser(Guid teamId, Guid userId)
        {
            await _teamService.RemoveUserFromTeamAsync(teamId, userId);
            return NoContent();
        }

        [HttpGet("{teamId:guid}/members")]
        public async Task<IActionResult> GetMembers(Guid teamId)
        {
            var members = await _teamService.GetTeamMembersAsync(teamId);

            var response = members.Select(m => new TeamMemberResponse
            {
                UserId = m.UserId,
                UserName = m.User?.DisplayName
            });

            return Ok(response);
        }
    }
}
