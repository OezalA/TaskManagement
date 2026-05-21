using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskManagement.Application.Interfaces
{
    /// <summary>
    /// Resolves user identifiers from various formats (name, email, partial match)
    /// </summary>
    public interface IUserResolutionService
    {
        /// <summary>
        /// Find user by exact name match
        /// </summary>
        Task<Guid?> ResolveByNameAsync(string name);

        /// <summary>
        /// Find user by email
        /// </summary>
        Task<Guid?> ResolveByEmailAsync(string email);

        /// <summary>
        /// Find user by partial name (case-insensitive)
        /// </summary>
        Task<Guid?> ResolveByPartialNameAsync(string partialName);

        /// <summary>
        /// Find multiple users matching a pattern
        /// </summary>
        Task<List<(Guid Id, string Name, string Email)>> ResolveMultipleAsync(string pattern);
    }
}
