using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FeatherSQL.Common
{
    public static class GetConfig
    {

        public static string GetConnectionString()
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;

            File.ReadAllText(basePath);
            //读取json
            //序列化为dynamic
            //获取对应的值
        }
    }
}
