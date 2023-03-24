using Demo.Entity;
using FeatherSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Dao
{
    /// <summary>
    /// 数据访问层
    /// </summary>
    public class UserInfoDAO : BaseDAO<UserInfo>
    {
        /// <summary>
        /// 可以指定连接数据库的构造函数
        /// </summary>
        /// <param name="connectionStr">连接字符串的name</param>
        public UserInfoDAO(string connectionStr = "maindb") : base(connectionStr)
        {

        }

        /// <summary>
        /// 涉及到SQL的尽量写在DAO层，业务层直接调用
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        public UserInfoOutput GetUserInfoOutput(int id)
        {
            string sql = $"select {GetColumns<UserInfoOutput>()} from UserInfo left Join RoleInfo On UserInfo.RoleId=RoleInfo.Id where UserInfo.Id=@Id";
            Dictionary<string, object> keyValues = new Dictionary<string, object>
            {
                { "Id", 123 }
            };
            return Get<UserInfoOutput>(sql, keyValues);
        }
    }
}
