using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatherSQL
{
    /// <summary>
    /// 分页实体
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageResult<T>
    {
        public PageResult() { }
        public PageResult(int totalCount, IList<T> items, int pageSize, int pageIndex) { }

        /// <summary>
        /// 分页后的数据
        /// </summary>
        public IEnumerable<T> Items { get; set; }

        /// <summary>
        /// 当前页码
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 每页显示的数量
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总记录数据
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int PageCount { get; set; }
    }
}
