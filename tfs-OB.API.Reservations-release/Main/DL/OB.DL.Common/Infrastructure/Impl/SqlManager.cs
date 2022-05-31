using Dapper;
using OB.DL.Common.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using OB.Api.Core;

namespace OB.DL.Common.Impl
{
    internal class SqlManager : ISqlManager
    {
        public IDbConnection _connection { get; set; }

        public SqlManager(IObjectContext context)
        {
            _connection = context.Context.Database.Connection;
            if (_connection.State != ConnectionState.Open)
                _connection.Open();

        }

        public SqlManager(IDbConnection connection)
        {
            _connection = connection;
            if (_connection.State != ConnectionState.Open)
                _connection.Open();
        }

        public IEnumerable<T> ExecuteSql<T>(string commandText, DynamicParameters parameters)
        {
            var result = _connection.Query<T>(commandText, parameters);
            return result;
        }

        public IEnumerable<T> ExecuteSql<T>(string commandText, DynamicParameters parameters, IDbTransaction tran)
        {
            var result = _connection.Query<T>(commandText, parameters, tran);
            return result;
        }

        public IDbTransaction BeginTransaction(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted)
        {
            //if (_connection.State == ConnectionState.Closed)
            //    _connection.Open();

            return _connection.BeginTransaction(isolationLevel);
        }

        public int AddAsyncTask(string sqlToExecuteAsync, string correlationID)
        {
            string command = @"INSERT INTO [AsyncTasks]
                                       ([correlationUID]
                                       ,[SqlToExecute]
                                       ,[CreationDate]
                                        )
                                 VALUES
                                       ({0}
                                       ,{1}
                                       ,getdate()
                                       )";

            command = string.Format(command, "'" + correlationID + "'", "'" + sqlToExecuteAsync + "'");


            return ExecuteSql(command.ToString());
        }

        public int ExecuteSql(string commandText)
        {
            var connection = _connection;

            //bool openedConnection = false;
            //if (connection.State != System.Data.ConnectionState.Open)
            //{
            //    connection.Open();
            //    openedConnection = true;
            //}
            
            var result = _connection.Execute(commandText);

            //if (openedConnection)
            //    connection.Close();

            return result;
        }

        public int ExecuteSql(string commandText, ref IDbTransaction tran)
        {
            var connection = _connection;

            //bool openedConnection = false;
            //if (connection.State != System.Data.ConnectionState.Open)
            //{
            //    connection.Open();
            //    openedConnection = true;
            //}
            
            var result = _connection.Execute(commandText, null, tran);

            //if (openedConnection)
            //    connection.Close();

            return result;
        }

        public int ExecuteSqlWithSpecificCommandTimeout(string commandText, int commandTimeout)
        {
            return _connection.ExecuteScalar<int>(commandText, commandTimeout: commandTimeout);
        }

        public IEnumerable<T> Query<T>(string commandText) where T : class
        {
            var connection = _connection;

            //bool openedConnection = false;
            //if (connection.State != System.Data.ConnectionState.Open)
            //{
            //    connection.Open();
            //    openedConnection = true;
            //}
            var result = connection.Query<T>(commandText);

            //if (openedConnection)
            //    connection.Close();

            return result;
        }

        public IEnumerable<T> Query<T>(string commandText, DynamicParameters parameters, CommandType? commandType = null) where T : class
        {
            var connection = _connection;

            //bool openedConnection = false;
            //if (connection.State != System.Data.ConnectionState.Open)
            //{
            //    connection.Open();
            //    openedConnection = true;
            //}
            var result = connection.Query<T>(commandText, parameters, null, true, null, commandType);

            //if (openedConnection)
            //    connection.Close();

            return result;
        }

        public IEnumerable<T> QueryStruct<T>(string commandText, DynamicParameters parameters, CommandType? commandType = null) where T : struct
        {
            var connection = _connection;

            //bool openedConnection = false;
            //if (connection.State != System.Data.ConnectionState.Open)
            //{
            //    connection.Open();
            //    openedConnection = true;
            //}
            var result = connection.Query<T>(commandText, parameters, null, true, null, commandType);

            //if (openedConnection)
            //    connection.Close();

            return result;
        }

        public T QuerySingle<T>(string commandText, DynamicParameters parameters, CommandType? commandType = null) where T : class
        {
            var connection = _connection;

            //bool openedConnection = false;
            //if (connection.State != System.Data.ConnectionState.Open)
            //{
            //    connection.Open();
            //    openedConnection = true;
            //}
            var result = connection.Query<T>(commandText, parameters, null, true, null, commandType);

            //if (openedConnection)
            //    connection.Close();

            return result.Any() ? result.First() : null;
        }

        public T ExecuteScalar<T>(string commandText, DynamicParameters parameters, CommandType? commandType = null) where T : struct
        {
            var connection = _connection;
            //bool openedConnection = false;
            //if (connection.State != System.Data.ConnectionState.Open)
            //{
            //    connection.Open();
            //    openedConnection = true;
            //}
            var result = connection.ExecuteScalar<T>(commandText, parameters, null, null, commandType);

            //if (openedConnection)
            //    connection.Close();

            return result;
        }

        public int ExecuteStoredProcedure(string name, params object[] values)
        {
            var connection = _connection;

            //bool openedConnection = false;
            //if (_connection.State != System.Data.ConnectionState.Open)
            //{
            //    connection.Open();
            //    openedConnection = true;
            //}

            var result = connection.Execute(name, values, commandType: System.Data.CommandType.StoredProcedure);

            //if (openedConnection)
            //    connection.Close();

            return result;
        }

        public int ExecuteStoredProcedure(string name, DynamicParameters parameters)
        {
            var connection = _connection;

            //bool openedConnection = false;
            //if (connection.State != System.Data.ConnectionState.Open)
            //{
            //    connection.Open();
            //    openedConnection = true;
            //}

            var result = connection.Execute(name, parameters, commandType: System.Data.CommandType.StoredProcedure);

            //if (openedConnection)
            //    connection.Close();

            return result;
        }
    }
}