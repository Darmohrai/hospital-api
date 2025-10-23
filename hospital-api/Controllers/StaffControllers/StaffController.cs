using hospital_api.Models.StaffAggregate;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers;

[Authorize]
[ApiController]
[Route("api/staff")] // Спрощений та більш чіткий маршрут
public class StaffController : ControllerBase
{
    private readonly IStaffService _staffService;

    public StaffController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    /// <summary>
    /// Отримує список всього персоналу (всіх типів).
    /// </summary>
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var staff = await _staffService.GetAllAsync();
        return Ok(staff);
    }

    /// <summary>
    /// Отримує співробітника за його ID.
    /// </summary>
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var staff = await _staffService.GetByIdAsync(id);
        if (staff == null)
            return NotFound();

        return Ok(staff);
    }

    /// <summary>
    /// Видаляє співробітника за його ID.
    /// </summary>
    [Authorize(Roles = "Operator, Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _staffService.DeleteAsync(id);
        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return NoContent();
    }

    /// <summary>
    /// Отримує всіх співробітників, що працюють у вказаній клініці.
    /// </summary>
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("clinic/{clinicId}")]
    public async Task<IActionResult> GetByClinic(int clinicId)
    {
        var staff = await _staffService.GetByClinicAsync(clinicId);
        return Ok(staff);
    }

    /// <summary>
    /// Отримує всіх співробітників, що працюють у вказаній лікарні.
    /// </summary>
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("hospital/{hospitalId}")]
    public async Task<IActionResult> GetByHospital(int hospitalId)
    {
        var staff = await _staffService.GetByHospitalAsync(hospitalId);
        return Ok(staff);
    }

    /// <summary>
    /// Отримує співробітників з досвідом роботи не менше вказаної кількості років.
    /// </summary>
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("experienced")]
    public async Task<IActionResult> GetExperienced([FromQuery] int minYears)
    {
        var staff = await _staffService.GetExperiencedStaffAsync(minYears);
        return Ok(staff);
    }

    /// <summary>
    /// Розраховує річний бонус для співробітника.
    /// </summary>
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("{id}/annual-bonus")]
    public async Task<IActionResult> GetAnnualBonus(int id)
    {
        var result = await _staffService.CalculateAnnualBonusAsync(id);

        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(new { StaffId = id, AnnualBonus = result.Data });
    }

    // ❌ Методи Create та Update були видалені, оскільки вони не можуть
    // коректно працювати з абстрактним класом Staff.
    // Створення та оновлення відбувається через специфічні контролери:
    // POST /api/staff/support (в SupportStaffController)
    // POST /api/hospitals/{id}/neurologists (в NeurologistController)
    // і так далі.
}