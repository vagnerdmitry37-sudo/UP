using UP.Api.Db;
using Mapster;
using UP.Api.Features.AuditLogFeature;

namespace UP.Api.Features.UserFeature
{
    public interface IUserService
    {
        //public Task AddRangeAsync(ICollection<UserDto> userDtos);
    }

    public class UserService(AppDbContext adc, IUserRepository ur, IAuditLogService als) : IUserService
    {
        //private readonly AppDbContext _adc = adc;
        //private readonly IUserRepository _ur = ur;
        //private readonly IAuditLogService _als = als;

        //public async Task AddRangeAsync(ICollection<UserDto> userDtos)
        //{
        //    var users = userDtos.Select(u => u.Adapt<User>());
        //    await _ur.AddRangeAsync(users);

        //    var auditLogs = users.Adapt<AuditLog>();

        //    await _adc.SaveChangesAsync();
        //}
    }
}
