using System;

namespace FeatherSQL
{
    public class CreateEntity
    {
        /// <summary>
        /// 创建人ID
        /// </summary>
        int CreateUserID { get; set; }

        /// <summary>
        /// 创建人姓名
        /// </summary>
        string CreateUserName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        DateTime CreateTime { get; set; }
    }
}
