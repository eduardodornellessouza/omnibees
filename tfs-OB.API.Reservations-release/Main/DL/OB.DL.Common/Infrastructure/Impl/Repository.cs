using OB.DL.Common.Exceptions;
using OB.DL.Common.Infrastructure;
using OB.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OB.Api.Core;
using EntityException = System.Data.Entity.Core.EntityException;
using OB.DL.Common.Repositories.Interfaces;

namespace OB.DL.Common.Impl
{
    internal class Repository<TEntity> : IRepository<TEntity>, IRepository where TEntity : Domain.DomainObject
    {
        protected IObjectContext _context;
        protected IDbConnection _connection;
        protected DbSet<TEntity> _objectSet;

        public Repository()
        {

        }

        public Repository(IObjectContext context)
        {
            this._context = context;
            _objectSet = context.Context.Set<TEntity>();
            _connection = context.Context.Database.Connection;
            if (_connection.State != ConnectionState.Open)
                _connection.Open();
        }

        internal IObjectContext Context
        {
            get
            {
                return _context;
            }
        }

        private ILogger _logger;

        public ILogger Logger
        {
            get
            {
                if (_logger == null)
                    _logger = LogsManager.CreateLogger(this.GetType());
                return _logger;
            }
        }

        public virtual TEntity Add(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            _objectSet.Add(entity);

            return entity;
        }

        public virtual IEnumerable<TEntity> Add(IEnumerable<TEntity> entitiesToAdd)
        {
            if (entitiesToAdd == null || entitiesToAdd.Count() == 0)
            {
                return Enumerable.Empty<TEntity>();
            }

            return _objectSet.AddRange(entitiesToAdd);
        }

        /// <summary>Check if exist at least one element</summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        public virtual bool Any(Expression<Func<TEntity, bool>> criteria)
        {
            return GetQuery().Any(criteria);
        }

        /// <summary>
        /// Applies paging (SortOrder, PageIndex starting at 0 and PageSize).
        /// </summary>
        /// <typeparam name="TOrderBy"></typeparam>
        /// <param name="query"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortOrder"></param>
        /// <param name="returnTotal"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> ApplyPaging<TOrderBy>(IQueryable<TEntity> query, Expression<Func<TEntity, TOrderBy>> orderBy, int pageIndex = 0, int pageSize = 1, SortOrder sortOrder = SortOrder.Ascending)
        {
            int totalRecords = -1;
            return this.ApplyPaging(out totalRecords, query, orderBy, pageIndex, pageSize, sortOrder, false);
        }

        /// <summary>
        /// Applies paging (SortOrder, PageIndex starting at 0 and PageSize) and gets the total records before paging if returnTotal is true.
        /// </summary>
        /// <typeparam name="TOrderBy"></typeparam>
        /// <param name="query"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortOrder"></param>
        /// <param name="returnTotal"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> ApplyPaging<TOrderBy>(out int totalRecords, IQueryable<TEntity> query, Expression<Func<TEntity, TOrderBy>> orderBy, int pageIndex = 0, int pageSize = 1, SortOrder sortOrder = SortOrder.Ascending, bool returnTotal = false)
        {
            var result = query;

            if (sortOrder == SortOrder.Descending)
                result = result.OrderByDescending(orderBy);
            else result = result.OrderBy(orderBy);

            if (returnTotal)
                totalRecords = query.Count();
            else totalRecords = -1;

            if (pageIndex > 0 && pageSize > 0)
            {
                result = result.Skip(pageIndex * pageSize);
            }

            if (pageSize > 0)
                result = result.Take(pageSize);

            return result;
        }

        /// <summary>
        /// Applies paging (SortOrder, PageIndex starting at 0 and PageSize) and gets the total records before paging if returnTotal is true.
        /// </summary>
        /// <typeparam name="TOrderBy"></typeparam>
        /// <param name="query"></param>
        /// <param name="orderByTuples"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="returnTotal"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> ApplyPaging<TOrderBy>(out int totalRecords, IQueryable<TEntity> query, List<Tuple<Expression<Func<TEntity, TOrderBy>>,SortOrder>> orderByTuples = null, int pageIndex = 0, int pageSize = 1, bool returnTotal = false)
        {
            var result = query;

            if (orderByTuples != null)
            {
                foreach (var orderByExpr in orderByTuples)
                {
                    if (orderByExpr.Item2 == SortOrder.Descending)
                        result = result.OrderByDescending(orderByExpr.Item1);
                    else result = result.OrderBy(orderByExpr.Item1);
                }
            }

            if (returnTotal)
                totalRecords = query.Count();
            else totalRecords = -1;

            if (pageIndex > 0 && pageSize > 0)
            {
                result = result.Skip(pageIndex * pageSize);
            }

            if (pageSize > 0)
                result = result.Take(pageSize);

            return result;
        }

        public virtual void Attach(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            _objectSet.Attach(entity);
        }

