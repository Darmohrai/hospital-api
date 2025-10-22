using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hospital_api.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientBedRelationshipAndDoctorUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PatientId",
                table: "Beds",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Beds_PatientId",
                table: "Beds",
                column: "PatientId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Beds_Patients_PatientId",
                table: "Beds",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Beds_Patients_PatientId",
                table: "Beds");

            migrationBuilder.DropIndex(
                name: "IX_Beds_PatientId",
                table: "Beds");

            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "Beds");
        }
    }
}
