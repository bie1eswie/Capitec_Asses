using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bams.Infrastructure.Migrations;

/// <inheritdoc />
public partial class IdentityInit : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AspNetRoles",
            columns: table => new
            {
                Id = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoles", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUsers",
            columns: table => new
            {
                Id = table.Column<string>(type: "text", nullable: false),
                UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                PasswordHash = table.Column<string>(type: "text", nullable: true),
                SecurityStamp = table.Column<string>(type: "text", nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                PhoneNumber = table.Column<string>(type: "text", nullable: true),
                PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUsers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "audit_entries",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                action = table.Column<string>(type: "text", nullable: false),
                entity_type = table.Column<string>(type: "text", nullable: false),
                entity_id = table.Column<string>(type: "text", nullable: false),
                actor_id = table.Column<string>(type: "text", nullable: true),
                occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_audit_entries", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "booking_requests",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                idempotency_key = table.Column<string>(type: "text", nullable: false),
                request_hash = table.Column<string>(type: "text", nullable: false),
                appointment_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_booking_requests", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "branch_closures",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                local_date = table.Column<DateOnly>(type: "date", nullable: false),
                reason = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_branch_closures", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "branch_schedules",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                day_of_week = table.Column<int>(type: "integer", nullable: false),
                opens_at = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                closes_at = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_branch_schedules", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "branches",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                address = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                time_zone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_branches", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "customers",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                first_name = table.Column<string>(type: "text", nullable: false),
                last_name = table.Column<string>(type: "text", nullable: false),
                email = table.Column<string>(type: "text", nullable: false),
                phone = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_customers", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "notifications",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                appointment_id = table.Column<Guid>(type: "uuid", nullable: false),
                outbox_event_id = table.Column<Guid>(type: "uuid", nullable: true),
                channel = table.Column<string>(type: "text", nullable: false),
                simulation_status = table.Column<string>(type: "text", nullable: false),
                rendered_content = table.Column<string>(type: "text", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_notifications", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "outbox_events",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                event_type = table.Column<string>(type: "text", nullable: false),
                payload_json = table.Column<string>(type: "jsonb", nullable: false),
                occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                attempts = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_outbox_events", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "services",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                duration_minutes = table.Column<int>(type: "integer", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_services", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "AspNetRoleClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                RoleId = table.Column<string>(type: "text", nullable: false),
                ClaimType = table.Column<string>(type: "text", nullable: true),
                ClaimValue = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<string>(type: "text", nullable: false),
                ClaimType = table.Column<string>(type: "text", nullable: true),
                ClaimValue = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserLogins",
            columns: table => new
            {
                LoginProvider = table.Column<string>(type: "text", nullable: false),
                ProviderKey = table.Column<string>(type: "text", nullable: false),
                ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                UserId = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                table.ForeignKey(
                    name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserRoles",
            columns: table => new
            {
                UserId = table.Column<string>(type: "text", nullable: false),
                RoleId = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserTokens",
            columns: table => new
            {
                UserId = table.Column<string>(type: "text", nullable: false),
                LoginProvider = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Value = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey(
                    name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "appointments",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                reference = table.Column<string>(type: "text", nullable: false),
                customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                service_id = table.Column<Guid>(type: "uuid", nullable: false),
                starts_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ends_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                status = table.Column<string>(type: "text", nullable: false),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_appointments", x => x.id);
                table.ForeignKey(
                    name: "FK_appointments_branches_branch_id",
                    column: x => x.branch_id,
                    principalTable: "branches",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_appointments_customers_customer_id",
                    column: x => x.customer_id,
                    principalTable: "customers",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_appointments_services_service_id",
                    column: x => x.service_id,
                    principalTable: "services",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "branch_services",
            columns: table => new
            {
                branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                service_id = table.Column<Guid>(type: "uuid", nullable: false),
                capacity = table.Column<int>(type: "integer", nullable: false),
                is_enabled = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_branch_services", x => new { x.branch_id, x.service_id });
                table.ForeignKey(
                    name: "FK_branch_services_branches_branch_id",
                    column: x => x.branch_id,
                    principalTable: "branches",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_branch_services_services_service_id",
                    column: x => x.service_id,
                    principalTable: "services",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "guest_access_grants",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                appointment_id = table.Column<Guid>(type: "uuid", nullable: false),
                token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_guest_access_grants", x => x.id);
                table.ForeignKey(
                    name: "FK_guest_access_grants_appointments_appointment_id",
                    column: x => x.appointment_id,
                    principalTable: "appointments",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_appointments_branch_id",
            table: "appointments",
            column: "branch_id");

        migrationBuilder.CreateIndex(
            name: "IX_appointments_customer_id",
            table: "appointments",
            column: "customer_id");

        migrationBuilder.CreateIndex(
            name: "IX_appointments_reference",
            table: "appointments",
            column: "reference",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_appointments_service_id",
            table: "appointments",
            column: "service_id");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetRoleClaims_RoleId",
            table: "AspNetRoleClaims",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "RoleNameIndex",
            table: "AspNetRoles",
            column: "NormalizedName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserClaims_UserId",
            table: "AspNetUserClaims",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserLogins_UserId",
            table: "AspNetUserLogins",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserRoles_RoleId",
            table: "AspNetUserRoles",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "EmailIndex",
            table: "AspNetUsers",
            column: "NormalizedEmail");

        migrationBuilder.CreateIndex(
            name: "UserNameIndex",
            table: "AspNetUsers",
            column: "NormalizedUserName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_booking_requests_idempotency_key",
            table: "booking_requests",
            column: "idempotency_key",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_branch_services_service_id",
            table: "branch_services",
            column: "service_id");

        migrationBuilder.CreateIndex(
            name: "IX_branches_code",
            table: "branches",
            column: "code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_guest_access_grants_appointment_id",
            table: "guest_access_grants",
            column: "appointment_id");

        migrationBuilder.CreateIndex(
            name: "IX_guest_access_grants_token_hash",
            table: "guest_access_grants",
            column: "token_hash",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_notifications_outbox_event_id",
            table: "notifications",
            column: "outbox_event_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_services_code",
            table: "services",
            column: "code",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AspNetRoleClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserLogins");

        migrationBuilder.DropTable(
            name: "AspNetUserRoles");

        migrationBuilder.DropTable(
            name: "AspNetUserTokens");

        migrationBuilder.DropTable(
            name: "audit_entries");

        migrationBuilder.DropTable(
            name: "booking_requests");

        migrationBuilder.DropTable(
            name: "branch_closures");

        migrationBuilder.DropTable(
            name: "branch_schedules");

        migrationBuilder.DropTable(
            name: "branch_services");

        migrationBuilder.DropTable(
            name: "guest_access_grants");

        migrationBuilder.DropTable(
            name: "notifications");

        migrationBuilder.DropTable(
            name: "outbox_events");

        migrationBuilder.DropTable(
            name: "AspNetRoles");

        migrationBuilder.DropTable(
            name: "AspNetUsers");

        migrationBuilder.DropTable(
            name: "appointments");

        migrationBuilder.DropTable(
            name: "branches");

        migrationBuilder.DropTable(
            name: "customers");

        migrationBuilder.DropTable(
            name: "services");
    }
}
