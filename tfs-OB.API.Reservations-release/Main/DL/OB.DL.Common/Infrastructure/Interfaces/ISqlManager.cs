using Dapper;
using System.Collections.Generic;
using System.Data;

namespace OB.DL.Common
{
    public interface ISqlManager
    {
        IDbConnection _connection { get; set; }

        int ExecuteSql(string commandText);

        int ExecuteSql(string commandText, ref IDbTransaction tran);

        int AddAsyncTask(string sqlToExecuteAsync, string correlationID);

        IEnumerable<T> Query<T>(string commandText) where T : class;

        IEnumerable<T> Query<T>(string commandText, DynamicParameters parameters, CommandType? commandType = null) where T : class;

        IEnumerable<T> QueryStruct<T>(string commandText, DynamicParameters parameters, CommandType? commandType = null) where T : struct;

        T QuerySingle<T>(string commandText, DynamicParameters parameters, CommandType? commandType = null) where T : class;

        T ExecuteScalar<T>(string commandText, DynamicParameters parameters, CommandType? commandType = null) where T : struct;

        int ExecuteStoredProcedure(string name, params object[] values);

        int ExecuteSqlWithSpecificCommandTimeout(string commandText, int commandTimeout);

        int ExecuteStoredProcedure(string name, DynamicParameters parameters);

        IEnumerable<T> ExecuteSql<T>(string commandText, DynamicParameters parameters);

        IEnumerable<T> ExecuteSql<T>(string commandText, DynamicParameters parameters, IDbTransaction tran);

        IDbTransaction BeginTransaction(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted);
    }
}