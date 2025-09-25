using AdminPanel.Models;

namespace AdminPanel.Services;

public interface IElasticService
{
    Task CreateIndexIfNotExistsAsync();
    Task<bool> AddOrUpdate(User user);
    Task<bool> AddOrUpdateBulk(List<User> users, string indexName);
    Task<User?> Get(string key);
    Task<List<User>> GetAll();
    Task<bool> BulkAddOrUpdateUsers(List<User> users);
    Task<long?> GetCount();
    Task<List<User>?> Search(string? query, CancellationToken cancellation);
    Task<bool> Remove(string key);
    Task<long?> RemoveAll();
}