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

        // --- Персонал (єдина точка входу для всіх співробітників) ---
        public DbSet<Staff> Staffs { get; set; }
        
        // --- Сутності для зв'язків ---
        public DbSet<Employment> Employments { get; set; }
        public DbSet<DoctorAssignment> DoctorAssignments { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // --- 1. Налаштування ієрархії персоналу (TPH) ---
            // Всі нащадки Staff будуть зберігатися в одній таблиці "Staffs"
            // з колонкою "StaffType" для розрізнення типів.
            builder.Entity<Staff>()
                .ToTable("Staffs")
                .HasDiscriminator<string>("StaffType");
                // EF Core автоматично знайде всі класи-нащадки
                // і налаштує значення для дискримінатора.

            // --- 2. Налаштування зв'язку "багато-до-багатьох" для працевлаштування ---
            builder.Entity<Employment>(entity =>
            {
                // Один співробітник може мати багато місць роботи
                entity.HasOne(e => e.Staff)
                      .WithMany(s => s.Employments)
                      .HasForeignKey(e => e.StaffId);

                // Одна лікарня може мати багато записів про працевлаштування
                entity.HasOne(e => e.Hospital)
                      .WithMany(h => h.Employments)
                      .HasForeignKey(e => e.HospitalId)
                      .OnDelete(DeleteBehavior.Restrict); // Уникаємо каскадного видалення

                // Одна клініка може мати багато записів про працевлаштування
                entity.HasOne(e => e.Clinic)
                      .WithMany(c => c.Employments)
                      .HasForeignKey(e => e.ClinicId)
                      .OnDelete(DeleteBehavior.Restrict); // Уникаємо каскадного видалення
            });

            // --- 3. Налаштування для збереження списку enum'ів ---
            builder.Entity<Hospital>()
                .Property(h => h.Specializations)
                .HasConversion(
                    // Функція для конвертації List<enum> у рядок для збереження в БД
                    v => string.Join(',', v.Select(e => e.ToString())),
                    // Функція для конвертації рядка з БД назад у List<enum>
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(e => Enum.Parse<HospitalSpecialization>(e)).ToList()
                );
            
            // --- Додаткові налаштування (якщо потрібні) ---
            // Тут ви можете додавати інші конфігурації для ваших моделей.
            // Наприклад, для DoctorAssignment, Operation тощо.
        }
    }
}