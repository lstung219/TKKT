//using Microsoft.AspNetCore.Http;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ASC.Tests.TestUtilities
//{
//    public class FakeSession : ISession
//    {
//        public bool IsAvailable => throw new NotImplementedException();
//        public string Id => throw new NotImplementedException();
//        public IEnumerable<string> Keys => throw new NotImplementedException();
//        public Dictionary<string, byte[]> sessionFactory => new Dictionary<string, byte[]>();
//        public void Clear()
//        {
//            throw new NotImplementedException();
//        }
//        public Task CommitAsync()
//        {
//            throw new NotImplementedException();
//        }
//        public Task LoadAsync()
//        {
//            throw new NotImplementedException();
//        }
//    }
//}

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASC.Tests.TestUtilities
{
    public class FakeSession : ISession
    {
        private Dictionary<string, byte[]> _sessionStorage = new Dictionary<string, byte[]>();

        public bool IsAvailable => true; // Giả lập session luôn sẵn sàng.
        public string Id => Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _sessionStorage.Keys;

        public void Clear()
        {
            _sessionStorage.Clear();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            _sessionStorage.Remove(key);
        }

        public void Set(string key, byte[] value)
        {
            _sessionStorage[key] = value;
        }

        public bool TryGetValue(string key, out byte[]? value)
        {
            return _sessionStorage.TryGetValue(key, out value);
        }
    }
}
