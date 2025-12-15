using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Exceptions;
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
            _dbContext = dbContext;
        }

        public async Task<Project> CreateAsync(Project project)
        {
            _dbContext.Projects.Add(project);
            await _dbContext.SaveChangesAsync();
            return project;
        }

        public async Task<Project> GetByIdAsync(Guid id)
        {
            var project = await _dbContext.Projects
                .Include(p => p.Tasks)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                throw new NotFoundException("Project not found", "ProjectNotFound");

            return project;
        }

        public async Task<List<Project>> GetAllAsync()
        {
            return await _dbContext.Projects
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task UpdateAsync(Project project)
        {
            var exists = await _dbContext.Projects
                .AnyAsync(p => p.Id == project.Id);

            if (!exists)
                throw new NotFoundException("Project not found", "ProjectNotFound");

            _dbContext.Projects.Update(project);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var project = await _dbContext.Projects.FindAsync(id);
            if (project == null)
                throw new NotFoundException("Project not found", "ProjectNotFound");

            _dbContext.Projects.Remove(project);
            await _dbContext.SaveChangesAsync();
        }
    }
}
