using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bams.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJwtAuthenticationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "guest_access_grants",
                newName: "guest_access_grants_archived");

            migrationBuilder.AddColumn<string>(
                name: "identity_user_id",
                table: "customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "details_json",
                table: "audit_entries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "branch_memberships",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branch_memberships", x => x.id);
                    table.ForeignKey(
                        name: "FK_branch_memberships_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_branch_memberships_branches_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    replaced_by_session_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_refresh_sessions_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_refresh_sessions_refresh_sessions_replaced_by_session_id",
                        column: x => x.replaced_by_session_id,
                        principalTable: "refresh_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_customers_identity_user_id",
                table: "customers",
                column: "identity_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_branch_memberships_branch_id_role",
                table: "branch_memberships",
                columns: new[] { "branch_id", "role" });

            migrationBuilder.CreateIndex(
                name: "IX_branch_memberships_user_id_branch_id",
                table: "branch_memberships",
                columns: new[] { "user_id", "branch_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_sessions_replaced_by_session_id",
                table: "refresh_sessions",
                column: "replaced_by_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_sessions_token_hash",
                table: "refresh_sessions",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_sessions_user_id_family_id",
                table: "refresh_sessions",
                columns: new[] { "user_id", "family_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_customers_AspNetUsers_identity_user_id",
                table: "customers",
                column: "identity_user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_customers_AspNetUsers_identity_user_id",
                table: "customers");

            migrationBuilder.DropTable(
                name: "branch_memberships");

            migrationBuilder.DropTable(
                name: "refresh_sessions");

            migrationBuilder.DropIndex(
                name: "IX_customers_identity_user_id",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "identity_user_id",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "details_json",
                table: "audit_entries");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "guest_access_grants_archived",
                newName: "guest_access_grants");
        }
    }
}
