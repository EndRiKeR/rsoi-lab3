using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BonusService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Privileges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Status = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Balance = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Privileges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrivilegeHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PrivilegeId = table.Column<int>(type: "integer", nullable: false),
                    TicketUid = table.Column<Guid>(type: "uuid", nullable: false),
                    Datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BalanceDiff = table.Column<int>(type: "integer", nullable: false),
                    OperationType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivilegeHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrivilegeHistories_Privileges_PrivilegeId",
                        column: x => x.PrivilegeId,
                        principalTable: "Privileges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrivilegeHistories_Datetime",
                table: "PrivilegeHistories",
                column: "Datetime");

            migrationBuilder.CreateIndex(
                name: "IX_PrivilegeHistories_PrivilegeId",
                table: "PrivilegeHistories",
                column: "PrivilegeId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivilegeHistories_TicketUid",
                table: "PrivilegeHistories",
                column: "TicketUid");

            migrationBuilder.CreateIndex(
                name: "IX_Privileges_Username",
                table: "Privileges",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrivilegeHistories");

            migrationBuilder.DropTable(
                name: "Privileges");
        }
    }
}
