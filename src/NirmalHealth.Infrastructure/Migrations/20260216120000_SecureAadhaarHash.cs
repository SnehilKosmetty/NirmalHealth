using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NirmalHealth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SecureAadhaarHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AadhaarHash",
                table: "Users",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            // Remove any stored Aadhaar digits from DB (security: no part of Aadhaar in plain text)
            migrationBuilder.Sql("UPDATE [Users] SET [AadhaarMasked] = NULL WHERE [AadhaarMasked] IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AadhaarHash",
                table: "Users");
        }
    }
}
