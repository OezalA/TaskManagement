using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Interfaces;
using TaskManagement.Infrastructure.Persistence;

namespace TaskManagement.Infrastructure.Services
{
    /// <summary>
    /// Resolves user identifiers from various formats
    /// </summary>
    public class UserResolutionService : IUserResolutionService
    {
        private readonly AppDbContext _dbContext;

        public UserResolutionService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Find user by exact name match (case-sensitive)
        /// </summary>
        public async Task<Guid?> ResolveByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.DisplayName == name || u.FirstName == name || u.LastName == name);

            return user?.Id;
        }

        /// <summary>
        /// Find user by email
        /// </summary>
        public async Task<Guid?> ResolveByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            return user?.Id;
        }

        /// <summary>
        /// Find user by partial name (case-insensitive, first match)
        /// </summary>
        public async Task<Guid?> ResolveByPartialNameAsync(string partialName)
        {
            if (string.IsNullOrWhiteSpace(partialName))
                return null;

            var nameLower = partialName.ToLower();

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => 
                    u.DisplayName.ToLower().Contains(nameLower) ||
                    u.FirstName.ToLower().Contains(nameLower) ||
                    u.LastName.ToLower().Contains(nameLower));

            return user?.Id;
        }

        /// <summary>
        /// Find multiple users matching a pattern (case-insensitive)
        /// </summary>
        public async Task<List<(Guid Id, string Name, string Email)>> ResolveMultipleAsync(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                return new();

            var patternLower = pattern.ToLower();

            var users = await _dbContext.Users
                .Where(u => u.DisplayName.ToLower().Contains(patternLower) || 
                           u.FirstName.ToLower().Contains(patternLower) ||
                           u.LastName.ToLower().Contains(patternLower) ||
                           (u.Email != null && u.Email.ToLower().Contains(patternLower)))
                .Select(u => new { u.Id, u.DisplayName, u.Email })
                .ToListAsync();

            return users
                .Select(u => (u.Id, u.DisplayName, u.Email ?? ""))
                .ToList();
        }
    }
}
