using OB.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OB.DL.Common.Test.Helper
{

    public class UnitTestContext : DbContext 
    {
        public UnitTestContext()
        {
        }

        private static Dictionary<Type, DbSet> _internalDbSets = new Dictionary<Type, DbSet>();
        private static Dictionary<Type, object> _internalGenericDbSets = new Dictionary<Type, object>();


        public int SaveChangesCount { get; private set; }
        public override int SaveChanges()
        {
            this.SaveChangesCount++;
            return 1;
        }

        public static void Reset()
        {
            _internalDbSets.Clear();
            _internalGenericDbSets.Clear();
        }

        public static DbSet<TEntity> GetStaticDbSet<TEntity>() where TEntity : class
        {
            if (!_internalGenericDbSets.ContainsKey(typeof(TEntity)))
            {
                _internalGenericDbSets.Add(typeof(TEntity), new UnitDbSet<TEntity>());
            }
            return _internalGenericDbSets[typeof(TEntity)] as DbSet<TEntity>;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public override Task<int> SaveChangesAsync()
        {
            return Task.Run(() =>
            {
                return SaveChanges();
            });
        }

        public override DbSet<TEntity> Set<TEntity>()
        {
            return GetStaticDbSet<TEntity>();
        }

        public override DbSet Set(Type entityType)
        {
            string genericFullName = entityType.AssemblyQualifiedName;

            if(!_internalDbSets.ContainsKey(entityType))
            {
                _internalDbSets.Add(entityType, new UnitDbSet(entityType));
            }
            return _internalDbSets[entityType];             
        }

        public override Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                return SaveChanges();
            });
        }

        protected override bool ShouldValidateEntity(System.Data.Entity.Infrastructure.DbEntityEntry entityEntry)
        {
            return base.ShouldValidateEntity(entityEntry);
        }

        protected override System.Data.Entity.Validation.DbEntityValidationResult ValidateEntity(System.Data.Entity.Infrastructure.DbEntityEntry entityEntry, IDictionary<object, object> items)
        {
            return new System.Data.Entity.Validation.DbEntityValidationResult(entityEntry, new List<System.Data.Entity.Validation.DbValidationError>());
        }
    }

    public class UnitDbSet<TEntity> : DbSet<TEntity>, IQueryable, IEnumerable<TEntity>, IDbAsyncEnumerable<TEntity>
        where TEntity : class
    {
        ObservableCollection<TEntity> _data;
        IQueryable _query;

        public UnitDbSet()
        {
            _data = new ObservableCollection<TEntity>();
            _query = _data.AsQueryable();
        }

        public override TEntity Add(TEntity item)
        {
            _data.Add(item);
            return item;
        }

        public override TEntity Remove(TEntity item)
        {
            _data.Remove(item);
            return item;
        }

        public override TEntity Attach(TEntity item)
        {
            _data.Add(item);
            return item;
        }

        public override TEntity Create()
        {
            return Activator.CreateInstance<TEntity>();
        }

        public override TDerivedEntity Create<TDerivedEntity>()
        {
            return Activator.CreateInstance<TDerivedEntity>();
        }

        public override ObservableCollection<TEntity> Local
        {
            get { return _data; }
        }

        Type IQueryable.ElementType
        {
            get { return _query.ElementType; }
        }

        Expression IQueryable.Expression
        {
            get { return _query.Expression; }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return new UnitTestDbAsyncQueryProvider<TEntity>(_query.Provider); }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IDbAsyncEnumerator<TEntity> IDbAsyncEnumerable<TEntity>.GetAsyncEnumerator()
        {
            return new UnitTestDbAsyncEnumerator<TEntity>(_data.GetEnumerator());
        }
    }

    public class UnitDbSet : DbSet
    {
        public Type EntityType { get; private set; }

        public UnitDbSet(Type entityType)
        {
            EntityType = entityType;
        }

    }

    internal class UnitTestDbAsyncQueryProvider<TEntity> : IDbAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal UnitTestDbAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new UnitTestDbAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new UnitTestDbAsyncEnumerable<TElement>(expression);
        }

        public object Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute(expression));
        }

        public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute<TResult>(expression));
        }
    }

    internal class UnitTestDbAsyncEnumerable<T> : EnumerableQuery<T>, IDbAsyncEnumerable<T>, IQueryable<T>
    {
        public UnitTestDbAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        public UnitTestDbAsyncEnumerable(Expression expression)
            : base(expression)
        { }

        public IDbAsyncEnumerator<T> GetAsyncEnumerator()
        {
            return new UnitTestDbAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
        {
            return GetAsyncEnumerator();
        }

        IQueryProvider IQueryable.Provider
        {
            get { return new UnitTestDbAsyncQueryProvider<T>(this); }
        }
    }

    internal class UnitTestDbAsyncEnumerator<T> : IDbAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public UnitTestDbAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _inner.Dispose();
                }
                disposed = true;
            }
        }

        public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_inner.MoveNext());
        }

        public T Current
        {
            get { return _inner.Current; }
        }

        object IDbAsyncEnumerator.Current
        {
            get { return Current; }
        }
    }
}
