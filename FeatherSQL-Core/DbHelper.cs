using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections;
using System.Data.SqlClient;

namespace FeatherSQL
{
    public partial class DbHelper
    {
        public string ConnectionString { get; private set; }
        private DbProviderFactory providerFactory;
        public DbProviderType DbType;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="providerType">数据库类型枚举，参见<paramref name="providerType"/></param>
        public DbHelper(string connectionString = "maindb", DbProviderType dbProviderType= DbProviderType.SqlServer)
        {
            
            ConnectionString = GetConnectionString(connectionString);
            ProviderFactory factory = new ProviderFactory();
            DbType = dbProviderType;
            providerFactory = factory.GetDbProviderFactory(DbProviderType.SqlServer);
            if (providerFactory == null)
            {
                throw new ArgumentException("未能找到数据库所对应程序集");
            }
        }

        /// <summary>   
        /// 对数据库执行增删改操作，返回受影响的行数。   
        /// </summary>   
        /// <param name="sql">要执行的增删改的SQL语句</param>   
        /// <param name="parameters">执行增删改语句所需要的参数</param>
        /// <returns></returns>  
        public int ExecuteNonQuery(string sql, IDictionary<string, object> keyValues)
        {
            return ExecuteNonQuery(sql, keyValues, CommandType.Text);
        }

        /// <summary>
        /// 对数据库执行增删改操作，返回受影响的行数。  
        /// </summary>
        /// <param name="sql">要执行的增删改的SQL语句</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sql)
        {
            return ExecuteNonQuery(sql, new Dictionary<string, object>());
        }


