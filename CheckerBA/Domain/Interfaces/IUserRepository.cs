using CheckerBA.Domain.Entities;

namespace CheckerBA.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<AppUser?> GetByUsernameAsync(string username);
        Task AddAsync(AppUser user);
    }
}
