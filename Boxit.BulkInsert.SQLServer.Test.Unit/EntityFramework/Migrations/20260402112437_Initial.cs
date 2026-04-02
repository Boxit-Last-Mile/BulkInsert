using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Boxit.BulkInsert.SQLServer.Test.Unit.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Test");

            migrationBuilder.CreateTable(
                name: "Parents",
                schema: "Test",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Children",
                schema: "Test",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentModelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Children", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Children_Parents_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "Test",
                        principalTable: "Parents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Children_Parents_ParentModelId",
                        column: x => x.ParentModelId,
                        principalSchema: "Test",
                        principalTable: "Parents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Children_ParentId",
                schema: "Test",
                table: "Children",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Children_ParentModelId",
                schema: "Test",
                table: "Children",
                column: "ParentModelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Children",
                schema: "Test");

            migrationBuilder.DropTable(
                name: "Parents",
                schema: "Test");
        }
    }
}
