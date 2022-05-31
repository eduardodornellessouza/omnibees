using Microsoft.Practices.Unity;
using NLog;
using OB.DL.Common.Exceptions;
using OB.DL.Common.Interfaces;
using OB.Domain;
using OB.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace OB.DL.Common.Impl
{
    internal class SessionFactory : ISessionFactory
    {
        private static readonly string _unitOfWorkThreadKey = "___Thread_UnitOfWork__";

        private static readonly string dataModelAssemblyFormat = "OB.DL.Model.{0}";
        private static readonly string dataModelContextTypeFormat = "OB.DL.Model.{0}.{0}Context";

        private static readonly Dictionary<DomainScope, Type> _typeCache = new Dictionary<DomainScope, Type>();

        /// <summary>
        /// Sync object for the DbContext cache
        /// </summary>
        private static readonly object _dbContextGate = new object();

        private static readonly object _unitOfWorkCacheGate = new object();

        private OB.Log.ILogger _logger = OB.Log.LogsManager.CreateLogger(typeof(SessionFactory));

        /// <summary>
        /// Sync object for the UnitOfWork Thread storage;
        /// </summary>
        private static ConcurrentDictionary<string, WeakReference<IUnitOfWork>> _unitOfWorkStorage = new ConcurrentDictionary<string, WeakReference<IUnitOfWork>>();
     

        private static ConcurrentDictionary<string, Tuple<object, int>> _threadDataStorage = new ConcurrentDictionary<string, Tuple<object, int>>();

        private static readonly Dictionary<string, Tuple<int, string>> _traceSessions = new Dictionary<string, Tuple<int, string>>();
        private static bool isTraceActive = false;

        public SessionFactory()
        {
            //Warm up Type cache
            lock (_typeCache)
            {
                if (!_typeCache.Any())
                {
                    var reservationsDomainScope = DomainScopes.GetAll();

                    foreach (var domainScope in reservationsDomainScope)
                    {
                        GetContextType(domainScope);
                    }
                }
            }
        }

        [Microsoft.Practices.Unity.Dependency]
        public virtual UnityContainer Container
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the UnitOfWork for the Current thread returning one if it already exists or null if it hasn't been created before.
        /// </summary>
        public virtual IUnitOfWork CurrentUnitOfWork
        {
            get
            {
                WeakReference<IUnitOfWork> result = null;
                var storage = GetUnitOfWorkStorage();
                var taskKey = GetCurrentTaskKey();

                IUnitOfWork unitOfWork;
                if (storage.TryGetValue(GetCurrentUnitOfWorkKey(), out result) && result.TryGetTarget(out unitOfWork))
                {
                    return unitOfWork;
                }

                //What is this for ????
                //storage.Remove(_unitOfWorkThreadKey + taskKey);
                return null;
            }
        }

        internal string GetUnitOfWorkStorageKey(IUnitOfWork unitOfWork)
        {
            return _unitOfWorkThreadKey + "_threadId_" + unitOfWork.ThreadId + "_" + (unitOfWork.TaskId.HasValue ? "_TaskId_" + unitOfWork.TaskId.Value.ToString() : string.Empty);
        }

        internal string GetCurrentUnitOfWorkKey()
        {
            return _unitOfWorkThreadKey + "_threadId_" + Thread.CurrentThread.ManagedThreadId + "_" + GetCurrentTaskKey();
        }

        private string GetCurrentTaskKey()
        {
            return Task.CurrentId.HasValue ? "_TaskId_" + Task.CurrentId.Value.ToString() : string.Empty;
        }

        internal string GetUnitOfWorkContextKey(Type contextType, Guid uowGuid)
        {
            return contextType.FullName + "_" + uowGuid.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ConcurrentDictionary<string, Tuple<object, int>> GetThreadDataStorage()
        {
            return _threadDataStorage;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual ConcurrentDictionary<string, WeakReference<IUnitOfWork>> GetUnitOfWorkStorage()
        {
            return _unitOfWorkStorage;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual Type GetContextType(DomainScope scope)
        {
            if (!_typeCache.ContainsKey(scope))
            {
                string assembly = string.Format(dataModelAssemblyFormat, scope.Name);
                string typeFullName = string.Format(dataModelContextTypeFormat, scope.Name);
                var type = Assembly.Load(assembly).GetType(typeFullName);

                _typeCache.Add(scope, type);
            }
            return _typeCache[scope];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int? GetTraceIdForContextInstanceType(Type contextType, Guid uowGuid)
        {
            var contextObjectCache = GetThreadDataStorage();
            string key = GetUnitOfWorkContextKey(contextType, uowGuid);
            if (contextObjectCache.ContainsKey(key))
            {
                return contextObjectCache[key].Item2;
            }
            return default(int);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetTraceSessionKey(DbConnection connection)
        {
            return connection.ConnectionString.GetHashCode().ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetTraceSessionKey(IObjectContext context)
        {
            var connection = context.Context.Database.Connection;
            string db = connection.Database.ToLower();
            string dataSource = connection.DataSource.ToLower();

            return context.Context.Database.Connection.ConnectionString.GetHashCode().ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T GetDynamicContextInstance<T>(Type t, DomainScope scope, Guid uowGuid)
        {         
            var contextObjectCache = GetThreadDataStorage();
            var typeFullName = t.FullName;
            var contextUnitOfWorkKey = GetUnitOfWorkContextKey(t, uowGuid);
            var result = default(Tuple<object, int>);

            //Not necessary since ThreadDataStorage is ThreadStatic
            //Monitor.Enter(contextObjectCache);
            //try
            //{
            if (!contextObjectCache.TryGetValue(contextUnitOfWorkKey, out result))
            {
                var contextObject = Activator.CreateInstance(t);
                var adapter = new ObjectContextAdapter(contextObject as DbContext, scope, uowGuid);

                if (CurrentUnitOfWork.IsReadOnly)
                {
                    var connectionString = ConfigurationManager.ConnectionStrings[scope.ReadOnlyConnectionStringName];

                    if (connectionString == null || string.IsNullOrWhiteSpace(connectionString.ConnectionString))
                        throw new ArgumentNullException("ReadOnlyConnectionStringName is required in DomainScope And in Web.config");

                    var entityBuilder = new EntityConnectionStringBuilder(connectionString.ConnectionString);
                    adapter.Context.Database.Connection.ConnectionString = entityBuilder.ProviderConnectionString;
                }

                int traceId = -1;
                if (isTraceActive)
                {
                    traceId = StartTrace(adapter);
                }

                result = new Tuple<object, int>(adapter, traceId);
                contextObjectCache[contextUnitOfWorkKey] = result;

                //Not necessary because the UnitOfWork.Dispose function already clears the ThreadDataStorage
                // adapter.OnDispose += adapter_OnDispose;
            }
            //}
            //finally
            //{
            //    Monitor.Exit(contextObjectCache);
            //}

            return (T)result.Item1;           
        }
    
        //private void adapter_OnDispose(IObjectContext adapter)
        //{
        //    //lock (_dbContextGate)
        //    //{
        //    var contextTypeFullName = adapter.Context.GetType().FullName;

        //    var contextObjectCache = GetThreadDataStorage();

        //    //Not necessary since ThreadDataStorage is ThreadStatic
        //    Monitor.Enter(contextObjectCache);

        //    try
        //    {

        //        if (contextObjectCache.ContainsKey(contextTypeFullName))
        //        {
        //            if (isTraceActive)
        //            {
        //                //StopTrace(adapter);
        //            }

        //            contextObjectCache.Remove(contextTypeFullName);
        //        }

        //    }
        //    finally
        //    {
        //        Monitor.Exit(contextObjectCache);
        //    }
        //    //}
        //}

        ///// <summary>
        ///// Gets a unit of work.
        ///// </summary>
        ///// <param name="createNew">if true, a NEW unit of work for the same thread will be returned, even if an existing one already exists. This one will manage a set of new EntityFramework contexts.</param>
        ///// <returns></returns>
        //public virtual IUnitOfWork GetUnitOfWork(bool createNew)
        //{
        //    if (createNew)
        //    {
        //        IUnitOfWork unitOfWork = null;

        //        //StackFrame frame = new StackFrame(2);

        //        var unitOfWorkStorage = GetUnitOfWorkStorage();

        //        unitOfWork = this.GetInternalUnitOfWork(new DomainScope[0]);//DomainScopes.GetAll().ToArray());
        //        unitOfWorkStorage.Add(_unitOfWorkThreadKey + unitOfWork.Guid.ToString(), unitOfWork);

        //        unitOfWork.OnDispose += (obj, e) =>
        //        {
        //            //lock (_dbContextGate)
        //            //{
        //            var disposedUow = (IUnitOfWork)obj;
        //            var uowGuid = disposedUow.Guid;

        //            var tStorage = GetThreadDataStorage();
        //            var contexts = ((UnitOfWork)disposedUow).Contexts;
        //            foreach(var context in contexts)
        //            {
        //                tStorage.Remove(context.GetType().FullName + uowGuid);
        //            }

        //            //}
        //            //lock (_unitOfWorkCacheGate)
        //            //{
        //            GetUnitOfWorkStorage().Remove(_unitOfWorkThreadKey);
        //            //}
        //        };

        //        return unitOfWork;
        //    }

        //    return GetUnitOfWork();
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IUnitOfWork GetUnitOfWork(params DomainScope[] domainScopes)
        {
            if(domainScopes != null)
                return this.GetUnitOfWork(false, domainScopes);
            else
                return this.GetUnitOfWork(false, new DomainScope[0]);
        }

        public virtual IUnitOfWork GetUnitOfWork(bool readOnly, params DomainScope[] domainScopes)
        {
            WeakReference<IUnitOfWork> unitOfWorkRef = null;
            IUnitOfWork unitOfWork = null;

            //StackFrame frame = new StackFrame(2);

            var unitOfWorkStorage = GetUnitOfWorkStorage();
            var key = GetCurrentUnitOfWorkKey();

            
            if (!unitOfWorkStorage.TryGetValue(key, out unitOfWorkRef) || !unitOfWorkRef.TryGetTarget(out unitOfWork))
            {
               
                unitOfWork = this.GetInternalUnitOfWork(domainScopes, readOnly);//DomainScopes.GetAll().ToArray());

                if (unitOfWorkRef == null)
                {
                    unitOfWorkRef = new WeakReference<IUnitOfWork>(unitOfWork);
                    unitOfWorkStorage[key] = unitOfWorkRef;
                }
                else
                {
                    unitOfWorkRef.SetTarget(unitOfWork);
                }

                unitOfWork.OnDispose += UnitOfWork_OnDispose;

                //StackFrame frame = new StackFrame(2);
                //Trace.WriteLine("CREATED UNIT OF WORK ({0}) for Parent Method:" + frame.GetMethod().Name, unitOfWork.GetHashCode());
            }
            else
            {
                //TMOREIRA: This log should help in addressing bug #23620 
                if (unitOfWork == null)
                {
                    _logger.Error("GetUnitOfWork: UnitOfWork ref Target is null. UnitOfWork KEY \"" + key + "\". StackTrace:" + new StackFrame().ToString());
                    //return GetUnitOfWork(domainScopes);
                }
            }
                                    
            //Trace.WriteLine("RETURNED UNIT OF WORK ({0}) for Parent Method:" + frame.GetMethod().Name, unitOfWork.GetHashCode());   
            return unitOfWork;
        }

        private void UnitOfWork_OnDispose(object obj, EventArgs e)
        {
            var disposedUow = obj as UnitOfWork;


            var uowGuid = disposedUow.Guid;

            var contexts = disposedUow.Contexts;
            var tStorage = GetThreadDataStorage();

            if (contexts != null)
            {
                foreach (var contextToRemove in contexts.ToList())
                {
                    Tuple<object, int> removedValue;
                    tStorage.TryRemove(GetUnitOfWorkContextKey(contextToRemove.Context.GetType(), uowGuid), out removedValue);
                }
            }


            WeakReference<IUnitOfWork> uowFromStorage;
            GetUnitOfWorkStorage().TryRemove(GetUnitOfWorkStorageKey(disposedUow), out uowFromStorage);
            
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IUnitOfWork GetInternalUnitOfWork(DomainScope[] domainScopes, bool readOnly)
        {
            if (domainScopes == null)//|| domainScopes.Count() == 0)
                throw new ArgumentNullException("DomainScopes can't be null");

            //var uniqueScopes = domainScopes.Distinct();

            try
            {
                //// this requires sql server broker to be enabled on db
                ////SqlDependency.Start(context.Database.Connection.ConnectionString);

                return new UnitOfWork(CreateContextFactoryMethod, readOnly);
            }
            catch (ArgumentException ex)
            {
                //log it
                LogManager.GetCurrentClassLogger().Error("Problem creating context", ex);
                throw new DataLayerException("Problem creating context");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IObjectContext CreateContextFactoryMethod(DomainScope domainScope, Guid uowGuid)
        {
            var contextType = GetContextType(domainScope);
            var context = (ObjectContextAdapter)GetDynamicContextInstance<ObjectContextAdapter>(contextType, domainScope, uowGuid);
            return context;
        }

        #region Profiling Methods

        #endregion Profiling Methods

        #region Trace Methods

        public virtual void SetTrace(bool isActive)
        {
            isTraceActive = isActive;
        }

        private int StartTrace(IObjectContext context)
        {
            int traceId = -1;

            if (isTraceActive)
            {
                lock (_traceSessions)
                {
                    var traceKey = GetTraceSessionKey(context);
                    if (_traceSessions.ContainsKey(traceKey))
                    {
                        return _traceSessions[traceKey].Item1;
                    }

                    var connection = context.Context.Database.Connection;

                    var traceIdParam = new SqlParameter
                    {
                        ParameterName = "TraceID",
                        DbType = System.Data.DbType.Int32,
                        Direction = System.Data.ParameterDirection.Output
                    };

                    var currentTraceKey = GetTraceSessionKey(connection);

                    //Sets the LockTIMEOUT POLICY to 4 seconds.
                    connection.StateChange += (s, state) =>
                    {
                        if (state.CurrentState == System.Data.ConnectionState.Open)
                        {
                            var command = (s as DbConnection).CreateCommand();
                            command.CommandText = "SET LOCK_TIMEOUT 4000";
                            command.CommandType = System.Data.CommandType.Text;
                            command.ExecuteNonQuery();
                        }
                    };

                    var contextName = context.Context.GetType().Name;
                    var threadId = Thread.CurrentThread.ManagedThreadId;

                    var traceLocation = Environment.CurrentDirectory + "\\__TraceSQL_" + contextName + "_ThreadID_" + threadId + "_" + DateTime.Now.Ticks.ToString() + "_" + currentTraceKey;
                    //var traceLocation = Environment.CurrentDirectory + "\\__TraceSQL_";// +threadId + "_" + DateTime.Now.Ticks;// +contextName + "_SQLTrace_ThreadID_" + threadId + "_" + DateTime.Now.Ticks;

                    Debug.WriteLine("SQL Trace: Starting sql trace info from Context DataSource\"{0}\" to \"{1}\".", context.Context.Database.Connection.Database, traceLocation);
                    context.Context.Database.ExecuteSqlCommand(string.Format(CREATE_AND_START_TRACE_SQL, traceLocation), traceIdParam);

                    if (!traceIdParam.IsNullable)
                    {
                        traceId = (int)traceIdParam.Value;
                        _traceSessions.Add(currentTraceKey, new Tuple<int, string>(traceId, connection.ConnectionString));
                    }
                }
            }

            return traceId;
        }

        public virtual void StartTrace()
        {
            this.SetTrace(true);
        }

        public virtual void StopTrace()
        {
            foreach (var trace in _traceSessions.ToArray())
            {
                if (trace.Value.Item1 >= 0)
                {
                    this.StopTrace(trace.Key);
                }
            }
        }

        private void StopTrace(string traceKey)
        {
            if (isTraceActive)
            {
                string connectionString = null;
                int traceId = -1;

                lock (_traceSessions)
                {
                    if (!_traceSessions.ContainsKey(traceKey))
                        return;

                    traceId = _traceSessions[traceKey].Item1;
                    connectionString = _traceSessions[traceKey].Item2;
                    _traceSessions.Remove(traceKey);
                }

                var traceIdParam = new SqlParameter
                {
                    ParameterName = "TraceID",
                    DbType = System.Data.DbType.Int32,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = traceId
                };

                var traceIdParam2 = new SqlParameter
                {
                    ParameterName = "TraceID",
                    DbType = System.Data.DbType.Int32,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = traceId
                };

                using (var connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        var command = connection.CreateCommand();

                        command.CommandText = STOP_TRACE_SQL;
                        command.Parameters.Add(traceIdParam);
                        command.ExecuteNonQuery();

                        command = connection.CreateCommand();

                        command.CommandText = CLOSE_TRACE_SQL;
                        command.Parameters.Add(traceIdParam2);
                        command.ExecuteNonQuery();
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        private static readonly string STOP_TRACE_SQL = @"exec sp_trace_setstatus @TraceID, 0";

        private static readonly string CLOSE_TRACE_SQL = @"exec sp_trace_setstatus @TraceID, 2";

        private static readonly string CREATE_AND_START_TRACE_SQL = @"
-- Create a Queue
declare @rc int
--declare @TraceID int  --Given as Parameter
declare @maxfilesize bigint
set @maxfilesize = 50

-- Please replace the text InsertFileNameHere, with an appropriate
-- filename prefixed by a path, e.g., c:\MyFolder\MyTrace. The .trc extension
-- will be appended to the filename automatically. If you are writing from
-- remote server to local drive, please use UNC path and make sure server has
-- write access to your network share

exec @rc = sp_trace_create @TraceID output, 0, N'{0}', @maxfilesize, NULL
if (@rc != 0) goto error

-- Client side File and Table cannot be scripted

-- Set the events
declare @on bit
set @on = 1
--exec sp_trace_setevent @TraceID, 14, 1, @on
--exec sp_trace_setevent @TraceID, 14, 9, @on
--exec sp_trace_setevent @TraceID, 14, 10, @on
--exec sp_trace_setevent @TraceID, 14, 11, @on
--exec sp_trace_setevent @TraceID, 14, 6, @on
--exec sp_trace_setevent @TraceID, 14, 12, @on
--exec sp_trace_setevent @TraceID, 14, 14, @on
--exec sp_trace_setevent @TraceID, 15, 11, @on
--exec sp_trace_setevent @TraceID, 15, 6, @on
--exec sp_trace_setevent @TraceID, 15, 9, @on
--exec sp_trace_setevent @TraceID, 15, 10, @on
--exec sp_trace_setevent @TraceID, 15, 12, @on
--exec sp_trace_setevent @TraceID, 15, 13, @on
--exec sp_trace_setevent @TraceID, 15, 14, @on
--exec sp_trace_setevent @TraceID, 15, 15, @on
--exec sp_trace_setevent @TraceID, 15, 16, @on
--exec sp_trace_setevent @TraceID, 15, 17, @on
--exec sp_trace_setevent @TraceID, 15, 18, @on
--exec sp_trace_setevent @TraceID, 17, 1, @on
--exec sp_trace_setevent @TraceID, 17, 9, @on
--exec sp_trace_setevent @TraceID, 17, 10, @on
--exec sp_trace_setevent @TraceID, 17, 11, @on
--exec sp_trace_setevent @TraceID, 17, 6, @on
--exec sp_trace_setevent @TraceID, 17, 12, @on
--exec sp_trace_setevent @TraceID, 17, 13, @on
--exec sp_trace_setevent @TraceID, 17, 14, @on
--exec sp_trace_setevent @TraceID, 10, 9, @on
--exec sp_trace_setevent @TraceID, 10, 10, @on
--exec sp_trace_setevent @TraceID, 10, 6, @on
--exec sp_trace_setevent @TraceID, 10, 11, @on
--exec sp_trace_setevent @TraceID, 10, 12, @on
--exec sp_trace_setevent @TraceID, 10, 13, @on
--exec sp_trace_setevent @TraceID, 10, 14, @on
--exec sp_trace_setevent @TraceID, 10, 15, @on
--exec sp_trace_setevent @TraceID, 10, 16, @on
--exec sp_trace_setevent @TraceID, 10, 17, @on
--exec sp_trace_setevent @TraceID, 10, 18, @on

--SELECTS
exec sp_trace_setevent @TraceID, 12, 1, @on
exec sp_trace_setevent @TraceID, 12, 9, @on
exec sp_trace_setevent @TraceID, 12, 11, @on
exec sp_trace_setevent @TraceID, 12, 6, @on
exec sp_trace_setevent @TraceID, 12, 10, @on
exec sp_trace_setevent @TraceID, 12, 12, @on
exec sp_trace_setevent @TraceID, 12, 13, @on
exec sp_trace_setevent @TraceID, 12, 14, @on
exec sp_trace_setevent @TraceID, 12, 15, @on
exec sp_trace_setevent @TraceID, 12, 16, @on
exec sp_trace_setevent @TraceID, 12, 17, @on
exec sp_trace_setevent @TraceID, 12, 18, @on
exec sp_trace_setevent @TraceID, 12, 48, @on
exec sp_trace_setevent @TraceID, 13, 1, @on
exec sp_trace_setevent @TraceID, 13, 9, @on
exec sp_trace_setevent @TraceID, 13, 11, @on
exec sp_trace_setevent @TraceID, 13, 6, @on
exec sp_trace_setevent @TraceID, 13, 10, @on
exec sp_trace_setevent @TraceID, 13, 12, @on
exec sp_trace_setevent @TraceID, 13, 14, @on
exec sp_trace_setevent @TraceID, 13, 48, @on

--UPDATES AND INSERTS
exec sp_trace_setevent @TraceID, 40, 1, @on
exec sp_trace_setevent @TraceID, 40, 9, @on
exec sp_trace_setevent @TraceID, 40, 11, @on
exec sp_trace_setevent @TraceID, 40, 6, @on
exec sp_trace_setevent @TraceID, 40, 10, @on
exec sp_trace_setevent @TraceID, 40, 12, @on
exec sp_trace_setevent @TraceID, 40, 13, @on
exec sp_trace_setevent @TraceID, 40, 14, @on
exec sp_trace_setevent @TraceID, 40, 15, @on
exec sp_trace_setevent @TraceID, 40, 16, @on
exec sp_trace_setevent @TraceID, 40, 17, @on
exec sp_trace_setevent @TraceID, 40, 18, @on
exec sp_trace_setevent @TraceID, 40, 48, @on

exec sp_trace_setevent @TraceID, 41, 1, @on
exec sp_trace_setevent @TraceID, 41, 9, @on
exec sp_trace_setevent @TraceID, 41, 11, @on
exec sp_trace_setevent @TraceID, 41, 6, @on
exec sp_trace_setevent @TraceID, 41, 10, @on
exec sp_trace_setevent @TraceID, 41, 12, @on
exec sp_trace_setevent @TraceID, 41, 13, @on
exec sp_trace_setevent @TraceID, 41, 14, @on
exec sp_trace_setevent @TraceID, 41, 48, @on

--STORED PROCEDURES
exec sp_trace_setevent @TraceID, 42, 1, @on
exec sp_trace_setevent @TraceID, 42, 9, @on
exec sp_trace_setevent @TraceID, 42, 11, @on
exec sp_trace_setevent @TraceID, 42, 6, @on
exec sp_trace_setevent @TraceID, 42, 10, @on
exec sp_trace_setevent @TraceID, 42, 12, @on
exec sp_trace_setevent @TraceID, 42, 13, @on
exec sp_trace_setevent @TraceID, 42, 14, @on
exec sp_trace_setevent @TraceID, 42, 15, @on
exec sp_trace_setevent @TraceID, 42, 16, @on
exec sp_trace_setevent @TraceID, 42, 17, @on
exec sp_trace_setevent @TraceID, 42, 18, @on
exec sp_trace_setevent @TraceID, 42, 48, @on

exec sp_trace_setevent @TraceID, 43, 1, @on
exec sp_trace_setevent @TraceID, 43, 9, @on
exec sp_trace_setevent @TraceID, 43, 11, @on
exec sp_trace_setevent @TraceID, 43, 6, @on
exec sp_trace_setevent @TraceID, 43, 10, @on
exec sp_trace_setevent @TraceID, 43, 12, @on
exec sp_trace_setevent @TraceID, 43, 14, @on
exec sp_trace_setevent @TraceID, 43, 13, @on
exec sp_trace_setevent @TraceID, 43, 48, @on

exec sp_trace_setevent @TraceID, 44, 1, @on
exec sp_trace_setevent @TraceID, 44, 9, @on
exec sp_trace_setevent @TraceID, 44, 11, @on
exec sp_trace_setevent @TraceID, 44, 6, @on
exec sp_trace_setevent @TraceID, 44, 10, @on
exec sp_trace_setevent @TraceID, 44, 12, @on
exec sp_trace_setevent @TraceID, 44, 13, @on
exec sp_trace_setevent @TraceID, 44, 14, @on
exec sp_trace_setevent @TraceID, 44, 15, @on
exec sp_trace_setevent @TraceID, 44, 16, @on
exec sp_trace_setevent @TraceID, 44, 17, @on
exec sp_trace_setevent @TraceID, 44, 18, @on
exec sp_trace_setevent @TraceID, 44, 48, @on

exec sp_trace_setevent @TraceID, 45, 1, @on
exec sp_trace_setevent @TraceID, 45, 9, @on
exec sp_trace_setevent @TraceID, 45, 11, @on
exec sp_trace_setevent @TraceID, 45, 13, @on

exec sp_trace_setevent @TraceID, 45, 6, @on
exec sp_trace_setevent @TraceID, 45, 10, @on
exec sp_trace_setevent @TraceID, 45, 12, @on
exec sp_trace_setevent @TraceID, 45, 14, @on
exec sp_trace_setevent @TraceID, 45, 15, @on
exec sp_trace_setevent @TraceID, 45, 16, @on
exec sp_trace_setevent @TraceID, 45, 48, @on

exec sp_trace_setevent @TraceID, 181, 1, @on
exec sp_trace_setevent @TraceID, 181, 9, @on
exec sp_trace_setevent @TraceID, 181, 11, @on
exec sp_trace_setevent @TraceID, 181, 13, @on

exec sp_trace_setevent @TraceID, 182, 6, @on
exec sp_trace_setevent @TraceID, 182, 10, @on
exec sp_trace_setevent @TraceID, 182, 13, @on
exec sp_trace_setevent @TraceID, 182, 12, @on
exec sp_trace_setevent @TraceID, 182, 14, @on
exec sp_trace_setevent @TraceID, 182, 15, @on

exec sp_trace_setevent @TraceID, 185, 1, @on
exec sp_trace_setevent @TraceID, 185, 9, @on
exec sp_trace_setevent @TraceID, 185, 11, @on
exec sp_trace_setevent @TraceID, 185, 13, @on

exec sp_trace_setevent @TraceID, 186, 6, @on
exec sp_trace_setevent @TraceID, 186, 10, @on
exec sp_trace_setevent @TraceID, 186, 13, @on
exec sp_trace_setevent @TraceID, 186, 12, @on
exec sp_trace_setevent @TraceID, 186, 14, @on
exec sp_trace_setevent @TraceID, 186, 15, @on

exec sp_trace_setevent @TraceID, 187, 1, @on
exec sp_trace_setevent @TraceID, 187, 9, @on
exec sp_trace_setevent @TraceID, 187, 11, @on
exec sp_trace_setevent @TraceID, 187, 13, @on
exec sp_trace_setevent @TraceID, 187, 15, @on

exec sp_trace_setevent @TraceID, 188, 6, @on
exec sp_trace_setevent @TraceID, 188, 10, @on
exec sp_trace_setevent @TraceID, 188, 13, @on
exec sp_trace_setevent @TraceID, 188, 12, @on
exec sp_trace_setevent @TraceID, 188, 14, @on
exec sp_trace_setevent @TraceID, 188, 15, @on

-- Set the Filters
declare @intfilter int
declare @bigintfilter bigint

--exec sp_trace_setfilter @TraceID, 10, 0, 7, N'SQL Server Profiler - 92585ce1-8691-4241-a593-beabc69b717c'
-- Set the trace status to start
exec sp_trace_setstatus @TraceID, 1

-- display trace id for future references
select TraceID=@TraceID
goto finish

error:
select ErrorCode=@rc

finish: ";

        #endregion Trace Methods
    }
}
