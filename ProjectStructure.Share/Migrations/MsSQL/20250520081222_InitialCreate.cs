using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectStructure.Share.Migrations.MsSQL
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())", comment: "使用者唯一識別碼"),
                    username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "使用者名稱"),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "電子郵件地址"),
                    password_hash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false, comment: "密碼雜湊值"),
                    first_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, comment: "名字"),
                    last_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, comment: "姓氏"),
                    phone_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, comment: "電話號碼"),
                    address = table.Column<string>(type: "ntext", nullable: true, comment: "地址"),
                    city = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, comment: "城市"),
                    postal_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, comment: "郵遞區號"),
                    country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, comment: "國家"),
                    is_active = table.Column<bool>(type: "bit", nullable: true, defaultValue: true, comment: "是否啟用帳號"),
                    registered_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())", comment: "註冊時間"),
                    last_login = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "最後登入時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pkey", x => x.user_id);
                },
                comment: "使用者資料表");

            migrationBuilder.CreateIndex(
                name: "users_email_key",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "users_username_key",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
