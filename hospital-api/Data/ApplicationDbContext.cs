using hospital_api.Models.Auth;
using Microsoft.EntityFrameworkCore;
using hospital_api.Models.HospitalAggregate;
using hospital_api.Models.ClinicAggregate;
using hospital_api.Models.StaffAggregate;
using hospital_api.Models.PatientAggregate;
using hospital_api.Models.LaboratoryAggregate;
using hospital_api.Models.OperationsAggregate;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Models.Tracking;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking; // <-- Додайте цей using

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
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Laboratory> Laboratories { get; set; }
        public DbSet<Operation> Operations { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Employment> Employments { get; set; }
        public DbSet<DoctorAssignment> DoctorAssignments { get; set; }

        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<LabAnalysis> LabAnalyses { get; set; }
        public DbSet<Admission> Admissions { get; set; }
        public DbSet<ClinicDoctorAssignment> ClinicDoctorAssignments { get; set; }

        public DbSet<UpgradeRequest> UpgradeRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Ваше правильне рішення, яке змушує EF Core бачити ієрархію
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
                entity.HasOne(e => e.Staff).WithMany(s => s.Employments).HasForeignKey(e => e.StaffId);

                // ✅ ВИПРАВЛЕНО:
                entity.HasOne(e => e.Hospital).WithMany(h => h.Employments).HasForeignKey(e => e.HospitalId)
                    .OnDelete(DeleteBehavior.SetNull); 

                // ✅ ВИПРАВЛЕНО:
                entity.HasOne(e => e.Clinic).WithMany(c => c.Employments).HasForeignKey(e => e.ClinicId)
                    .OnDelete(DeleteBehavior.SetNull); 
            });

            // ✅ ВИПРАВЛЕНО: Додано ValueComparer для коректної роботи зі списком enum'ів
            builder.Entity<Hospital>()
                .Property(h => h.Specializations)
                .HasConversion(
                    v => string.Join(',', v.Select(e => e.ToString())),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(e => Enum.Parse<HospitalSpecialization>(e)).ToList())
                .Metadata.SetValueComparer(new ValueComparer<List<HospitalSpecialization>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            builder.Entity<Patient>()
                .HasOne(p => p.Bed) // У Пацієнта є одне Ліжко
                .WithOne(b => b.Patient) // У Ліжка є один Пацієнт
                .HasForeignKey<Bed>(b => b.PatientId) // Зовнішній ключ знаходиться в Bed
                .OnDelete(DeleteBehavior.SetNull);

            // ✅ НОВИЙ БЛОК: Налаштування композитного ключа для M:M
            builder.Entity<ClinicDoctorAssignment>(entity =>
            {
                // Встановлюємо композитний первинний ключ
                entity.HasKey(cda => new { cda.PatientId, cda.DoctorId, cda.ClinicId });

                // Оскільки ми не додавали List<> у навігаційні властивості (в Patient, Staff, Clinic),
                // ми налаштовуємо зв'язок M:M таким чином:
                entity.HasOne(cda => cda.Patient)
                    .WithMany() // У Patient немає List<ClinicDoctorAssignment>
                    .HasForeignKey(cda => cda.PatientId);

                entity.HasOne(cda => cda.Doctor)
                    .WithMany() // У Staff немає List<ClinicDoctorAssignment>
                    .HasForeignKey(cda => cda.DoctorId);

                entity.HasOne(cda => cda.Clinic)
                    .WithMany() // У Clinic немає List<ClinicDoctorAssignment>
                    .HasForeignKey(cda => cda.ClinicId);
            });

            builder.Entity<UpgradeRequest>()
                .HasOne(r => r.User)
                .WithMany() // У IdentityUser немає навігаційної властивості
                .HasForeignKey(r => r.UserId);
        }
    }
}