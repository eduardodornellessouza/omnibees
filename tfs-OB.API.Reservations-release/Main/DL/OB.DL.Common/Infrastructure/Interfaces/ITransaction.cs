using System;
using System.Data;

namespace OB.DL.Common.Interfaces
{
    /// <summary>
    /// Interface that exposes the local transaction Commit and Rollback operations.
    /// This interface allows the abstraction of the local transaction (it can be entity framework transaction or ADO.NET or even another type of database transaction).
    /// </summary>
    public interface ITransaction : IDbTransaction
    {
        //
        // Summary:
        //     Specifies the System.Data.IsolationLevel for this transaction.
        //
        // Returns:
        //     The System.Data.IsolationLevel for this transaction. The default is ReadCommitted.
        IsolationLevel IsolationLevel { get; }

        // Summary:
        //     Commits the database transaction.
        //
        // Exceptions:
        //   System.Exception:
        //     An error occurred while trying to commit the transaction.
        //
        //   System.InvalidOperationException:
        //     The transaction has already been committed or rolled back.-or- The connection
        //     is broken.
        void Commit();

        //
        // Summary:
        //     Rolls back a transaction from a pending state.
        //
        // Exceptions:
        //   System.Exception:
        //     An error occurred while trying to commit the transaction.
        //
        //   System.InvalidOperationException:
        //     The transaction has already been committed or rolled back.-or- The connection
        //     is broken.
        void Rollback();
    }
}