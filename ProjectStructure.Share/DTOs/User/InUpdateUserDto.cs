using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectStructure.Share.DTOs.User
{
    /// <summary>
    /// 用於更新使用者的輸入 DTO
    /// </summary>
    public class InUpdateUserDto
    {
        /// <summary>
        /// 使用者 ID
        /// </summary>
        [Required(ErrorMessage = "使用者 ID 是必填欄位")]
        public Guid UserId { get; set; }

        /// <summary>
        /// 用戶名
        /// </summary>
        [StringLength(50, ErrorMessage = "用戶名長度不能超過 50 個字元")]
        public string? Username { get; set; }

        /// <summary>
        /// 電子郵件
        /// </summary>
        [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
        [StringLength(100, ErrorMessage = "電子郵件長度不能超過 100 個字元")]
        public string? Email { get; set; }

        /// <summary>
        /// 名字
        /// </summary>
        [StringLength(50, ErrorMessage = "名字長度不能超過 50 個字元")]
        public string? FirstName { get; set; }

        /// <summary>
        /// 姓氏
        /// </summary>
        [StringLength(50, ErrorMessage = "姓氏長度不能超過 50 個字元")]
        public string? LastName { get; set; }

        /// <summary>
        /// 電話號碼
        /// </summary>
        [StringLength(20, ErrorMessage = "電話號碼長度不能超過 20 個字元")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        [StringLength(50, ErrorMessage = "城市長度不能超過 50 個字元")]
        public string? City { get; set; }

        /// <summary>
        /// 郵遞區號
        /// </summary>
        [StringLength(20, ErrorMessage = "郵遞區號長度不能超過 20 個字元")]
        public string? PostalCode { get; set; }

        /// <summary>
        /// 國家
        /// </summary>
        [StringLength(50, ErrorMessage = "國家長度不能超過 50 個字元")]
        public string? Country { get; set; }

        /// <summary>
        /// 是否活躍
        /// </summary>
        public bool? IsActive { get; set; }
    }
}
