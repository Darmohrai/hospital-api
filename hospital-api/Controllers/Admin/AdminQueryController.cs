using hospital_api.DTOs.Admin;
using hospital_api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using hospital_api.Services.Interfaces.Admin;

namespace hospital_api.Controllers.Admin;

[Authorize(Roles = "Admin")] // Тільки Адміністратор
[ApiController]
[Route("api/admin-query")]
public class AdminQueryController : ControllerBase
{
    private readonly IAdminQueryService _queryService;

    public AdminQueryController(IAdminQueryService queryService)
    {
        _queryService = queryService;
    }

    /// <summary>
    /// Виконує сирий SQL запит (SELECT, INSERT, UPDATE, DELETE).
    /// Заборонено ALTER, DROP, CREATE.
    /// </summary>
    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteRawSql([FromBody] RawSqlDto dto)
    {
        try
        {
            var result = await _queryService.ExecuteRawSqlAsync(dto.SqlQuery);
            return Ok(result);
        }
        catch (ArgumentException ex) // Помилка валідації (заборонена команда)
        {
            return BadRequest(new { Error = "Validation Error", Message = ex.Message });
        }
        catch (InvalidOperationException ex) // Помилка виконання SQL
        {
            // У production не варто повертати повний текст помилки SQL,
            // але для курсової це допустимо.
            return StatusCode(500, new { Error = "SQL Execution Error", Message = ex.Message });
        }
        catch (Exception ex) // Інші неочікувані помилки
        {
            return StatusCode(500, new { Error = "Internal Server Error", Message = ex.Message });
        }
    }
}