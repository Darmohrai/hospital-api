using hospital_api.Data;
using hospital_api.Repositories.Implementations;
using hospital_api.Repositories.Implementations.HospitalRepo;
using hospital_api.Repositories.Implementations.StaffRepo;
using hospital_api.Repositories.Interfaces;
using hospital_api.Repositories.Interfaces.HospitalRepo;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Implementations;
using hospital_api.Services.Implementations.HospitalServices;
using hospital_api.Services.Implementations.StaffServices;
using hospital_api.Services.Interfaces;
using hospital_api.Services.Interfaces.HospitalServices;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Налаштування бази даних (DbContext) ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- 2. Налаштування Identity ---
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// --- 3. Реєстрація репозиторіїв (Dependency Injection) ---
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
#endregion

// --- 4. Реєстрація сервісів ---
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
#endregion

// --- 5. Налаштування API та Swagger ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- 6. Налаштування конвеєра запитів ---
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

app.Run();