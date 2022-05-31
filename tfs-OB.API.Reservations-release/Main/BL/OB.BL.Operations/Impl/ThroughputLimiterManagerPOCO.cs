using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Internal.BusinessObjects;
using System.Collections.Concurrent;

namespace OB.BL.Operations.Impl
{
    /// <summary>
    /// Class that manages throughput limits for specific resources and provides classes that are maintained across the entire lifecycle of the Application, 
    /// that manages those resources (through the use of distributed Semaphores).
    /// </summary>
    public class ThroughputLimiterManagerPOCO : BusinessPOCOBase, IThroughputLimiterManagerPOCO
    {
        private ConcurrentDictionary<string, ThroughputLimiter> _throughputLimiterRegistry = new ConcurrentDictionary<string, ThroughputLimiter>();

        /// <summary>
        /// Gets the unique ThrougputLimiter instance given its key.
        /// If there is an existing ThroughputLimiter for the given key it returns that instance otherwise a new instance of
        /// ThroughputLimiter class is created with initial resource count of initialCount and maxCount of maxCount.
        /// </summary>
        /// <param name="uniqueThroughputLimiterKey"></param>
        /// <param name="initialCount"></param>
        /// <param name="maxCount"></param>
        /// <see cref="System.Threading.SemaphoreSlim"/>
        /// <returns></returns>
        public Internal.BusinessObjects.ThroughputLimiter GetThroughputLimiter(string uniqueThroughputLimiterKey, int initialCount, int maxCount)
        {
            ThroughputLimiter value = _throughputLimiterRegistry.GetOrAdd(uniqueThroughputLimiterKey, (key) =>
            {
                value = new ThroughputLimiter(key, initialCount, maxCount);
                return value;
            });
            return value;
        }

        private bool disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                Logger.Info("WARNING ThroughputLimiterManagerPOCO has been DISPOSED!!! Semaphore locks will be unuseable after this point");
                disposed = true;
                base.Dispose(disposing);
            }
        }
    }
}
