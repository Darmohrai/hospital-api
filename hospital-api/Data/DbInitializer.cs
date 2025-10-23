using hospital_api.Models.HospitalAggregate;
using hospital_api.Models.ClinicAggregate;
using hospital_api.Models.StaffAggregate;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Models.PatientAggregate;
using hospital_api.Models.LaboratoryAggregate;
using hospital_api.Models.OperationsAggregate;
using hospital_api.Models.Tracking;
using hospital_api.Models.Auth; // Для UpgradeRequest
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Data;

public static class DbInitializer
{
    public static async Task Initialize(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Перевіряємо, чи база даних взагалі існує і чи є міграції
        context.Database.EnsureCreated(); // Або context.Database.Migrate();

        // 1. Створення Ролей (ми це вже робили в Program.cs, але тут дублюємо для повноти)
        string[] roleNames = { "Admin", "Operator", "Authorized", "Guest" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 2. Створення Користувачів (якщо ще немає)
        IdentityUser adminUser = null;
        if (await userManager.FindByNameAsync("admin") == null)
        {
            adminUser = new IdentityUser { UserName = "admin", Email = "admin@example.com", EmailConfirmed = true };
            await userManager.CreateAsync(adminUser, "Admin123!"); // Встановіть надійний пароль!
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        else { adminUser = await userManager.FindByNameAsync("admin"); }

        IdentityUser operatorUser = null;
        if (await userManager.FindByNameAsync("operator") == null)
        {
            operatorUser = new IdentityUser { UserName = "operator", Email = "operator@example.com", EmailConfirmed = true };
            await userManager.CreateAsync(operatorUser, "Operator123!");
            await userManager.AddToRoleAsync(operatorUser, "Operator");
        }
         else { operatorUser = await userManager.FindByNameAsync("operator"); }

        IdentityUser authorizedUser = null;
        if (await userManager.FindByNameAsync("authorized") == null)
        {
            authorizedUser = new IdentityUser { UserName = "authorized", Email = "authorized@example.com", EmailConfirmed = true };
            await userManager.CreateAsync(authorizedUser, "Authorized123!");
            await userManager.AddToRoleAsync(authorizedUser, "Authorized");
        }
         else { authorizedUser = await userManager.FindByNameAsync("authorized"); }

        IdentityUser guestUser = null;
        if (await userManager.FindByNameAsync("guest") == null)
        {
            guestUser = new IdentityUser { UserName = "guest", Email = "guest@example.com", EmailConfirmed = true };
            await userManager.CreateAsync(guestUser, "Guest123!");
            await userManager.AddToRoleAsync(guestUser, "Guest");
            // Створюємо заявку для гостя
            if (!context.UpgradeRequests.Any(r => r.UserId == guestUser.Id))
            {
                 context.UpgradeRequests.Add(new UpgradeRequest { UserId = guestUser.Id, User = guestUser });
            }
        }
         else { guestUser = await userManager.FindByNameAsync("guest"); }

        // 3. Перевірка, чи потрібно заповнювати основні дані (наприклад, по лікарнях)
        if (context.Hospitals.Any())
        {
            return; // База даних вже заповнена
        }

        // --- Створення Довідників ---

        var hospital1 = new Hospital { Name = "Міська лікарня №1", Address = "вул. Головна, 1", Phone = "111-22-33", Specializations = new List<HospitalSpecialization> { HospitalSpecialization.Surgeon, HospitalSpecialization.Cardiologist } };
        var hospital2 = new Hospital { Name = "Обласна дитяча лікарня", Address = "просп. Незалежності, 100", Phone = "555-66-77", Specializations = new List<HospitalSpecialization> { HospitalSpecialization.Neurologist, HospitalSpecialization.Ophthalmologist } };
        context.Hospitals.AddRange(hospital1, hospital2);
        await context.SaveChangesAsync(); // Важливо зберігати ID

        var building1_1 = new Building { HospitalId = hospital1.Id, Name = "Корпус А" };
        var building1_2 = new Building { HospitalId = hospital1.Id, Name = "Корпус Б" };
        var building2_1 = new Building { HospitalId = hospital2.Id, Name = "Головний корпус" };
        context.Buildings.AddRange(building1_1, building1_2, building2_1);
        await context.SaveChangesAsync();

        var dept1_1 = new Department { BuildingId = building1_1.Id, Name = "Хірургічне", Specialization = "Загальна хірургія" };
        var dept1_2 = new Department { BuildingId = building1_2.Id, Name = "Кардіологічне", Specialization = "Кардіологія" };
        var dept2_1 = new Department { BuildingId = building2_1.Id, Name = "Неврологічне", Specialization = "Дитяча неврологія" };
        context.Departments.AddRange(dept1_1, dept1_2, dept2_1);
        await context.SaveChangesAsync();

        var room1_1_1 = new Room { DepartmentId = dept1_1.Id, Number = "101", Capacity = 2 };
        var room1_1_2 = new Room { DepartmentId = dept1_1.Id, Number = "102", Capacity = 4 };
        var room1_2_1 = new Room { DepartmentId = dept1_2.Id, Number = "201", Capacity = 3 };
        var room2_1_1 = new Room { DepartmentId = dept2_1.Id, Number = "301", Capacity = 1 };
        context.Rooms.AddRange(room1_1_1, room1_1_2, room1_2_1, room2_1_1);
        await context.SaveChangesAsync();

        // Додаємо ліжка до палат (важливо, щоб кількість відповідала Capacity)
        var beds = new List<Bed>();
        beds.AddRange(Enumerable.Range(1, 2).Select(i => new Bed { RoomId = room1_1_1.Id, IsOccupied = false }));
        beds.AddRange(Enumerable.Range(1, 4).Select(i => new Bed { RoomId = room1_1_2.Id, IsOccupied = false }));
        beds.AddRange(Enumerable.Range(1, 3).Select(i => new Bed { RoomId = room1_2_1.Id, IsOccupied = false }));
        beds.AddRange(Enumerable.Range(1, 1).Select(i => new Bed { RoomId = room2_1_1.Id, IsOccupied = false }));
        context.Beds.AddRange(beds);
        await context.SaveChangesAsync();

        var clinic1 = new Clinic { Name = "Поліклініка №1", Address = "вул. Комарова, 5", HospitalId = hospital1.Id }; // Прив'язана до лікарні 1
        var clinic2 = new Clinic { Name = "Приватна клініка 'Здоров'я'", Address = "вул. Садова, 15" }; // Незалежна
        context.Clinics.AddRange(clinic1, clinic2);
        await context.SaveChangesAsync();

        var lab1 = new Laboratory { Name = "Лабораторія 'Сінево'", Profile = new List<string>{"Біохімія", "Загальний аналіз"} };
        var lab2 = new Laboratory { Name = "Діагностичний центр", Profile = new List<string>{"Гістологія"} };
        // Прив'язуємо лабораторії до клінік/лікарень (M:M)
        lab1.Hospitals.Add(hospital1);
        lab1.Clinics.Add(clinic1);
        lab1.Clinics.Add(clinic2);
        lab2.Hospitals.Add(hospital1);
        lab2.Hospitals.Add(hospital2);
        context.Laboratories.AddRange(lab1, lab2);
        await context.SaveChangesAsync();


        // --- Створення Персоналу ---

        var surgeon1 = new Surgeon { FullName = "Петренко Сергій Іванович", WorkExperienceYears = 15, Specialty = "Surgeon", AcademicDegree = AcademicDegree.Candidate, AcademicTitle = AcademicTitle.AssociateProfessor };
        var cardiologist1 = new Cardiologist { FullName = "Іванова Марія Олегівна", WorkExperienceYears = 10, Specialty = "Cardiologist", AcademicDegree = AcademicDegree.None, AcademicTitle = AcademicTitle.None };
        var dentist1 = new Dentist { FullName = "Ковальчук Андрій Петрович", WorkExperienceYears = 5, Specialty = "Dentist", AcademicDegree = AcademicDegree.None, AcademicTitle = AcademicTitle.None, HazardPayCoefficient = 1.15f };
        var support1 = new SupportStaff { FullName = "Васильєва Олена Сергіївна", WorkExperienceYears = 3, Role = SupportRole.Nurse };
        var support2 = new SupportStaff { FullName = "Гончарук Віктор Львович", WorkExperienceYears = 1, Role = SupportRole.Orderly };
        context.Staffs.AddRange(surgeon1, cardiologist1, dentist1, support1, support2);
        await context.SaveChangesAsync();

        // --- Працевлаштування (Employments) ---

        context.Employments.AddRange(
            new Employment { StaffId = surgeon1.Id, HospitalId = hospital1.Id }, // Хірург в лікарні 1
            new Employment { StaffId = cardiologist1.Id, HospitalId = hospital1.Id }, // Кардіолог в лікарні 1
            new Employment { StaffId = cardiologist1.Id, ClinicId = clinic1.Id }, // Кардіолог сумісник в клініці 1
            new Employment { StaffId = dentist1.Id, ClinicId = clinic2.Id }, // Стоматолог в клініці 2
            new Employment { StaffId = support1.Id, HospitalId = hospital1.Id }, // Медсестра в лікарні 1
            new Employment { StaffId = support2.Id, ClinicId = clinic1.Id } // Санітар в клініці 1
        );
        await context.SaveChangesAsync();

        // --- Створення Пацієнтів ---

        var patient1 = new Patient { FullName = "Сидоренко Ігор Валентинович", DateOfBirth = new DateTime(1985, 5, 10), HealthStatus = "Задовільний", Temperature = 36.6f, ClinicId = clinic1.Id };
        var patient2 = new Patient { FullName = "Мельник Ольга Ярославівна", DateOfBirth = new DateTime(1992, 11, 22), HealthStatus = "Середньої тяжкості", Temperature = 37.2f, ClinicId = clinic2.Id };
        var patient3 = new Patient { FullName = "Шевченко Тарас Григорович", DateOfBirth = new DateTime(1970, 1, 1), HealthStatus = "Тяжкий", Temperature = 38.5f, ClinicId = clinic1.Id };
        context.Patients.AddRange(patient1, patient2, patient3);
        await context.SaveChangesAsync();

        // --- Створення Подій ---

        // Госпіталізації (Admissions)
        var admission1 = new Admission { PatientId = patient3.Id, HospitalId = hospital1.Id, DepartmentId = dept1_2.Id, AdmissionDate = DateTime.UtcNow.AddDays(-10), AttendingDoctorId = cardiologist1.Id }; // Ще лежить
        var admission2 = new Admission { PatientId = patient1.Id, HospitalId = hospital1.Id, DepartmentId = dept1_1.Id, AdmissionDate = DateTime.UtcNow.AddDays(-20), DischargeDate = DateTime.UtcNow.AddDays(-15), AttendingDoctorId = surgeon1.Id }; // Вже виписаний
        context.Admissions.AddRange(admission1, admission2);
        await context.SaveChangesAsync();

        // Призначення ліжок (тільки для тих, хто зараз лежить)
        var bedToOccupy1 = await context.Beds.FirstOrDefaultAsync(b => b.RoomId == room1_2_1.Id && !b.IsOccupied);
        if (bedToOccupy1 != null)
        {
            bedToOccupy1.PatientId = patient3.Id;
            bedToOccupy1.IsOccupied = true;
            context.Beds.Update(bedToOccupy1);
            await context.SaveChangesAsync();
        }

        // Візити (Appointments)
        context.Appointments.AddRange(
            new Appointment { PatientId = patient1.Id, DoctorId = cardiologist1.Id, ClinicId = clinic1.Id, VisitDateTime = DateTime.UtcNow.AddDays(-30), Summary = "Плановий огляд" },
            new Appointment { PatientId = patient2.Id, DoctorId = dentist1.Id, ClinicId = clinic2.Id, VisitDateTime = DateTime.UtcNow.AddDays(-5), Summary = "Лікування карієсу" },
            new Appointment { PatientId = patient1.Id, DoctorId = cardiologist1.Id, ClinicId = clinic1.Id, VisitDateTime = DateTime.UtcNow.AddDays(-1), Summary = "Контрольний огляд" } // Ще один візит
        );
        await context.SaveChangesAsync();

        // Операції (Operations)
        context.Operations.AddRange(
            new Operation { PatientId = patient1.Id, DoctorId = surgeon1.Id, HospitalId = hospital1.Id, Date = DateTime.UtcNow.AddDays(-18), Type = "Апендектомія", IsFatal = false },
            new Operation { PatientId = patient3.Id, DoctorId = surgeon1.Id, HospitalId = hospital1.Id, Date = DateTime.UtcNow.AddDays(-5), Type = "Стентування коронарних артерій", IsFatal = false },
             new Operation { PatientId = patient2.Id, DoctorId = dentist1.Id, ClinicId = clinic2.Id, Date = DateTime.UtcNow.AddDays(-4), Type = "Видалення зуба мудрості", IsFatal = false }
        );
        await context.SaveChangesAsync();

        // Аналізи (LabAnalyses)
        context.LabAnalyses.AddRange(
            new LabAnalysis { PatientId = patient1.Id, LaboratoryId = lab1.Id, AnalysisDate = DateTime.UtcNow.AddDays(-31), AnalysisType = "Загальний аналіз крові", ResultSummary = "В нормі" },
            new LabAnalysis { PatientId = patient3.Id, LaboratoryId = lab1.Id, AnalysisDate = DateTime.UtcNow.AddDays(-10), AnalysisType = "Біохімія", ResultSummary = "Підвищений холестерин" },
            new LabAnalysis { PatientId = patient3.Id, LaboratoryId = lab2.Id, AnalysisDate = DateTime.UtcNow.AddDays(-8), AnalysisType = "Кардіограма", ResultSummary = "Без патологій" }
        );
        await context.SaveChangesAsync();

        // Призначення Лікар-Пацієнт в клініці (M:M)
        context.ClinicDoctorAssignments.AddRange(
            new ClinicDoctorAssignment { PatientId = patient1.Id, DoctorId = cardiologist1.Id, ClinicId = clinic1.Id }, // Пацієнт 1 у кардіолога в клініці 1
            new ClinicDoctorAssignment { PatientId = patient2.Id, DoctorId = dentist1.Id, ClinicId = clinic2.Id } // Пацієнт 2 у стоматолога в клініці 2
        );
        await context.SaveChangesAsync();
    }
}