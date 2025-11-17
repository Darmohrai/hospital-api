using hospital_api.DTOs.Staff;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers;

[Authorize]
[ApiController]
[Route("api/staff/ophthalmologists")] // Більш чіткий маршрут
public class OphthalmologistController : ControllerBase
{
    private readonly IOphthalmologistService _ophthalmologistService;

    public OphthalmologistController(IOphthalmologistService ophthalmologistService)
    {
        _ophthalmologistService = ophthalmologistService;
    }

    /// <summary>
    /// Отримує список всіх офтальмологів.
    /// </summary>
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var ophthalmologists = await _ophthalmologistService.GetAllAsync();
        return Ok(ophthalmologists);
    }

    /// <summary>
    /// Отримує офтальмолога за його ID.
    /// </summary>
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ophthalmologist = await _ophthalmologistService.GetByIdAsync(id);
        if (ophthalmologist == null)
            return NotFound();

        return Ok(ophthalmologist);
    }

    /// <summary>
    /// Створює нового офтальмолога.
    /// </summary>
    [Authorize(Roles = "Operator, Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOphthalmologistDto dto)
    {
        var ophthalmologist = new Ophthalmologist
        {
            FullName = dto.FullName,
            WorkExperienceYears = dto.WorkExperienceYears,
            AcademicDegree = dto.AcademicDegree,
            AcademicTitle = dto.AcademicTitle,
            ExtendedVacationDays = dto.ExtendedVacationDays
        };

        var result = await _ophthalmologistService.CreateAsync(ophthalmologist);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Оновлює дані існуючого офтальмолога.
    /// </summary>
    [Authorize(Roles = "Operator, Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Ophthalmologist ophthalmologist)
    {
        if (id != ophthalmologist.Id)
            return BadRequest("ID mismatch.");

        var result = await _ophthalmologistService.UpdateAsync(ophthalmologist);

        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return NoContent();
    }

    /// <summary>
    /// Видаляє офтальмолога за його ID.
    /// </summary>
    [Authorize(Roles = "Operator, Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _ophthalmologistService.DeleteAsync(id);

        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return NoContent();
    }

    /// <summary>
    /// Отримує офтальмологів з розширеною відпусткою.
    /// </summary>
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("extended-vacation")]
    public async Task<IActionResult> GetByExtendedVacation([FromQuery] int minDays)
    {
        var result = await _ophthalmologistService.GetByExtendedVacationDaysAsync(minDays);
        return Ok(result);
    }

    /// <summary>
    /// Отримує профіль офтальмолога у вигляді текстового звіту.
    /// </summary>
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("{id}/profile-summary")]
    public async Task<IActionResult> GetProfileSummary(int id)
    {
        var summary = await _ophthalmologistService.GetProfileSummaryAsync(id);

        if (summary.Contains("not found"))
            return NotFound(summary);

        return Ok(summary);
    }
    
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("vacation/{ophthalmologistId}")]
    public async Task<IActionResult> GetExtendVacationDays(int ophthalmologistId)
    {
        var days = await _ophthalmologistService.GetOphthalmologistExtendedVacationDaysAsync(ophthalmologistId);

        return Ok(days);
    }
}