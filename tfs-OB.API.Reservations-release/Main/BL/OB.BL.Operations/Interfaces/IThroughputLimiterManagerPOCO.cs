using OB.BL.Operations.Internal.BusinessObjects;

namespace OB.BL.Operations.Interfaces
{
    /// <summary>
    /// Class that manages throughput limits for specific resources and provides classes that are maintained across the entire lifecycle of the Application, 
    /// that manages those resources (through the use of distributed Semaphores).
    /// </summary>
    public interface IThroughputLimiterManagerPOCO : IBusinessPOCOBase
    {
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
        ThroughputLimiter GetThroughputLimiter(string uniqueThroughputLimiterKey, int initialCount, int maxCount);

        
    }
}
