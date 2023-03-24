using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FeatherSQL
{
    /// <summary>
    /// 异步方法
    /// </summary>
    public partial class BaseDAO<T>
    {
        #region 自定义Delete相关方法

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="func">筛选条件</param>
        /// <returns></returns>
        public async Task<int> DeleteAsync(Expression<Func<T, bool>> func)
        {
            return await DeleteAsync<T>(func);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="func">筛选条件</param>
        /// <returns></returns>
        public async Task<int> DeleteAsync<M>(Expression<Func<M, bool>> func)
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
                return await ExecuteNonQueryAsync(sql, keyValues);
            }
            else
            {
                return await ExecuteNonQueryAsync(sql);
            }
        }

        /// <summary>
        /// 删除单个
        /// </summary>
        /// <param name="entity">实体对象（必须主键必须赋值）</param>
        /// <returns>受影响的行数</returns>
        public async Task<int> DeleteAsync(T entity)
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
            return await ExecuteNonQueryAsync(sb.ToString(), GetPrimaryKeyValue(entity));
        }

        /// <summary>
        /// 删除单个
        /// </summary>
        /// <param name="primaryKey">主键ID</param>
        /// <returns>受影响的行数</returns>
        public async Task<int> DeleteAsync(object primaryKey)
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
            return await ExecuteNonQueryAsync(sb.ToString(), keyValues);
        }

        /// <summary>
        /// 删除多条数据
        /// </summary>
        /// <param name="primaryKeys">多个ID以,隔开</param>
        /// <returns>受影响的行数</returns>
        public int DeleteListAsync(string primaryKeys)
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
        /// <param name="arrPrimaryKey">主键ID的数组</param>
        /// <typeparam name="K">主键类型</typeparam>
        /// <returns>受影响的行数</returns>
        public async Task<int> DeleteListAsync<K>(IEnumerable<K> arrPrimaryKey)
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
            return await ExecuteNonQueryAsync(sb.ToString());
        }

        /// <summary>
        /// 删除多条数据
        /// </summary>
        /// <param name="list">对象的集合</param>
        /// <returns>受影响的行数</returns>
        public async Task<int> DeleteListAsync(IEnumerable<T> list)
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
            return await ExecuteNonQueryAsync(sb.ToString());
        }
        #endregion

        #region 自定义Insert相关方法
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>返回自增的主键ID</returns>
        public async Task<int> InsertAsync(T entity)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"insert into [{GetTableAttribute(entity).TableName}] ({GetColumns(entity, "")}) values ({GetColumns(entity, "@")} ) select SCOPE_IDENTITY()");
            return Convert.ToInt32(await ExecuteScalarAsync(sb.ToString(), GetColumnValue(entity, "", false)));
        }

        /// <summary>
        /// 添加多个
        /// </summary>
        /// <param name="list">对象的集合</param>
        /// <returns>受影响的行数</returns>
        public async Task<int> InsertListAsync(IList<T> list)
        {
            StringBuilder sb = new StringBuilder();
            IDictionary<string, object> ht = new Dictionary<string, object>();
            for (int i = 0; i < list.Count; i++)
            {
                var entity = list[i];
                sb.Append($"insert into [{GetTableAttribute(entity).TableName}] ({GetColumns(entity, string.Empty)})values({GetColumns(entity, "@_" + i)}) ");
                foreach (var item in GetColumnValue(entity, "_" + i, false))
                {
                    ht.Add(item.Key, item.Value);
                }
            }
            return await ExecuteNonQueryAsync(sb.ToString(), ht);
        }
        #endregion

        #region 自定义Update相关方法

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="entity">要修改的实体</param>
        /// <returns>受影响的行数</returns>
        public async Task<int> UpdateAsync(T entity)
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
            return await ExecuteNonQueryAsync(sb.ToString(), GetColumnValue(entity, "", true));
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="entity">要修改的实体</param>
        /// <param name="func">筛选条件</param>
        /// <returns>受影响的行数</returns>
        public async Task<int> UpdateAsync(T entity, Expression<Func<T, bool>> func)
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
            return await ExecuteNonQueryAsync(sb.ToString(), keyValues);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="obj">要修改的对象（推荐匿名对象、DTO）</param>
        /// <param name="func">筛选条件</param>
        /// <returns>受影响的行数</returns>
        public async Task<int> UpdateAsync(object obj, Expression<Func<T, bool>> func)
        {
            return await UpdateAsync<T>(obj, func);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <typeparam name="M">指定修改数据实体</typeparam>
        /// <param name="obj">要修改的对象（推荐匿名对象、DTO）</param>
        /// <param name="func">筛选条件</param>
        /// <returns>受影响的行数</returns>
        public async Task<int> UpdateAsync<M>(object obj, Expression<Func<M, bool>> func)
        {
            var m = Activator.CreateInstance<M>();
            StringBuilder sb = new StringBuilder();
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            string key = GetPrimaryKey();
            sb.Append($"update [{GetTableAttribute(m.GetType()).TableName}] set ");
            Dictionary<string, object> parmares = new Dictionary<string, object>();
            foreach (PropertyInfo item in obj.GetType().GetProperties())
            {
                sb.Append($"{item.Name}=@{item.Name},");
                parmares.Add(item.Name, item.GetValue(obj));
            }
            string s = sb.ToString().Trim(',');
            sb.Clear();
            sb.Append(s);
            sb.Append($" where {SqlSugor.GetWhereByLambda(func, out keyValues, base.DbType)}");
            keyValues = keyValues.Union(parmares).ToDictionary(x => x.Key, x => x.Value);
            return await ExecuteNonQueryAsync(sb.ToString(), keyValues);
        }

        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="list">要修改的实体集合</param>
        /// <returns>受影响的行数</returns>
        public async Task<int> UpdateListAsync(IList<T> list)
        {
            StringBuilder sb = new StringBuilder();
            IDictionary<string, object> ht = new Dictionary<string, object>();
            for (int i = 0; i < list.Count; i++)
            {
                var entity = list[i];
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
                foreach (var de in GetColumnValue(entity, "_" + i, true))
                {
                    ht.Add(de.Key, de.Value);
                }
            }
            return await ExecuteNonQueryAsync(sb.ToString(), ht);
        }
        #endregion

        #region 自定义Query相关方法

        #region 获取单个
        /// <summary>
        /// 获取指定类型单个实体对象
        /// </summary>
        /// <param name="func">筛选条件</param>
        /// <returns></returns>
        public async Task<M> GetAsync<M>(Expression<Func<M, bool>> func)
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
            using (SqlDataReader reader = await ExecuteReaderAsync(sql, keyValues))
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
        public async Task<T> GetAsync(Expression<Func<T, bool>> func)
        {
            return await GetAsync<T>(func);
        }

        /// <summary>
        /// 获取单个实体
        /// </summary>
        /// <param name="primaryKey">主键ID</param>
        /// <returns>返回查询到的实体对象</returns>
        public async Task<T> GetAsync(object primaryKey)
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
            using (SqlDataReader reader =await ExecuteReaderAsync(sb.ToString(), ht))
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
                    return default(T);
                }
            }
        }

        /// <summary>
        /// 获取指定类型的单个实体
        /// </summary>
        /// <typeparam name="M">指定的类型（推荐为DTO）</typeparam>
        /// <param name="sql">查询的SQL语句</param>
        /// <returns></returns>
        public async Task<M> GetAsync<M>(string sql)
        {
            return await GetAsync<M>(sql, null);
        }

        /// <summary>
        /// 获取指定类型的单个实体
        /// </summary>
        /// <typeparam name="M">指定的类型（推荐为DTO）</typeparam>
        /// <param name="sql">查询的SQL语句</param>
        /// <returns></returns>
        public async Task<T> GetAsync(string sql)
        {
            return await GetAsync<T>(sql);
        }

        /// <summary>
        /// 获取指定类型的单个实体
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<T> GetAsync(string sql, Dictionary<string, object> param)
        {
            return await GetAsync<T>(sql, param);
        }

        /// <summary>
        /// 获取指定类型的单个实体
        /// </summary>
        /// <typeparam name="M">指定的类型（推荐为DTO）</typeparam>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<M> GetAsync<M>(string sql, Dictionary<string, object> param)
        {
            M entity = Activator.CreateInstance<M>();
            using (SqlDataReader reader =await ExecuteReaderAsync(sql, param))
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
        public async Task<int> GetCountAsync()
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
            return (int)await ExecuteScalarAsync(sql);
        }

        /// <summary>
        /// 获取数量
        /// </summary>
        /// <param name="sql">查询的SQL语句（需要count函数）</param>
        /// <returns>查询到的数量</returns>
        public async Task<int> GetCountAsync(string sql)
        {
            return (int)await ExecuteScalarAsync(sql);
        }

        /// <summary>
        /// 获取数量
        /// </summary>
        /// <param name="sql">查询的SQL语句（需要count函数）</param>
        /// <param name="param">SQL中需要的参数</param>
        /// <returns>查询到的数量</returns>
        public async Task<int> GetCountAsync(string sql, IDictionary<string, object> param)
        {
            return (int)await ExecuteScalarAsync(sql, param);
        }

        /// <summary>
        /// 获取数量
        /// </summary>
        /// <param name="func">筛选条件</param>
        /// <returns>查询到的数量</returns>
        public async Task<int> GetCountAsync(Expression<Func<T, bool>> func)
        {
            return await GetCountAsync<T>(func);
        }

        /// <summary>
        /// 获取数量
        /// </summary>
        /// <typeparam name="M">指定的类型（推荐为DTO或Entity）</typeparam>
        /// <param name="func">筛选条件</param>
        /// <returns>查询到的数量</returns>
        public async Task<int> GetCountAsync<M>(Expression<Func<M, bool>> func)
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
            return  (int)await ExecuteScalarAsync(sql, keyValues);
        }
        #endregion

        #region 获取列表
        /// <summary>
        /// 获取指定类型的列表
        /// </summary>
        /// <param name="func">筛选条件</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetListAysnc(Expression<Func<T, bool>> func)
        {
            return await GetListAysnc<T>(func);
        }

        /// <summary>
        /// 获取指定类型的列表
        /// </summary>
        /// <param name="func">筛选条件</param>
        /// <returns></returns>
        public async Task<IEnumerable<M>> GetListAysnc<M>(Expression<Func<M, bool>> func)
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
            return await GetListAysnc<M>(sql, keyValues);
        }

        /// <summary>
        /// 获取指定对象所有集合
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetListAysnc()
        {
            string sql = $"select {GetColumns(typeof(T), string.Empty, true)} from [{GetTableAttribute(typeof(T)).TableName}]";
            if (IsSoftDelete)
            {
                sql += " Where IsDeleted=0";
            }
            return await GetListAysnc(sql);
        }

        /// <summary>
        /// 查询sql语句返回结果集
        /// </summary>
        /// <param name="sql">需要查询的sql语句</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetListAysnc(string sql)
        {
            return await GetListAysnc<T>(sql, null);
        }

        /// <summary>
        /// 查询sql语句返回结果集
        /// </summary>
        /// <param name="sql">需要查询的sql语句</param>
        /// <param name="param">参数集合</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetListAysnc(string sql, IDictionary<string, object> param)
        {
            return await GetListAysnc<T>(sql, param);
        }

        /// <summary>
        /// 查询sql语句返回结果集
        /// </summary>
        /// <param name="sql">需要查询的sql语句</param>
        /// <param name="param">参数集合</param>
        /// <returns></returns>
        public async Task<IEnumerable<M>> GetListAysnc<M>(string sql, IDictionary<string, object> param)
        {
            List<M> list = new List<M>();
            using (SqlDataReader reader =await ExecuteReaderAsync(sql, param))
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
        public async Task<PageResult<M>> GetPagerResultAsync<M>(int pageIndex, int pageSize, Expression<Func<M, bool>> func, string sortString = "")
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
            return await GetPagerResultAsync<M>(sql, pageIndex, pageSize, keyValues, sortString);
        }

        /// <summary>
        /// 获取分页结果
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示数量</param>
        /// <param name="func">筛选条件</param>
        /// <returns>分页输出对象</returns>
        public async Task<PageResult<T>> GetPagerResultAsync(int pageIndex, int pageSize, Expression<Func<T, bool>> func, string sortString = "")
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
            return await GetPagerResultAsync<T>(sql, pageIndex, pageSize, keyValues, sortString);
        }

        /// <summary>
        /// 获取分页结果
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页显示多少行数据</param>
        /// <returns>分页输出对象</returns>
        public async Task<PageResult<T>> GetPagerResultAsync(int pageIndex, int pageSize, string sortString = "")
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
            return await GetPagerResultAsync<T>(sql, pageIndex, pageSize, null, sortString);
        }

        /// <summary>
        /// 获取分页结果
        /// </summary>
        /// <param name="sql">要查询的sql语句</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页显示多少行数据</param>
        /// <returns>分页输出对象</returns>
        public async Task<PageResult<T>> GetPagerResultAsync(string sql, int pageIndex, int pageSize, string sortString = "")
        {
            return await GetPagerResultAsync<T>(sql, pageIndex, pageSize, null, sortString);
        }

        /// <summary>
        /// 获取分页结果
        /// </summary>
        /// <param name="sql">要查询的sql语句</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页显示多少行数据</param>
        /// <param name="param">分页输出对象</param>
        /// <returns></returns>
        public async Task<PageResult<T>> GetPagerResultAsync(string sql, int pageIndex, int pageSize, IDictionary<string, object> param, string sortString = "")
        {
            return await GetPagerResultAsync<T>(sql, pageIndex, pageSize, param, sortString);
        }

        /// <summary>
        /// 获取分页结果
        /// </summary>
        /// <param name="sql">要查询的sql语句</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页显示多少行数据</param>
        /// <param name="param"></param>
        /// <returns>分页输出对象</returns>
        public async Task<PageResult<M>> GetPagerResultAsync<M>(string sql, int pageIndex, int pageSize, IDictionary<string, object> param, string sortString = "")
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
            SqlDataReader reader = await ExecuteReaderAsync(sqlex, param);
            lastResult.Items = ReaderToList<M>(reader);
            //获取总记录数
            string sql2 = $"SELECT COUNT(*) FROM ({sql}) AS TEMP";
            object value =await ExecuteScalarAsync(sql2, param);
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
        public async Task<IEnumerable<M>> GetPagerListAsync<M>(int pageIndex, int pageSize, Expression<Func<M, bool>> func, string sortString = "")
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
            return await GetPagerListAsync<M>(sql, pageIndex, pageSize, keyValues, sortString);
        }

        /// <summary>
        /// 获取分页结果
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示数量</param>
        /// <param name="func">筛选条件</param>
        /// <returns>分页输出对象</returns>
        public async Task<IEnumerable<T>> GetPagerListAsync(int pageIndex, int pageSize, Expression<Func<T, bool>> func, string sortString = "")
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
            return await GetPagerListAsync<T>(sql, pageIndex, pageSize, keyValues, sortString);
        }

        /// <summary>
        /// 获取分页结果
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页显示多少行数据</param>
        /// <returns>分页输出对象</returns>
        public async Task<IEnumerable<T>> GetPagerListAsync(int pageIndex, int pageSize, string sortString = "")
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
            return await GetPagerListAsync<T>(sql, pageIndex, pageSize, null, sortString);
        }

        /// <summary>
        /// 获取分页结果
        /// </summary>
        /// <param name="sql">要查询的sql语句</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页显示多少行数据</param>
        /// <returns>分页输出对象</returns>
        public async Task<IEnumerable<T>> GetPagerListAsync(string sql, int pageIndex, int pageSize, string sortString = "")
        {
            return await GetPagerListAsync<T>(sql, pageIndex, pageSize, null, sortString);
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        /// <param name="sql">要查询的sql语句</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页显示多少行数据</param>
        /// <param name="param">分页后的数据</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetPagerListAsync(string sql, int pageIndex, int pageSize, IDictionary<string, object> param, string sortString = "")
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
            SqlDataReader reader = await ExecuteReaderAsync(sqlex, param);
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
        public async Task<IEnumerable<M>> GetPagerListAsync<M>(string sql, int pageIndex, int pageSize, IDictionary<string, object> param, string sortString = "")
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
            SqlDataReader reader = await ExecuteReaderAsync(sqlex, param);
            return ReaderToList<M>(reader);
        }
        #endregion
        #endregion
    }
}
