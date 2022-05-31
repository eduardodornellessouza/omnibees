using Microsoft.Practices.Unity;
using OB.BL.Operations.Extensions;
using OB.BL.Operations.Interfaces;
using OB.DL.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using OB.DL.Common;
using OB.Log;
using OB.Log.Messages;
using OB.Api.Core;

namespace OB.BL.Operations
{
    public abstract class BusinessPOCOBase : IBusinessPOCOBase
    {
        public BusinessPOCOBase()
        {

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
        /// Used for logging on LogEntries
        /// </summary>
        private ILogger _logEntrieslogger;
        /// <summary>
        /// Used for logging on LogEntries
        /// </summary>
        public ILogger LogEntriesLogger
        {
            get
            {
                if (_logEntrieslogger == null)
                    _logEntrieslogger = LogsManager.CreateLogger(this.GetType().ToString() + ".LogEntries");
                return _logEntrieslogger;
            }
        }
        
        /// <summary>
        /// Injected by IoC
        /// </summary>
        [Dependency]
        public IUnityContainer Container { get; set; }

        /// <summary>
        /// Injected by IoC
        /// </summary>
        [Dependency]
        public IRepositoryFactory RepositoryFactory { get; set; }

        /// <summary>
        /// Injected by IoC
        /// </summary>
        [Dependency]
        public ISessionFactory SessionFactory { get; set; }

        /// <summary>
        /// Injected by IoC
        /// </summary>
        [Dependency]
        public ITransactionManager TransactionManager { get; set; }


        [Dependency]
        public IRegisteredTasksManager RegisteredTasksManager { get; set; }


        private static readonly TaskFactory _taskFactory = new TaskFactory(
            CancellationToken.None,
            TaskCreationOptions.DenyChildAttach,
            TaskContinuationOptions.DenyChildAttach,
            // NOTE: Consider using a task scheduler with a maximum concurrency level
            //       or a task scheduler with its own set of threads
            //       if sharing the global ThreadPool is still a source of problems.
            TaskScheduler.Default);

        private static readonly Task _completedTask = Task.FromResult<object>(null) /* .NET 4.6: Task.CompletedTask */;

        private Task _backgroundTask = null;


        #region Methods

        /// <summary>
        /// Gets the implementation of type T from the IoC container.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Resolve<T>()
        {
            return this.Container.Resolve<T>();
        }

        /// <summary>
        /// Gets the implementation of type T registered with the given name, from the IoC container.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public T Resolve<T>(string name)
        {
            return this.Container.Resolve<T>(name);
        }

        public static void SaveAllUnitsOfWork(params IUnitOfWork[] units)
        {
            foreach (var unit in units)
            {
                unit.Save();
            }
        }

        protected Task StartConcurrentWork(Action action)
        {
            Task task = _taskFactory.StartNew(action);
            RegisteredTasksManager.RegisterTask(task);
            return task;
        }

        protected Task ContinueConcurrentWork(Task previousTask, Action<Task> action)
        {
            Task task = _taskFactory.ContinueWhen(previousTask, action);
            RegisteredTasksManager.RegisterTask(task);
            return task;
        }

        protected void QueueBackgroundWork(Action action, bool disposeUnitOfWork = true)
        {
            if (SessionFactory.CurrentUnitOfWork?.IsDisposed == false)
                Logger.Error(new LogMessageBase
                {
                    ErrorCode = ((int)Internal.BusinessObjects.Errors.Errors.LiveUnitOfWorkOnBackgroundThread).ToString(), 
                    AppName = Configuration.AppName,
                    MethodName = action?.Method?.Name,
                    Description = EnumExtensions.GetDisplayAttribute(Internal.BusinessObjects.Errors.Errors.LiveUnitOfWorkOnBackgroundThread)
                });

            Action<object> taskAction = previousTask =>
            {
                try
                {
                    action();
                }
                finally
                {
                    if (disposeUnitOfWork)
                    {
                        var uow = SessionFactory.GetUnitOfWork();
                        if (!uow.IsDisposed)
                        {
                            uow.Dispose();
                        }
                    }
                }
            };
            if (_backgroundTask == null)
            {
                _backgroundTask = _taskFactory.StartNew(taskAction, _completedTask);
            }
            else
            {
                _backgroundTask = _taskFactory.ContinueWhen(_backgroundTask, taskAction);
            }
            RegisteredTasksManager.RegisterTask(_backgroundTask);
        }

        protected virtual void QueueBackgroundWork(Func<Task> asyncAction)
        {
            if (SessionFactory.CurrentUnitOfWork?.IsDisposed == false)
                Logger.Error(new LogMessageBase
                {
                    ErrorCode = ((int)Internal.BusinessObjects.Errors.Errors.LiveUnitOfWorkOnBackgroundThread).ToString(),
                    AppName = Configuration.AppName,
                    MethodName = asyncAction?.Method?.Name,
                    Description = EnumExtensions.GetDisplayAttribute(Internal.BusinessObjects.Errors.Errors.LiveUnitOfWorkOnBackgroundThread)
                });

            Func<object, Task> asyncTaskAction = async previousTask =>
            {
                try
                {
                    await asyncAction();
                }
                finally
                {
                    var uow = SessionFactory.GetUnitOfWork();
                    if (!uow.IsDisposed)
                    {
                        uow.Dispose();
                    }
                }
            };
            if (_backgroundTask == null)
            {
                _backgroundTask = _taskFactory.StartNew(asyncTaskAction, _completedTask)
                    .Unwrap();
            }
            else
            {
                _backgroundTask = _taskFactory.ContinueWhen(_backgroundTask, asyncTaskAction)
                    .Unwrap();
            }
            RegisteredTasksManager.RegisterTask(_backgroundTask);
        }

        public void WaitForAllBackgroundWorkers()
        {
            var task = _backgroundTask;
            while (task != null && !(task.IsCanceled || task.IsCompleted || task.IsFaulted))
            {
                try
                {
                    task.GetAwaiter().GetResult();
                }
                catch
                {
                }
                task = _backgroundTask;
            }
        }


        #endregion Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
