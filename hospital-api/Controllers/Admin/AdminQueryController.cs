using hospital_api.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using hospital_api.Services.Interfaces.Admin;

namespace hospital_api.Controllers.Admin;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin-query")]
public class AdminQueryController : ControllerBase
{
    private readonly IAdminQueryService _queryService;

    public AdminQueryController(IAdminQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteRawSql([FromBody] RawSqlDto dto)
    {
        try
        {
            var result = await _queryService.ExecuteRawSqlAsync(dto.SqlQuery);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = "Validation Error", Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { Error = "SQL Execution Error", Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Internal Server Error", Message = ex.Message });
        }
    }
}