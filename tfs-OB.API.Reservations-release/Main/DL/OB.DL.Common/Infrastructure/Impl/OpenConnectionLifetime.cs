using System;
using System.Data;
using System.Data.Common;

namespace OB.DL.Common.Infrastructure.Impl
{
    internal class OpenConnectionLifetime : IDisposable
    {
        private readonly DbConnection connection;
        private readonly bool closeOnDispose;

        internal OpenConnectionLifetime(DbConnection connection)
        {
            this.connection = connection;
            this.closeOnDispose = (connection.State == ConnectionState.Closed);
            if (this.closeOnDispose)
            {
                this.connection.Open();
            }
        }

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
                    if (closeOnDispose && connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
                disposed = true;
            }
        }
    }
}
