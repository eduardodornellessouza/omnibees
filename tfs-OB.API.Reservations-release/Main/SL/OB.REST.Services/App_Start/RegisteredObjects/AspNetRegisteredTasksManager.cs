using OB.BL.Operations.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace OB.REST.Services.RegisteredObjects
{
    public class AspNetRegisteredTasksManager : IRegisteredObject, IRegisteredTasksManager
    {
        private readonly CountdownEvent countdown;
        private int stopped;

        public AspNetRegisteredTasksManager()
        {
            this.countdown = new CountdownEvent(1);
            this.stopped = 0;

            HostingEnvironment.RegisterObject(this);
        }

        internal void SignalCountdown()
        {
            if (countdown.Signal())
            {
                HostingEnvironment.UnregisterObject(this);
            }
        }

        void IRegisteredTasksManager.RegisterTask(Task task)
        {
            countdown.AddCount();
            task.ContinueWith(t =>
                {
                    SignalCountdown();
                }, CancellationToken.None, TaskContinuationOptions.DenyChildAttach | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        void IRegisteredObject.Stop(bool immediate)
        {
            if (Interlocked.CompareExchange(ref stopped, 1, 0) == 0)
            {
                SignalCountdown();
            }
            if (immediate)
            {
                countdown.Wait();
            }
        }
    }
}
