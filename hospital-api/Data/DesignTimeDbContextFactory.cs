using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace hospital_api.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Створюємо конфігурацію, щоб прочитати appsettings.json
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Отримуємо рядок підключення з appsettings.json
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Вказуємо провайдера бази даних (у вашому випадку, скоріш за все, SQL Server)
        builder.UseSqlServer(connectionString);

        return new ApplicationDbContext(builder.Options);
    }
}