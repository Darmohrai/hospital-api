namespace hospital_api.Services.Interfaces.Admin;

public interface IAdminQueryService
{
    Task<object> ExecuteRawSqlAsync(string sqlQuery);
}