        /// <summary>   
        /// 对数据库执行增删改操作，返回受影响的行数。   
        /// </summary>   
        /// <param name="sql">要执行的增删改的SQL语句</param>   
        /// <param name="parameters">执行增删改语句所需要的参数</param>
        /// <param name="commandType">执行的SQL语句的类型</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sql, IDictionary<string, object> keyValues, CommandType commandType)
        {
            List<SqlParameter> parameter = HaskeyValuesableToDbParameterList(keyValues);
            using (SqlCommand command = CreateDbCommand(sql, parameter, commandType))
            {
                command.Connection.Open();
                int affectedRows = command.ExecuteNonQuery();
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
        public bool ExecuteSqlWithTran(List<string> sqlList)
        {
            bool result = false;
            using (SqlCommand command = CreateDbCommand())
            {
                command.Connection.Open();
                SqlTransaction tx = command.Connection.BeginTransaction();
                command.Transaction = tx;
                try
                {
                    for (int n = 0; n < sqlList.Count; n++)
                    {
                        string strsql = sqlList[n];
                        if (strsql.Trim().Length > 0)
                        {
                            command.CommandText = strsql;
                            command.ExecuteNonQuery();
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
        public SqlDataReader ExecuteReader(string sql, IDictionary<string, object> keyValues)
        {
            return ExecuteReader(sql, keyValues, CommandType.Text);
        }

        /// <summary>   
        /// 执行一个查询语句，返回一个关联的DataReader实例   
        /// </summary>   
        /// <param name="sql">要执行的查询语句</param>   
        /// <param name="parameters">执行SQL查询语句所需要的参数</param>
        /// <param name="commandType">执行的SQL语句的类型</param>
        /// <returns></returns> 
        public SqlDataReader ExecuteReader(string sql, IDictionary<string, object> keyValues, CommandType commandType)
        {
            List<SqlParameter> parameter = HaskeyValuesableToDbParameterList(keyValues);
            using (SqlCommand command = CreateDbCommand(sql, parameter, commandType))
            {
                command.Connection.Open();
                var result = command.ExecuteReader(CommandBehavior.CloseConnection);
                command.Parameters.Clear();
                return result;
            }
        }

        /// <summary>   
        /// 执行一个查询语句，返回一个包含查询结果的DataTable   
        /// </summary>   
        /// <param name="sql">要执行的查询语句</param>   
        /// <param name="parameters">执行SQL查询语句所需要的参数</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string sql, IDictionary<string, object> keyValues)
        {
            return ExecuteDataTable(sql, keyValues, CommandType.Text);
        }

        /// <summary>   
        /// 执行一个查询语句，返回一个包含查询结果的DataTable   
        /// </summary>   
        /// <param name="sql">要执行的查询语句</param>   
        /// <param name="parameters">执行SQL查询语句所需要的参数</param>
        /// <param name="commandType">执行的SQL语句的类型</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string sql, IDictionary<string, object> keyValues, CommandType commandType)
        {
            List<SqlParameter> parameter = HaskeyValuesableToDbParameterList(keyValues);
            using (SqlCommand command = CreateDbCommand(sql, parameter, commandType))
            {
                DbDataAdapter adapter = providerFactory.CreateDataAdapter();
                adapter.SelectCommand = command;
                DataTable data = new DataTable();
                adapter.Fill(data);
                command.Parameters.Clear();
                return data;
            }
        }

        /// <summary>   
        /// 执行一个查询语句，返回查询结果的第一行第一列   
        /// </summary>   
        /// <param name="sql">要执行的查询语句</param>   
        /// <param name="parameters">执行SQL查询语句所需要的参数</param>   
        /// <returns></returns>   
        public object ExecuteScalar(string sql, IDictionary<string, object> keyValues)
        {
            return ExecuteScalar(sql, keyValues, CommandType.Text);
        }

        /// <summary>   
        /// 执行一个查询语句，返回查询结果的第一行第一列   
        /// </summary>   
        /// <param name="sql">要执行的查询语句</param>
        /// <returns></returns>   
        public object ExecuteScalar(string sql)
        {
            return ExecuteScalar(sql, null, CommandType.Text);
        }

        /// <summary>   
        /// 执行一个查询语句，返回查询结果的第一行第一列   
        /// </summary>   
        /// <param name="sql">要执行的查询语句</param>   
        /// <param name="parameters">执行SQL查询语句所需要的参数</param>   
        /// <param name="commandType">执行的SQL语句的类型</param>
        /// <returns></returns>   
        public object ExecuteScalar(string sql, IDictionary<string, object> keyValues, CommandType commandType)
        {
            List<SqlParameter> parameter = HaskeyValuesableToDbParameterList(keyValues);
            using (SqlCommand command = CreateDbCommand(sql, parameter, commandType))
            {
                command.Connection.Open();
                object result = command.ExecuteScalar();
                command.Parameters.Clear();
                command.Connection.Close();
                return result;
            }
        }

        /// <summary>
        /// 创建一个SqlCommand对象
        /// </summary>
        /// <param name="sql">要执行的查询语句</param>   
        /// <param name="parameters">执行SQL查询语句所需要的参数</param>
        /// <param name="commandType">执行的SQL语句的类型</param>
        /// <returns></returns>
        private SqlCommand CreateDbCommand(string sql, IList<SqlParameter> parameters, CommandType commandType)
        {
            SqlConnection connection = new SqlConnection(ConnectionString);
            SqlCommand command = new SqlCommand();
            command.CommandText = sql;
            command.CommandType = commandType;
            command.Connection = connection;
            if (!(parameters == null || parameters.Count == 0))
            {
                foreach (SqlParameter parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
            }
            return command;
        }

        /// <summary>
        /// 创建一个SqlCommand对象
        /// </summary>
        /// <returns></returns>
        private SqlCommand CreateDbCommand()
        {
            SqlConnection connection = new SqlConnection(ConnectionString);
            SqlCommand command = new SqlCommand();
            command.Connection = connection;
            return command;
        }

        #region 本类的一些私有方法
        /// <summary>
        /// 根据配置文件name获取连接字符串
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private string GetConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "maindb";
            }
            else
            {
                connectionString = connectionString.ToLower();
            }
            
            if (ConfigurationManager.ConnectionStrings[connectionString] == null)
            {
                throw new ArgumentException("未找到" + connectionString + "所对应的数据库连接字符串！");
            }
            else
            {
                return ConfigurationManager.ConnectionStrings[connectionString].ConnectionString;
            }
        }

        /// <summary>
        /// 外部参数haskeyValuesable转为SqlParameter参数
        /// </summary>
        /// <returns></returns>
        private List<SqlParameter> HaskeyValuesableToDbParameterList(IDictionary<string, object> keyValues)
        {
            List<SqlParameter> param = new List<SqlParameter>();
            if (keyValues != null&&keyValues.Count>0)
            {
                foreach (var key in keyValues.Keys)
                {
                    var value = keyValues[key];
                    if (value==null)
                    {
                        value = DBNull.Value;
                    }
                    if (value.GetType()==typeof(DateTime))
                    {
                        if (Convert.ToDateTime(value) == default(DateTime))
                        {
                            value = DBNull.Value;
                        }
                    }
                    SqlParameter item = new SqlParameter("@" + key, value);
                    param.Add(item);
                }
            }
            return param;
        }
        #endregion
    }
}