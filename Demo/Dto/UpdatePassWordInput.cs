using FeatherSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo
{
    /// <summary>
    /// 用于修改密码的输入对象
    /// </summary>
    [Table("UserInfo")]
    public class UpdatePassWordInput
    {
        /// <summary>
        /// int自增长主键
        /// </summary>
        [Column("Id", ColumnType.PrimaryKey)]
        public int Id { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string PassWord { get; set; }
    }
}
