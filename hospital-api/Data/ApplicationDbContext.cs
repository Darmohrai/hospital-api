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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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

        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<LabAnalysis> LabAnalyses { get; set; }
        public DbSet<Admission> Admissions { get; set; }
        public DbSet<ClinicDoctorAssignment> ClinicDoctorAssignments { get; set; }

        public DbSet<UpgradeRequest> UpgradeRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Ignore<IdentityUserToken<string>>();
            builder.Ignore<IdentityUserLogin<string>>();
            builder.Ignore<IdentityUserClaim<string>>();
            builder.Ignore<IdentityRoleClaim<string>>();
            
            builder.Entity<IdentityUser>(entity =>
            {
                entity.Ignore(u => u.PhoneNumber);
                entity.Ignore(u => u.PhoneNumberConfirmed);

                entity.Ignore(u => u.TwoFactorEnabled);

                entity.Ignore(u => u.LockoutEnabled);
                entity.Ignore(u => u.LockoutEnd);
                entity.Ignore(u => u.AccessFailedCount);

                entity.Ignore(u => u.ConcurrencyStamp);
            });

            builder.Entity<Staff>().ToTable("Staffs"); 
    
            builder.Entity<Doctor>().ToTable("Doctors");
            builder.Entity<SupportStaff>().ToTable("SupportStaffs");
    
            builder.Entity<Cardiologist>().ToTable("Cardiologists");
            builder.Entity<Dentist>().ToTable("Dentists");
            builder.Entity<Gynecologist>().ToTable("Gynecologists");
            builder.Entity<Neurologist>().ToTable("Neurologists");
            builder.Entity<Ophthalmologist>().ToTable("Ophthalmologists");
            builder.Entity<Radiologist>().ToTable("Radiologists");
            builder.Entity<Surgeon>().ToTable("Surgeons");

            builder.Entity<Employment>(entity =>
            {
                entity.HasOne(e => e.Staff).WithMany(s => s.Employments).HasForeignKey(e => e.StaffId);

                entity.HasOne(e => e.Hospital).WithMany(h => h.Employments).HasForeignKey(e => e.HospitalId)
                    .OnDelete(DeleteBehavior.SetNull); 

                entity.HasOne(e => e.Clinic).WithMany(c => c.Employments).HasForeignKey(e => e.ClinicId)
                    .OnDelete(DeleteBehavior.SetNull); 
            });

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
                .HasOne(p => p.Bed)
                .WithOne(b => b.Patient)
                .HasForeignKey<Bed>(b => b.PatientId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ClinicDoctorAssignment>(entity =>
            {
                entity.HasKey(cda => new { cda.PatientId, cda.DoctorId, cda.ClinicId });

                entity.HasOne(cda => cda.Patient)
                    .WithMany()
                    .HasForeignKey(cda => cda.PatientId);

                entity.HasOne(cda => cda.Doctor)
                    .WithMany()
                    .HasForeignKey(cda => cda.DoctorId);

                entity.HasOne(cda => cda.Clinic)
                    .WithMany()
                    .HasForeignKey(cda => cda.ClinicId);
            });

            builder.Entity<UpgradeRequest>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId);
        }
    }
}