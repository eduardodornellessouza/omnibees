using OB.DL.Common.Infrastructure;
using OB.Log;
using System.Data;

namespace OB.DL.Common.Impl
{
    internal class SqlRepository : ISqlRepository
    {
        protected IDbConnection _context;

        public SqlRepository(IDbConnection context, ICacheProvider cacheProvider)
        {
            _context = context;
            CacheProvider = cacheProvider;
        }

        public SqlRepository(IDbConnection context)
            : this(context, null)
        {
        }

        internal IDbConnection Connection
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
    }
}
