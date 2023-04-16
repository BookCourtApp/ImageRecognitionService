﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class IRSMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Image = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookMarkups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PhotoId = table.Column<Guid>(type: "uuid", nullable: false),
                    x0 = table.Column<int>(type: "integer", nullable: false),
                    x1 = table.Column<int>(type: "integer", nullable: false),
                    x2 = table.Column<int>(type: "integer", nullable: false),
                    x3 = table.Column<int>(type: "integer", nullable: false),
                    y0 = table.Column<int>(type: "integer", nullable: false),
                    y1 = table.Column<int>(type: "integer", nullable: false),
                    y2 = table.Column<int>(type: "integer", nullable: false),
                    y3 = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookMarkups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookMarkups_Photos_PhotoId",
                        column: x => x.PhotoId,
                        principalTable: "Photos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TextMarkups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookMarkupId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    x0 = table.Column<int>(type: "integer", nullable: false),
                    x1 = table.Column<int>(type: "integer", nullable: false),
                    x2 = table.Column<int>(type: "integer", nullable: false),
                    x3 = table.Column<int>(type: "integer", nullable: false),
                    y0 = table.Column<int>(type: "integer", nullable: false),
                    y1 = table.Column<int>(type: "integer", nullable: false),
                    y2 = table.Column<int>(type: "integer", nullable: false),
                    y3 = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextMarkups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TextMarkups_BookMarkups_BookMarkupId",
                        column: x => x.BookMarkupId,
                        principalTable: "BookMarkups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookMarkups_PhotoId",
                table: "BookMarkups",
                column: "PhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_TextMarkups_BookMarkupId",
                table: "TextMarkups",
                column: "BookMarkupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TextMarkups");

            migrationBuilder.DropTable(
                name: "BookMarkups");

            migrationBuilder.DropTable(
                name: "Photos");
        }
    }
}
