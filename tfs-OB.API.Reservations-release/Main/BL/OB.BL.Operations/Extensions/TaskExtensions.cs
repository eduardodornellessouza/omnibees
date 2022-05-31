using System;
using System.Threading.Tasks;

namespace OB.BL.Operations.Extensions
{
    public static class TaskExtensions
    {
        public static Task ContinueWhen(this TaskFactory taskFactory, Task task, Action<Task> continuationAction)
        {
            return task.ContinueWith(continuationAction, taskFactory.CancellationToken, taskFactory.ContinuationOptions, taskFactory.Scheduler);
        }

        public static Task<TResult> ContinueWhen<TResult>(this TaskFactory taskFactory, Task task, Func<Task, TResult> continuationFunction)
        {
            return task.ContinueWith(continuationFunction, taskFactory.CancellationToken, taskFactory.ContinuationOptions, taskFactory.Scheduler);
        }
    }
}
