using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace hospital_api;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23503")
            {
                _logger.LogWarning("Спроба видалення зв'язаного запису.");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;

                var response = new
                {
                    message = "Неможливо видалити цей об'єкт, оскільки він пов'язаний з іншими даними."
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
            else
            {
                // Якщо це інша помилка, прокидаємо її далі (буде 500)
                throw;
            }
        }
    }
}