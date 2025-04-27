using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace McGurkin.Api.Features.Kv.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "kv");

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                schema: "kv",
                columns: table => new
                {
                    UserProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShowAllChannels = table.Column<bool>(type: "bit", nullable: false),
                    ShowHidden = table.Column<bool>(type: "bit", nullable: false),
                    ShowRated = table.Column<bool>(type: "bit", nullable: false),
                    ShowWatchList = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.UserProfileId);
                });

            migrationBuilder.CreateTable(
                name: "UserProviders",
                schema: "kv",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProviders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProviders_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalSchema: "kv",
                        principalTable: "UserProfiles",
                        principalColumn: "UserProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRatings",
                schema: "kv",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InWatchlist = table.Column<bool>(type: "bit", nullable: false),
                    IsHidden = table.Column<bool>(type: "bit", nullable: false),
                    MovieId = table.Column<int>(type: "int", nullable: false),
                    Stars = table.Column<int>(type: "int", nullable: false),
                    TvId = table.Column<int>(type: "int", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRatings_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalSchema: "kv",
                        principalTable: "UserProfiles",
                        principalColumn: "UserProfileId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserProviders_UserProfileId",
                schema: "kv",
                table: "UserProviders",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRatings_UserProfileId",
                schema: "kv",
                table: "UserRatings",
                column: "UserProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserProviders",
                schema: "kv");

            migrationBuilder.DropTable(
                name: "UserRatings",
                schema: "kv");

            migrationBuilder.DropTable(
                name: "UserProfiles",
                schema: "kv");
        }
    }
}
