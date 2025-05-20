using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ProjectStructure.Share.Entities.MsSQL
{
    public partial class MsSQLContext : DbContext
    {
        public MsSQLContext()
        {
        }

        public MsSQLContext(DbContextOptions<MsSQLContext> options)
            : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 使用 Fluent API 設定較複雜的配置
            modelBuilder.Entity<User>(entity =>
            {
                // 表格設定
                entity.ToTable(tb => tb.HasComment("使用者資料表"));

                // 主鍵設定
                entity.HasKey(e => e.UserId)
                      .HasName("users_pkey");

                // 欄位資料類型和預設值設定
                entity.Property(e => e.UserId)
                      .HasDefaultValueSql("(newid())")
                      .HasComment("使用者唯一識別碼");

                entity.Property(e => e.Address)
                      .HasColumnType("ntext")
                      .HasComment("地址");

                entity.Property(e => e.IsActive)
                      .HasDefaultValue(true)
                      .HasComment("是否啟用帳號");

                entity.Property(e => e.RegisteredAt)
                      .HasDefaultValueSql("(getdate())")
                      .HasComment("註冊時間");

                // 欄位註解設定
                entity.Property(e => e.Username).HasComment("使用者名稱");
                entity.Property(e => e.Email).HasComment("電子郵件地址");
                entity.Property(e => e.PasswordHash).HasComment("密碼雜湊值");
                entity.Property(e => e.FirstName).HasComment("名字");
                entity.Property(e => e.LastName).HasComment("姓氏");
                entity.Property(e => e.PhoneNumber).HasComment("電話號碼");
                entity.Property(e => e.City).HasComment("城市");
                entity.Property(e => e.PostalCode).HasComment("郵遞區號");
                entity.Property(e => e.Country).HasComment("國家");
                entity.Property(e => e.LastLogin).HasComment("最後登入時間");

                // 索引設定
                entity.HasIndex(e => e.Email, "users_email_key")
                      .IsUnique();

                entity.HasIndex(e => e.Username, "users_username_key")
                      .IsUnique();
            });
        }
    }
}
