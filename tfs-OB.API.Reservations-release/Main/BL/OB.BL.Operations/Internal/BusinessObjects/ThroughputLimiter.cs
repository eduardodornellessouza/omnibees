using System;
using System.Threading;

namespace OB.BL.Operations.Internal.BusinessObjects
{
    public class ThroughputLimiter : IDisposable
    {
        private readonly SemaphoreSlim _innerSemaphore;
        private readonly string _uniqueKey;

        private ThroughputLimiter()
        {

        }

        internal ThroughputLimiter(string uniqueKey, int initialCount, int maxCount)
        {
            _uniqueKey = uniqueKey;
            _innerSemaphore = new SemaphoreSlim(initialCount, maxCount);
        }

        public void Wait()
        {
            _innerSemaphore.Wait();
        }


        public bool Wait(int milliseconds)
        {
            return _innerSemaphore.Wait(milliseconds);
        }

        public bool Wait(TimeSpan timeSpan)
        {
            return _innerSemaphore.Wait(timeSpan);
        }

        public int Release()
        {
            return _innerSemaphore.Release();
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
                    _innerSemaphore.Dispose();
                }
                disposed = true;
            }
        }
    }
}
