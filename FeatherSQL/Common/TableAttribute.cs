using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatherSQL
{
    /// <summary>
    /// Table特性，用于指定class属于哪个数据表
    /// </summary>
    public class TableAttribute : Attribute
    {
        /// <summary>
        /// 用于指定数据表名称
        /// </summary>
        /// <param name="tableName"></param>
        public TableAttribute(string tableName)
        {
            TableName = tableName;
        }

        /// <summary>
        /// 数据表名称
        /// </summary>
        public string TableName { get; set; }
    }

}
