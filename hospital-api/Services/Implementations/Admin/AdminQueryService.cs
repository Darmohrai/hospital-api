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

        if (trimmedQuery.StartsWith("ALTER ") || trimmedQuery.StartsWith("DROP ") || trimmedQuery.StartsWith("CREATE "))
        {
            throw new ArgumentException("Команди зміни схеми (ALTER, DROP, CREATE) заборонені.");
        }

        if (trimmedQuery.StartsWith("SELECT "))
        {
            return await ExecuteSelectQueryAsync(sqlQuery);
        }
        else if (trimmedQuery.StartsWith("INSERT ") || trimmedQuery.StartsWith("UPDATE ") || trimmedQuery.StartsWith("DELETE "))
        {
            int affectedRows = await _context.Database.ExecuteSqlRawAsync(sqlQuery);
            return new { AffectedRows = affectedRows };
        }
        else
        {
            throw new ArgumentException("Підтримуються тільки запити SELECT, INSERT, UPDATE, DELETE.");
        }
    }

    private async Task<List<Dictionary<string, object>>> ExecuteSelectQueryAsync(string sqlQuery)
    {
        var results = new List<Dictionary<string, object>>();
        
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
                    row[columnName] = value == DBNull.Value ? null : value;
                }
                results.Add(row);
            }
        }
        catch (DbException ex)
        {
            throw new InvalidOperationException($"Помилка виконання SQL-запиту: {ex.Message}", ex);
        }

        return results;
    }
}