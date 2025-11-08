using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hospital_api.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Employment_DeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employments_Clinics_ClinicId",
                table: "Employments");

            migrationBuilder.DropForeignKey(
                name: "FK_Employments_Hospitals_HospitalId",
                table: "Employments");

            migrationBuilder.DropColumn(
                name: "Dentist_FatalOperationCount",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "Dentist_OperationCount",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "FatalOperationCount",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "Gynecologist_FatalOperationCount",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "Gynecologist_OperationCount",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "OperationCount",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "Surgeon_FatalOperationCount",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "Surgeon_OperationCount",
                table: "Staffs");

            migrationBuilder.AddForeignKey(
                name: "FK_Employments_Clinics_ClinicId",
                table: "Employments",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Employments_Hospitals_HospitalId",
                table: "Employments",
                column: "HospitalId",
                principalTable: "Hospitals",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employments_Clinics_ClinicId",
                table: "Employments");

            migrationBuilder.DropForeignKey(
                name: "FK_Employments_Hospitals_HospitalId",
                table: "Employments");

            migrationBuilder.AddColumn<int>(
                name: "Dentist_FatalOperationCount",
                table: "Staffs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Dentist_OperationCount",
                table: "Staffs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FatalOperationCount",
                table: "Staffs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gynecologist_FatalOperationCount",
                table: "Staffs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gynecologist_OperationCount",
                table: "Staffs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OperationCount",
                table: "Staffs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Surgeon_FatalOperationCount",
                table: "Staffs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Surgeon_OperationCount",
                table: "Staffs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Employments_Clinics_ClinicId",
                table: "Employments",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employments_Hospitals_HospitalId",
                table: "Employments",
                column: "HospitalId",
                principalTable: "Hospitals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
