using hospital_api.Data;
using hospital_api.Repositories.Implementations;
using hospital_api.Repositories.Implementations.HospitalRepo;
using hospital_api.Repositories.Implementations.StaffRepo;
using hospital_api.Repositories.Interfaces;
using hospital_api.Repositories.Interfaces.HospitalRepo;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Implementations;
using hospital_api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// repositories
builder.Services.AddScoped<IClinicRepository, ClinicRepository>();
builder.Services.AddScoped<ILaboratoryRepository, LaboratoryRepository>();
builder.Services.AddScoped<IOperationRepository, OperationRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
    //hospital repo
builder.Services.AddScoped<IBedRepository, BedRepository>();
builder.Services.AddScoped<IBuildingRepository, BuildingRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IHospitalRepository, HospitalRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
    //staff repo
builder.Services.AddScoped<ICardiologistRepository, CardiologistRepository>();
builder.Services.AddScoped<IDentistRepository, DentistRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IGynecologistRepository, GynecologistRepository>();
builder.Services.AddScoped<INeurologistRepository, NeurologistRepository>();
builder.Services.AddScoped<IOphthalmologistRepository, OphthalmologistRepository>();
builder.Services.AddScoped<IRadiologistRepository, RadiologistRepository>();
builder.Services.AddScoped<ISurgeonRepository, SurgeonRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<ISupportStaffRepository, SupportStaffRepository>();

//services
builder.Services.AddScoped<IHospitalService, HospitalService>();
builder.Services.AddScoped<IClinicService, ClinicService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));


builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}


app.Run();