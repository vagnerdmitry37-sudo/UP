using UP.Api.Db;

namespace UP.Api.Features.UserFeature
{
    public interface IUserRepository
    {
        //public Task AddRangeAsync(IEnumerable<User> users);
    }

    public class UserRepository(AppDbContext context) : IUserRepository
    {
        private readonly AppDbContext _context = context;

        public async Task AddRangeAsync(IEnumerable<User> users)
        {
            //await _context.Users.AddRangeAsync(users);
        }
    }
}
