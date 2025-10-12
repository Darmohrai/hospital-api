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

        // --- Основні сутності ---
        public DbSet<Hospital> Hospitals { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Bed> Beds { get; set; }
        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Laboratory> Laboratories { get; set; }
        public DbSet<Operation> Operations { get; set; }

        // --- Персонал (ЄДИНА ТОЧКА ВХОДУ) ---
        public DbSet<Staff> Staffs { get; set; }
        
        // --- Сутності для зв'язків ---
        public DbSet<Employment> Employments { get; set; }
        public DbSet<DoctorAssignment> DoctorAssignments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Явно вказуємо всі конкретні класи, що успадковують Staff
            builder.Entity<Staff>()
                .ToTable("Staffs")
                .HasDiscriminator<string>("StaffType")
                .HasValue<SupportStaff>(nameof(SupportStaff))
                .HasValue<Cardiologist>(nameof(Cardiologist))
                .HasValue<Dentist>(nameof(Dentist))
                .HasValue<Gynecologist>(nameof(Gynecologist))
                .HasValue<Neurologist>(nameof(Neurologist))
                .HasValue<Ophthalmologist>(nameof(Ophthalmologist))
                .HasValue<Radiologist>(nameof(Radiologist))
                .HasValue<Surgeon>(nameof(Surgeon));

            builder.Entity<Employment>(entity =>
            {
                entity.HasOne(e => e.Staff)
                    .WithMany(s => s.Employments)
                    .HasForeignKey(e => e.StaffId);

                entity.HasOne(e => e.Hospital)
                    .WithMany(h => h.Employments)
                    .HasForeignKey(e => e.HospitalId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Clinic)
                    .WithMany(c => c.Employments)
                    .HasForeignKey(e => e.ClinicId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Hospital>()
                .Property(h => h.Specializations)
                .HasConversion(
                    v => string.Join(',', v.Select(e => e.ToString())),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(e => Enum.Parse<HospitalSpecialization>(e)).ToList()
                );
        }
    }
}