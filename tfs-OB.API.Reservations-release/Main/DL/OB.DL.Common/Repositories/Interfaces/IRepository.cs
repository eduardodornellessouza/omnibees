using OB.DL.Common.Infrastructure;
using OB.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OB.DL.Common.Repositories.Interfaces
{
    /// <summary>
    /// Base Interface for all Repository Classes.
    /// </summary>
    public interface IRepository<TEntity> : IRepository where TEntity : DomainObject
    {
        /// <summary>
        /// Asynchronously enumerates the query such.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task LoadAsync(IQueryable<TEntity> source);

        /// <summary>
        /// Gets entity by key.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="keyValue">The key value.</param>
        /// <returns></returns>
        TEntity Get(params object[] keyValues);

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns></returns>
        IQueryable<TEntity> GetQuery();

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        IQueryable<TEntity> GetQuery(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Gets a list for the given criteria.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        List<TEntity> ToList(Expression<Func<TEntity, bool>> criteria);

        /// <summary>
        /// Gets one entity based on matching criteria
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        TEntity Single(Expression<Func<TEntity, bool>> criteria);

        /// <summary>
        /// Gets one entity based on matching criteria
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        TEntity SingleOrDefault(Expression<Func<TEntity, bool>> criteria);

        /// <summary>
        /// Gets the First entity from the given predicate.
        /// Throws an exception if the predicate doesn't select at least one element.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        TEntity First(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Gets the First entity from the given predicate or null if there isn't one returned by the
        /// given predicate.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Gets the First entity from the given predicate or null if there isn't one returned by the
        /// given predicate. Executes in an asynchrounous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate = null);

        /// <summary>
        /// Counts entities with the specified criteria. Executes in an asynchrounous way.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="criteria">The criteria.</param>
        /// <returns>A Task with the result</returns>
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Gets a list for the given criteria. Executes in an asynchrounous way.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns>A Task with the result</returns>
        Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>> criteria);

        void Refresh(TEntity entity);

        /// <summary>
        /// Adds the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        TEntity Add(TEntity entity);

        /// <summary>
        /// Adds the specified entities.
        /// </summary>
        /// <param name="entitiesToAdd">The entity.</param>
        IEnumerable<TEntity> Add(IEnumerable<TEntity> entitiesToAdd);

        /// <summary>
        /// Attaches the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        void Attach(TEntity entity);

        /// <summary>
        /// Detaches the specified entity changing it's state to Detached, which stops the ChangeTracker from tracking changes to it.
        /// </summary>
        /// <param name="entity"></param>
        void Detach(TEntity entity);

        void Detach(ICollection<TEntity> entities);

        /// <summary>
        /// Attaches the specified entity as modified maintaining the current property values.
        /// It will be saved to database when SaveChanges is invoked.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        void AttachAsModified(TEntity entity);

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        TEntity Delete(TEntity entity);

        /// <summary>
        /// Deletes the specified entities.
        /// </summary>
        /// <param name="entitiesToDelete"></param>
        /// <returns></returns>
        IEnumerable<TEntity> Delete(IEnumerable<TEntity> entitiesToDelete);

        /// <summary>
        /// Deletes one or many entities matching the specified criteria
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="criteria">The criteria.</param>
        IEnumerable<TEntity> Delete(Expression<Func<TEntity, bool>> criteria);

        /// <summary>
        /// Updates changes of the existing entity.
        /// The caller must later call SaveChanges() on the repository explicitly to save the entity to database
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        void Update(TEntity entity);

        /// <summary>
        /// Finds the entities that match the given criteria.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> criteria);

        /// <summary>Finds the entities that match the given criteria. Executes in a asynchronous fashion.</summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns>An awaitable Task with the result</returns>
        Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> criteria);

        /// <summary>
        /// Finds one entity based on provided criteria.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        TEntity FindOne(Expression<Func<TEntity, bool>> criteria);

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns></returns>
        IEnumerable<TEntity> GetAll();

        /// <summary>
        /// Gets the specified order by.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TOrderBy">The type of the order by.</typeparam>
        /// <param name="orderBy">The order by.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <returns></returns>
        IEnumerable<TEntity> Get<TOrderBy>(Expression<Func<TEntity, TOrderBy>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending);

        /// <summary>
        /// Gets the specified criteria.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TOrderBy">The type of the order by.</typeparam>
        /// <param name="criteria">The criteria.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <returns></returns>
        IEnumerable<TEntity> Get<TOrderBy>(Expression<Func<TEntity, bool>> criteria, Expression<Func<TEntity, TOrderBy>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending);

        /// <summary>
        /// Counts the specified entities.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns></returns>
        int Count();

        /// <summary>
        /// Counts entities with the specified criteria.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        int Count(Expression<Func<TEntity, bool>> criteria);

        /// <summary>
        /// Check if here is at least one element
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        bool Any(Expression<Func<TEntity, bool>> criteria);

        /// <summary>
        /// Applies paging (SortOrder, PageIndex starting at 0 and PageSize)
        /// </summary>
        /// <typeparam name="TOrderBy"></typeparam>
        /// <param name="query"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        IQueryable<TEntity> ApplyPaging<TOrderBy>(IQueryable<TEntity> query, Expression<Func<TEntity, TOrderBy>> orderBy, int pageIndex = 0, int pageSize = 1, SortOrder sortOrder = SortOrder.Ascending);

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
        IQueryable<TEntity> ApplyPaging<TOrderBy>(out int totalRecords, IQueryable<TEntity> query, List<Tuple<Expression<Func<TEntity, TOrderBy>>, SortOrder>> orderByTuples = null, int pageIndex = 0, int pageSize = 1, bool returnTotal = false);

        /// <summary>
        /// Applies paging (SortOrder, PageIndex starting at 0 and PageSize) and gets the total records before paging.
        /// </summary>
        /// <typeparam name="TOrderBy"></typeparam>
        /// <param name="query"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortOrder"></param>
        /// <param name="returnTotal"></param>
        /// <returns></returns>
        IQueryable<TEntity> ApplyPaging<TOrderBy>(out int totalRecords, IQueryable<TEntity> query, Expression<Func<TEntity, TOrderBy>> orderBy, int pageIndex = 0, int pageSize = 1, SortOrder sortOrder = SortOrder.Ascending, bool returnTotal = false);
    }

    //public interface IRepository<TEntity> where TEntity : DomainEntity
    //{
    //    TEntity Find(params object[] keyValues);
    //    TEntity Remove(TEntity entity);
    //    TEntity Add(TEntity entity);
    //    IQueryable<TEntity> GetAll();
    //}
}