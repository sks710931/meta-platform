using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatsAppPlatform.Infrastructure.WhatsAppAccounts.Migrations
{
    /// <inheritdoc />
    public partial class TreatExternalIdentifiersAsOpaque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_phone_external_id",
                schema: "whatsapp",
                table: "phone_numbers");

            migrationBuilder.DropCheckConstraint(
                name: "ck_messaging_external_id",
                schema: "whatsapp",
                table: "messaging_accounts");

            migrationBuilder.DropCheckConstraint(
                name: "ck_account_external_id",
                schema: "whatsapp",
                table: "accounts");

            migrationBuilder.AddCheckConstraint(
                name: "ck_phone_external_id",
                schema: "whatsapp",
                table: "phone_numbers",
                sql: "char_length(external_phone_number_id) BETWEEN 1 AND 100 AND btrim(external_phone_number_id, U&'\\0020\\00A0\\1680\\2000\\2001\\2002\\2003\\2004\\2005\\2006\\2007\\2008\\2009\\200A\\2028\\2029\\202F\\205F\\3000') <> '' AND external_phone_number_id !~ U&'[\\0001-\\001F\\007F-\\009F]'");

            migrationBuilder.AddCheckConstraint(
                name: "ck_messaging_external_id",
                schema: "whatsapp",
                table: "messaging_accounts",
                sql: "char_length(external_messaging_account_id) BETWEEN 1 AND 100 AND btrim(external_messaging_account_id, U&'\\0020\\00A0\\1680\\2000\\2001\\2002\\2003\\2004\\2005\\2006\\2007\\2008\\2009\\200A\\2028\\2029\\202F\\205F\\3000') <> '' AND external_messaging_account_id !~ U&'[\\0001-\\001F\\007F-\\009F]'");

            migrationBuilder.AddCheckConstraint(
                name: "ck_account_external_id",
                schema: "whatsapp",
                table: "accounts",
                sql: "char_length(external_whatsapp_account_id) BETWEEN 1 AND 100 AND btrim(external_whatsapp_account_id, U&'\\0020\\00A0\\1680\\2000\\2001\\2002\\2003\\2004\\2005\\2006\\2007\\2008\\2009\\200A\\2028\\2029\\202F\\205F\\3000') <> '' AND external_whatsapp_account_id !~ U&'[\\0001-\\001F\\007F-\\009F]'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_phone_external_id",
                schema: "whatsapp",
                table: "phone_numbers");

            migrationBuilder.DropCheckConstraint(
                name: "ck_messaging_external_id",
                schema: "whatsapp",
                table: "messaging_accounts");

            migrationBuilder.DropCheckConstraint(
                name: "ck_account_external_id",
                schema: "whatsapp",
                table: "accounts");

            migrationBuilder.AddCheckConstraint(
                name: "ck_phone_external_id",
                schema: "whatsapp",
                table: "phone_numbers",
                sql: "external_phone_number_id ~ '^[1-9][0-9]{0,99}$'");

            migrationBuilder.AddCheckConstraint(
                name: "ck_messaging_external_id",
                schema: "whatsapp",
                table: "messaging_accounts",
                sql: "external_messaging_account_id ~ '^[1-9][0-9]{0,99}$'");

            migrationBuilder.AddCheckConstraint(
                name: "ck_account_external_id",
                schema: "whatsapp",
                table: "accounts",
                sql: "external_whatsapp_account_id ~ '^[1-9][0-9]{0,99}$'");
        }
    }
}
