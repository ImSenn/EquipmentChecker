using CheckerBA.Domain.Entities;
using CheckerBA.Domain.Interfaces;
using CheckerBA.Infrastructure.MongoDB;
using MongoDB.Driver;

namespace CheckerBA.Infrastructure.Repositories
{
    public class MongoUserRepository : IUserRepository
    {
        private readonly IMongoCollection<AppUser> _users;

        public MongoUserRepository(MongoDbContext context)
        {
            _users = context.Users;
        }

        public async Task<AppUser?> GetByUsernameAsync(string username)
            => await _users.Find(u => u.Username == username).FirstOrDefaultAsync();

        public async Task AddAsync(AppUser user)
            => await _users.InsertOneAsync(user);
    }
}
