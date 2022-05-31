using Couchbase.Core;
using Couchbase.Linq;
using Couchbase.Views;
using Newtonsoft.Json.Linq;
using OB.Reservation.BL.Operations.Exceptions;
using OB.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OB.DL.Common.Repositories.Interfaces;

namespace OB.DL.Common.Infrastructure.Impl
{
    internal class CouchbaseRepository<TEntity> : IRepository<TEntity>, IRepository
                                                                 where TEntity : DomainObject
                                                                                // where QREntity : ICouchBaseEntity<TEntity>,TEntity,new()
    {
        protected readonly BucketContext _bucketContext;
        readonly List<TEntity> _entitiesToAdd;

        public CouchbaseRepository(IBucket bucket)
        {
            Bucket = bucket;
            _bucketContext  = new BucketContext(bucket);
            _entitiesToAdd = new List<TEntity>();
        }

        protected  IBucket Bucket { get; set; }

        public virtual TEntity Add(TEntity entity)
        {
            var documentId = Guid.NewGuid().ToString("n");
            Couchbase.Document<TEntity> document = new Couchbase.Document<TEntity>();
            document.Content = entity;
            document.Id = documentId;
            document.Cas = 1;
            
            if (entity is DocumentDomainObject)
            {
                (entity as DocumentDomainObject).DocumentId = documentId;
            }

            var result = Bucket.Insert<TEntity>(document);

            result.ThrowIfNotSuccess();

            return entity;
        }

        protected void Update(DocumentDomainObject entity)
        {
            var result = Bucket.Upsert<DocumentDomainObject>(entity.DocumentId, entity);

            result.ThrowIfNotSuccess(entity.DocumentId);            
        }

        public void Update(TEntity entity)
        {
            if (!(entity is DocumentDomainObject))
            {
                throw new NotSupportedException("Please use the Update(entity, documentId) method overload");
            }
            Update(entity as DocumentDomainObject);
        }

        public IEnumerable<TEntity> Save(IEnumerable<TEntity> entitiesToAdd)
        {
            if (entitiesToAdd == null || entitiesToAdd.Count() == 0)
                return Enumerable.Empty<TEntity>();

            var result = Bucket.Upsert<TEntity>(entitiesToAdd.ToDictionary(x => ((ICouchBaseEntity<TEntity>)x).Id));
            if (result.Any(x => !x.Value.Success))
            {
                var withError = result.FirstOrDefault(x => !x.Value.Success).Value;
                withError.ThrowIfNotSuccess(((ICouchBaseEntity<TEntity>)withError.Value).Id);
            }

            return result.Select(x => x.Value.Value).ToList();
        }

        public void Remove(TEntity entity)
        {
            var couchbaseEntity = entity as ICouchBaseEntity<TEntity>;
            var result = Bucket.Remove(couchbaseEntity.Wrap());
            result.ThrowIfNotSuccess(couchbaseEntity.Id);
        }

        //TMOREIRA:N1SQL STILL NOT SUPPORTED (PRODUCTION READY)
        //public IEnumerable<TEntity> Select(IQueryRequest queryRequest)
        //{
        //    var results = Bucket.Query<TEntity>(queryRequest);
        //    if (!results.Success)
        //    {
        //        var message = JsonConvert.SerializeObject(results.Errors);
        //        throw new QueryRequestException(message, results.Status);
        //    }
        //    return results.Rows;
        //}

        public IEnumerable<TEntity> Select(IViewQuery viewQuery)
        {
            var results = Bucket.Query<TEntity>(viewQuery);
            if (!results.Success)
            {
                var message = results.Error;
                throw new ViewRequestException(message, results.StatusCode);
            }
            return results.Values;
        }

        public TEntity Find(string key)
        {
            var result = Bucket.GetDocument<TEntity>(key);
            result.ThrowIfNotSuccess();

            //var qrE = new QREntity();
            //qrE.DomainObject = result.Document.Content;
            //qrE.Cas = result.Document.Cas;
            //qrE.Id = result.Document.Id;

            return null;

            //return result.Document.UnWrap();
        }

        public IEnumerable<TEntity> Find(System.Linq.Expressions.Expression<Func<TEntity, bool>> criteria)
        {
            return this.GetQuery(criteria);
        }

        public IEnumerable<TEntity> Select(IList<string> keys)
        {
            throw new System.NotImplementedException();
        }

        //#########################################################################################################################################

        public Task LoadAsync(IQueryable<TEntity> source)
        {
            throw new NotImplementedException();
        }

        public TEntity Get(params object[] keyValues)
        {
            var jObject = JObject.FromObject(keyValues);

            var result = Bucket.Get<TEntity>(jObject.ToString());

            if (result != null && result.Status == Couchbase.IO.ResponseStatus.Success)
            {
                return result.Value;
            }
            return null;
        }

        /// <summary>
        /// List of Document uids.
        /// </summary>
        /// <param name="uids"></param>
        /// <returns></returns>
        protected IDictionary<string, TEntity> GetDocuments(List<string> uids)
        {
            if (uids == null || !uids.Any())
                return new Dictionary<string, TEntity>();

            var result = new Dictionary<string, TEntity>();
            var query = Bucket.Get<TEntity>(uids);
            foreach (var item in query)
            {
                if(item.Value.Status == Couchbase.IO.ResponseStatus.Success
                    && item.Value.Value != null)
                {
                    result.Add(item.Key, item.Value.Value);
                }                
            }
            
            //Parallel.ForEach(uids, uid =>
            //{
            //    var docOperationResult = Bucket.GetDocument<TEntity>(uid);
            //    if(docOperationResult.Status == Couchbase.IO.ResponseStatus.Success
            //        && docOperationResult.Content != null)
            //    {
            //        result.TryAdd(uid, docOperationResult.Content);
            //    }                
            //});
            
            return result;
        }

        public Dictionary<Guid, string> GetDocumentByUids(out int totalRecords, List<Guid> Uids, int pageIndex = 0, int pageSize = 0, bool returnTotal = false)
        {
            return FindByUIDs<string>(out totalRecords, Uids.Select(x => x.ToString()).ToList(), pageIndex, pageSize, returnTotal);
        }

        public Dictionary<Guid, string> GetDocumentByUids(out int totalRecords, List<string> Uids, int pageIndex = 0, int pageSize = 0, bool returnTotal = false)
        {
            return FindByUIDs<string>(out totalRecords, Uids, pageIndex, pageSize, returnTotal);
        }

        public Dictionary<Guid, T> FindByUIDs<T>(out int totalRecords, List<string> Uids, int pageIndex = 0, int pageSize = 0, bool returnTotal = false)        
            where T : class
        {
            totalRecords = 0;
            var result = new Dictionary<Guid, T>();
            if (Uids == null || !Uids.Any())
                return result;

            var keys = Uids.Select(x => x.ToLower()).ToList();
            var query = Bucket.Get<T>(keys);

            foreach (var item in query)
                result.Add(Guid.Parse(item.Key), item.Value.Value);
            
            if(returnTotal)
                totalRecords = result.Count;

            return result;
        }

        public T FindByUID<T>(string Uid, bool uidToLower = false)
            where T : class
        {
            if (!string.IsNullOrWhiteSpace(Uid) && uidToLower)
                Uid = Uid.ToLower();

            var result = Bucket.Get<T>(Uid);
            return result.Value;
        }

        public IDictionary<string, T> FindByUIDs<T>(IList<string> uids, bool uidsToLower = false)
           where T : class
        {
            var result = new Dictionary<string, T>();
            if (uids == null || !uids.Any())
                return result;

            var keys = uidsToLower ? uids.Select(x => x.ToLower()).ToList() : uids;

            var tmp = Bucket.Get<T>(keys);
            return tmp.ToDictionary(k => k.Key, v => v.Value.Value);
        }

        public virtual IQueryable<TEntity> GetQuery()
        {
            return _bucketContext.Query<TEntity>();
        }

        public IQueryable<TEntity> GetQuery(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
        {
            return _bucketContext.Query<TEntity>().Where(predicate);
        }

        public List<TEntity> ToList(System.Linq.Expressions.Expression<Func<TEntity, bool>> criteria)
        {
            return GetQuery(criteria).ToList();
        }

        public TEntity Single(System.Linq.Expressions.Expression<Func<TEntity, bool>> criteria)
        {
            return GetQuery(criteria).Single();
        }

        public TEntity SingleOrDefault(System.Linq.Expressions.Expression<Func<TEntity, bool>> criteria)
        {
            return GetQuery(criteria).SingleOrDefault();
        }

        public TEntity First(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
        {
            return GetQuery(predicate).First();
        }

        public TEntity FirstOrDefault(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
        {
            return GetQuery(predicate).FirstOrDefault();
        }

        public Task<TEntity> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate = null)
        {
            return Task.Run<TEntity>(() => {

                return GetQuery(predicate).FirstOrDefault();

            });
        }

        public Task<int> CountAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
        {
            return Task.Run<int>(() =>
            {

                return GetQuery(predicate).Count();

            });
        }

        public Task<List<TEntity>> ToListAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> criteria)
        {
            return Task.Run<List<TEntity>>(() =>
            {

                return GetQuery(criteria).ToList();

            });
        }

        public void Refresh(TEntity entity)
        {
            throw new NotImplementedException();
        }

      

        public IEnumerable<TEntity> Add(IEnumerable<TEntity> entitiesToAdd)
        {
            foreach (var entity in entitiesToAdd)
            {
                this.Add(entity);
            }

            return entitiesToAdd;
        }

        public void Attach(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public void Detach(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public void AttachAsModified(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public TEntity Delete(TEntity entity)
        {
            Bucket.Remove((entity as DocumentDomainObject).DocumentId);
            
            return entity;
        }

        public IEnumerable<TEntity> Delete(IEnumerable<TEntity> entitiesToDelete)
        {
            foreach (var entity in entitiesToDelete)
            {
                this.Delete(entity);
            }

            return entitiesToDelete;
        }

        public IEnumerable<TEntity> Delete(System.Linq.Expressions.Expression<Func<TEntity, bool>> criteria)
        {
            throw new NotImplementedException();
        }

      

        public Task<List<TEntity>> FindAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> criteria)
        {
            return Task.Run<List<TEntity>>(() => {
                return this.Find(criteria).ToList();
            });
        }

        public TEntity FindOne(System.Linq.Expressions.Expression<Func<TEntity, bool>> criteria)
        {
            return Find(criteria).Single();
        }

        public IEnumerable<TEntity> GetAll()
        {
            return this.GetQuery().ToList();
        }

        public IEnumerable<TEntity> Get<TOrderBy>(System.Linq.Expressions.Expression<Func<TEntity, TOrderBy>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TEntity> Get<TOrderBy>(System.Linq.Expressions.Expression<Func<TEntity, bool>> criteria, System.Linq.Expressions.Expression<Func<TEntity, TOrderBy>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending)
        {
            throw new NotImplementedException();
        }

        public int Count()
        {
            return this.GetQuery().Count();
        }

        public int Count(System.Linq.Expressions.Expression<Func<TEntity, bool>> criteria)
        {
            return this.GetQuery().Count(criteria);
        }

        public bool Any(System.Linq.Expressions.Expression<Func<TEntity, bool>> criteria)
        {
            return this.GetQuery(criteria).Any();
        }

        public IQueryable<TEntity> ApplyPaging<TOrderBy>(IQueryable<TEntity> query, System.Linq.Expressions.Expression<Func<TEntity, TOrderBy>> orderBy, int pageIndex = 0, int pageSize = 1, SortOrder sortOrder = SortOrder.Ascending)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TEntity> ApplyPaging<TOrderBy>(out int totalRecords, IQueryable<TEntity> query, System.Linq.Expressions.Expression<Func<TEntity, TOrderBy>> orderBy, int pageIndex = 0, int pageSize = 1, SortOrder sortOrder = SortOrder.Ascending, bool returnTotal = false)
        {
            throw new NotImplementedException();
        }


        public IQueryable<TEntity> ApplyPaging<TOrderBy>(out int totalRecords, IQueryable<TEntity> query, List<Tuple<System.Linq.Expressions.Expression<Func<TEntity, TOrderBy>>, SortOrder>> orderByTuples = null, int pageIndex = 0, int pageSize = 1, bool returnTotal = false)
        {
            throw new NotImplementedException();
        }


        public void Detach(ICollection<TEntity> entities)
        {
            throw new NotImplementedException();
        }

    }
}
