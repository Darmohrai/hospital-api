using Microsoft.EntityFrameworkCore;
using hospital_api.Models.HospitalAggregate;
using hospital_api.Models.ClinicAggregate;
using hospital_api.Models.StaffAggregate;
using hospital_api.Models.PatientAggregate;
using hospital_api.Models.LaboratoryAggregate;
using hospital_api.Models.OperationsAggregate;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace hospital_api.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Hospital> Hospitals { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Bed> Beds { get; set; }

        public DbSet<Clinic> Clinics { get; set; }

        //public DbSet<Staff> Staffs { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Laboratory> Laboratories { get; set; }
        public DbSet<Operation> Operations { get; set; }

        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<SupportStaff> SupportStaffs { get; set; }
        public DbSet<DoctorAssignment> DoctorAssignments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Staff>().ToTable("Staff");
            builder.Entity<Doctor>().ToTable("Doctors");
            builder.Entity<SupportStaff>().ToTable("SupportStaffs");

            builder.Entity<Surgeon>().ToTable("Surgeons");
            builder.Entity<Dentist>().ToTable("Dentists");
            builder.Entity<Gynecologist>().ToTable("Gynecologists");
            builder.Entity<Radiologist>().ToTable("Radiologists");
            builder.Entity<Neurologist>().ToTable("Neurologists");
            builder.Entity<Ophthalmologist>().ToTable("Ophthalmologists");
            builder.Entity<Cardiologist>().ToTable("Cardiologists");
        }
    }
}