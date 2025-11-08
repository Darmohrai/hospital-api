using hospital_api.DTOs.Staff;
using hospital_api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers;

[Authorize(Roles = "Admin, Operator")]
[ApiController]
[Route("api/employment")]
public class EmploymentController : ControllerBase
{
    private readonly IEmploymentService _employmentService;

    public EmploymentController(IEmploymentService employmentService)
    {
        _employmentService = employmentService;
    }

    /// <summary>
    /// Створює зв'язок Staff-Hospital/Clinic
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateEmployment([FromBody] CreateEmploymentDto dto)
    {
        var response = await _employmentService.CreateEmploymentAsync(dto);

        // ✅ ВИПРАВЛЕНО: Перевіряємо .IsSuccess
        if (!response.IsSuccess)
        {
            // ✅ ВИПРАВЛЕНО: Повертаємо .ErrorMessage
            return BadRequest(response.ErrorMessage);
        }

        return Ok(response.Data);
    }

    /// <summary>
    /// Видаляє запис про працевлаштування
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployment(int id)
    {
        var response = await _employmentService.DeleteEmploymentAsync(id);

        // ✅ ВИПРАВЛЕНО: Перевіряємо .IsSuccess
        if (!response.IsSuccess)
        {
            // ✅ ВИПРАВЛЕНО: Повертаємо .ErrorMessage
            return NotFound(response.ErrorMessage);
        }

        return NoContent();
    }
}