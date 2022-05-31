using OB.DL.Common.Interfaces;
using OB.Domain;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace OB.DL.Common.Impl
{
    /// <summary>
    /// UnitOfWork class. Adds support for transactions and holds a reference to the currently used database contexts;
    /// </summary>
    internal class UnitOfWork : IUnitOfWork
    {
        private Dictionary<DomainScope, IObjectContext> _contexts = new Dictionary<DomainScope, IObjectContext>();
        private Dictionary<string, IDbConnection> _sqlContexts = new Dictionary<string, IDbConnection>();
        private Func<DomainScope, Guid, IObjectContext> _createContextFactoryMethod;

        private Guid _unitOfWorkIdentifier = Guid.NewGuid();

        public event EventHandler OnDispose;

        private int _threadId;
        private int? _taskId;

        private bool _isReadOnly = false;
        public bool IsReadOnly
        {
            get
            {
                return _isReadOnly;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public UnitOfWork(IEnumerable<IObjectContext> contexts, bool readOnly)
        {
            _contexts = contexts.ToDictionary(x => x.DomainScope);
            _threadId = Thread.CurrentThread.ManagedThreadId;
            _taskId = Task.CurrentId;
            _isReadOnly = readOnly;
        }

        public UnitOfWork(Func<DomainScope, Guid, IObjectContext> createContextFactoryMethod, bool readOnly)
        {
            _createContextFactoryMethod = createContextFactoryMethod;
            _threadId = Thread.CurrentThread.ManagedThreadId;
            _taskId = Task.CurrentId;
            _isReadOnly = readOnly;
        }

        /// <summary>
        /// Unique identifier of the UnitOfWork.
        /// </summary>
        public Guid Guid
        {
            get
            {
                return _unitOfWorkIdentifier;
            }
        }

        /// <summary>
        /// Gets the ThreadId of the Thread that the UnitOfWork corresponds to.
        /// </summary>
        public int ThreadId
        {
            get
            {
                return _threadId;
            }
        }

        /// <summary>
        /// Gets the TaskId of the Task that the UnitOfWork corresponds to.
        /// </summary>
        public int? TaskId
        {
            get
            {
                return _taskId;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is in an Ambient transaction.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is in an Ambient transaction; otherwise, <c>false</c>.
        /// </value>
        public bool IsInAmbientTransaction
        {
            get
            {
                return Transaction.Current != null && Transaction.Current.TransactionInformation != null
                    && Transaction.Current.TransactionInformation.Status == TransactionStatus.Active;
            }
        }

        /// <summary>
        /// Gets the current transaction.
        /// </summary>
        private Transaction CurrentAmbientTransaction
        {
            get
            {
                return Transaction.Current;
            }
        }

        /// <summary>Gets the db context.</summary>
        /// <value>The db context.</value>
        public ICollection<IObjectContext> Contexts
        {
            get { return _contexts.Values; }
        }

        #region LocalTransaction support

        /// <summary>
        /// Starts a .NET Local Data source Transaction (different from an TransactionScope ambient Transaction)
        /// A Local transaction can only be binded to one database, for a distributed/cross transactions use TransactionScope before declaring UnitOfWork.
        /// </summary>
        /// <param name="isolationLevel"></param>
        /// <param name="domainScope">A specific domain scope to bind the transaction to. A Local transaction can only be binded to one database</param>
        /// <returns></returns>
        public ITransaction BeginTransaction(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted,
                                                         DomainScope domainScope = null)
        {
            IObjectContext context = null;

            if (domainScope != null)
            {
                if (this._contexts.ContainsKey(domainScope))
                    context = this._contexts[domainScope];
                else context = this.GetContext(domainScope);
            }
            else
            {
                var keyPair = _contexts.FirstOrDefault();
                if (keyPair.Key != null)
                {
                    context = keyPair.Value;
                }
                else context = this.GetContext(DomainScopes.GetAll().First());
            }

            return new LocalDbTransaction(context.Context.Database.BeginTransaction(isolationLevel));
        }

        /// <summary>
        /// Commits a .NET data source local Transaction.
        /// </summary>
        /// <param name="transaction"></param>
        public void CommitTransaction(ITransaction transaction)
        {
            if (transaction != null)
                transaction.Commit();
        }

        /// <summary>
        /// Rollback of the .NET data source local transaction.
        /// </summary>
        /// <param name="transaction"></param>
        public void RollbackTransaction(ITransaction transaction)
        {
            transaction.Rollback();
        }

        #endregion LocalTransaction support

        /// <summary>
        /// Finds the database context wrapper for the given DomainScope.
        /// </summary>
        /// <param name="domainScope"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IObjectContext GetContext(DomainScope domainScope)
        {
            IObjectContext c = null;
            if (!this._contexts.TryGetValue(domainScope, out c))
            {
                if (_createContextFactoryMethod == null)
                    throw new NotSupportedException(string.Format("The unit of work wasn't created with the purpose of providing access to the database of scope \"{0}\"", domainScope.Name));

                c = _createContextFactoryMethod(domainScope, this._unitOfWorkIdentifier);
                this._contexts.Add(domainScope, c);
            }
            return c;
        }

        /// <summary>
        /// Finds the database context wrapper for the given DomainScope.
        /// </summary>
        /// <param name="domainScope"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IDbConnection GetSqlContext(string connectionString)
        {
            IDbConnection c = null;
            if (!this._sqlContexts.TryGetValue(connectionString, out c))
            {
                c = new SqlConnection(ConfigurationManager.ConnectionStrings[connectionString].ConnectionString);
                this._sqlContexts.Add(connectionString, c);
            }
            return c;
        }

        public IEnumerable<Task<int>> SaveAsync()
        {
            if (IsReadOnly)
                throw new InvalidOperationException("UnitOfWork is ReadOnly");

            var asyncTasks = new List<Task<int>>();

            foreach (var objectContext in _contexts)
            {
                if (objectContext.Value.Context.ChangeTracker.HasChanges())
                {
                    var task = objectContext.Value.Context.SaveChangesAsync();
                    asyncTasks.Add(task);
                }
            }

            return asyncTasks;
        }

        public int Save(int? timeoutSeconds = null)
        {
            if (IsReadOnly)
                throw new InvalidOperationException("UnitOfWork is ReadOnly");

            int numberOfAffectedRows = 0;

            if (this.IsDisposed)
                throw new InvalidOperationException("The UnitOfWork has been disposed previously!");

            foreach (var context in _contexts)
            {
                try
                {
                    var dbContext = context.Value.Context;

                    if (dbContext.ChangeTracker.HasChanges())
                    {
                        if (timeoutSeconds.HasValue)
                            dbContext.Database.CommandTimeout = timeoutSeconds;

                        numberOfAffectedRows += dbContext.SaveChanges();
                    }
                }
                catch (DbEntityValidationException ex)
                {
                    //LOG this:

                    throw ex;
                }
            }
            return numberOfAffectedRows;
        }

        /// <summary>
        /// Discard all modifications from all contexts in unitof work
        /// </summary>
        public void DiscardChanges()
        {
            if (this.IsDisposed)
                throw new InvalidOperationException("The UnitOfWork has been disposed previously!");

            foreach (var context in _contexts)
            {
                try
                {
                    var dbContext = context.Value.Context;
                    if (dbContext.ChangeTracker.HasChanges())
                    {
                        var changedEntries = dbContext.ChangeTracker.Entries().Where(x => x.State != System.Data.Entity.EntityState.Unchanged).ToList();
                        var deletedEntries = changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Deleted);
                        var addedEntries = changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Added);
                        var modifedEntries = changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Modified);

                        foreach (var entry in deletedEntries)
                        {
                            entry.State = System.Data.Entity.EntityState.Unchanged;
                        }

                        foreach (var entry in addedEntries)
                        {
                            entry.State = System.Data.Entity.EntityState.Detached;
                        }

                        foreach (var entry in modifedEntries)
                        {
                            entry.CurrentValues.SetValues(entry.OriginalValues);
                            entry.State = System.Data.Entity.EntityState.Unchanged;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExceptionDispatchInfo.Capture(ex).Throw();
                }
            }
        }

        //public void AcceptAllChanges()
        //{
        //    foreach (var context in Contexts.Distinct())
        //    {
        //        try
        //        {
        //            var dbContext = (context.Context as System.Data.Entity.Infrastructure.IObjectContextAdapter).ObjectContext;
        //            if (context.Context.ChangeTracker.HasChanges())
        //            {
        //                dbContext.AcceptAllChanges();

        //            }
        //        }
        //        catch (DbEntityValidationException ex)
        //        {
        //            //LOG this:
        //            throw ex;
        //        }
        //    }

        //}

        #region Implementation of IDisposable
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;

        /// <summary>
        /// Disposes off the managed and unmanaged resources used.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (ThreadId != Thread.CurrentThread.ManagedThreadId || TaskId != Task.CurrentId)
                        throw new InvalidOperationException("UnitOfWork cannot be disposed in a different Thread/Task then the one where it was created!");

                    var onDispose = OnDispose;
                    if (onDispose != null)
                        onDispose(this, new EventArgs());

                    foreach (var context in _contexts)
                    {
                        if (context.Value != null)
                        {
                            var connection = context.Value.Context.Database.Connection;
                            if (connection != null && connection.State != ConnectionState.Closed)
                                connection.Close();

                            context.Value.Dispose();
                        }
                    }

                    foreach (var sqlContext in _sqlContexts)
                    {
                        var connection = sqlContext.Value;

                        if (connection != null && connection.State != ConnectionState.Closed)
                        {
                            connection.Close();
                            connection.Dispose();
                        }
                    }

                    _contexts.Clear();
                    _contexts = null;

                    _sqlContexts.Clear();
                    _sqlContexts = null;

                    _createContextFactoryMethod = null;
                    OnDispose = null;
                }
                disposed = true;
            }
        }

        private void ObjectContextAdapter_OnDispose(IObjectContext adapter)
        {
            throw new InvalidOperationException("The Context cannot be disposed while it's being used by another Unit Of Work");
        }

        public bool IsDisposed
        {
            get
            {
                return disposed;
            }
        }

        #endregion Implementation of IDisposable
    }
}
