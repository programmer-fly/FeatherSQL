using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatherSQL
{
    public class BaseEntitySoftDelete : KeyEntity,ISoftDelete
    {
        /// <summary>
        /// 创建人ID
        /// </summary>
        public int CreateUserID { get; set; }

        /// <summary>
        /// 创建人姓名
        /// </summary>
        public string CreateUserName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 删除时间
        /// </summary>
        public DateTime DeleteTime { get; set; }

        /// <summary>
        /// 删除人ID
        /// </summary>
        public int DeleteUserID { get; set; }

        /// <summary>
        /// 删除人姓名
        /// </summary>
        public string DeleteUserName { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
