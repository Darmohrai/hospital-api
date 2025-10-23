using System.Text;
using hospital_api.Data;
using hospital_api.Repositories.Implementations;
using hospital_api.Repositories.Implementations.Auth;
using hospital_api.Repositories.Implementations.HospitalRepo;
using hospital_api.Repositories.Implementations.StaffRepo;
using hospital_api.Repositories.Implementations.Tracking;
using hospital_api.Repositories.Interfaces;
using hospital_api.Repositories.Interfaces.Auth;
using hospital_api.Repositories.Interfaces.HospitalRepo;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Repositories.Interfaces.Tracking;
using hospital_api.Services.Implementations;
using hospital_api.Services.Implementations.Admin;
using hospital_api.Services.Implementations.HospitalServices;
using hospital_api.Services.Implementations.StaffServices;
using hospital_api.Services.Implementations.Tracking;
using hospital_api.Services.Interfaces;
using hospital_api.Services.Interfaces.Admin;
using hospital_api.Services.Interfaces.HospitalServices;
using hospital_api.Services.Interfaces.StaffServices;
using hospital_api.Services.Interfaces.Tracking;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Налаштування бази даних (DbContext) ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- 2. Додаємо Identity ---
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// --- 3. Налаштування JWT ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        // RoleClaimType = "role",
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// --- 4. Реєстрація репозиторіїв (Dependency Injection) ---
#region Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IHospitalRepository, HospitalRepository>();
builder.Services.AddScoped<IClinicRepository, ClinicRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<ILaboratoryRepository, LaboratoryRepository>();
builder.Services.AddScoped<IOperationRepository, OperationRepository>();
builder.Services.AddScoped<IBuildingRepository, BuildingRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IBedRepository, BedRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IEmploymentRepository, EmploymentRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<ILabAnalysisRepository, LabAnalysisRepository>();
builder.Services.AddScoped<IAdmissionRepository, AdmissionRepository>();
builder.Services.AddScoped<IClinicDoctorAssignmentRepository, ClinicDoctorAssignmentRepository>();
builder.Services.AddScoped<IClinicDoctorAssignmentService, ClinicDoctorAssignmentService>();
builder.Services.AddScoped<IUpgradeRequestRepository, UpgradeRequestRepository>();
#endregion

// --- 5. Реєстрація сервісів ---
#region Services
builder.Services.AddScoped<IHospitalService, HospitalService>();
builder.Services.AddScoped<IClinicService, ClinicService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<ILaboratoryService, LaboratoryService>();
builder.Services.AddScoped<IOperationService, OperationService>();
builder.Services.AddScoped<IBuildingService, BuildingService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IBedService, BedService>();
builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<ISupportStaffService, SupportStaffService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<INeurologistService, NeurologistService>();
builder.Services.AddScoped<ISurgeonService, SurgeonService>();
builder.Services.AddScoped<ICardiologistService, CardiologistService>();
builder.Services.AddScoped<IDentistService, DentistService>();
builder.Services.AddScoped<IGynecologistService, GynecologistService>();
builder.Services.AddScoped<IOphthalmologistService, OphthalmologistService>();
builder.Services.AddScoped<IRadiologistService, RadiologistService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IAdmissionService, AdmissionService>();
builder.Services.AddScoped<ILabAnalysisService, LabAnalysisService>();
builder.Services.AddScoped<IAdminQueryService, AdminQueryService>();
#endregion

// --- 6. API та Swagger ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- 7. Middleware ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// --- 8. Ініціалізація бази даних і ролей ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Якщо у тебе є DbInitializer
        await DbInitializer.Initialize(context, userManager, roleManager);

        // Якщо хочеш автоматично створити ролі:
        string[] roleNames = { "Admin", "Operator", "Authorized", "Guest" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Помилка під час ініціалізації бази даних.");
    }
}

app.Run();
