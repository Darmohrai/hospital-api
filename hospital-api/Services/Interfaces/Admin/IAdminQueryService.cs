namespace hospital_api.Services.Interfaces.Admin;

public interface IAdminQueryService
{
    /// <summary>
    /// Виконує сирий SQL-запит.
    /// Забороняє команди зміни схеми (ALTER, DROP, CREATE).
    /// </summary>
    /// <param name="sqlQuery">SQL-запит</param>
    /// <returns>
    /// Для SELECT: Список словників (рядок -> значення).
    /// Для INSERT/UPDATE/DELETE: Кількість змінених рядків.
    /// </returns>
    Task<object> ExecuteRawSqlAsync(string sqlQuery);
}