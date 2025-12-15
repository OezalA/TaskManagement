using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Persistence;

namespace TaskManagement.Infrastructure.Services
{
    public class TeamService : ITeamService
    {
        private readonly AppDbContext _dbContext;

        public TeamService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Team> CreateAsync(Team team)
        {
            _dbContext.Teams.Add(team);
            await _dbContext.SaveChangesAsync();
            return team;
        }

        public async Task<Team> GetByIdAsync(Guid id)
        {
            var team = await _dbContext.Teams
                .Include(t => t.Members)
                    .ThenInclude(tu => tu.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
                throw new NotFoundException("Team not found", "TeamNotFound");

            return team;
        }

        public async Task<List<Team>> GetAllAsync()
        {
            return await _dbContext.Teams
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var team = await _dbContext.Teams.FindAsync(id);
            if (team == null)
                throw new NotFoundException("Team not found", "TeamNotFound");

            _dbContext.Teams.Remove(team);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddUserToTeamAsync(Guid teamId, Guid userId)
        {
            if (!await _dbContext.Teams.AnyAsync(t => t.Id == teamId))
                throw new NotFoundException("Team not found", "TeamNotFound");

            if (!await _dbContext.Users.AnyAsync(u => u.Id == userId))
                throw new NotFoundException("User not found", "UserNotFound");

            if (await _dbContext.TeamUsers.AnyAsync(tu => tu.TeamId == teamId && tu.UserId == userId))
                throw new ConflictException(
                    "User is already a member of this team",
                    "UserAlreadyMember"
                );

            _dbContext.TeamUsers.Add(new TeamUser
            {
                TeamId = teamId,
                UserId = userId
            });

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<TeamUser>> GetTeamMembersAsync(Guid teamId)
        {
            return await _dbContext.TeamUsers
                .Include(tu => tu.User)
                .Where(tu => tu.TeamId == teamId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task RemoveUserFromTeamAsync(Guid teamId, Guid userId)
        {
            var teamUser = await _dbContext.TeamUsers
                .FirstOrDefaultAsync(tu => tu.TeamId == teamId && tu.UserId == userId);

            if (teamUser == null)
                throw new NotFoundException(
                    "User is not a member of this team",
                    "TeamMemberNotFound"
                );

            _dbContext.TeamUsers.Remove(teamUser);
            await _dbContext.SaveChangesAsync();
        }
    }
}
