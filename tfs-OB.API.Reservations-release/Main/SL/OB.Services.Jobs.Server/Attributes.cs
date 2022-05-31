using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Dashboard;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using Microsoft.Owin;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.Services.Jobs.Server
{
    /// <summary>
    /// HangFire Authorization
    /// </summary>
    public class MyRestrictiveAuthorizationFilter : IAuthorizationFilter
    {
        public bool Authorize(IDictionary<string, object> owinEnvironment)
        {
            // In case you need an OWIN context, use the next line,
            // `OwinContext` class is the part of the `Microsoft.Owin` package.
            var context = new OwinContext(owinEnvironment);

            // Allow all authenticated users to see the Dashboard (potentially dangerous).
            return context.Authentication.User.IsInRole("Admin");
            //return context.Authentication.User.Identity.IsAuthenticated;
        }
    }

    /// <summary>
    /// Timespan to delete Succeeded or Deleted Jobs from Database
    /// </summary>
    public class ProlongExpirationTimeAttribute : JobFilterAttribute, IApplyStateFilter
    {
        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            context.JobExpirationTimeout = TimeSpan.FromDays(10);
        }


        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            context.JobExpirationTimeout = TimeSpan.FromDays(10);
        }
    }

    /// <summary>
    /// Attribute to log every stage og jobs
    /// </summary>
    public class LogEverythingAttribute : JobFilterAttribute,
        IClientFilter, IServerFilter, IElectStateFilter, IApplyStateFilter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void OnCreating(CreatingContext filterContext)
        {
            Logger.Info(
                "Creating a job based on method `{0}`...",
                filterContext.Job.Method.Name);
        }

        public void OnCreated(CreatedContext filterContext)
        {
            Logger.Info(
                "Job that is based on method `{0}` has been created with id `{1}`",
                filterContext.Job.Method.Name,
                filterContext.JobId);
        }

        public void OnPerforming(PerformingContext filterContext)
        {
            Logger.Info(
                "Starting to perform job `{0}`",
                filterContext.JobId);
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            Logger.Info(
                "Job `{0}` has been performed",
                filterContext.JobId);
        }

        public void OnStateElection(ElectStateContext context)
        {
            var failedState = context.CandidateState as FailedState;
            if (failedState != null)
            {
                Logger.Warn(
                    "Job `{0}` has been failed due to exception `{1}` but will be retried automatically until retry attempts exceeded",
                    context.JobId,
                    failedState.Exception);
            }
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            Logger.Info(
                "Job `{0}` state was changed from `{1}` to `{2}`",
                context.JobId,
                context.OldStateName,
                context.NewState.Name);
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            Logger.Info(
                "Job `{0}` state `{1}` was unapplied.",
                context.JobId,
                context.OldStateName);
        }
    }
}
