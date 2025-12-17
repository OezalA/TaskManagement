using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Persistence;

namespace TaskManagement.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _dbContext;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            AppDbContext dbContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        public async Task<User> GetCurrentUserAsync()
        {
            var userPrincipal = _httpContextAccessor.HttpContext?.User;
            if (userPrincipal == null || !userPrincipal.Identity!.IsAuthenticated)
                throw new UnauthorizedException("User is not authenticated");

            // Entra ObjectId (oid)
            //var entraObjectId = userPrincipal.FindFirstValue("oid");
            var entraObjectId =
                userPrincipal.FindFirstValue("oid")
                ?? userPrincipal.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");

            if (string.IsNullOrWhiteSpace(entraObjectId))
                throw new UnauthorizedException("Missing Entra ObjectId claim");

            // Try find existing user
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.EntraObjectId == entraObjectId);

            if (user != null)
                return user;

            // First login → create user
            //var userName =
            //    userPrincipal.FindFirstValue("givenname")
            //    ?? userPrincipal.FindFirstValue(ClaimTypes.Email)
            //    ?? "unknown";

            var firstName =
                userPrincipal.FindFirstValue(ClaimTypes.GivenName)
                ?? string.Empty;

            var lastName =
                userPrincipal.FindFirstValue(ClaimTypes.Surname)
                ?? string.Empty;

            var displayName =
                userPrincipal.FindFirstValue("name")
                ?? $"{firstName} {lastName}".Trim();

            var email =
                userPrincipal.FindFirstValue(ClaimTypes.Upn)
                ?? userPrincipal.FindFirstValue("preferred_username");

            var newUser = new User
            {
                FirstName = firstName,
                LastName = lastName,
                EntraObjectId = entraObjectId,
                DisplayName = displayName,
                Email = email,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();

            return newUser;
        }
    }
}
