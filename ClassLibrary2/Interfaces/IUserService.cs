using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetAllAsync();
    }
}
