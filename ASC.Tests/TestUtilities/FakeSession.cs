using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class FakeSession : ISession
{
    private Dictionary<string, byte[]> _sessionStorage = new Dictionary<string, byte[]>();

    public bool IsAvailable => true; // Mocked as always available
    public string Id => Guid.NewGuid().ToString(); // Fake session ID
    public IEnumerable<string> Keys => _sessionStorage.Keys;

    public void Clear()
    {
        _sessionStorage.Clear();
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask; // No real commit logic needed for testing
    }

    public Task LoadAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask; // No real load logic needed for testing
    }

    public void Remove(string key)
    {
        _sessionStorage.Remove(key);
    }

    public void Set(string key, byte[] value)
    {
        _sessionStorage[key] = value;
    }

    public bool TryGetValue(string key, out byte[] value)
    {
        return _sessionStorage.TryGetValue(key, out value);
    }
}
