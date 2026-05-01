using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TenPercent.Data.Migrations
{
    /// <inheritdoc />
    public partial class fixWorldStateAndSeason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentGameweek",
                table: "WorldStates");

            migrationBuilder.DropColumn(
                name: "NextMatchdayDate",
                table: "WorldStates");

            migrationBuilder.DropColumn(
                name: "TotalGameweeks",
                table: "WorldStates");

            migrationBuilder.AlterColumn<int>(
                name: "CurrentSeasonId",
                table: "WorldStates",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CurrentGameweek",
                table: "Seasons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalGameweeks",
                table: "Seasons",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentGameweek",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "TotalGameweeks",
                table: "Seasons");

            migrationBuilder.AlterColumn<int>(
                name: "CurrentSeasonId",
                table: "WorldStates",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentGameweek",
                table: "WorldStates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextMatchdayDate",
                table: "WorldStates",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "TotalGameweeks",
                table: "WorldStates",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
