using AdminPanel.Data;
using AdminPanel.Models;
using AdminPanel.Validators;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;

namespace AdminPanel.Services;

public interface IUserService
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<User>> GetUsersAsync(GetUsersFilter filter, CancellationToken cancellationToken);
    Task<Result> AddAsync(User user);
    Task<bool> DeleteAsync(Guid id);
}

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;
    private readonly IConnectionMultiplexer _redisCache;
    private readonly IDatabaseAsync _redisDatabase;

    public UserService(AppDbContext dbContext, IConnectionMultiplexer connectionMultiplexer)
    {
        _dbContext = dbContext;
        _redisCache = connectionMultiplexer;
        _redisDatabase = _redisCache.GetDatabase();
    }

    public async Task<List<User>> GetUsersAsync(GetUsersFilter filter, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"users:all_{filter.Search}_{filter.OrderBy}_{filter.OrderASC}";
        var cachedUsers = await _redisDatabase.StringGetAsync(cacheKey);
        if (cachedUsers.HasValue)
        {
            return JsonSerializer.Deserialize<List<User>>(cachedUsers);
        }

        var query = _dbContext.Users.AsQueryable();

        if (filter.Search is not null)
        {
            query = query.Where(u => u.FirstName.Contains(filter.Search) || u.LastName.Contains(filter.Search) || u.Email.Contains(filter.Search));
        }

        if (filter.OrderBy is not null)
        {
            query = filter.OrderBy switch
            {
                GetUserOrderBy.FirstName => filter.OrderASC ? query.OrderBy(u => u.FirstName) : query.OrderByDescending(u => u.FirstName),
                GetUserOrderBy.LastName => filter.OrderASC ? query.OrderBy(u => u.LastName) : query.OrderByDescending(u => u.LastName),
                GetUserOrderBy.Email => filter.OrderASC ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email),
                GetUserOrderBy.Phone => filter.OrderASC ? query.OrderBy(u => u.Phone) : query.OrderByDescending(u => u.Phone),
                _ => throw new NotImplementedException(),
            };
        }


        var users =  await query
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        await _redisDatabase.StringSetAsync(cacheKey, JsonSerializer.Serialize(users), TimeSpan.FromMinutes(5));

        return users;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"users:{id}";
        var cachedUser = await _redisDatabase.StringGetAsync(cacheKey);
        if (cachedUser.HasValue)
        {
            return JsonSerializer.Deserialize<User>(cachedUser);
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        await _redisDatabase.StringSetAsync(cacheKey, JsonSerializer.Serialize(user), TimeSpan.FromMinutes(5));

        return user;

    }

    public async Task<Result> AddAsync(User user)
    {
        var validator = new UserValidator();
        var result = await validator.ValidateAsync(user);
        if (!result.IsValid)
            return new Result(false, result.ToString());

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        return new Result(true);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await GetByIdAsync(id);
        if (user is null)
            return false;

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}
