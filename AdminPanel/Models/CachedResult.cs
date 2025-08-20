namespace AdminPanel.Models;

public class CachedResult<T>
{
    public T? Data { get; }
    public bool CacheHit { get; }
    public CachedResult(T? data, bool cacheHit) 
    {
        Data = data;
        CacheHit = cacheHit;
    }
}