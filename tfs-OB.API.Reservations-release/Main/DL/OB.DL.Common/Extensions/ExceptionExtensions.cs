using Couchbase;
using OB.Domain;
using System;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Linq;
using System.Data.Entity.Validation;
using OB.Log;
using System.Diagnostics.Contracts;
using System.Text;
using System.Data.SqlClient;

namespace System
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Checks if the Exception is a Data Access Layer exception, e.g. an ORM (Entity Framework) exception or not.
        /// It searches for the inner exception recursively.
        /// It searches for the InnerExceptions if the exception is an AggregateException.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static bool IsDataLayerException(this Exception ex)
        {
            if(ex is AggregateException)
            {
                return ((AggregateException)ex).InnerExceptions.Any(x => x.IsDataLayerException());
            }            
            return ex is DbUpdateException || ex is DbUpdateConcurrencyException || ex is EntityException
                || (ex.InnerException != null && ex.InnerException.IsDataLayerException());
        }

        /// <summary>
        /// Checks if the Exception is a Data Access Layer UPDATE Exception, e.g. an ORM (Entity Framework) update exception or not.
        /// It searches for the inner exception recursively.
        /// It searches for the InnerExceptions if the exception is an AggregateException.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static bool IsDataLayerUpdateException(this Exception ex)
        {
            if (ex is AggregateException)
            {
                return ((AggregateException)ex).InnerExceptions.Any(x => x.IsDataLayerUpdateException());
            }
            return ex is DbUpdateException || ex is DbUpdateConcurrencyException
                || (ex.InnerException != null && ex.InnerException.IsDataLayerUpdateException());
        }

        public static bool IsSqlException(this Exception ex)
        {
            if (ex is AggregateException)
            {
                return ((AggregateException)ex).InnerExceptions.Any(x => x.IsSqlException());
            }
            return ex is EntitySqlException || ex is System.Data.SqlClient.SqlException
                || (ex.InnerException != null && ex.InnerException.IsSqlException());
        }

        /// <summary>
        /// Checks if the Exception is a Data Access Layer VALIDATION Exception, e.g. an ORM (Entity Framework) validation exception or not.
        /// It searches for the inner exception recursively.
        /// It searches for the InnerExceptions if the exception is an AggregateException.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static bool IsDataLayerValidationException(this Exception ex)
        {
            if (ex is AggregateException)
            {
                return ((AggregateException)ex).InnerExceptions.Any(x => x.IsDataLayerValidationException());
            }
            if (ex is SqlException)
            {
                if (((SqlException)ex).InnerException != null)
                    return ((SqlException)ex).InnerException.IsDataLayerValidationException();
            }
            if (ex != null && ex.InnerException == null)
            {
                return ex is DbEntityValidationException;
            }

            return ex is DbEntityValidationException || IsDataLayerValidationException(ex.InnerException);
        }

        /// <summary>
        /// Logs the Validation errors if the Exception is a Data Access Layer VALIDATION Exception, e.g. an ORM(Entity Framework) validation exception.
        /// It searches for the inner exception recurively.
        /// It searches for the inner exceptions if the exception is an AggregateException.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="logger"></param>
        public static void LogDataLayerValidationErrors(this Exception exception, ILogger logger)
        {
            Contract.Requires(logger != null);
            
            DbEntityValidationException ex = null;

            if (exception == null)
                return;

            if (exception is AggregateException)
            {
                var innerExceptions = ((AggregateException)exception).InnerExceptions;
                foreach(var innerException in innerExceptions)
                {
                    LogDataLayerValidationErrors(innerException, logger);
                }
                return;
            }

            if (exception is SqlException)
            {
                var innerException = ((SqlException)exception).InnerException;
                LogDataLayerValidationErrors(innerException, logger);
            }

            if (!(exception is DbEntityValidationException) && exception.InnerException != null)
            {
                LogDataLayerValidationErrors(exception.InnerException, logger);
            }

            ex = exception as DbEntityValidationException;

            if(ex != null)
            {
                StringBuilder error = new StringBuilder("DbEntityValidationException:\t");
                foreach (var item in ex.EntityValidationErrors)
                {
                    foreach (var validationError in item.ValidationErrors)
                    {
                        error.Append(string.Format("PropertyName: {0} - Error Message: {1} \t", validationError.PropertyName, validationError.ErrorMessage));
                    }
                }
                logger.Error(error.ToString());
            }            
        }
    }
}
