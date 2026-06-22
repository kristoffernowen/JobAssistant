using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobAssistant.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateJobAdSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Category",
                table: "JobAds",
                newName: "OccupationGroup");

            migrationBuilder.RenameIndex(
                name: "IX_JobAds_Location_Category",
                table: "JobAds",
                newName: "IX_JobAds_Location_OccupationGroup");

            migrationBuilder.AddColumn<string>(
                name: "FullData",
                table: "JobAds",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OccupationField",
                table: "JobAds",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PublicationDate",
                table: "JobAds",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Removed",
                table: "JobAds",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_JobAds_Location_OccupationField",
                table: "JobAds",
                columns: new[] { "Location", "OccupationField" });

            migrationBuilder.CreateIndex(
                name: "IX_JobAds_PublicationDate",
                table: "JobAds",
                column: "PublicationDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobAds_Location_OccupationField",
                table: "JobAds");

            migrationBuilder.DropIndex(
                name: "IX_JobAds_PublicationDate",
                table: "JobAds");

            migrationBuilder.DropColumn(
                name: "FullData",
                table: "JobAds");

            migrationBuilder.DropColumn(
                name: "OccupationField",
                table: "JobAds");

            migrationBuilder.DropColumn(
                name: "PublicationDate",
                table: "JobAds");

            migrationBuilder.DropColumn(
                name: "Removed",
                table: "JobAds");

            migrationBuilder.RenameColumn(
                name: "OccupationGroup",
                table: "JobAds",
                newName: "Category");

            migrationBuilder.RenameIndex(
                name: "IX_JobAds_Location_OccupationGroup",
                table: "JobAds",
                newName: "IX_JobAds_Location_Category");
        }
    }
}
