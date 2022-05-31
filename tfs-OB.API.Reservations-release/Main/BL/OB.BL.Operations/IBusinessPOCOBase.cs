using System;

namespace OB.BL.Operations
{
    public interface IBusinessPOCOBase : IDisposable
    {     
        /// <summary>
        /// Waits until all the background workers finish execution.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait per task/thread.</param>
        void WaitForAllBackgroundWorkers();

        T Resolve<T>();

    }
}
