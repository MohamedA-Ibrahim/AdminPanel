using AdminPanel.Models;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
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
                .EnableDebugMode();

        _client = new ElasticsearchClient(settings);
    }

    public async Task CreateIndexIfNotExistsAsync()
    {
        var indexName = _elasticSettings.IndexName;

        var indexExists = (await _client.Indices.ExistsAsync(indexName)).Exists;
        if (!indexExists)
        {
            await _client.Indices.CreateAsync<User>(indexName, i => i
                .Mappings(mappings => mappings
                    .Properties(properties => properties
                        .Keyword(x => x.Id)
                        .Text(x => x.FirstName)
                        .Text(x => x.LastName)
                        .Keyword(x => x.Email)
                )));
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

    public async Task<bool> BulkAddOrUpdateUsers(List<User> users)
    {
        var response = await _client.BulkAsync(idx => idx
            .Index(_elasticSettings.IndexName)
            .UpdateMany(users, (ud, u) => ud.Doc(u).DocAsUpsert(true)));

        return response.IsValidResponse;
    }

    public async Task<long?> GetCount()
    {
        var response = await _client.CountAsync<User>(s => s.Indices(_elasticSettings.IndexName));

        return response.IsValidResponse ? response.Count : null;
    }

    public async Task<List<User>> Search(string? query)
    {
        SearchResponse<User> response;

        if (string.IsNullOrEmpty(query))
        {
            response = await _client.SearchAsync<User>(s => s.Indices(_elasticSettings.IndexName));
        }
        else
        {
            response = await _client.SearchAsync<User>(s => s
            .Indices(_elasticSettings.IndexName)
            .Query(q => q
                .Term(t => t
                    .Field(x => x.Email)
                    .Value(query))
                .Match(m => m
                    .Field(x => x.FirstName)
                    .Field(x => x.LastName)
                    .Query(query)
                    )));
        }

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
