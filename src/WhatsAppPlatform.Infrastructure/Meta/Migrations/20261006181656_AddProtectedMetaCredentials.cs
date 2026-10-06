using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatsAppPlatform.Infrastructure.Meta.Migrations
{
    /// <inheritdoc />
    public partial class AddProtectedMetaCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "external_whatsapp_account_id",
                schema: "whatsapp",
                table: "accounts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateTable(
                name: "meta_credentials",
                schema: "whatsapp",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    signup_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    protected_credential = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meta_credentials", x => x.id);
                    table.CheckConstraint("ck_meta_credential_protected", "protected_credential IS NULL OR length(protected_credential) > 0");
                    table.CheckConstraint("ck_meta_credential_time", "updated_at >= created_at");
                    table.ForeignKey(
                        name: "FK_meta_credentials_embedded_signup_sessions_signup_session_id",
                        column: x => x.signup_session_id,
                        principalSchema: "whatsapp",
                        principalTable: "embedded_signup_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_meta_credentials_signup_session_id",
                schema: "whatsapp",
                table: "meta_credentials",
                column: "signup_session_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "meta_credentials",
                schema: "whatsapp");

            migrationBuilder.AlterColumn<string>(
                name: "external_whatsapp_account_id",
                schema: "whatsapp",
                table: "accounts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
