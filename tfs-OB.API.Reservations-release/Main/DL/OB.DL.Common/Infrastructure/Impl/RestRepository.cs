using OB.DL.Common.Infrastructure;
using OB.Log;
using System.Data.Entity;
using OB.Api.Core;
using OB.DL.Common.Repositories.Interfaces.Rest;

namespace OB.DL.Common.Impl
{
    internal class RestRepository<TEntity> : IRestRepository<TEntity> where TEntity : class
    {
        protected IObjectContext _context;
        protected DbSet<TEntity> _objectSet;


        public RestRepository(ICacheProvider cacheProvider)
        {
            CacheProvider = cacheProvider;
        }

        public RestRepository()
        {
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

        /// <summary>
        /// Cache implementation.
        /// </summary>
        protected virtual ICacheProvider CacheProvider
        {
            get;
            private set;
        }
        #region AsyncMethods

        #endregion AsyncMethods
    }
}