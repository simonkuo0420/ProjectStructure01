using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectStructure.Share.Configs
{
    public class MsSQLConfig
    {
        /// <summary>
        /// AppSetting位置
        /// </summary>
        public const string Position = nameof(MsSQLConfig);

        /// <summary>
        /// 連線字串
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;
    }
}