        public virtual void AttachAsModified(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            //Old
            //context.Attach(entity);
            //context.ObjectStateManager
            //     .MarkAllPropertiesModified(entity);

            _objectSet.Attach(entity);
            _context.Context.Entry<TEntity>(entity).State = System.Data.Entity.EntityState.Modified;
        }

        /// <summary>Counts this instance.</summary>
        /// <returns></returns>
        public virtual int Count()
        {
            return GetQuery().Count();
        }

        /// <summary>Counts the specified criteria.</summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        public virtual int Count(Expression<Func<TEntity, bool>> criteria)
        {
            return GetQuery().Count(criteria);
        }

        public virtual TEntity Delete(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            return _objectSet.Remove(entity);
        }

        public virtual IEnumerable<TEntity> Delete(IEnumerable<TEntity> entitiesToDelete)
        {
            if (entitiesToDelete == null || entitiesToDelete.Count() == 0)
            {
                return Enumerable.Empty<TEntity>();
            }

            //var oldValue = _context.Context.Configuration.ValidateOnSaveEnabled;
            //_context.Context.Configuration.ValidateOnSaveEnabled = false;

            var result = _objectSet.RemoveRange(entitiesToDelete);

            //_context.Context.Configuration.ValidateOnSaveEnabled = oldValue;

            return result;
        }

        public virtual IEnumerable<TEntity> Delete(Expression<Func<TEntity, bool>> criteria)
        {
            IEnumerable<TEntity> records = Find(criteria);

            return _objectSet.RemoveRange(records);
        }

        public virtual void Detach(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            _context.Context.Entry<TEntity>(entity).State = System.Data.Entity.EntityState.Detached;
        }

        public virtual void Detach(ICollection<TEntity> entities)
        {
            for (int i = entities.Count - 1; i >= 0; i--)
            {
                if (entities.ElementAt(i) == null)
                    throw new ArgumentNullException("entity");

                _context.Context.Entry<TEntity>(entities.ElementAt(i)).State = System.Data.Entity.EntityState.Detached;   
            }
        }

        /// <summary>Finds the entities that match the given criteria.</summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        public virtual IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> criteria)
        {
            return GetQuery().Where(criteria);
        }

        /// <summary>Finds the one.</summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        public virtual TEntity FindOne(Expression<Func<TEntity, bool>> criteria)
        {
            return GetQuery().Where(criteria).FirstOrDefault();
        }

        public virtual TEntity First(Expression<Func<TEntity, bool>> predicate)
        {
            return GetQuery(predicate).First();
        }

        public virtual TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return GetQuery(predicate).FirstOrDefault();
        }

        /// <summary>Gets the given primary key.</summary>
        /// <param name="keyValue">The key value.</param>
        /// <returns>The object or null.</returns>
        public virtual TEntity Get(params object[] values)
        {
            return _objectSet.Find(values);
            //return default(TEntity);
        }

        public virtual IEnumerable<TEntity> Get<TOrderBy>(Expression<Func<TEntity, TOrderBy>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending)
        {
            if (sortOrder == SortOrder.Ascending)
            {
                return GetQuery().OrderBy(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).AsEnumerable();
            }
            return GetQuery().OrderByDescending(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).AsEnumerable();
        }

        public virtual IEnumerable<TEntity> Get<TOrderBy>(Expression<Func<TEntity, bool>> criteria, Expression<Func<TEntity, TOrderBy>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending)
        {
            if (sortOrder == SortOrder.Ascending)
            {
                return GetQuery(criteria).OrderBy(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).AsEnumerable();
            }
            return GetQuery(criteria).OrderByDescending(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).AsEnumerable();
        }

        public virtual IEnumerable<TEntity> GetAll()
        {
            return GetQuery().AsEnumerable();
        }

