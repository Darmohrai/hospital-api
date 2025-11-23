using hospital_api.DTOs.Staff;
using hospital_api.Models.StaffAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers;

[Authorize]
[ApiController]
[Route("api/staff/support")] // Більш чіткий базовий маршрут
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

    // GET: api/staff/support
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupportStaff>>> GetAll()
    {
        var staff = await _service.GetAllAsync();
        return Ok(staff);
    }

    // GET: api/staff/support/{id}
    // Оновлено: повертає розширені дані для форми редагування
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetById(int id)
    {
        var staff = await _service.GetByIdAsync(id);
        if (staff == null)
        {
            return NotFound($"Support staff with ID {id} not found.");
        }

        // Отримуємо місце роботи, щоб заповнити форму на фронтенді
        var employments = await _employmentRepository.GetEmploymentsByStaffIdAsync(id);
        var activeEmployment = employments.FirstOrDefault();

        // Повертаємо анонімний об'єкт з додатковими полями
        return Ok(new 
        {
            staff.Id,
            staff.FullName,
            staff.Role,
            staff.WorkExperienceYears,
            // Додаємо ID прив'язок
            HospitalId = activeEmployment?.HospitalId,
            ClinicId = activeEmployment?.ClinicId
        });
    }

    // POST: api/staff/support
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
            //
            // Передаємо нові параметри у сервіс
            await _service.CreateAsync(staff, dto.HospitalId, dto.ClinicId);
            
            return CreatedAtAction(nameof(GetById), new { id = staff.Id }, staff);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/staff/support/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateSupportStaffDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingStaff = await _service.GetByIdAsync(id);
        if (existingStaff == null)
            return NotFound();

        // Оновлюємо поля моделі
        existingStaff.FullName = dto.FullName;
        existingStaff.Role = dto.Role;
        existingStaff.WorkExperienceYears = dto.WorkExperienceYears;

        try
        {
            //
            // Передаємо нові параметри у сервіс для оновлення зв'язків
            await _service.UpdateAsync(existingStaff, dto.HospitalId, dto.ClinicId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // DELETE: api/staff/support/{id}
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

    // GET: api/staff/support/role/{role}
    [HttpGet("role/{role}")]
    public async Task<ActionResult<IEnumerable<SupportStaff>>> GetByRole(SupportRole role)
    {
        var staff = await _service.GetByRoleAsync(role);
        return Ok(staff);
    }

    // GET: api/staff/support/{id}/profile-summary
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