using hospital_api.DTOs.Staff;
using hospital_api.Models.StaffAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers;

[Authorize]
[ApiController]
[Route("api/staff/support")]
public class SupportStaffController : ControllerBase
{
    private readonly ISupportStaffService _service;
    private readonly IEmploymentRepository _employmentRepository;

    public SupportStaffController(
        ISupportStaffService service,
        IEmploymentRepository employmentRepository)
    {
        _service = service;
        _employmentRepository = employmentRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupportStaff>>> GetAll()
    {
        var staff = await _service.GetAllAsync();
        return Ok(staff);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetById(int id)
    {
        var staff = await _service.GetByIdAsync(id);
        if (staff == null)
        {
            return NotFound($"Support staff with ID {id} not found.");
        }

        var employments = await _employmentRepository.GetEmploymentsByStaffIdAsync(id);
        var activeEmployment = employments.FirstOrDefault();

        return Ok(new 
        {
            staff.Id,
            staff.FullName,
            staff.Role,
            staff.WorkExperienceYears,
            HospitalId = activeEmployment?.HospitalId,
            ClinicId = activeEmployment?.ClinicId
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupportStaffDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var staff = new SupportStaff
        {
            FullName = dto.FullName,
            Role = dto.Role,
            WorkExperienceYears = dto.WorkExperienceYears
        };

        try
        {
            await _service.CreateAsync(staff, dto.HospitalId, dto.ClinicId);
            
            return CreatedAtAction(nameof(GetById), new { id = staff.Id }, staff);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateSupportStaffDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingStaff = await _service.GetByIdAsync(id);
        if (existingStaff == null)
            return NotFound();

        existingStaff.FullName = dto.FullName;
        existingStaff.Role = dto.Role;
        existingStaff.WorkExperienceYears = dto.WorkExperienceYears;

        try
        {
            await _service.UpdateAsync(existingStaff, dto.HospitalId, dto.ClinicId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existingStaff = await _service.GetByIdAsync(id);
        if (existingStaff == null)
        {
            return NotFound();
        }

        await _service.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("role/{role}")]
    public async Task<ActionResult<IEnumerable<SupportStaff>>> GetByRole(SupportRole role)
    {
        var staff = await _service.GetByRoleAsync(role);
        return Ok(staff);
    }

    [HttpGet("{id}/profile-summary")]
    public async Task<ActionResult<string>> GetProfileSummary(int id)
    {
        var summary = await _service.GetProfileSummaryAsync(id);
        if (summary == "Support staff not found.")
        {
            return NotFound(summary);
        }
        return Ok(summary);
    }
}