using AdminPanel.Data;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace AdminPanel.gRPC;

public class GrpcUserService : UserService.UserServiceBase
{
    private readonly AppDbContext _dbContext;
    public GrpcUserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<UserReply> GetUserById(UserRequest request,
        ServerCallContext context)
    {
        var userId = Guid.Parse(request.Id);
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"User with id {userId} not found"));
        }

        return new UserReply
        {
            Id = user.Id.ToString(),
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
        };
    }
}
