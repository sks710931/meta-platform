using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatsAppPlatform.Infrastructure.WhatsAppAccounts.Migrations
{
    /// <inheritdoc />
    public partial class AddWhatsAppOnboarding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "whatsapp");

            migrationBuilder.CreateTable(
                name: "embedded_signup_sessions",
                schema: "whatsapp",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_embedded_signup_sessions", x => x.id);
                    table.UniqueConstraint("AK_embedded_signup_sessions_id_organization_id", x => new { x.id, x.organization_id });
                    table.CheckConstraint("ck_signup_completion", "(status = 'Completed' AND completed_at IS NOT NULL AND completed_at >= started_at AND completed_at < expires_at) OR (status <> 'Completed' AND completed_at IS NULL)");
                    table.CheckConstraint("ck_signup_expiry", "expires_at > started_at");
                    table.CheckConstraint("ck_signup_status", "status IN ('Pending', 'Completed', 'Failed', 'Expired')");
                    table.ForeignKey(
                        name: "FK_embedded_signup_sessions_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "organizations",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "accounts",
                schema: "whatsapp",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    signup_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_whatsapp_account_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    connected_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.id);
                    table.CheckConstraint("ck_account_connected", "status <> 'Connected' OR (connected_at IS NOT NULL AND connected_at >= created_at)");
                    table.CheckConstraint("ck_account_external_id", "external_whatsapp_account_id ~ '^[1-9][0-9]{0,99}$'");
                    table.CheckConstraint("ck_account_name", "length(btrim(display_name)) > 0");
                    table.CheckConstraint("ck_account_status", "status IN ('Pending', 'Connected', 'Suspended', 'Disconnected')");
                    table.ForeignKey(
                        name: "FK_accounts_embedded_signup_sessions_signup_session_id_organiz~",
                        columns: x => new { x.signup_session_id, x.organization_id },
                        principalSchema: "whatsapp",
                        principalTable: "embedded_signup_sessions",
                        principalColumns: new[] { "id", "organization_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_accounts_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "organizations",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "messaging_accounts",
                schema: "whatsapp",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    whatsapp_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_messaging_account_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messaging_accounts", x => x.id);
                    table.CheckConstraint("ck_messaging_external_id", "external_messaging_account_id ~ '^[1-9][0-9]{0,99}$'");
                    table.ForeignKey(
                        name: "FK_messaging_accounts_accounts_whatsapp_account_id",
                        column: x => x.whatsapp_account_id,
                        principalSchema: "whatsapp",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "phone_numbers",
                schema: "whatsapp",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    whatsapp_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_phone_number_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    display_phone_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    verified_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_phone_numbers", x => x.id);
                    table.CheckConstraint("ck_phone_display", "length(btrim(display_phone_number)) > 0");
                    table.CheckConstraint("ck_phone_external_id", "external_phone_number_id ~ '^[1-9][0-9]{0,99}$'");
                    table.CheckConstraint("ck_phone_status", "status = 'Registered'");
                    table.ForeignKey(
                        name: "FK_phone_numbers_accounts_whatsapp_account_id",
                        column: x => x.whatsapp_account_id,
                        principalSchema: "whatsapp",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_accounts_external_whatsapp_account_id",
                schema: "whatsapp",
                table: "accounts",
                column: "external_whatsapp_account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_accounts_organization_id_created_at_id",
                schema: "whatsapp",
                table: "accounts",
                columns: new[] { "organization_id", "created_at", "id" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_accounts_signup_session_id",
                schema: "whatsapp",
                table: "accounts",
                column: "signup_session_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_accounts_signup_session_id_organization_id",
                schema: "whatsapp",
                table: "accounts",
                columns: new[] { "signup_session_id", "organization_id" });

            migrationBuilder.CreateIndex(
                name: "IX_embedded_signup_sessions_organization_id_started_at",
                schema: "whatsapp",
                table: "embedded_signup_sessions",
                columns: new[] { "organization_id", "started_at" });

            migrationBuilder.CreateIndex(
                name: "IX_messaging_accounts_external_messaging_account_id",
                schema: "whatsapp",
                table: "messaging_accounts",
                column: "external_messaging_account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_messaging_accounts_whatsapp_account_id",
                schema: "whatsapp",
                table: "messaging_accounts",
                column: "whatsapp_account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_phone_numbers_external_phone_number_id",
                schema: "whatsapp",
                table: "phone_numbers",
                column: "external_phone_number_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_phone_numbers_whatsapp_account_id",
                schema: "whatsapp",
                table: "phone_numbers",
                column: "whatsapp_account_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "messaging_accounts",
                schema: "whatsapp");

            migrationBuilder.DropTable(
                name: "phone_numbers",
                schema: "whatsapp");

            migrationBuilder.DropTable(
                name: "accounts",
                schema: "whatsapp");

            migrationBuilder.DropTable(
                name: "embedded_signup_sessions",
                schema: "whatsapp");
        }
    }
}
