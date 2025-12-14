using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
        
        public async Task<Team> CreatAsync(Team team)
        {
            _dbContext.Teams.Add(team);
            await _dbContext.SaveChangesAsync();

            return team;
        }

        public async Task<Team?> GetByIdAsync(Guid id)
        {
            var team = await _dbContext.Teams
                 .Include(t => t.Members)
                     .ThenInclude(tu => tu.User)
                 .AsNoTracking()
                 .FirstOrDefaultAsync(t => t.Id == id);

            return team;
        }

        public async Task<List<Team>> GetAllAsync()
        {
            var teams = await _dbContext.Teams
                .AsNoTracking()
                .ToListAsync();

            return teams;

                
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var team = await _dbContext.Teams.FindAsync(id);
            if(team == null) return false;

            _dbContext.Teams.Remove(team);
            await _dbContext.SaveChangesAsync();
            return true;
        }




        public async Task<bool> AddUserToTeamAsync(Guid teamId, Guid userId)
        {
            var teamExist = await _dbContext.Teams.AnyAsync(t => t.Id ==  teamId);
            if(!teamExist) return false;

            var userExist = await _dbContext.Users.AnyAsync(u => u.Id == userId);
            if(!userExist) return false;

            var alreadyMember = await _dbContext.TeamUsers
                .AllAsync(tu => tu.UserId == userId && tu.TeamId == teamId);

            if(!alreadyMember) return false;

            _dbContext.TeamUsers.Add(new TeamUser
            {
                UserId = userId,
                TeamId = teamId,
            });
            
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<List<User>> GetTeamMembersAsync(Guid teamId)
        {
            return await _dbContext.TeamUsers
                .Where(tu => tu.UserId == teamId)
                .Select(tu => tu.User)
                .AsNoTracking()
                .ToListAsync();
        }

        public Task<bool> RemoveUserFromTeamAsync(Guid teamId, Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}
