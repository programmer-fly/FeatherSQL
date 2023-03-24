using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatherSQL
{
    /// <summary>
    /// 特性，列名称
    /// </summary>
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute(string fieldName, ColumnType fieldType = ColumnType.None)
        {
            ColumnName = fieldName;
            ColumnType = fieldType;
        }

        /// <summary>
        /// 字段名称
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// 字段类型
        /// </summary>
        public ColumnType ColumnType { get; set; }
    }
}
