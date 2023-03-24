using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatherSQL
{
    public partial class DbHelper
    {
        /// <summary>   
        /// 对数据库执行增删改操作，返回受影响的行数。   
        /// </summary>   
        /// <param name="sql">要执行的增删改的SQL语句</param>   
        /// <param name="parameters">执行增删改语句所需要的参数</param>
        /// <returns></returns>  
        public async Task<int> ExecuteNonQueryAsync(string sql, IDictionary<string, object> keyValues)
        {
            return await ExecuteNonQueryAsync(sql, keyValues, CommandType.Text);
        }

        /// <summary>
        /// 对数据库执行增删改操作，返回受影响的行数。  
        /// </summary>
        /// <param name="sql">要执行的增删改的SQL语句</param>
        /// <returns></returns>
        public async Task<int> ExecuteNonQueryAsync(string sql)
        {
            return await ExecuteNonQueryAsync(sql, new Dictionary<string, object>());
        }


        /// <summary>   
        /// 对数据库执行增删改操作，返回受影响的行数。   
        /// </summary>   
        /// <param name="sql">要执行的增删改的SQL语句</param>   
        /// <param name="parameters">执行增删改语句所需要的参数</param>
        /// <param name="commandType">执行的SQL语句的类型</param>
        /// <returns></returns>
        public async Task<int> ExecuteNonQueryAsync(string sql, IDictionary<string, object> keyValues, CommandType commandType)
        {
            List<SqlParameter> parameter = HaskeyValuesableToDbParameterList(keyValues);
            using (DbCommand command = CreateDbCommand(sql, parameter, commandType))
            {
                command.Connection.Open();
                int affectedRows = await command.ExecuteNonQueryAsync();
                command.Parameters.Clear();
                command.Connection.Close();
                return affectedRows;
            }
        }

        /// <summary>
        /// 带事务执行多条SQL语句
        /// </summary>
        /// <param name="sql">sql集合</param>
        /// <returns></returns>
        public async Task<bool> ExecuteSqlWithTranAsync(List<string> sqlList)
        {
            bool result = false;
            using (DbCommand command = CreateDbCommand())
            {
                command.Connection.Open();
                DbTransaction tx = command.Connection.BeginTransaction();
                command.Transaction = tx;
                try
                {
                    for (int n = 0; n < sqlList.Count; n++)
                    {
                        string strsql = sqlList[n];
                        if (strsql.Trim().Length > 0)
                        {
                            command.CommandText = strsql;
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                    tx.Commit();
                    result = true;
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    throw ex;
                }
                finally
                {
                    if (command.Connection != null)
                    {
                        command.Connection.Close();
                        command.Dispose();
                    }
                }
                return result;
            }
        }

        /// <summary>   
        /// 执行一个查询语句，返回一个关联的DataReader实例   
        /// </summary>   
        /// <param name="sql">要执行的查询语句</param>   
        /// <param name="parameters">执行SQL查询语句所需要的参数</param>
        /// <returns></returns> 
        public async Task<SqlDataReader> ExecuteReaderAsync(string sql, IDictionary<string, object> keyValues)
        {
            return await ExecuteReaderAsync(sql, keyValues, CommandType.Text);
        }

        /// <summary>   
        /// 执行一个查询语句，返回一个关联的DataReader实例   
        /// </summary>   
        /// <param name="sql">要执行的查询语句</param>   
        /// <param name="parameters">执行SQL查询语句所需要的参数</param>
        /// <param name="commandType">执行的SQL语句的类型</param>
        /// <returns></returns> 
        public async Task<SqlDataReader> ExecuteReaderAsync(string sql, IDictionary<string, object> keyValues, CommandType commandType)
        {
            List<SqlParameter> parameter = HaskeyValuesableToDbParameterList(keyValues);
            using (SqlCommand command = CreateDbCommand(sql, parameter, commandType))
            {
                command.Connection.Open();
                var result = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
                command.Parameters.Clear();
                return result;
            }
        }

        /// <summary>   
        /// 执行一个查询语句，返回查询结果的第一行第一列   
        /// </summary>   
        /// <param name="sql">要执行的查询语句</param>   
        /// <param name="parameters">执行SQL查询语句所需要的参数</param>   
        /// <returns></returns>   
        public async Task<object> ExecuteScalarAsync(string sql, IDictionary<string, object> keyValues)
        {
            return await ExecuteScalarAsync(sql, keyValues, CommandType.Text);
        }

        /// <summary>   
        /// 执行一个查询语句，返回查询结果的第一行第一列   
        /// </summary>   
        /// <param name="sql">要执行的查询语句</param>
        /// <returns></returns>   
        public async Task<object> ExecuteScalarAsync(string sql)
        {
            return await ExecuteScalarAsync(sql, null, CommandType.Text);
        }

        /// <summary>   
        /// 执行一个查询语句，返回查询结果的第一行第一列   
        /// </summary>   
        /// <param name="sql">要执行的查询语句</param>   
        /// <param name="parameters">执行SQL查询语句所需要的参数</param>   
        /// <param name="commandType">执行的SQL语句的类型</param>
        /// <returns></returns>   
        public async Task<object> ExecuteScalarAsync(string sql, IDictionary<string, object> keyValues, CommandType commandType)
        {
            List<SqlParameter> parameter = HaskeyValuesableToDbParameterList(keyValues);
            using (DbCommand command = CreateDbCommand(sql, parameter, commandType))
            {
                command.Connection.Open();
                object result = await command.ExecuteScalarAsync();
                command.Parameters.Clear();
                command.Connection.Close();
                return result;
            }
        }
    }
}
