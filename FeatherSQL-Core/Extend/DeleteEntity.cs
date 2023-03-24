using System;

namespace FeatherSQL
{
    public class DeleteEntity
    {
        /// <summary>
        /// 删除时间
        /// </summary>
        DateTime DeleteTime { get; set; }

        /// <summary>
        /// 删除人ID
        /// </summary>
        int DeleteUserID { get; set; }

        /// <summary>
        /// 删除人姓名
        /// </summary>
        string DeleteUserName { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        bool IsDeleted { get; set; }
    }
}
