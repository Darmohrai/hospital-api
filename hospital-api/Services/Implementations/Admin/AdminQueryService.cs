using System.Data.Common;
using hospital_api.Data;
using hospital_api.Services.Interfaces.Admin;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace hospital_api.Services.Implementations.Admin;

public class AdminQueryService : IAdminQueryService
{
    private readonly ApplicationDbContext _context;
    private readonly string _connectionString;

    public AdminQueryService(ApplicationDbContext context)
    {
        _context = context;
        _connectionString = _context.Database.GetConnectionString()
            ?? throw new InvalidOperationException("Connection string not found.");
    }

    public async Task<object> ExecuteRawSqlAsync(string sqlQuery)
    {
        var trimmedQuery = sqlQuery.Trim().ToUpperInvariant();

        // 1. ВАЛІДАЦІЯ: Заборона небезпечних команд
        if (trimmedQuery.StartsWith("ALTER ") || trimmedQuery.StartsWith("DROP ") || trimmedQuery.StartsWith("CREATE "))
        {
            throw new ArgumentException("Команди зміни схеми (ALTER, DROP, CREATE) заборонені.");
        }

        // 2. Визначення типу запиту
        if (trimmedQuery.StartsWith("SELECT "))
        {
            // Виконання SELECT за допомогою ADO.NET для гнучкості
            return await ExecuteSelectQueryAsync(sqlQuery);
        }
        else if (trimmedQuery.StartsWith("INSERT ") || trimmedQuery.StartsWith("UPDATE ") || trimmedQuery.StartsWith("DELETE "))
        {
            // Виконання команд модифікації через EF Core
            int affectedRows = await _context.Database.ExecuteSqlRawAsync(sqlQuery);
            return new { AffectedRows = affectedRows }; // Повертаємо об'єкт з кількістю рядків
        }
        else
        {
            throw new ArgumentException("Підтримуються тільки запити SELECT, INSERT, UPDATE, DELETE.");
        }
    }

    private async Task<List<Dictionary<string, object>>> ExecuteSelectQueryAsync(string sqlQuery)
    {
        var results = new List<Dictionary<string, object>>();
        
        // Використовуємо NpgsqlConnection, якщо у тебе PostgreSQL.
        // Заміни на SqlConnection, якщо використовуєш SQL Server.
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new NpgsqlCommand(sqlQuery, connection);
        
        try
        {
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var columnName = reader.GetName(i);
                    var value = reader.GetValue(i);
                    // Обробка DBNull (якщо значення в базі NULL)
                    row[columnName] = value == DBNull.Value ? null : value;
                }
                results.Add(row);
            }
        }
        catch (DbException ex)
        {
            // Обробка помилок SQL (синтаксис, права доступу тощо)
            throw new InvalidOperationException($"Помилка виконання SQL-запиту: {ex.Message}", ex);
        }

        return results;
    }
}