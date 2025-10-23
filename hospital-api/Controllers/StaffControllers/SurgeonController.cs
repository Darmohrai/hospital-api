using hospital_api.DTOs.Staff;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers;

[Authorize]
[ApiController]
[Route("api/staff/surgeons")] // Більш чіткий та REST-сумісний маршрут
public class SurgeonController : ControllerBase
{
    private readonly ISurgeonService _surgeonService;

    public SurgeonController(ISurgeonService surgeonService)
    {
        _surgeonService = surgeonService;
    }

    /// <summary>
    /// Отримує список всіх хірургів.
    /// </summary>
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var surgeons = await _surgeonService.GetAllAsync();
        return Ok(surgeons);
    }

    /// <summary>
    /// Отримує хірурга за його ID.
    /// </summary>
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var surgeon = await _surgeonService.GetByIdAsync(id);
        if (surgeon == null)
            return NotFound();

        return Ok(surgeon);
    }

    /// <summary>
    /// Створює нового хірурга.
    /// </summary>
    [Authorize(Roles = "Operator, Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSurgeonDto dto)
    {
        var surgeon = new Surgeon
        {
            FullName = dto.FullName,
            WorkExperienceYears = dto.WorkExperienceYears,
            AcademicDegree = dto.AcademicDegree,
            AcademicTitle = dto.AcademicTitle,
            OperationCount = dto.OperationCount,
            FatalOperationCount = dto.FatalOperationCount
        };

        var result = await _surgeonService.CreateAsync(surgeon);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Оновлює дані існуючого хірурга.
    /// </summary>
    [Authorize(Roles = "Operator, Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Surgeon surgeon)
    {
        if (id != surgeon.Id)
            return BadRequest("ID mismatch.");

        var result = await _surgeonService.UpdateAsync(surgeon);

        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return NoContent();
    }

    /// <summary>
    /// Видаляє хірурга за його ID.
    /// </summary>
    [Authorize(Roles = "Operator, Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _surgeonService.DeleteAsync(id);

        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return NoContent();
    }

    /// <summary>
    /// Отримує хірургів з кількістю операцій не менше вказаної.
    /// </summary>
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("min-operations/{count}")]
    public async Task<IActionResult> GetByMinimumOperationCount(int count)
    {
        var surgeons = await _surgeonService.GetByMinimumOperationCountAsync(count);
        return Ok(surgeons);
    }

    /// <summary>
    /// Отримує профіль хірурга у вигляді текстового звіту.
    /// </summary>
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("{id}/profile-summary")]
    public async Task<IActionResult> GetProfileSummary(int id)
    {
        var summary = await _surgeonService.GetProfileSummaryAsync(id);

        if (summary.Contains("not found"))
            return NotFound(summary);

        return Ok(summary);
    }
}