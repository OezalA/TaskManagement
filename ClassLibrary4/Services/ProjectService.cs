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
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _dbContext;

        public ProjectService(AppDbContext dbContext)
        {
            _dbContext =  dbContext;
        }

        public async Task<Project> CreateAsync(Project project)
        {
            _dbContext.Projects.Add(project);
            await _dbContext.SaveChangesAsync();
            return project;
        }

        public async Task<Project> GeByIdAsync(Guid id)
        {
            var project = await _dbContext.Projects
                .Include(p => p.Tasks)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            return project;

        }

        public async Task<List<Project>> GetAllAsync()
        {
            return await _dbContext.Projects
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> UpdateAsync (Project project)
        {
            var exist = await _dbContext.Projects.AnyAsync(p => p.Id == project.Id);
            if (!exist)
                return false;

            _dbContext.Projects.Update(project);
            await _dbContext.SaveChangesAsync();

            return true;
            
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var project = await _dbContext.Projects.FindAsync(id);
            if (project == null)
                return false;

            _dbContext.Projects.Remove(project);
            await _dbContext.SaveChangesAsync();

            return true;
        }


    }
}
