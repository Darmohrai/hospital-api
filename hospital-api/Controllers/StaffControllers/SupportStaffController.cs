using hospital_api.Models.StaffAggregate;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers;

[ApiController]
[Route("api/staff/[controller]")]
public class SupportStaffController : ControllerBase
{
    private readonly ISupportStaffService _supportStaffService;

    public SupportStaffController(ISupportStaffService supportStaffService)
    {
        _supportStaffService = supportStaffService;
    }

    // Отримати всіх обслуговуючих співробітників
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var staff = await _supportStaffService.GetAllSupportStaffAsync();
        return Ok(staff);
    }

    // Отримати співробітника за ID
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var staff = await _supportStaffService.GetSupportStaffByIdAsync(id);
        if (staff == null)
            return NotFound();

        return Ok(staff);
    }

    // Додати нового співробітника
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SupportStaff staff)
    {
        await _supportStaffService.AddSupportStaffAsync(staff);
        return CreatedAtAction(nameof(Get), new { id = staff.Id }, staff);
    }

    // Оновити співробітника
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SupportStaff staff)
    {
        if (id != staff.Id)
            return BadRequest();

        await _supportStaffService.UpdateSupportStaffAsync(staff);
        return NoContent();
    }

    // Видалити співробітника
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _supportStaffService.DeleteSupportStaffAsync(id);
        return NoContent();
    }

    // Фільтрація за роллю
    [HttpGet("role/{role}")]
    public async Task<IActionResult> GetByRole(SupportRole role)
    {
        var staff = await _supportStaffService.GetByRoleAsync(role);
        return Ok(staff);
    }

    // Фільтрація за клінікою та роллю
    [HttpGet("clinic/{clinicId}/role/{role}")]
    public async Task<IActionResult> GetByClinicAndRole(int clinicId, SupportRole role)
    {
        var staff = await _supportStaffService.GetByClinicIdAndRoleAsync(clinicId, role);
        return Ok(staff);
    }

    // Фільтрація за лікарнею та роллю
    [HttpGet("hospital/{hospitalId}/role/{role}")]
    public async Task<IActionResult> GetByHospitalAndRole(int hospitalId, SupportRole role)
    {
        var staff = await _supportStaffService.GetByHospitalIdAndRoleAsync(hospitalId, role);
        return Ok(staff);
    }

    // Профіль співробітника у вигляді текстового звіту
    [HttpGet("{id}/profile-summary")]
    public async Task<IActionResult> GetProfileSummary(int id)
    {
        var summary = await _supportStaffService.GetSupportStaffProfileSummaryAsync(id);
        return Ok(summary);
    }
}