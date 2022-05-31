using OB.Api.Core;
using OB.DL.Common.Cache;
using OB.DL.Common.Infrastructure;
using OB.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OB.DL.Common.Impl
{
    class CachedRepository<TEntity> : Repository<TEntity>, ICachedRepository<TEntity>
        where TEntity : DomainObject
    {
#pragma warning disable S2743
#pragma warning disable RECS0108
        static dynamic emptyObjOfCachedDataType;
#pragma warning restore RECS0108
#pragma warning restore S2743

        object reusedCachedData;
        protected readonly TimeSpan _refreshTime;
        protected readonly string _cacheKey;
        protected readonly ICacheProvider _cacheProvider;
        protected readonly string _connectionString;

        /// <summary>
        /// Construtor with RefreshTime = 1 day.
        /// </summary>
        public CachedRepository(ICacheProvider cacheProvider) : this(cacheProvider, Configuration.DefaultCacheRefresh)
        {

        }

        /// <summary>
        /// Construtor with RefreshTime = 1 day.
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="refreshTime"></param>
        public CachedRepository(ICacheProvider cacheProvider, TimeSpan refreshTime)
        {
            _cacheProvider = cacheProvider;
            _refreshTime = refreshTime;
            _cacheKey = GetType().GetInterfaces().Last().FullName;
        }

        /// <summary>
        /// Construtor with RefreshTime = 1 day.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cacheProvider"></param>
        public CachedRepository(IObjectContext context, ICacheProvider cacheProvider)
            : this(context, cacheProvider, Configuration.DefaultCacheRefresh)
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cacheProvider"></param>
        /// <param name="refreshTime"></param>
        public CachedRepository(IObjectContext context, ICacheProvider cacheProvider, TimeSpan refreshTime)
           : base(context)
        {
            _cacheProvider = cacheProvider;
            _refreshTime = refreshTime;
            _cacheKey = GetType().GetInterfaces().Last().FullName;
            _connectionString = _connection.ConnectionString;
        }

        #region Set/Get Cached Data

        /// <summary>
        /// Sets the CacheProvider for this CacheKey.
        /// This method must be called on repository constructor.
        /// </summary>
        /// <param name="GetDataCallback"></param>
        protected virtual void SetCacheProvider<T>(Func<T> GetDataCallback) where T : class, new()
        {
            // Creates an empty instance to know about the type of cached data
            // It's static between calls of the same repository. Different repositories have differentes instances of this field.
            if (emptyObjOfCachedDataType == null)
#pragma warning disable S2696
                emptyObjOfCachedDataType = new T();
#pragma warning restore S2696

            _cacheProvider.Set(_cacheKey, new CacheEntry
            {
                CacheKey = _cacheKey,
                GetDataCallback = () => GetDataCallback.Invoke(),
                UpdateInterval = _refreshTime
            });
        }

        T GetDataFromCacheProvider<T>() where T : class
        {
            // Reuse cached data in the same call if repository is called multiple times
            if (reusedCachedData == null)
                reusedCachedData = _cacheProvider.Get<T>(_cacheKey);

            return (T)reusedCachedData;
        }

        /// <summary>
        /// Get Data from cache with failover to SQL.
        /// </summary>
        /// <typeparam name="T">The type of CachedData.</typeparam>
        /// <returns>Cached data.</returns>
        protected virtual T GetDataFromCache<T>() where T : class
        {
            T data = null;
            Exception exception = null;
            bool failed;

            try
            {
                data = GetDataFromCacheProvider<T>();
                failed = data == null;
            }
#pragma warning disable S2221
            catch (Exception ex)
#pragma warning restore S2221
            {
                exception = ex;
                failed = true;
            }

            // Failover to sql if cache fails
            if (failed)
            {
                Logger.Fatal(exception, "Get data from cache '{0}' failed.", _cacheKey);

                // Force update cache to get data from SQL
                _cacheProvider.Invalidate(_cacheKey, true);
                data = GetDataFromCacheProvider<T>();
            }

            return data;
        }

#pragma warning disable S1172
#pragma warning disable RECS0154
        T GetDataFromCache<T>(T objType) where T : class
#pragma warning restore RECS0154
#pragma warning restore S1172
        {
            return GetDataFromCache<T>();
        }

        #endregion Set/Get Cached Data

        #region Implementation of ICachedRepository

        public override IEnumerable<TEntity> GetAll()
        {
            var data = (object)GetDataFromCache(emptyObjOfCachedDataType);

            // Dictionary
            if (data is IDictionary dict)
            {
                if (dict.Values is IEnumerable<TEntity> dictObjValues)
                    return dictObjValues;
                if (dict.Values is IEnumerable<IEnumerable<TEntity>> dictListValues)
                    return dictListValues.SelectMany(x => x);
            }

            // Enumerable
            else if (data is IEnumerable<TEntity> list)
                return list;

            // Single object
            else if (data is TEntity obj)
                return new List<TEntity> { obj };

            throw new NotImplementedException("Implement 'GetAll' method in repository.");
        }

        public override bool Any(Expression<Func<TEntity, bool>> criteria)
        {
            return GetAll().AsQueryable().Any(criteria);
        }

        public override int Count()
        {
            return GetAll().Count();
        }

        public override int Count(Expression<Func<TEntity, bool>> criteria)
        {
            return GetAll().AsQueryable().Count(criteria);
        }

        public override IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> criteria)
        {
            return GetAll().AsQueryable().Where(criteria);
        }

        public override TEntity First(Expression<Func<TEntity, bool>> predicate)
        {
            return GetAll().AsQueryable().First(predicate);
        }

        public override TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return GetAll().AsQueryable().FirstOrDefault(predicate);
        }

        public override TEntity Single(Expression<Func<TEntity, bool>> criteria)
        {
            return GetAll().AsQueryable().Single(criteria);
        }

        public override TEntity SingleOrDefault(Expression<Func<TEntity, bool>> criteria)
        {
            return GetAll().AsQueryable().SingleOrDefault(criteria);
        }

        public void Invalidate(bool forceUpdate = true)
        {
            _cacheProvider.Invalidate(_cacheKey, forceUpdate);
        }

        public void Invalidate(IEnumerable<long> ids)
        {
            Invalidate();
        }

        public void Invalidate(long id)
        {
            Invalidate();
        }

        #endregion Implementation of ICachedRepository

        #region Methods Not Exposed on Interface

        public override IQueryable<TEntity> GetQuery()
        {
            return GetAll().AsQueryable();
        }

        public override IQueryable<TEntity> GetQuery(Expression<Func<TEntity, bool>> predicate)
        {
            return Find(predicate).AsQueryable();
        }

        public override TEntity Get(params object[] values)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<TEntity> Get<TOrderBy>(Expression<Func<TEntity, bool>> criteria, Expression<Func<TEntity, TOrderBy>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<TEntity> Get<TOrderBy>(Expression<Func<TEntity, TOrderBy>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending)
        {
            throw new NotImplementedException();
        }

        public override Task LoadAsync(IQueryable<TEntity> source)
        {
            throw new NotImplementedException();
        }

        public override List<TEntity> ToList(Expression<Func<TEntity, bool>> criteria)
        {
            throw new NotImplementedException();
        }

        public override Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate = null)
        {
            throw new NotImplementedException();
        }

        public override Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public override Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>> criteria)
        {
            throw new NotImplementedException();
        }

        public override void Refresh(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public override TEntity Add(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<TEntity> Add(IEnumerable<TEntity> entitiesToAdd)
        {
            throw new NotImplementedException();
        }

        public override void Attach(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public override void Detach(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public override void Detach(ICollection<TEntity> entities)
        {
            throw new NotImplementedException();
        }

        public override void AttachAsModified(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public override TEntity Delete(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<TEntity> Delete(IEnumerable<TEntity> entitiesToDelete)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<TEntity> Delete(Expression<Func<TEntity, bool>> criteria)
        {
            throw new NotImplementedException();
        }

        public override void Update(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public override Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> criteria)
        {
            throw new NotImplementedException();
        }

        public override TEntity FindOne(Expression<Func<TEntity, bool>> criteria)
        {
            throw new NotImplementedException();
        }

        public override IQueryable<TEntity> ApplyPaging<TOrderBy>(IQueryable<TEntity> query, Expression<Func<TEntity, TOrderBy>> orderBy, int pageIndex = 0, int pageSize = 1, SortOrder sortOrder = SortOrder.Ascending)
        {
            throw new NotImplementedException();
        }

        public override IQueryable<TEntity> ApplyPaging<TOrderBy>(out int totalRecords, IQueryable<TEntity> query, List<Tuple<Expression<Func<TEntity, TOrderBy>>, SortOrder>> orderByTuples = null, int pageIndex = 0, int pageSize = 1, bool returnTotal = false)
        {
            throw new NotImplementedException();
        }

        public override IQueryable<TEntity> ApplyPaging<TOrderBy>(out int totalRecords, IQueryable<TEntity> query, Expression<Func<TEntity, TOrderBy>> orderBy, int pageIndex = 0, int pageSize = 1, SortOrder sortOrder = SortOrder.Ascending, bool returnTotal = false)
        {
            throw new NotImplementedException();
        }

        #endregion Methods Not Exposed on Interface

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                reusedCachedData = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}