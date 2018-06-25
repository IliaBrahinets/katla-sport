using System.Data.Entity;
using System.Linq;
using KatlaSport.DataAccess;

namespace KatlaSport.Services.Tests
{
    internal class FakeAsyncEntitySet<TEntity> : EntitySetBase<TEntity>
    where TEntity : class
    {
        public FakeAsyncEntitySet(IDbSet<TEntity> dbSet)
        {
            DbSet = dbSet;
        }

        internal IDbSet<TEntity> DbSet { get; }

        protected override IQueryable<TEntity> Queryable => DbSet;

        public override TEntity Add(TEntity entity)
        {
            return DbSet.Add(entity);
        }

        public override TEntity Remove(TEntity entity)
        {
            return DbSet.Remove(entity);
        }
    }
}
