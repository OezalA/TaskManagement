using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces
{
    public interface ITeamService
    {
        Task<Team> CreateAsync(Team team);
        Task<Team> GetByIdAsync(Guid id);
        Task<List<Team>> GetAllAsync();


        Task DeleteAsync(Guid id);
        Task AddUserToTeamAsync(Guid teamId, Guid userId);
        Task RemoveUserFromTeamAsync(Guid teamId, Guid userId);
        Task<List<TeamUser>> GetTeamMembersAsync(Guid teamId);

    }
}
