using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ProjectStructure.Share.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// 將字串使用 SHA256 雜湊演算法處理後輸出十六進位表示
        /// </summary>
        /// <param name="value">原始字串</param>
        /// <returns>SHA256 雜湊後的十六進位字串 (小寫)</returns>
        /// <exception cref="ArgumentNullException">輸入字串為 NULL</exception>
        public static string ToSHA256(this string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            using (SHA256 hash = SHA256.Create())
            {
                byte[] hashBytes = hash.ComputeHash(Encoding.UTF8.GetBytes(value));
                return Convert.ToHexString(hashBytes).ToLower();
            }
        }
    }
}
