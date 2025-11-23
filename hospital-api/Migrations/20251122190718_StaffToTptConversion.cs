using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hospital_api.Migrations
{
    /// <inheritdoc />
    public partial class StaffToTptConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorAssignments_Staffs_DoctorId",
                table: "DoctorAssignments");

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

            migrationBuilder.DropColumn(
                name: "AdminComment",
                table: "UpgradeRequests");

            migrationBuilder.DropColumn(
                name: "AcademicDegree",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "AcademicTitle",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "ExtendedVacationDays",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "HazardPayCoefficient",
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
                name: "Specialty",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "StaffType",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Hospitals");

            migrationBuilder.CreateTable(
                name: "Doctors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Specialty = table.Column<string>(type: "text", nullable: false),
                    AcademicDegree = table.Column<int>(type: "integer", nullable: false),
                    AcademicTitle = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doctors_Staffs_Id",
                        column: x => x.Id,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupportStaffs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportStaffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportStaffs_Staffs_Id",
                        column: x => x.Id,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cardiologists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cardiologists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cardiologists_Doctors_Id",
                        column: x => x.Id,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dentists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    HazardPayCoefficient = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dentists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dentists_Doctors_Id",
                        column: x => x.Id,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Gynecologists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gynecologists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gynecologists_Doctors_Id",
                        column: x => x.Id,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Neurologists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ExtendedVacationDays = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Neurologists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Neurologists_Doctors_Id",
                        column: x => x.Id,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ophthalmologists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ExtendedVacationDays = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ophthalmologists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ophthalmologists_Doctors_Id",
                        column: x => x.Id,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Radiologists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    HazardPayCoefficient = table.Column<float>(type: "real", nullable: false),
                    ExtendedVacationDays = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Radiologists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Radiologists_Doctors_Id",
                        column: x => x.Id,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Surgeons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Surgeons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Surgeons_Doctors_Id",
                        column: x => x.Id,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorAssignments_Doctors_DoctorId",
                table: "DoctorAssignments",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Cardiologists_CardiologistId",
                table: "Operations",
                column: "CardiologistId",
                principalTable: "Cardiologists",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Dentists_DentistId",
                table: "Operations",
                column: "DentistId",
                principalTable: "Dentists",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Gynecologists_GynecologistId",
                table: "Operations",
                column: "GynecologistId",
                principalTable: "Gynecologists",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Surgeons_SurgeonId",
                table: "Operations",
                column: "SurgeonId",
                principalTable: "Surgeons",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorAssignments_Doctors_DoctorId",
                table: "DoctorAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Cardiologists_CardiologistId",
                table: "Operations");

            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Dentists_DentistId",
                table: "Operations");

            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Gynecologists_GynecologistId",
                table: "Operations");

            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Surgeons_SurgeonId",
                table: "Operations");

            migrationBuilder.DropTable(
                name: "Cardiologists");

            migrationBuilder.DropTable(
                name: "Dentists");

            migrationBuilder.DropTable(
                name: "Gynecologists");

            migrationBuilder.DropTable(
                name: "Neurologists");

            migrationBuilder.DropTable(
                name: "Ophthalmologists");

            migrationBuilder.DropTable(
                name: "Radiologists");

            migrationBuilder.DropTable(
                name: "SupportStaffs");

            migrationBuilder.DropTable(
                name: "Surgeons");

            migrationBuilder.DropTable(
                name: "Doctors");

            migrationBuilder.AddColumn<string>(
                name: "AdminComment",
                table: "UpgradeRequests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "AcademicDegree",
                table: "Staffs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcademicTitle",
                table: "Staffs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExtendedVacationDays",
                table: "Staffs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "HazardPayCoefficient",
                table: "Staffs",
                type: "real",
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

            migrationBuilder.AddColumn<string>(
                name: "Specialty",
                table: "Staffs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StaffType",
                table: "Staffs",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Hospitals",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorAssignments_Staffs_DoctorId",
                table: "DoctorAssignments",
                column: "DoctorId",
                principalTable: "Staffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
    }
}
