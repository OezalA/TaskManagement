using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Services
{
    public class UserService : IUserService
    {
        public Task<List<User>> GetAllAsync()
        {
            var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), UserName = "test", Email = "test@example.com" }
        };

            return Task.FromResult(users);
        }
    }
}
