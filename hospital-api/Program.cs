using System.Text;
using System.Text.Json.Serialization;
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

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true; 
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var MyAllowSpecificOrigins = "AllowFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


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
builder.Services.AddScoped<IEmploymentService, EmploymentService>();

#endregion


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapFallbackToFile("index.html");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await DbInitializer.Initialize(context, userManager, roleManager);

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