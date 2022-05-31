using OB.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OB.Api.Core;
using OB.DL.Common.Repositories.Interfaces;

namespace OB.DL.Common.Test.Mock
{
    public class RepositoryMock<T> : IRepository<T> where T : DomainObject
    {
        private int _managedThreadId = -1;

        public int ThreadManagedId
        {
            get
            {
                return _managedThreadId;
            }
        }

        private Guid _unitOfWorkGuid;
        public Guid UnitOfWorkGuid
        {
            get
            {
                return _unitOfWorkGuid;
            }
        }

        private int? _taskId;

        public int? TaskId
        {
            get
            {
                return _taskId;
            }
        }

        public override string ToString()
        {
            return "RepositoryMock<" + typeof(T).Name + ">" + " UoW guid:" + _unitOfWorkGuid + "  TID:" + _managedThreadId + "  TaskId:" + _taskId;
        }

        public RepositoryMock(ISessionFactory sessionFactory)
        {
            _managedThreadId = Thread.CurrentThread.ManagedThreadId;
            _unitOfWorkGuid = sessionFactory.GetUnitOfWork().Guid;
            _taskId = Task.CurrentId;
        }

        public Task LoadAsync(IQueryable<T> source)
        {
            throw new NotImplementedException();
        }

        public T Get(params object[] keyValues)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> GetQuery()
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> GetQuery(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public List<T> ToList(System.Linq.Expressions.Expression<Func<T, bool>> criteria)
        {
            throw new NotImplementedException();
        }

        public T Single(System.Linq.Expressions.Expression<Func<T, bool>> criteria)
        {
            throw new NotImplementedException();
        }

        public T SingleOrDefault(System.Linq.Expressions.Expression<Func<T, bool>> criteria)
        {
            throw new NotImplementedException();
        }

        public T First(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public T FirstOrDefault(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<T> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate = null)
        {
            throw new NotImplementedException();
        }

        public Task<int> CountAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> ToListAsync(System.Linq.Expressions.Expression<Func<T, bool>> criteria)
        {
            throw new NotImplementedException();
        }

        public void Refresh(T entity)
        {
            throw new NotImplementedException();
        }

        public T Add(T entity)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Add(IEnumerable<T> entitiesToAdd)
        {
            throw new NotImplementedException();
        }

        public void Attach(T entity)
        {
            throw new NotImplementedException();
        }

        public void Detach(T entity)
        {
            throw new NotImplementedException();
        }

        public void AttachAsModified(T entity)
        {
            throw new NotImplementedException();
        }

        public T Delete(T entity)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Delete(IEnumerable<T> entitiesToDelete)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Delete(System.Linq.Expressions.Expression<Func<T, bool>> criteria)
        {
            throw new NotImplementedException();
        }

        public void Update(T entity)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Find(System.Linq.Expressions.Expression<Func<T, bool>> criteria)
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> FindAsync(System.Linq.Expressions.Expression<Func<T, bool>> criteria)
        {
            throw new NotImplementedException();
        }

        public T FindOne(System.Linq.Expressions.Expression<Func<T, bool>> criteria)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetAll()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Get<TOrderBy>(System.Linq.Expressions.Expression<Func<T, TOrderBy>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Get<TOrderBy>(System.Linq.Expressions.Expression<Func<T, bool>> criteria, System.Linq.Expressions.Expression<Func<T, TOrderBy>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending)
        {
            throw new NotImplementedException();
        }

        public int Count()
        {
            throw new NotImplementedException();
        }

        public int Count(System.Linq.Expressions.Expression<Func<T, bool>> criteria)
        {
            throw new NotImplementedException();
        }

        public bool Any(System.Linq.Expressions.Expression<Func<T, bool>> criteria)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> ApplyPaging<TOrderBy>(IQueryable<T> query, System.Linq.Expressions.Expression<Func<T, TOrderBy>> orderBy, int pageIndex = 0, int pageSize = 1, SortOrder sortOrder = SortOrder.Ascending)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> ApplyPaging<TOrderBy>(out int totalRecords, IQueryable<T> query, List<Tuple<System.Linq.Expressions.Expression<Func<T, TOrderBy>>, SortOrder>> orderByTuples = null, int pageIndex = 0, int pageSize = 1, bool returnTotal = false)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> ApplyPaging<TOrderBy>(out int totalRecords, IQueryable<T> query, System.Linq.Expressions.Expression<Func<T, TOrderBy>> orderBy, int pageIndex = 0, int pageSize = 1, SortOrder sortOrder = SortOrder.Ascending, bool returnTotal = false)
        {
            throw new NotImplementedException();
        }


        public T Save(T entity)
        {
            throw new NotImplementedException();
        }





        public void Detach(ICollection<T> entities)
        {
            throw new NotImplementedException();
        }
    }
}
