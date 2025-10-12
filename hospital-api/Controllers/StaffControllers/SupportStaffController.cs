using hospital_api.DTOs.Staff;
using hospital_api.Models.StaffAggregate;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers;

[ApiController]
[Route("api/staff/support")] // Більш чіткий базовий маршрут
public class SupportStaffController : ControllerBase
{
    private readonly ISupportStaffService _supportStaffService;

    public SupportStaffController(ISupportStaffService supportStaffService)
    {
        _supportStaffService = supportStaffService;
    }

    /// <summary>
    /// Отримує весь допоміжний персонал.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var staff = await _supportStaffService.GetAllAsync();
        return Ok(staff);
    }

    /// <summary>
    /// Отримує співробітника за його ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var staff = await _supportStaffService.GetByIdAsync(id);
        if (staff == null)
            return NotFound();

        return Ok(staff);
    }

    /// <summary>
    /// Створює нового співробітника допоміжного персоналу.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupportStaffDto dto)
    {
        var staff = new SupportStaff
        {
            FullName = dto.FullName,
            WorkExperienceYears = dto.WorkExperienceYears,
            Role = dto.Role
        };

        await _supportStaffService.CreateAsync(staff);
        return CreatedAtAction(nameof(GetById), new { id = staff.Id }, staff);
    }

    /// <summary>
    /// Оновлює дані існуючого співробітника.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SupportStaff staff)
    {
        if (id != staff.Id)
            return BadRequest("ID mismatch.");

        await _supportStaffService.UpdateAsync(staff);
        return NoContent();
    }

    /// <summary>
    /// Видаляє співробітника за його ID.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _supportStaffService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Отримує персонал за вказаною роллю.
    /// </summary>
    [HttpGet("role/{role}")]
    public async Task<IActionResult> GetByRole(SupportRole role)
    {
        var staff = await _supportStaffService.GetByRoleAsync(role);
        return Ok(staff);
    }

    /// <summary>
    /// Отримує персонал з вказаної клініки (опціонально - за роллю).
    /// </summary>
    [HttpGet("clinic/{clinicId}")]
    public async Task<IActionResult> GetByClinic(int clinicId, [FromQuery] SupportRole? role)
    {
        var staff = await _supportStaffService.GetByClinicAsync(clinicId, role);
        return Ok(staff);
    }

    /// <summary>
    /// Отримує персонал з вказаної лікарні (опціонально - за роллю).
    /// </summary>
    [HttpGet("hospital/{hospitalId}")]
    public async Task<IActionResult> GetByHospital(int hospitalId, [FromQuery] SupportRole? role)
    {
        var staff = await _supportStaffService.GetByHospitalAsync(hospitalId, role);
        return Ok(staff);
    }

    /// <summary>
    /// Отримує профіль співробітника у вигляді текстового звіту.
    /// </summary>
    [HttpGet("{id}/profile-summary")]
    public async Task<IActionResult> GetProfileSummary(int id)
    {
        var summary = await _supportStaffService.GetProfileSummaryAsync(id);

        if (summary.Contains("not found")) // Проста перевірка на помилку
            return NotFound(summary);

        return Ok(summary);
    }
}