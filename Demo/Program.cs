using Demo.Dao;
using Demo.Entity;
using FeatherSQL;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Demo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            /// <summary>
            /// 创建数据访问层对象
            /// </summary>
            UserInfoDAO userInfoDAO = new UserInfoDAO();

            #region Insert方法
            //这里返回的不是受影响的行数，是新增成功的Id，我们依然可以用是否大于0来判断是否操作成功
            int insertId = userInfoDAO.Insert(new UserInfo
            {
                UserName = "测试新增",
                RoleId = 1,
                Phone = "185****9782",
                PassWord = "666666"
            });

            //新增多个
            //List<UserInfo> insertUserInfoList = new List<UserInfo>();
            //for (int i = 0; i < 5; i++)
            //{
            //    insertUserInfoList.Add(new UserInfo
            //    {
            //        UserName = "批量新增"+i,
            //        RoleId = 1,
            //        Phone = "185****9782",
            //        PassWord = "666666"
            //    });
            //}
            ////这里返回的是受影响的行数，此方法一次执行，失败则回滚
            //int InsertListcount= userInfoDAO.InsertList(insertUserInfoList);
            #endregion

            #region Update方法
            //var entity = userInfoDAO.Get(66);
            //entity.PassWord = "888888";
            //entity.RoleId = 2;
            //entity.UserName = "测试修改";
            //entity.Phone = "1300000000";
            //int updateCount= userInfoDAO.Update(entity);
            //var entity = userInfoDAO.Get(66);
            //entity.PassWord = "888888";
            //entity.RoleId = 2;
            //entity.UserName = "测试修改";
            //entity.Phone = "1300000000";
            //userInfoDAO.Update<UserInfo>(entity, x => x.RoleId == 2);

            //UpdatePassWordInput input = new UpdatePassWordInput {
            //     Id=66,
            //     PassWord="123456"
            //};
            //int updateTCount =userInfoDAO.Update<UpdatePassWordInput>(input, x => x.Id == input.Id);

            //List<UserInfo> updateUserInfoList = new List<UserInfo>();
            //for (int i = 0; i < 5; i++)
            //{
            //    updateUserInfoList.Add(new UserInfo
            //    {
            //        UserName = "批量修改" + i,
            //        RoleId = 1,
            //        Phone = "185****9782",
            //        PassWord = "666666",
            //        Id = i//注意这里必须指定id
            //    });
            //}
            //int updateListCount = userInfoDAO.UpdateList(updateUserInfoList);

            #endregion

            #region Delete
            //返回受影响的行数
            //int count= userInfoDAO.Delete(123);


            //UserInfo userInfo = new UserInfo
            //{
            //    Id = 123,//id不能为空
            //    UserName = "模拟数据实体"
            //};
            //int count = userInfoDAO.Delete(userInfo);

            //如果需要删除指定条件的数据，可以使用以下方法：
            //例如删除手机号为185****9782的用户
            //userInfoDAO.Delete(x => x.Phone == "185****9782");

            //返回受影响的行数
             userInfoDAO.DeleteListAsync("1,3,4,512");

            ////模拟数据实体
            //List<UserInfo> delteList = new List<UserInfo>();
            //for (int i = 0; i < 5; i++)
            //{
            //    delteList.Add(new UserInfo
            //    {
            //        UserName = "批量删除" + i,
            //        RoleId = 1,
            //        Phone = "185****9782",
            //        PassWord = "666666",
            //        Id = i//注意这里必须指定id
            //    });
            //}
            ////返回受影响的行数
            //int count= userInfoDAO.DeleteList(delteList);

            ////模拟数据
            //List<int> idList = new List<int> {1,2,3,4};
            ////返回受影响的行数
            //int count = userInfoDAO.DeleteList(idList);

            #endregion

            #region Get方法

            //根据主键查询
            //UserInfo userInfo= userInfoDAO.Get(123);
            //UserInfo userInfo=userInfoDAO.Get(x => x.Id == 123);

            // UserInfoOutput userInfoOutput = userInfoDAO.Get<UserInfoOutput>(x => x.Id == 123);

            //UserInfoOutput userInfoOutput = userInfoDAO.GetUserInfoOutput(123);

            #endregion

            #region GetCount方法
            //int totalCount= userInfoDAO.GetCount();

            //int totalCount = userInfoDAO.GetCount(x=>x.UserName.Contains("王"));

            //Dictionary<string, object> keyValues = new Dictionary<string, object>
            //{
            //    { "RoleName", "高级会员" }
            //};
            //string sql = "Select Count(*) From UserInfo Left Join RoleInfo On UserInfo.RoleId=RoleInfo.Id Where RoleName=@RoleName";
            //int totalCount = userInfoDAO.GetCount(sql, keyValues);

            #endregion

            #region Query方法

            //var list= userInfoDAO.GetList(x=>x.RoleId==233);

            //Expression<Func<UserInfo, bool>> expression = null;
            //expression = expression.And(x => x.UserName.Contains("王"));
            //var result = userInfoDAO.GetPagerResult(0, 10, expression);

            #endregion
            Console.ReadKey();
        }
    }

    [Table("UserInfo")]
    public class UserInfoOutput
    {
        /// <summary>
        /// int自增长主键
        /// </summary>
        [Column("Id", ColumnType.PrimaryKey)]
        public int Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        public string RoleName { get; set; }
    }
}
