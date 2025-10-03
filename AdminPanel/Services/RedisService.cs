using StackExchange.Redis;

namespace AdminPanel.Services;

public interface IRedisService
{
    IDatabaseAsync Database { get; }
    bool IsEnabled { get; }
}

public class RedisService : IRedisService
{
    private readonly IDatabaseAsync _database;
    private readonly bool _isEnabled;

    public RedisService(IConfiguration configuration, IConnectionMultiplexer connectionMultiplexer)
    {
        _isEnabled = configuration.GetValue<bool>("Redis:Enabled");
        _database = connectionMultiplexer.GetDatabase();
    }

    public IDatabaseAsync Database => _database;
    public bool IsEnabled => _isEnabled;
}