using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace KatlaSport.Services.Tests
{
    internal class FakeDbAsyncEnumerator<TEntity> : IDbAsyncEnumerator<TEntity>
    {
        private IEnumerator<TEntity> _inner;

        public FakeDbAsyncEnumerator(IEnumerator<TEntity> inner)
        {
            _inner = inner;
        }

        public TEntity Current => _inner.Current;

        object IDbAsyncEnumerator.Current => _inner.Current;

        public void Dispose()
        {
            _inner.Dispose();
        }

        public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<bool>(_inner.MoveNext());
        }
    }
}