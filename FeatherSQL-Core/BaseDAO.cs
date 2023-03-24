using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FeatherSQL
{
    /// <summary>
    /// 数据层基类
    /// </summary>
    /// <typeparam name="T">指定实体类型</typeparam>
    public partial class BaseDAO<T> : DbHelper
    {
        private DbHelper db;
        private bool IsSoftDelete;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionStr">指定连接数据库的name</param>
        public BaseDAO(string connectionStr = "mainDB")
        {
            string val = ConfigurationManager.AppSettings["IsSoftDelete"];
            if (!string.IsNullOrEmpty(val))
            {
                IsSoftDelete = true;
            }
            db = new DbHelper(connectionStr);
        }

        #region 自定义Delete相关方法

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="func">筛选条件</param>
        /// <returns></returns>
        public int Delete(Expression<Func<T, bool>> func)
        {
            return Delete<T>(func);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="func">筛选条件</param>
        /// <returns></returns>
        public int Delete<M>(Expression<Func<M, bool>> func)
        {
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            string sql = string.Empty;
            if (IsSoftDelete)//软删除
            {
                sql = $"Update [{GetTableAttribute(typeof(M)).TableName}] SET IsDeleted=1,DeleteTime=GETDATE() where {SqlSugor.GetWhereByLambda(func, out keyValues, base.DbType)}";
            }
            else
            {
                sql = $"delete  from [{GetTableAttribute(typeof(M)).TableName}] where {SqlSugor.GetWhereByLambda(func, out keyValues, base.DbType)}";
            }
            if (keyValues.Count > 0)
            {
                return base.ExecuteNonQuery(sql, keyValues);
            }
            else
            {
                return base.ExecuteNonQuery(sql);
            }
        }

        /// <summary>
        /// 删除单个
        /// </summary>
        /// <param name="entity">实体对象（必须主键必须赋值）</param>
        /// <returns>受影响的行数</returns>
        public int Delete(T entity)
        {
            string key = GetPrimaryKey(entity);
            StringBuilder sb = new StringBuilder();
            if (IsSoftDelete)
            {
                sb.Append($"Update [{GetTableAttribute(entity).TableName}] SET IsDeleted=1,DeleteTime=GETDATE() where {key}=@{key}");
            }
            else
            {
                sb.Append($"delete from [{GetTableAttribute(entity).TableName}] where {key}=@{key}");
            }
            return ExecuteNonQuery(sb.ToString(), GetPrimaryKeyValue(entity));
        }

        /// <summary>
        /// 删除单个
        /// </summary>
        /// <param name="primaryKey">主键PrimaryKey</param>
        /// <returns>受影响的行数</returns>
        public int Delete(object primaryKey)
        {
            string key = GetPrimaryKey();
            StringBuilder sb = new StringBuilder();
            if (IsSoftDelete)
            {
                sb.Append($"Update [{GetTableAttribute(typeof(T)).TableName}] SET IsDeleted=1,DeleteTime=GETDATE() where {key}=@{key}");
            }
            else
            {
                sb.Append($"delete  from [{GetTableAttribute(typeof(T)).TableName}] where {key}=@{key}");
            }
            Dictionary<string, object> keyValues = new Dictionary<string, object>
            {
                { key, primaryKey }
            };
            return ExecuteNonQuery(sb.ToString(), keyValues);
        }

        /// <summary>
        /// 删除多条数据
        /// </summary>
        /// <param name="primaryKeys">多个PrimaryKey以,隔开</param>
        /// <returns>受影响的行数</returns>
        public int DeleteList(string primaryKeys)
        {
            primaryKeys = primaryKeys.Replace('，', ',');
            string key = GetPrimaryKey();
            StringBuilder sb = new StringBuilder();
            if (IsSoftDelete)
            {
                sb.Append($"Update [{GetTableAttribute(typeof(T)).TableName}] SET IsDeleted=1,DeleteTime=GETDATE() where {key} in ({primaryKeys.Trim(',')}");
            }
            else
            {
                sb.Append($"delete  from [{GetTableAttribute(typeof(T)).TableName}] where {key} in ({primaryKeys.Trim(',')})");
            }
            return ExecuteNonQuery(sb.ToString());
        }

        /// <summary>
        /// 删除多条数据
        /// </summary>
        /// <param name="arrPrimaryKey">主键PrimaryKey的数组</param>
        /// <typeparam name="K">主键类型</typeparam>
        /// <returns>受影响的行数</returns>
        public int DeleteList<K>(IEnumerable<K> arrPrimaryKey)
        {
            string primaryKeys = string.Empty;
            foreach (K item in arrPrimaryKey)
            {
                primaryKeys += item.ToString() + ',';
            }
            string key = GetPrimaryKey();
            StringBuilder sb = new StringBuilder();
            if (IsSoftDelete)
            {
                sb.Append($"Update [{GetTableAttribute(typeof(T)).TableName}] SET IsDeleted=1,DeleteTime=GETDATE() where {key} in ({primaryKeys.Trim(',')}");
            }
            else
            {
                sb.Append($"delete  from [{GetTableAttribute(typeof(T)).TableName}] where {key} in ({primaryKeys.Trim(',')})");
            }
            return ExecuteNonQuery(sb.ToString());
        }

        /// <summary>
        /// 删除多条数据
        /// </summary>
        /// <param name="list">对象的集合</param>
        /// <returns>受影响的行数</returns>
        public int DeleteList(IEnumerable<T> list)
        {
            string primaryKeys = string.Empty;
            foreach (T item in list)
            {
                primaryKeys += GetPrimaryValue(item) + ",";
            }
            string key = GetPrimaryKey();
            StringBuilder sb = new StringBuilder();
            if (IsSoftDelete)
            {
                sb.Append($"Update [{GetTableAttribute(typeof(T)).TableName}] SET IsDeleted=1,DeleteTime=GETDATE() where {key} in ({primaryKeys.Trim(',')}");
            }
            else
            {
                sb.Append($"delete  from [{GetTableAttribute(typeof(T)).TableName}] where {key} in ({primaryKeys.Trim(',')})");
            }
            return ExecuteNonQuery(sb.ToString());
        }
        #endregion

        #region 自定义Insert相关方法
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>返回自增的主键PrimaryKey</returns>
        public int Insert(T entity)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"insert into [{GetTableAttribute(entity).TableName}] ({GetColumns(entity, "")}) values ({GetColumns(entity, "@")} ) select SCOPE_IDENTITY()");
            return Convert.ToInt32(ExecuteScalar(sb.ToString(), GetColumnValue(entity, "", false)));
        }

        /// <summary>
        /// 添加多个
        /// </summary>
        /// <param name="list">对象的集合</param>
        /// <returns>受影响的行数</returns>
        public int InsertList(IEnumerable<T> list)
        {
            StringBuilder sb = new StringBuilder();
            IDictionary<string, object> ht = new Dictionary<string, object>();
            T[] arr = list.ToArray();
            for (int i = 0; i < arr.Length; i++)
            {
                T entity = arr[i];
                sb.Append($"insert into [{GetTableAttribute(entity).TableName}] ({GetColumns(entity, string.Empty)})values({GetColumns(entity, "@_" + i)}) ");
                foreach (KeyValuePair<string, object> item in GetColumnValue(entity, "_" + i, false))
                {
                    ht.Add(item.Key, item.Value);
                }
            }
            return ExecuteNonQuery(sb.ToString(), ht);
        }
        #endregion

        #region 自定义Update相关方法

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="entity">要修改的实体</param>
        /// <returns>受影响的行数</returns>
        public int Update(T entity)
        {
            StringBuilder sb = new StringBuilder();
            string key = GetPrimaryKey();
            sb.Append($"update [{GetTableAttribute(entity).TableName}] set ");
            foreach (string item in GetColumns(entity, "").Split(','))
            {
                sb.Append($"{item}=@{item},");
            }
            string s = sb.ToString().Trim(',');
            sb.Clear();
            sb.Append(s);
            sb.Append($" where {key}=@{key}");
            return ExecuteNonQuery(sb.ToString(), GetColumnValue(entity, "", true));
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="entity">要修改的实体</param>
        /// <param name="func">筛选条件</param>
        /// <returns>受影响的行数</returns>
        public int Update(T entity, Expression<Func<T, bool>> func)
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            string key = GetPrimaryKey();
            sb.Append($"update [{GetTableAttribute(entity).TableName}] set ");
            foreach (string item in GetColumns(entity, "").Split(','))
            {
                sb.Append($"{item}=@{item},");
            }
            string s = sb.ToString().Trim(',');
            sb.Clear();
            sb.Append(s);
            sb.Append($" where {SqlSugor.GetWhereByLambda(func, out keyValues, base.DbType)}");
            keyValues = keyValues.Union(GetColumnValue(entity, "", true)).ToDictionary(x => x.Key, x => x.Value);
            return ExecuteNonQuery(sb.ToString(), keyValues);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <typeparam name="M">指定修改数据实体</typeparam>
        /// <param name="obj">要修改的对象（推荐匿名对象、DTO）</param>
        /// <param name="func">筛选条件</param>
        /// <returns>受影响的行数</returns>
        public int Update<M>(object obj, Expression<Func<M, bool>> func)
        {
            M m = Activator.CreateInstance<M>();
            StringBuilder sb = new StringBuilder();
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            string key = GetPrimaryKey();
            sb.Append($"update [{GetTableAttribute(m.GetType()).TableName}] set ");
            Dictionary<string, object> parmares = new Dictionary<string, object>();
            foreach (PropertyInfo item in obj.GetType().GetProperties())
            {
                ColumnAttribute column = item.GetCustomAttribute<ColumnAttribute>();
                if (column == null || column.ColumnType == ColumnType.None)
                {
                    sb.Append($"{item.Name}=@{item.Name},");
                    parmares.Add(item.Name, item.GetValue(obj));
                }
            }
            string s = sb.ToString().Trim(',');
            sb.Clear();
            sb.Append(s);
            sb.Append($" where {SqlSugor.GetWhereByLambda(func, out keyValues, base.DbType)}");
            keyValues = keyValues.Union(parmares).ToDictionary(x => x.Key, x => x.Value);
            return ExecuteNonQuery(sb.ToString(), keyValues);
        }

        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="list">要修改的实体集合</param>
        /// <returns>受影响的行数</returns>
        public int UpdateList(IEnumerable<T> list)
        {
            StringBuilder sb = new StringBuilder();
            T[] arr = list.ToArray();
            IDictionary<string, object> ht = new Dictionary<string, object>();
            for (int i = 0; i < arr.Length; i++)
            {
                T entity = arr[i];
                string key = GetPrimaryKey();
                sb.Append($"update [{GetTableAttribute(entity).TableName}] set ");
                string[] arrColumns = GetColumns(entity, "").Split(',');
                foreach (string item in arrColumns)
                {
                    sb.Append($"{item}=@_{i}{item},");
                }
                string s = sb.ToString().Trim(',');
                sb.Clear();
                sb.Append(s);
                sb.Append($" where {key}=@_{i}{key} ");
                foreach (KeyValuePair<string, object> de in GetColumnValue(entity, "_" + i, true))
                {
                    ht.Add(de.Key, de.Value);
                }
            }
            return ExecuteNonQuery(sb.ToString(), ht);
        }
        #endregion

        #region 自定义Query相关方法

        #region 获取单个
        /// <summary>
        /// 获取指定类型单个实体对象
        /// </summary>
        /// <param name="func">筛选条件</param>
        /// <returns></returns>
        public M Get<M>(Expression<Func<M, bool>> func)
        {
            M entity = Activator.CreateInstance<M>();
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            string sql = string.Empty;
            if (IsSoftDelete)
            {
                sql = $"select top 1 {GetColumns(entity.GetType(), string.Empty, true)}  from [{GetTableAttribute(typeof(M)).TableName}] where IsDeleted=0 AND {SqlSugor.GetWhereByLambda(func, out keyValues, base.DbType)}";
            }
            else
            {
                sql = $"select top 1 {GetColumns(entity.GetType(), string.Empty, true)}  from [{GetTableAttribute(typeof(M)).TableName}] where {SqlSugor.GetWhereByLambda(func, out keyValues, base.DbType)}";
            }
            using (SqlDataReader reader = ExecuteReader(sql, keyValues))
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        entity = ReaderToEntity<M>(reader);
                    }
                    return entity;
                }
                else
                {
                    return default(M);
                }
            }
        }

        /// <summary>
        /// 获取指定类型单个实体对象
        /// </summary>
        /// <param name="func">筛选条件</param>
        /// <returns></returns>
        public T Get(Expression<Func<T, bool>> func)
        {
            return Get<T>(func);
        }

        /// <summary>
        /// 获取单个实体
        /// </summary>
        /// <param name="primaryKey">主键PrimaryKey</param>
        /// <returns>返回查询到的实体对象</returns>
        public T Get(object primaryKey)
        {
            StringBuilder sb = new StringBuilder();
            T entity = Activator.CreateInstance<T>();
            string primaryKeyName = GetPrimaryKey(entity);
            if (IsSoftDelete)
            {
                sb.Append($"select top 1 {GetColumns(entity, string.Empty, true)} from [{GetTableAttribute(entity).TableName}] where IsDeleted=0 AND {primaryKeyName}=@{primaryKeyName}");
            }
            else
            {
                sb.Append($"select top 1 {GetColumns(entity, string.Empty, true)} from [{GetTableAttribute(entity).TableName}] where {primaryKeyName}=@{primaryKeyName}");
            }
            IDictionary<string, object> ht = new Dictionary<string, object>
            {
                { primaryKeyName, primaryKey }
            };
            using (SqlDataReader reader = ExecuteReader(sb.ToString(), ht))
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        entity = ReaderToEntity<T>(reader);
                    }
                    return entity;
                }
                else
                {
                    return default (T);
                }
            }
        }

        /// <summary>
        /// 获取指定类型的单个实体
        /// </summary>
        /// <typeparam name="M">指定的类型（推荐为DTO）</typeparam>
        /// <param name="sql">查询的SQL语句</param>
        /// <returns></returns>
        public M Get<M>(string sql)
        {
            return Get<M>(sql, null);
        }

        /// <summary>
        /// 获取指定类型的单个实体
        /// </summary>
        /// <typeparam name="M">指定的类型（推荐为DTO）</typeparam>
        /// <param name="sql">查询的SQL语句</param>
        /// <returns></returns>
        public T Get(string sql)
        {
            return Get<T>(sql);
        }

        /// <summary>
        /// 获取指定类型的单个实体
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public T Get(string sql, Dictionary<string, object> param)
        {
            return Get<T>(sql, param);
        }

        /// <summary>
        /// 获取指定类型的单个实体
        /// </summary>
        /// <typeparam name="M">指定的类型（推荐为DTO）</typeparam>
        /// <param name="param"></param>
        /// <returns></returns>
        public M Get<M>(string sql, Dictionary<string, object> param)
        {
            M entity = Activator.CreateInstance<M>();
            using (SqlDataReader reader = ExecuteReader(sql, param))
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        entity = ReaderToEntity<M>(reader);
                    }
                    return entity;
                }
                else
                {
                    return default(M);
                }
            }
        }
        #endregion

        #region 获取数量
        /// <summary>
        /// 获取数量
        /// </summary>
        /// <returns>查询到的数量</returns>
        public int GetCount()
        {
            T entity = Activator.CreateInstance<T>();
            string sql = string.Empty;
            if (IsSoftDelete)
            {
                sql = $"select count(*) from [{GetTableAttribute(entity).TableName}] Where IsDeleted=0";
            }
            else
            {
                sql = $"select count(*) from [{GetTableAttribute(entity).TableName}]";
            }
            return (int)ExecuteScalar(sql);
        }

        /// <summary>
        /// 获取数量
        /// </summary>
        /// <param name="sql">查询的SQL语句（需要count函数）</param>
        /// <returns>查询到的数量</returns>
        public int GetCount(string sql)
        {
            return (int)ExecuteScalar(sql);
        }

        /// <summary>
        /// 获取数量
        /// </summary>
        /// <param name="sql">查询的SQL语句（需要count函数）</param>
        /// <param name="param">SQL中需要的参数</param>
        /// <returns>查询到的数量</returns>
        public int GetCount(string sql, IDictionary<string, object> param)
        {
            return (int)ExecuteScalar(sql, param);
        }

        /// <summary>
        /// 获取数量
        /// </summary>
        /// <param name="func">筛选条件</param>
        /// <returns>查询到的数量</returns>
        public int GetCount(Expression<Func<T, bool>> func)
        {
            return GetCount<T>(func);
        }

        /// <summary>
        /// 获取数量
        /// </summary>
        /// <typeparam name="M">指定的类型（推荐为DTO或Entity）</typeparam>
        /// <param name="func">筛选条件</param>
        /// <returns>查询到的数量</returns>
        public int GetCount<M>(Expression<Func<M, bool>> func)
        {
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            string sql = string.Empty;
            if (IsSoftDelete)
            {
                sql = $"select count(*) from [{GetTableAttribute(typeof(M)).TableName}] where IsDeleted=0 And {SqlSugor.GetWhereByLambda(func, out keyValues, base.DbType)}";
            }
            else
            {
                sql = $"select count(*) from [{GetTableAttribute(typeof(M)).TableName}] where {SqlSugor.GetWhereByLambda(func, out keyValues, base.DbType)}";
            }
            return (int)ExecuteScalar(sql, keyValues);
        }
        #endregion

        #region 获取列表
        /// <summary>
        /// 获取指定类型的列表
        /// </summary>
        /// <param name="func">筛选条件</param>
        /// <returns></returns>
        public IEnumerable<T> GetList(Expression<Func<T, bool>> func)
        {
            return GetList<T>(func);
        }

        /// <summary>
        /// 获取指定类型的列表
        /// </summary>
        /// <param name="func">筛选条件</param>
        /// <returns></returns>
        public IEnumerable<M> GetList<M>(Expression<Func<M, bool>> func)
        {
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            string sql = string.Empty;
            if (IsSoftDelete)
            {
                sql = $"select {GetColumns(typeof(M), string.Empty, true)} from [{GetTableAttribute(typeof(M)).TableName}] where IsDeleted=0 AND {SqlSugor.GetWhereByLambda(func, out keyValues, base.DbType)}";
            }
            else
            {
                sql = $"select {GetColumns(typeof(M), string.Empty, true)} from [{GetTableAttribute(typeof(M)).TableName}] where {SqlSugor.GetWhereByLambda(func, out keyValues, base.DbType)}";
            }
            return GetList<M>(sql, keyValues);
        }

        /// <summary>
        /// 获取指定对象所有集合
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> GetList()
        {
            string sql = $"select {GetColumns(typeof(T), string.Empty, true)} from [{GetTableAttribute(typeof(T)).TableName}]";
            if (IsSoftDelete)
            {
                sql += " Where IsDeleted=0";
            }
            return GetList(sql);
        }

        /// <summary>
        /// 查询sql语句返回结果集
        /// </summary>
        /// <param name="sql">需要查询的sql语句</param>
        /// <returns></returns>
        public IEnumerable<T> GetList(string sql)
        {
            return GetList<T>(sql, null);
        }

        /// <summary>
        /// 查询sql语句返回结果集
        /// </summary>
        /// <param name="sql">需要查询的sql语句</param>
        /// <param name="param">参数集合</param>
        /// <returns></returns>
        public IEnumerable<T> GetList(string sql, IDictionary<string, object> param)
        {
            return GetList<T>(sql, param);
        }

        /// <summary>
        /// 查询sql语句返回结果集
        /// </summary>
        /// <param name="sql">需要查询的sql语句</param>
        /// <param name="param">参数集合</param>
        /// <returns></returns>
        public IEnumerable<M> GetList<M>(string sql, IDictionary<string, object> param)
        {
            List<M> list = new List<M>();
            using (SqlDataReader reader = ExecuteReader(sql, param))
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        list.Add(ReaderToEntity<M>(reader));
                    }
                    return list;
                }
                else
                {
                    return list;
                }
            }
        }
        #endregion

        #region 分页结果
        /// <summary>
        /// 获取分页结果
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示数量</param>
        /// <param name="func">筛选条件</param>
        /// <returns>分页输出对象</returns>
        public PageResult<M> GetPagerResult<M>(int pageIndex, int pageSize, Expression<Func<M, bool>> func, string sortString = "")
        {
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            string sql = string.Empty;
            if (IsSoftDelete)
            {
                sql = $"select {GetColumns(typeof(M), string.Empty, true)} from [{GetTableAttribute(typeof(M)).TableName}] where IsDeleted=0 And {SqlSugor.GetWhereByLambda(func, out keyValues, base.DbType)}";
            }
            else
            {
                sql = $"select {GetColumns(typeof(M), string.Empty, true)} from [{GetTableAttribute(typeof(M)).TableName}] where {SqlSugor.GetWhereByLambda(func, out keyValues, base.DbType)}";
            }
            return GetPagerResult<M>(sql, pageIndex, pageSize, keyValues, sortString);
        }

        /// <summary>
        /// 获取分页结果
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示数量</param>
        /// <param name="func">筛选条件</param>
        /// <returns>分页输出对象</returns>
        public PageResult<T> GetPagerResult(int pageIndex, int pageSize, Expression<Func<T, bool>> func, string sortString = "")
        {
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            string sql = string.Empty;
            if (IsSoftDelete)
            {
                sql = $"select {GetColumns(typeof(T), string.Empty, true)} from [{GetTableAttribute(typeof(T)).TableName}] where IsDeleted=0 AND {SqlSugor.GetWhereByLambda(func, out keyValues, base.DbType)}";
            }
            else
            {
                sql = $"select {GetColumns(typeof(T), string.Empty, true)} from [{GetTableAttribute(typeof(T)).TableName}] where {SqlSugor.GetWhereByLambda(func, out keyValues, base.DbType)}";
            }
            return GetPagerResult<T>(sql, pageIndex, pageSize, keyValues, sortString);
        }

        /// <summary>
        /// 获取分页结果
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页显示多少行数据</param>
        /// <returns>分页输出对象</returns>
        public PageResult<T> GetPagerResult(int pageIndex, int pageSize, string sortString = "")
        {
            string sql = string.Empty;
            if (IsSoftDelete)
            {
                sql = $"select {GetColumns(typeof(T), "", true)} from [{GetTableAttribute(typeof(T)).TableName}] Where IsDeleted=0";
            }
            else
            {
                sql = $"select {GetColumns(typeof(T), "", true)} from [{GetTableAttribute(typeof(T)).TableName}]";
            }
            return GetPagerResult<T>(sql, pageIndex, pageSize, null, sortString);
        }

        /// <summary>
        /// 获取分页结果
        /// </summary>
        /// <param name="sql">要查询的sql语句</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页显示多少行数据</param>
        /// <returns>分页输出对象</returns>
        public PageResult<T> GetPagerResult(string sql, int pageIndex, int pageSize, string sortString = "")
        {
            return GetPagerResult<T>(sql, pageIndex, pageSize, null, sortString);
        }

        /// <summary>
        /// 获取分页结果
        /// </summary>
        /// <param name="sql">要查询的sql语句</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页显示多少行数据</param>
        /// <param name="param">分页输出对象</param>
        /// <returns></returns>
        public PageResult<T> GetPagerResult(string sql, int pageIndex, int pageSize, IDictionary<string, object> param, string sortString = "")
        {
            return GetPagerResult<T>(sql, pageIndex, pageSize, param, sortString);
        }

        /// <summary>
        /// 获取分页结果
        /// </summary>
        /// <param name="sql">要查询的sql语句</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页显示多少行数据</param>
        /// <param name="param"></param>
        /// <returns>分页输出对象</returns>
        public PageResult<M> GetPagerResult<M>(string sql, int pageIndex, int pageSize, IDictionary<string, object> param, string sortString = "")
        {
            if (pageIndex != 0)
            {
                pageIndex = --pageIndex;
            }
            PageResult<M> lastResult = new PageResult<M>();
            string sort = " order by (select 1) ";
            if (!string.IsNullOrEmpty(sortString))
            {
                sort = $"ORDER BY {sortString}";
            }
            int start = pageIndex * pageSize + 1;
            int end = (pageIndex + 1) * pageSize;
            string sqlex =
                $@"SELECT * FROM (SELECT *,ROW_NUMBER() OVER({sort}) AS Temp_ROWNUM_Temp
                        FROM({sql}) AS A ) AS B
                        WHERE Temp_ROWNUM_Temp BETWEEN {start} AND { end}";
            SqlDataReader reader = ExecuteReader(sqlex, param);
            lastResult.Items = ReaderToList<M>(reader);
            //获取总记录数
            string sql2 = $"SELECT COUNT(*) FROM ({sql}) AS TEMP";
            object value = ExecuteScalar(sql2, param);
            lastResult.TotalCount = Convert.ToInt32(value);
            lastResult.PageIndex = pageIndex;
            lastResult.PageSize = pageSize;
            lastResult.PageCount = Convert.ToInt32(Math.Ceiling(lastResult.TotalCount * 1.0 / pageSize));
            return lastResult;
        }
        #endregion

        #region 分页列表
        /// <summary>
        /// 获取分页结果
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示数量</param>
        /// <param name="func">筛选条件</param>
        /// <returns>分页输出对象</returns>
        public IEnumerable<M> GetPagerList<M>(int pageIndex, int pageSize, Expression<Func<M, bool>> func, string sortString = "")
        {
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            string sql = string.Empty;
            if (IsSoftDelete)
            {
                sql = $"select {GetColumns(typeof(M), string.Empty, true)} from [{GetTableAttribute(typeof(M)).TableName}] where IsDeleted=0 And  {SqlSugor.GetWhereByLambda(func, out keyValues, base.DbType)}";
            }
            else
            {
                sql = $"select {GetColumns(typeof(M), string.Empty, true)} from [{GetTableAttribute(typeof(M)).TableName}] where {SqlSugor.GetWhereByLambda(func, out keyValues, base.DbType)}";
            }
            return GetPagerList<M>(sql, pageIndex, pageSize, keyValues, sortString);
        }

        /// <summary>
        /// 获取分页结果
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示数量</param>
        /// <param name="func">筛选条件</param>
        /// <returns>分页输出对象</returns>
        public IEnumerable<T> GetPagerList(int pageIndex, int pageSize, Expression<Func<T, bool>> func, string sortString = "")
        {
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            string sql = string.Empty;
            if (IsSoftDelete)
            {
                sql = $"select {GetColumns(typeof(T), string.Empty, true)} from [{GetTableAttribute(typeof(T)).TableName}] where IsDeleted=0 And {SqlSugor.GetWhereByLambda(func, out keyValues, base.DbType)}";
            }
            else
            {
                sql = $"select {GetColumns(typeof(T), string.Empty, true)} from [{GetTableAttribute(typeof(T)).TableName}] where {SqlSugor.GetWhereByLambda(func, out keyValues, base.DbType)}";
            }
            return GetPagerList<T>(sql, pageIndex, pageSize, keyValues, sortString);
        }

        /// <summary>
        /// 获取分页结果
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页显示多少行数据</param>
        /// <returns>分页输出对象</returns>
        public IEnumerable<T> GetPagerList(int pageIndex, int pageSize, string sortString = "")
        {
            string sql = string.Empty;
            if (IsSoftDelete)
            {
                sql = $"select {GetColumns(typeof(T), "", true)} from [{GetTableAttribute(typeof(T)).TableName}] Where IsDeleted=0";
            }
            else
            {
                sql = $"select {GetColumns(typeof(T), "", true)} from [{GetTableAttribute(typeof(T)).TableName}]";
            }
            return GetPagerList<T>(sql, pageIndex, pageSize, null, sortString);
        }

        /// <summary>
        /// 获取分页结果
        /// </summary>
        /// <param name="sql">要查询的sql语句</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页显示多少行数据</param>
        /// <returns>分页输出对象</returns>
        public IEnumerable<T> GetPagerList(string sql, int pageIndex, int pageSize, string sortString = "")
        {
            return GetPagerList<T>(sql, pageIndex, pageSize, null, sortString);
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        /// <param name="sql">要查询的sql语句</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页显示多少行数据</param>
        /// <param name="param">分页后的数据</param>
        /// <returns></returns>
        public IEnumerable<T> GetPagerList(string sql, int pageIndex, int pageSize, IDictionary<string, object> param, string sortString = "")
        {
            if (pageIndex != 0)
            {
                pageIndex = --pageIndex;
            }
            string sort = " order by (select 1) ";
            if (!string.IsNullOrEmpty(sortString))
            {
                sort = $"ORDER BY {sortString}";
            }
            int start = pageIndex * pageSize + 1;
            int end = (pageIndex + 1) * pageSize;
            string sqlex =
                $@"SELECT * FROM (SELECT *,ROW_NUMBER() OVER({sort}) AS Temp_ROWNUM_Temp
                        FROM({sql}) AS A ) AS B
                        WHERE Temp_ROWNUM_Temp BETWEEN {start} AND { end}";
            SqlDataReader reader = ExecuteReader(sqlex, param);
            return ReaderToList<T>(reader);
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        /// <param name="sql">要查询的sql语句</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页显示多少行数据</param>
        /// <param name="param">分页后的数据</param>
        /// <returns></returns>
        public IEnumerable<M> GetPagerList<M>(string sql, int pageIndex, int pageSize, IDictionary<string, object> param, string sortString = "")
        {
            if (pageIndex != 0)
            {
                pageIndex = --pageIndex;
            }
            string sort = " order by (select 1) ";
            if (!string.IsNullOrEmpty(sortString))
            {
                sort = $"ORDER BY {sortString}";
            }
            int start = pageIndex * pageSize + 1;
            int end = (pageIndex + 1) * pageSize;
            string sqlex =
                $@"SELECT * FROM (SELECT *,ROW_NUMBER() OVER({sort}) AS Temp_ROWNUM_Temp
                        FROM({sql}) AS A ) AS B
                        WHERE Temp_ROWNUM_Temp BETWEEN {start} AND { end}";
            SqlDataReader reader = ExecuteReader(sqlex, param);
            return ReaderToList<M>(reader);
        }
        #endregion

        #endregion

        #region 获取特性相关


        /// <summary>
        /// 关系转对象
        /// </summary>
        /// <param name="reader">数据阅读器</param>
        /// <returns></returns>
        private M ReaderToEntity<M>(SqlDataReader dr)
        {
            M entity = Activator.CreateInstance<M>();
            List<ColumnAttribute> list = GetColumnAttribute(entity);
            Type obj = entity.GetType();

            for (int i = 0; i < dr.FieldCount; i++)
            {
                string columnName = dr.GetName(i);
                object value = dr[columnName];
                if (value == DBNull.Value)
                {
                    value = list.Find(p => p.ColumnName == columnName).GetType().TypeInitializer;
                }
                if (list.Find(p => p.ColumnName == columnName) != null)
                {
                    PropertyInfo info = obj.GetProperty(columnName);
                    if (info == null)
                    {
                        foreach (PropertyInfo item in obj.GetProperties())
                        {
                            ColumnAttribute attr = item.GetCustomAttribute<ColumnAttribute>();
                            if (attr != null && attr.ColumnName == columnName)
                            {
                                obj.GetProperty(item.Name).SetValue(entity, value);
                            }
                        }
                    }
                    else
                    {
                        obj.GetProperty(columnName).SetValue(entity, value);
                    }
                }
            }
            return entity;
        }

        /// <summary>
        /// 关系转对象集合
        /// </summary>
        /// <param name="dr">数据阅读器</param>
        /// <returns></returns>
        private IEnumerable<M> ReaderToList<M>(SqlDataReader dr)
        {
            List<M> list = new List<M>();
            while (dr.Read())
            {
                list.Add(ReaderToEntity<M>(dr));
            }
            dr.Close();
            dr.Dispose();
            return list;
        }

        /// <summary>
        /// 获取字段名称
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private string GetFieldName(PropertyInfo info)
        {
            return info.GetCustomAttribute<ColumnAttribute>().ColumnName;
        }

        /// <summary>
        /// 获取实体的TableAttribute特性
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns></returns>
        private TableAttribute GetTableAttribute(T entity)
        {
            TableAttribute tableAttribute = entity.GetType().GetCustomAttribute<TableAttribute>();
            if (tableAttribute == null)
            {
                tableAttribute = new TableAttribute(entity.GetType().Name);
            }
            return tableAttribute;
        }

        /// <summary>
        /// 获取实体的TableAttribute特性
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private TableAttribute GetTableAttribute(Type type)
        {
            TableAttribute tableAttribute = type.GetCustomAttribute<TableAttribute>();
            if (tableAttribute == null)
            {
                tableAttribute = new TableAttribute(type.Name);
            }
            return tableAttribute;
        }

        /// <summary>
        /// 获取实体的TableAttribute特性
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns></returns>
        private TableAttribute GetTableAttribute(MemberInfo entity)
        {
            return entity.GetCustomAttribute<TableAttribute>();
        }

        /// <summary>
        /// 获取ColumnAttribute的集合
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns></returns>
        public List<ColumnAttribute> GetColumnAttribute<M>(M entity)
        {
            List<ColumnAttribute> list = new List<ColumnAttribute>();
            PropertyInfo[] attrArr = entity.GetType().GetProperties();
            foreach (PropertyInfo info in attrArr)
            {
                ColumnAttribute columnAttribute = info.GetCustomAttribute<ColumnAttribute>();
                if (columnAttribute == null)
                {
                    list.Add(new ColumnAttribute(info.Name));
                }
                else
                {
                    list.Add(columnAttribute);
                }
            }
            return list;
        }

        /// <summary>
        /// 获取所有的ColumnName多个以,分割；已经去除末尾,
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="prefix">前缀如@</param>
        /// <param name="containsReadonly">是否包含只读属性，默认不包含</param>
        /// <returns></returns>
        public string GetColumns(Type type, string prefix, bool containsID = false, bool containsReadonly = false)
        {
            StringBuilder sb = new StringBuilder();
            PropertyInfo[] attrArr = type.GetProperties();
            foreach (PropertyInfo info in attrArr)
            {
                ColumnAttribute dataField = info.GetCustomAttribute<ColumnAttribute>();
                if (dataField == null)
                {
                    dataField = new ColumnAttribute(info.Name);
                }
                //包含PrimaryKey
                if (containsID)
                {
                    //不包含Readonly
                    if (containsReadonly)
                    {
                        sb.Append(prefix);
                        sb.Append(dataField.ColumnName);
                        sb.Append(',');
                    }
                    else//全部
                    {
                        if (dataField.ColumnType != ColumnType.ReadOnly)
                        {
                            sb.Append(prefix);
                            sb.Append(dataField.ColumnName);
                            sb.Append(',');
                        }
                    }
                }
                else//不包含PrimaryKey
                {
                    //不包含Readonly
                    if (containsReadonly)
                    {
                        sb.Append(prefix);
                        sb.Append(dataField.ColumnName);
                        sb.Append(',');
                    }
                    else//全部
                    {
                        if (dataField.ColumnType != ColumnType.ReadOnly && dataField.ColumnType != ColumnType.PrimaryKey)
                        {
                            sb.Append(prefix);
                            sb.Append(dataField.ColumnName);
                            sb.Append(',');
                        }
                    }
                }
            }
            return sb.ToString().Trim(',');
        }

        /// <summary>
        /// 获取所有的ColumnName多个以,分割；已经去除末尾,
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="prefix">前缀如@</param>
        /// <param name="containsReadonly">是否包含只读属性，默认不包含</param>
        /// <returns></returns>
        public string GetColumns(T entity, string prefix, bool containsID = false, bool containsReadonly = false)
        {
            List<ColumnAttribute> listColumnAttribute = GetColumnAttribute(entity);
            StringBuilder sb = new StringBuilder();
            if (listColumnAttribute.Count > 0)
            {
                for (int i = 0; i < listColumnAttribute.Count; i++)
                {
                    ColumnAttribute dataField = listColumnAttribute[i];
                    //包含PrimaryKey
                    if (containsID)
                    {
                        //不包含Readonly
                        if (containsReadonly)
                        {
                            sb.Append(prefix);
                            sb.Append(dataField.ColumnName);
                            sb.Append(',');
                        }
                        else//全部
                        {
                            if (dataField.ColumnType != ColumnType.ReadOnly)
                            {
                                sb.Append(prefix);
                                sb.Append(dataField.ColumnName);
                                sb.Append(',');
                            }
                        }
                    }
                    else//不包含PrimaryKey
                    {
                        //不包含Readonly
                        if (containsReadonly)
                        {
                            sb.Append(prefix);
                            sb.Append(dataField.ColumnName);
                            sb.Append(',');
                        }
                        else//全部
                        {
                            if (dataField.ColumnType != ColumnType.ReadOnly && dataField.ColumnType != ColumnType.PrimaryKey)
                            {
                                sb.Append(prefix);
                                sb.Append(dataField.ColumnName);
                                sb.Append(',');
                            }
                        }
                    }
                }
            }
            return sb.ToString().Trim(',');
        }

        /// <summary>
        /// 获取所有的ColumnName多个以,分割；已经去除末尾,
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="prefix">前缀如@</param>
        /// <param name="containsReadonly">是否包含只读属性，默认不包含</param>
        /// <returns></returns>
        public string GetColumns<M>(M entity, string prefix, bool containsID = false, bool containsReadonly = false)
        {
            List<ColumnAttribute> listColumnAttribute = GetColumnAttribute(entity);
            StringBuilder sb = new StringBuilder();
            if (listColumnAttribute.Count > 0)
            {
                for (int i = 0; i < listColumnAttribute.Count; i++)
                {
                    ColumnAttribute dataField = listColumnAttribute[i];
                    //包含PrimaryKey
                    if (containsID)
                    {
                        //不包含Readonly
                        if (containsReadonly)
                        {
                            sb.Append(prefix);
                            sb.Append(dataField.ColumnName);
                            sb.Append(',');
                        }
                        else//全部
                        {
                            if (dataField.ColumnType != ColumnType.ReadOnly)
                            {
                                sb.Append(prefix);
                                sb.Append(dataField.ColumnName);
                                sb.Append(',');
                            }
                        }
                    }
                    else//不包含PrimaryKey
                    {
                        //不包含Readonly
                        if (containsReadonly)
                        {
                            sb.Append(prefix);
                            sb.Append(dataField.ColumnName);
                            sb.Append(',');
                        }
                        else//全部
                        {
                            if (dataField.ColumnType != ColumnType.ReadOnly && dataField.ColumnType != ColumnType.PrimaryKey)
                            {
                                sb.Append(prefix);
                                sb.Append(dataField.ColumnName);
                                sb.Append(',');
                            }
                        }
                    }
                }
            }
            return sb.ToString().Trim(',');
        }

        /// <summary>
        /// 获取主键
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns></returns>
        public string GetPrimaryKey(T entity)
        {
            PropertyInfo[] attrArr = entity.GetType().GetProperties();
            string key = string.Empty;
            foreach (PropertyInfo info in attrArr)
            {
                ColumnAttribute dataField = info.GetCustomAttribute<ColumnAttribute>();
                if (dataField != null && dataField.ColumnType == ColumnType.PrimaryKey)
                {
                    key = dataField.ColumnName;
                }
            }
            return key;
        }

        /// <summary>
        /// 获取当前T类型的主键
        /// </summary>
        /// <returns></returns>
        public string GetPrimaryKey()
        {
            PropertyInfo[] attrArr = typeof(T).GetProperties();
            string key = string.Empty;
            foreach (PropertyInfo info in attrArr)
            {
                ColumnAttribute dataField = info.GetCustomAttribute<ColumnAttribute>();
                if (dataField != null && dataField.ColumnType == ColumnType.PrimaryKey)
                {
                    key = dataField.ColumnName;
                }
            }
            return key;
        }

        /// <summary>
        /// 获取主键的值
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns></returns>
        private IDictionary<string, object> GetPrimaryKeyValue(T entity)
        {
            PropertyInfo[] attrArr = entity.GetType().GetProperties();
            IDictionary<string, object> ht = new Dictionary<string, object>();
            foreach (PropertyInfo info in attrArr)
            {
                ColumnAttribute dataField = info.GetCustomAttribute<ColumnAttribute>();
                if (dataField != null && dataField.ColumnType == ColumnType.PrimaryKey)
                {
                    ht.Add(dataField.ColumnName, entity.GetType().GetProperty(dataField.ColumnName).GetValue(entity));
                }
            }
            return ht;
        }

        /// <summary>
        /// 获取对象的值，并以键值对方式返回
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="containsReadonly">是否包含只读属性，默认不包含</param>
        /// <returns></returns>
        private IDictionary<string, object> GetColumnValue(T entity, string prefix, bool containsID = false, bool containsReadonly = false)
        {
            PropertyInfo[] attrArr = entity.GetType().GetProperties();
            IDictionary<string, object> ht = new Dictionary<string, object>();
            foreach (PropertyInfo info in attrArr)
            {
                object value = DBNull.Value;
                Type targetType = null;
                ColumnAttribute item = info.GetCustomAttribute<ColumnAttribute>();
                if (item == null)
                {
                    item = new ColumnAttribute(info.Name);
                    value = entity.GetType().GetProperty(item.ColumnName).GetValue(entity);
                    targetType = entity.GetType().GetProperty(item.ColumnName).GetType();
                }
                else if (item.ColumnName != info.Name)
                {
                    if (item.ColumnType != ColumnType.ReadOnly)
                    {
                        value = info.GetValue(entity);
                        targetType = info.GetType();
                    }
                }
                else
                {
                    value = entity.GetType().GetProperty(item.ColumnName).GetValue(entity);
                    targetType = entity.GetType().GetProperty(item.ColumnName).GetType();
                }
                if (value == null)
                {
                    if (targetType.IsValueType)
                    {
                        value = Activator.CreateInstance(targetType);
                    }
                }
                #region 赋值
                //包含PrimaryKey
                if (containsID)
                {
                    //包含只读
                    if (containsReadonly)
                    {
                        ht.Add(prefix + item.ColumnName, value);
                    }
                    else//不包含只读
                    {
                        if (item.ColumnType != ColumnType.ReadOnly)
                        {
                            ht.Add(prefix + item.ColumnName, value);
                        }
                    }
                }
                else//不包含PrimaryKey
                {
                    //包含只读
                    if (containsReadonly)
                    {
                        if (item.ColumnType != ColumnType.PrimaryKey)
                        {
                            ht.Add(prefix + item.ColumnName, value);
                        }
                    }
                    else//不包含只读
                    {
                        if (item.ColumnType != ColumnType.ReadOnly && item.ColumnType != ColumnType.PrimaryKey)
                        {
                            ht.Add(prefix + item.ColumnName, value);
                        }
                    }
                }
                #endregion
            }
            return ht;
        }

        /// <summary>
        /// 获取主键的值并以键值对的方式存入IDictionary<string, object>
        /// </summary>
        /// <returns></returns>
        private IDictionary<string, object> GetPrimaryKeyValue()
        {
            PropertyInfo[] attrArr = typeof(T).GetProperties();
            IDictionary<string, object> ht = new Dictionary<string, object>();
            T entity = Activator.CreateInstance<T>();
            foreach (PropertyInfo info in attrArr)
            {
                ColumnAttribute item = info.GetCustomAttribute<ColumnAttribute>();
                if (item != null && item.ColumnType == ColumnType.PrimaryKey)
                {
                    object value = typeof(T).GetProperty(item.ColumnName).GetValue(entity);
                    if (value == null)
                    {
                        value = DBNull.Value;
                    }
                    ht.Add(item.ColumnName, value);
                }
            }
            return ht;
        }

        /// <summary>
        /// 获取主键的值
        /// </summary>
        /// <returns></returns>
        private object GetPrimaryValue(T entity)
        {
            PropertyInfo[] attrArr = entity.GetType().GetProperties();
            object val = null;
            foreach (PropertyInfo info in attrArr)
            {
                ColumnAttribute item = info.GetCustomAttribute<ColumnAttribute>();
                if (item != null && item.ColumnType == ColumnType.PrimaryKey)
                {
                    val = typeof(T).GetProperty(item.ColumnName).GetValue(entity);
                }
            }
            return val;
        }

        /// <summary>
        /// 获取属性的值并以键值对的方式存入IDictionary<string, object>
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="containsReadonly">是否包含只读属性，默认不包含</param>
        /// <returns></returns>
        private IDictionary<string, object> GetValues(T entity, bool containsReadonly = false)
        {
            IDictionary<string, object> ht = new Dictionary<string, object>();
            List<ColumnAttribute> listColumnAttribute = GetColumnAttribute(entity);
            if (listColumnAttribute.Count > 0)
            {
                for (int i = 0; i < listColumnAttribute.Count; i++)
                {
                    ColumnAttribute dataField = listColumnAttribute[i];
                    //不包含Readonly
                    if (containsReadonly)
                    {
                        if (dataField.ColumnType != ColumnType.None)
                        {
                            ht.Add(dataField.ColumnName, entity.GetType().GetProperty(dataField.ColumnName).GetValue(entity));
                        }
                    }
                    else//全部
                    {
                        ht.Add(dataField.ColumnName, entity.GetType().GetProperty(dataField.ColumnName).GetValue(entity));
                    }
                }
            }
            return ht;
        }
        #endregion
    }
}
