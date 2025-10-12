using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hospital_api.Migrations
{
    /// <inheritdoc />
    public partial class InitialModelWithTPHAndEmploymentSecond : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "StaffType",
                table: "Staffs",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(8)",
                oldMaxLength: 8);

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
                name: "ExtendedVacationDays",
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

            migrationBuilder.AddColumn<float>(
                name: "HazardPayCoefficient",
                table: "Staffs",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OperationCount",
                table: "Staffs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Ophthalmologist_ExtendedVacationDays",
                table: "Staffs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Radiologist_ExtendedVacationDays",
                table: "Staffs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Radiologist_HazardPayCoefficient",
                table: "Staffs",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Role",
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

            migrationBuilder.AddColumn<int>(
                name: "CardiologistId",
                table: "Operations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DentistId",
                table: "Operations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GynecologistId",
                table: "Operations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SurgeonId",
                table: "Operations",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Operations_CardiologistId",
                table: "Operations",
                column: "CardiologistId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_DentistId",
                table: "Operations",
                column: "DentistId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_GynecologistId",
                table: "Operations",
                column: "GynecologistId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_SurgeonId",
                table: "Operations",
                column: "SurgeonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Staffs_CardiologistId",
                table: "Operations",
                column: "CardiologistId",
                principalTable: "Staffs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Staffs_DentistId",
                table: "Operations",
                column: "DentistId",
                principalTable: "Staffs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Staffs_GynecologistId",
                table: "Operations",
                column: "GynecologistId",
                principalTable: "Staffs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Staffs_SurgeonId",
                table: "Operations",
                column: "SurgeonId",
                principalTable: "Staffs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Staffs_CardiologistId",
                table: "Operations");

            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Staffs_DentistId",
                table: "Operations");

            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Staffs_GynecologistId",
                table: "Operations");

            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Staffs_SurgeonId",
                table: "Operations");

            migrationBuilder.DropIndex(
                name: "IX_Operations_CardiologistId",
                table: "Operations");

            migrationBuilder.DropIndex(
                name: "IX_Operations_DentistId",
                table: "Operations");

            migrationBuilder.DropIndex(
                name: "IX_Operations_GynecologistId",
                table: "Operations");

            migrationBuilder.DropIndex(
                name: "IX_Operations_SurgeonId",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "Dentist_FatalOperationCount",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "Dentist_OperationCount",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "ExtendedVacationDays",
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
                name: "HazardPayCoefficient",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "OperationCount",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "Ophthalmologist_ExtendedVacationDays",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "Radiologist_ExtendedVacationDays",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "Radiologist_HazardPayCoefficient",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "Surgeon_FatalOperationCount",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "Surgeon_OperationCount",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "CardiologistId",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "DentistId",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "GynecologistId",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "SurgeonId",
                table: "Operations");

            migrationBuilder.AlterColumn<string>(
                name: "StaffType",
                table: "Staffs",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(21)",
                oldMaxLength: 21);
        }
    }
}
