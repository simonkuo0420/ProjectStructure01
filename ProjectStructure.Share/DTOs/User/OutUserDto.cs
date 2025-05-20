using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectStructure.Share.DTOs.User
{
    /// <summary>
    /// 用於輸出使用者資訊的 DTO
    /// </summary>
    public class OutUserDto
    {
        /// <summary>
        /// 使用者 ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 用戶名
        /// </summary>
        public string Username { get; set; } = null!;

        /// <summary>
        /// 電子郵件
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// 名字
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// 姓氏
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// 電話號碼
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// 郵遞區號
        /// </summary>
        public string? PostalCode { get; set; }

        /// <summary>
        /// 國家
        /// </summary>
        public string? Country { get; set; }

        /// <summary>
        /// 是否活躍
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// 註冊時間
        /// </summary>
        public DateTime? RegisteredAt { get; set; }

        /// <summary>
        /// 最後登入時間
        /// </summary>
        public DateTime? LastLogin { get; set; }
    }
}
