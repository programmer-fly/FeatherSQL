using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatherSQL
{
    /// <summary>
    /// 实体基类实现了：IKeyEntity，ICreateEntity，IDeleteEntity接口默认开启软删除
    /// </summary>
    public class BaseEntity : KeyEntity
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
    }
}
