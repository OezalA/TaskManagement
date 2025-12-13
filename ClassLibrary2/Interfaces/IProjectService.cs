using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces
{
    public  interface IProjectService
    {
        Task<Project> CreateAsync(Project project);
        Task<Project?> GeByIdAsync(Guid id);
        Task<List<Project>> GetAllAsync();
        Task<bool> UpdateAsync(Project project);
        Task<bool> DeleteAsync(Guid id);
    }
}
