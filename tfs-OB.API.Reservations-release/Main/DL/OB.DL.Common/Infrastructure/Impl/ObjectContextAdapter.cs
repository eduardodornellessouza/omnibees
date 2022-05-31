using OB.DL.Common.Interfaces;
using OB.Domain;
using System;
using System.Data.Entity;
using System.Runtime.CompilerServices;

namespace OB.DL.Common.Impl
{
    /// <summary>   Object context adapter. </summary>
    /// <seealso cref="IObjectContext"/>
    internal class ObjectContextAdapter : IObjectContext
    {
        private readonly DbContext _context;
        private readonly DomainScope _domainScope;
        private Guid _unitOfWorkGuid;

        internal delegate void DisposeHandler(IObjectContext adapter);

        internal event DisposeHandler OnDispose;

        /// <summary>   Constructor. </summary>
        /// <param name="context">  The context. </param>
        public ObjectContextAdapter(DbContext context, DomainScope scope, Guid unitOfWorkGuid)
        {
            _context = context;
            _domainScope = scope;
            _unitOfWorkGuid = unitOfWorkGuid;
        }

        /// <summary>
        /// Gets the Guid of the associated UnitOfWork.
        /// </summary>
        public Guid UnitOfWorkGuid
        {
            get
            {
                return _unitOfWorkGuid;
            }
        }

        /// <summary>Gets the context.</summary>
        /// <value>The context.</value>
        public DbContext Context
        {
            get { return _context; }
        }

        public DomainScope DomainScope
        {
            get { return _domainScope; }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources. </summary>
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
                    if (OnDispose != null)
                    {
                        OnDispose.Invoke(this);
                    }

                    _context.Dispose();
                }
                disposed = true;
            }
        }

        /// <summary>
        /// Gets or sets the flag if the ObjectContextAdapter has been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                return disposed;
            }
        }

        /// <summary>   Creates the object set. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <returns>   The new object set&lt; t&gt; </returns>
        /// <seealso cref="ConfigurationManager.DL.Interfaces.IObjectContext.CreateObjectSet<T>()"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDbSet<T> CreateObjectSet<T>() where T : class
        {
            return Context.Set<T>();
        }

        /// <summary>   Saves the changes. </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SaveChanges()
        {
            return Context.SaveChanges();
        }
    }

    //public class ObjectContext
    //{
    //    internal ObjectContext(DbContext context)
    //    {
    //        this.context = context;
    //    }

    //    internal DbContext context {get; private set;}

    //    public T Find<T>(params object[] keyValues) where T : class
    //    {
    //        return context.Set<T>().Find(keyValues);
    //    }

    //    public T Add<T>(T entity) where T : class
    //    {
    //        return context.Set<T>().Add(entity);
    //    }

    //    public IEnumerable<T> AddRange<T>(IEnumerable<T> entities) where T : class
    //    {
    //        return context.Set<T>().AddRange(entities);
    //    }

    //    public T Remove<T>(T entity) where T : class
    //    {
    //        return context.Set<T>().Remove(entity);
    //    }

    //    public IEnumerable<T> RemoveRange<T>(IEnumerable<T> entities) where T : class
    //    {
    //        return context.Set<T>().RemoveRange(entities);
    //    }

    //    public IQueryable<T> GetAll<T>() where T : class
    //    {
    //        return context.Set<T>();
    //    }

    //}
}