        /// <summary>   Gets the query. </summary>
        /// <exception cref="DataLayerException">   Thrown when a data layer error condition occurs. </exception>
        /// <returns>   The query. </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IQueryable<TEntity> GetQuery()
        {
            try
            {
                return _objectSet;
            }
            catch (InvalidOperationException ex)
            {
                throw new DataLayerException("Error with database in GetQuery", ex);
            }
            catch (EntityException ex)
            {
                throw new DataLayerException("Error with entity framework in GetQuery", ex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IQueryable<TEntity> GetQuery(Expression<Func<TEntity, bool>> predicate)
        {
            return GetQuery().Where(predicate);
        }

        public virtual void Refresh(TEntity entity)
        {
            (_context.Context as IObjectContextAdapter).ObjectContext.Refresh(RefreshMode.ClientWins, entity);
        }

        public virtual TEntity Save(TEntity entity)
        {
            Add(entity);
            _context.Context.SaveChanges();
            return entity;
        }

        public virtual TEntity Single(Expression<Func<TEntity, bool>> criteria)
        {
            return GetQuery(criteria).Single();
        }

        /// <summary>
        /// Gets one entity based on matching criteria
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        public virtual TEntity SingleOrDefault(Expression<Func<TEntity, bool>> criteria)
        {
            return GetQuery(criteria).SingleOrDefault();
        }

        /// <summary>Gets a list for the given criteria.</summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        public virtual List<TEntity> ToList(Expression<Func<TEntity, bool>> criteria)
        {
            return GetQuery(criteria).ToList();
        }

        public virtual void Update(TEntity entity)
        {
            _context.Context.Entry<TEntity>(entity).CurrentValues.SetValues(entity);
        }

        #region AsyncMethods

        //
        // Summary:
        //     Asynchronously returns the number of elements in a sequence.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable<T> that contains the elements to be counted.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation.  The task result contains
        //     the number of elements in the input sequence.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     source is null .
        //
        //   System.InvalidOperationException:
        //     source doesn't implement System.Data.Entity.Infrastructure.IDbAsyncQueryProvider
        //     .
        //
        //   System.OverflowException:
        //     The number of elements in source is larger than System.Int32.MaxValue .
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported.
        //     Use 'await' to ensure that any asynchronous operations have completed before
        //     calling another method on this context.
        public virtual Task<int> CountAsync(IQueryable<TEntity> source)
        {
            return source.CountAsync();
        }

        //
        // Summary:
        //     Asynchronously returns the number of elements in a sequence.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable<T> that contains the elements to be counted.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task
        //     to complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation.  The task result contains
        //     the number of elements in the input sequence.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     source is null .
        //
        //   System.InvalidOperationException:
        //     source doesn't implement System.Data.Entity.Infrastructure.IDbAsyncQueryProvider
        //     .
        //
        //   System.OverflowException:
        //     The number of elements in source is larger than System.Int32.MaxValue .
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported.
        //     Use 'await' to ensure that any asynchronous operations have completed before
        //     calling another method on this context.
        public virtual Task<int> CountAsync(IQueryable<TEntity> source, CancellationToken cancellationToken)
        {
            return source.CountAsync(cancellationToken);
        }

        //
        // Summary:
        //     Asynchronously returns the number of elements in a sequence that satisfy
        //     a condition.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable<T> that contains the elements to be counted.
        //
        //   predicate:
        //     A function to test each element for a condition.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation.  The task result contains
        //     the number of elements in the sequence that satisfy the condition in the
        //     predicate function.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     source or predicate is null .
        //
        //   System.InvalidOperationException:
        //     source doesn't implement System.Data.Entity.Infrastructure.IDbAsyncQueryProvider
        //     .
        //
        //   System.OverflowException:
        //     The number of elements in source that satisfy the condition in the predicate
        //     function is larger than System.Int32.MaxValue .
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported.
        //     Use 'await' to ensure that any asynchronous operations have completed before
        //     calling another method on this context.
        public virtual Task<int> CountAsync(IQueryable<TEntity> source, Expression<Func<TEntity, bool>> predicate)
        {
            return source.CountAsync(predicate);
        }

        public virtual Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return GetQuery(predicate).CountAsync();
        }

        //
        // Summary:
        //     Asynchronously returns the number of elements in a sequence that satisfy
        //     a condition.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable<T> that contains the elements to be counted.
        //
        //   predicate:
        //     A function to test each element for a condition.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task
        //     to complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation.  The task result contains
        //     the number of elements in the sequence that satisfy the condition in the
        //     predicate function.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     source or predicate is null .
        //
        //   System.InvalidOperationException:
        //     source doesn't implement System.Data.Entity.Infrastructure.IDbAsyncQueryProvider
        //     .
        //
        //   System.OverflowException:
        //     The number of elements in source that satisfy the condition in the predicate
        //     function is larger than System.Int32.MaxValue .
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported.
        //     Use 'await' to ensure that any asynchronous operations have completed before
        //     calling another method on this context.
        public virtual Task<int> CountAsync(IQueryable<TEntity> source, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        {
            return source.CountAsync(predicate, cancellationToken);
        }

        /// <summary>Finds the entities that match the given criteria. Executes in a asynchronous fashion.</summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns>An awaitable Task with the result</returns>
        public virtual Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> criteria)
        {
            return GetQuery(criteria).ToListAsync();
        }

        public virtual Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate = null)
        {
            return _objectSet.FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// Asynchronously enumerates the query such.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public virtual Task LoadAsync(IQueryable<TEntity> source)
        {
            return source.LoadAsync();
        }

        // Summary:
        //     Creates a System.Collections.Generic.List<T> from an System.Linq.IQueryable<T>
        //     by enumerating it asynchronously.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable<T> to create a System.Collections.Generic.List<T>
        //     from.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation.  The task result contains
        //     a System.Collections.Generic.List<T> that contains elements from the input
        //     sequence.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported.
        //     Use 'await' to ensure that any asynchronous operations have completed before
        //     calling another method on this context.
        public virtual Task<List<TEntity>> ToListAsync(IQueryable<TEntity> source)
        {
            return source.ToListAsync();
        }

        public virtual Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>> criteria)
        {
            return GetQuery(criteria).ToListAsync();
        }

        #endregion AsyncMethods
    }
}
