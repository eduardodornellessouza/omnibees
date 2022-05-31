using OB.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OB.DL.Common.Interfaces
{
    /// <summary>
    /// UnitOfWork class. Adds support for transactions and holds a reference to the currently used database contexts.
    /// Only one UnitOfWork instance can be created (exists) per thread.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        event EventHandler OnDispose;

        /// <summary>
        /// Unique identifier of the UnitOfWork.
        /// </summary>
        Guid Guid { get; }

        /// <summary>
        /// Gets if unit of work is readonly
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// Gets the ThreadId of the Thread that the UnitOfWork corresponds to.
        /// </summary>
        int ThreadId { get; }

        /// <summary>
        /// Gets the TaskId of the Task that the UnitOfWork corresponds to.
        /// </summary>
        int? TaskId { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is in transaction.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is in transaction; otherwise, <c>false</c>.
        /// </value>
        bool IsInAmbientTransaction { get; }

        /// <summary>Saves the changes.</summary>
        int Save(int? timeoutInSeconds = null);

        // void AcceptAllChanges();

        /// <summary>Saves the changes in an asynchrounous was.</summary>
        IEnumerable<Task<int>> SaveAsync();

        bool IsDisposed { get; }

        #region Transaction Support methods

        /// <summary>
        /// Starts a .NET local Data source Transaction (different from an TransactionScope ambient Transaction)
        /// A local transaction can only be binded to one database, for a distributed/cross transactions use TransactionScope before declaring UnitOfWork.
        /// </summary>
        /// <param name="isolationLevel"></param>
        /// <param name="domainScope">A specific domain scope to bind the transaction to. A Local transaction can only be binded to one database</param>
        /// <returns></returns>
        ITransaction BeginTransaction(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted,
                                                         DomainScope domainScope = null);

        /// <summary>
        /// Commits a .NET data source local transaction.
        /// </summary>
        /// <param name="transaction"></param>
        void CommitTransaction(ITransaction transaction);

        /// <summary>
        /// Rollback of the .NET data source local transaction.
        /// </summary>
        /// <param name="transaction"></param>
        void RollbackTransaction(ITransaction transaction);

        #endregion Transaction Support methods

        /// <summary>
        /// Discard all modifications from all contexts in unitof work
        /// </summary>
        void DiscardChanges();
    }
}