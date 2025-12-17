using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces
{
    public  interface IProjectService
    {
        Task<Project> CreateAsync(Project project);
        Task<Project?> GetByIdAsync(Guid id);
        Task<List<Project>> GetAllAsync();
        Task UpdateAsync(Project project);
        Task DeleteAsync(Guid id);
    }
}
