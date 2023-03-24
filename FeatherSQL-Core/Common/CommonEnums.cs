using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatherSQL
{
    /// <summary>
    /// 指定列的类型
    /// </summary>
    public enum ColumnType
    {
        /// <summary>
        /// int自增长主键
        /// </summary>
        PrimaryKey,

        /// <summary>
        /// 只读属性，仅查询时赋值，增删不做操作
        /// </summary>
        ReadOnly,

        /// <summary>
        /// 默认
        /// </summary>
        None
    }

    /// <summary>
    /// 数据库类型枚举
    /// </summary>
    public enum DbProviderType : byte
    {
        SqlServer = 0,
        MySql = 1
    }
}
