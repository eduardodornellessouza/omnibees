using OB.Domain;
using System;
using System.Data.Entity;

namespace OB.DL.Common.Interfaces
{
    /// <summary>
    ///
    /// </summary>
    internal interface IObjectContext : IDisposable
    {
        /// <summary>Creates the object set.</summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IDbSet<T> CreateObjectSet<T>() where T : class;

        /// <summary>Gets the context.</summary>
        /// <value>The context.</value>
        DbContext Context { get; }

        DomainScope DomainScope { get; }

        /// <summary>
        /// Gets the Guid of the associated UnitOfWork.
        /// </summary>
        Guid UnitOfWorkGuid { get; }

        bool IsDisposed { get; }

        /// <summary>Saves the changes.</summary>
        int SaveChanges();
    }
}