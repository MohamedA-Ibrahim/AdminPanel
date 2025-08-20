using AdminPanel.Data;
using AdminPanel.Models;
using AdminPanel.Validators;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;

namespace AdminPanel.Services;

public interface IUserService
{
    Task<CachedResult<User?>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CachedResult<List<User>>> GetUsersAsync(GetUsersFilter filter, CancellationToken cancellationToken);
    Task<Result> AddAsync(User user);
    Task<bool> DeleteAsync(Guid id);
}

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;
    private readonly IConnectionMultiplexer _redisCache;
    private readonly IDatabaseAsync _redisDatabase;
    private readonly IConfiguration _configuration;

    public UserService(AppDbContext dbContext, IConnectionMultiplexer connectionMultiplexer, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _redisCache = connectionMultiplexer;
        _redisDatabase = _redisCache.GetDatabase();
        _configuration = configuration;
    }

    public async Task<CachedResult<List<User>>> GetUsersAsync(GetUsersFilter filter, CancellationToken cancellationToken = default)
    {
        var redisEnabled = _configuration.GetValue<bool>("Redis:Enabled");
        var cacheKey = $"users:all_{filter.Search}_{filter.OrderBy}_{filter.OrderASC}";

        if (redisEnabled)
        {
            var cachedUsers = await _redisDatabase.StringGetAsync(cacheKey);
            if (cachedUsers.HasValue)
            {
                var deseralizedUsers = JsonSerializer.Deserialize<List<User>>(cachedUsers);
                return new CachedResult<List<User>>(deseralizedUsers, true);
            }
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

        var users = await query
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (redisEnabled)
        {
            await _redisDatabase.StringSetAsync(cacheKey, JsonSerializer.Serialize(users), TimeSpan.FromMinutes(5));

            await _redisDatabase.SetAddAsync("users:all_keys", cacheKey);

        }

        return new CachedResult<List<User>>(users, false);
    }

    public async Task<CachedResult<User?>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"users:{id}";
        var redisEnabled = _configuration.GetValue<bool>("Redis:Enabled");

        if (redisEnabled)
        {
            var cachedUser = await _redisDatabase.StringGetAsync(cacheKey);
            if (cachedUser.HasValue)
            {
                var deseralizedUser = JsonSerializer.Deserialize<User>(cachedUser);
                return new CachedResult<User?>(deseralizedUser, true);
            }
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (redisEnabled)
            await _redisDatabase.StringSetAsync(cacheKey, JsonSerializer.Serialize(user), TimeSpan.FromMinutes(5));

        return new CachedResult<User?>(user, false);

    }

    public async Task<Result> AddAsync(User user)
    {
        var validator = new UserValidator();
        var result = await validator.ValidateAsync(user);
        if (!result.IsValid)
            return new Result(false, result.ToString());

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        await InvalidateUserCacheAsync();
        
        return new Result(true);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var userResult = await GetByIdAsync(id);
        var user = userResult.Data;

        if (user is null)
            return false;

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();

        await InvalidateUserCacheAsync(id);
        return true;
    }

    private async Task InvalidateUserCacheAsync(Guid? userId = null)
    {
        var redisEnabled = _configuration.GetValue<bool>("Redis:Enabled");
        if (!redisEnabled) return;

        if (userId is not null)
        {
            var userKey = $"users:{userId}";
            await _redisDatabase.KeyDeleteAsync(userKey);
        }

        var userListCacheKeyValues = await _redisDatabase.SetMembersAsync("users:all_keys");
        var userListCacheKeys = userListCacheKeyValues.Select(k => new RedisKey(k)).ToArray();

        if (userListCacheKeys.Length > 0)
            await _redisDatabase.KeyDeleteAsync(userListCacheKeys);

        await _redisDatabase.KeyDeleteAsync("users:all_keys");

    }
}
