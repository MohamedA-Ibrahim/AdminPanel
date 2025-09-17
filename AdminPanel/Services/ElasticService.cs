using AdminPanel.Models;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Options;

namespace AdminPanel.Services;

public class ElasticService
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticSettings _elasticSettings;

    public ElasticService(IOptions<ElasticSettings> optionsMonitor)
    {
        _elasticSettings = optionsMonitor.Value;

        var settings = new ElasticsearchClientSettings(new Uri(_elasticSettings.Url))
                .DefaultMappingFor<User>(i => i
                    .IndexName(_elasticSettings.IndexName)
                    .IdProperty(p => p.Id)
                )
                .EnableDebugMode()
                .PrettyJson()
                .RequestTimeout(TimeSpan.FromMinutes(2));

        _client = new ElasticsearchClient(settings);
    }

    public async Task CreateIndexIfNotExistsAsync(string indexName)
    {
        var indexExists = (await _client.Indices.ExistsAsync(indexName)).Exists;
        if (!indexExists)
        {
            await _client.Indices.CreateAsync(indexName);
        }
    }

    public async Task<bool> AddOrUpdate(User user)
    {
        var response = await _client.IndexAsync(user, idx => idx.Index(_elasticSettings.IndexName).OpType(OpType.Index));

        return response.IsValidResponse;
    }

    public async Task<bool> AddOrUpdateBulk(List<User> users, string indexName)
    {
        var response = await _client
            .BulkAsync(idx => idx.Index(indexName)
                .UpdateMany(users,
                    (ud, u) => ud.Doc(u).DocAsUpsert(true)));

        return response.IsValidResponse;
    }

    public async Task<User?> Get(string key)
    {
        var response = await _client.GetAsync<User>(key, g => g.Index(_elasticSettings.IndexName));

        return response.Source;
    }

    public async Task<List<User>> GetAll()
    {
        var response = await _client.SearchAsync<User>(s => s.Indices(_elasticSettings.IndexName));

        return response.IsValidResponse ? response.Documents.ToList() : [];
    }

    public async Task<bool> Remove(string key)
    {
        var response = await _client.DeleteAsync<User>(key,
            d => d.Index(_elasticSettings.IndexName));

        return response.IsValidResponse;
    }

    public async Task<long?> RemoveAll()
    {
        var response = await _client.DeleteByQueryAsync<User>(d => d.Indices(_elasticSettings.IndexName));

        return response.IsValidResponse ? response.Deleted : default;
    }

}
