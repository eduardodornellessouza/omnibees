using System;
using System.Threading;
using System.Threading.Tasks;

namespace OB.DL.Common.Infrastructure
{
    public static class MultithreadExtensions
    {
        public static IDisposable StartPeriodic<T>(this Func<T> func, TimeSpan startAfter, TimeSpan runEvery)
        {
            Timer timer = null;

            timer = new Timer(obj =>
            {
                func();
                timer.Change(runEvery, Timeout.InfiniteTimeSpan);
            }
            , null, startAfter, Timeout.InfiniteTimeSpan);

            return timer;
        }
    }
}
