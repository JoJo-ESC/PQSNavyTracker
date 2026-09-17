using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PqsTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Qualifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Qualifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trainees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trainees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LineItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QualificationId = table.Column<int>(type: "INTEGER", nullable: false),
                    Section = table.Column<string>(type: "TEXT", nullable: false),
                    Number = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    IsRequired = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LineItems_Qualifications_QualificationId",
                        column: x => x.QualificationId,
                        principalTable: "Qualifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TraineeQualifications",
                columns: table => new
                {
                    HeldQualificationsId = table.Column<int>(type: "INTEGER", nullable: false),
                    HolderTraineesId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraineeQualifications", x => new { x.HeldQualificationsId, x.HolderTraineesId });
                    table.ForeignKey(
                        name: "FK_TraineeQualifications_Qualifications_HeldQualificationsId",
                        column: x => x.HeldQualificationsId,
                        principalTable: "Qualifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TraineeQualifications_Trainees_HolderTraineesId",
                        column: x => x.HolderTraineesId,
                        principalTable: "Trainees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SignOffs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LineItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    TraineeId = table.Column<int>(type: "INTEGER", nullable: false),
                    QualifierId = table.Column<int>(type: "INTEGER", nullable: false),
                    SignedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RevocationReason = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignOffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignOffs_LineItems_LineItemId",
                        column: x => x.LineItemId,
                        principalTable: "LineItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SignOffs_Trainees_QualifierId",
                        column: x => x.QualifierId,
                        principalTable: "Trainees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SignOffs_Trainees_TraineeId",
                        column: x => x.TraineeId,
                        principalTable: "Trainees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LineItems_QualificationId",
                table: "LineItems",
                column: "QualificationId");

            migrationBuilder.CreateIndex(
                name: "IX_SignOffs_LineItemId",
                table: "SignOffs",
                column: "LineItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SignOffs_QualifierId",
                table: "SignOffs",
                column: "QualifierId");

            migrationBuilder.CreateIndex(
                name: "IX_SignOffs_TraineeId",
                table: "SignOffs",
                column: "TraineeId");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeQualifications_HolderTraineesId",
                table: "TraineeQualifications",
                column: "HolderTraineesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SignOffs");

            migrationBuilder.DropTable(
                name: "TraineeQualifications");

            migrationBuilder.DropTable(
                name: "LineItems");

            migrationBuilder.DropTable(
                name: "Trainees");

            migrationBuilder.DropTable(
                name: "Qualifications");
        }
    }
}
