using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProjectStructure.Share.Entities.MsSQL;
// 使用 Data Annotations 設定基本結構
[Table("users")]
public partial class User
{
    [Key]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("username")]
    [StringLength(50)]
    public string Username { get; set; } = null!;

    [Required]
    [Column("email")]
    [StringLength(100)]
    public string Email { get; set; } = null!;

    [Required]
    [Column("password_hash")]
    [StringLength(256)]
    public string PasswordHash { get; set; } = null!;

    [Column("first_name")]
    [StringLength(50)]
    public string? FirstName { get; set; }

    [Column("last_name")]
    [StringLength(50)]
    public string? LastName { get; set; }

    [Column("phone_number")]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [Column("address")]
    public string? Address { get; set; }

    [Column("city")]
    [StringLength(50)]
    public string? City { get; set; }

    [Column("postal_code")]
    [StringLength(20)]
    public string? PostalCode { get; set; }

    [Column("country")]
    [StringLength(50)]
    public string? Country { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [Column("registered_at")]
    public DateTime? RegisteredAt { get; set; }

    [Column("last_login")]
    public DateTime? LastLogin { get; set; }
}