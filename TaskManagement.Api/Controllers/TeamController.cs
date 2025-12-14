using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Services;

namespace TaskManagement.Api.Controllers
{
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
        public async Task<IActionResult> Create(Team team)
        {
            var createdTeam = await _teamService.CreatAsync(team);
            return CreatedAtAction(nameof(GetById), new { team.Id }, createdTeam);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var teams = await _teamService.GetAllAsync();
            return Ok(teams);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var team = await _teamService.GetByIdAsync(id);
            if(team == null) return NotFound();
            return Ok(team);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = _teamService.DeleteAsync(id);
            if(deleted == null) return NotFound();

            return NoContent();
        }

        [HttpPost("{teamId:guid}/users/{userId:guid}")]
        public async Task<IActionResult> AddUser(Guid teamId, Guid userId)
        {
            var result = await _teamService.AddUserToTeamAsync(teamId, userId);
            if(!result) return NotFound();

            return NoContent();
        }

        [HttpDelete("{teamId:guid}/users/{userId:guid}")]
        public async Task<IActionResult> RemoveUser(Guid teamId, Guid userId)
        {
            var result = await _teamService.RemoveUserFromTeamAsync(teamId, userId);
            if(!result) return NotFound();
            return NoContent();
        }

        [HttpGet("{teamId:guid}")]
        public async Task<IActionResult> GetMembers(Guid teamId)
        {
           var members = await _teamService.GetTeamMembersAsync(teamId);
            
            return Ok(members);
        }

                
    }
}
