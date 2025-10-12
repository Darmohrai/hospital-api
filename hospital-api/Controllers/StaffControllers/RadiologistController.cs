using hospital_api.DTOs.Staff;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers;

[ApiController]
[Route("api/staff/radiologists")] // Більш чіткий маршрут
public class RadiologistController : ControllerBase
{
    private readonly IRadiologistService _radiologistService;

    public RadiologistController(IRadiologistService radiologistService)
    {
        _radiologistService = radiologistService;
    }

    /// <summary>
    /// Отримує список всіх радіологів.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var radiologists = await _radiologistService.GetAllAsync();
        return Ok(radiologists);
    }

    /// <summary>
    /// Отримує радіолога за його ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var radiologist = await _radiologistService.GetByIdAsync(id);
        if (radiologist == null)
            return NotFound();

        return Ok(radiologist);
    }

    /// <summary>
    /// Створює нового радіолога.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRadiologistDto dto)
    {
        var radiologist = new Radiologist
        {
            FullName = dto.FullName,
            WorkExperienceYears = dto.WorkExperienceYears,
            AcademicDegree = dto.AcademicDegree,
            AcademicTitle = dto.AcademicTitle,
            HazardPayCoefficient = dto.HazardPayCoefficient,
            ExtendedVacationDays = dto.ExtendedVacationDays
        };

        var result = await _radiologistService.CreateAsync(radiologist);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Оновлює дані існуючого радіолога.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Radiologist radiologist)
    {
        if (id != radiologist.Id)
            return BadRequest("ID mismatch.");

        var result = await _radiologistService.UpdateAsync(radiologist);

        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return NoContent();
    }

    /// <summary>
    /// Видаляє радіолога за його ID.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _radiologistService.DeleteAsync(id);

        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return NoContent();
    }

    /// <summary>
    /// Фільтрація за коефіцієнтом шкідливості.
    /// </summary>
    [HttpGet("hazard-pay")]
    public async Task<IActionResult> GetByHazardPay([FromQuery] float minCoefficient)
    {
        var result = await _radiologistService.GetByHazardPayCoefficientAsync(minCoefficient);
        return Ok(result);
    }

    /// <summary>
    /// Фільтрація за розширеною відпусткою.
    /// </summary>
    [HttpGet("extended-vacation")]
    public async Task<IActionResult> GetByExtendedVacation([FromQuery] int minDays)
    {
        var result = await _radiologistService.GetByExtendedVacationDaysAsync(minDays);
        return Ok(result);
    }

    /// <summary>
    /// Фільтрація за коефіцієнтом шкідливості та відпусткою.
    /// </summary>
    [HttpGet("hazard-and-vacation")]
    public async Task<IActionResult> GetByHazardAndVacation([FromQuery] float minCoefficient, [FromQuery] int minDays)
    {
        var result = await _radiologistService.GetByHazardPayAndVacationAsync(minCoefficient, minDays);
        return Ok(result);
    }

    /// <summary>
    /// Отримує профіль радіолога у вигляді текстового звіту.
    /// </summary>
    [HttpGet("{id}/profile-summary")]
    public async Task<IActionResult> GetProfileSummary(int id)
    {
        var summary = await _radiologistService.GetProfileSummaryAsync(id);

        if (summary.Contains("not found"))
            return NotFound(summary);

        return Ok(summary);
    }
}