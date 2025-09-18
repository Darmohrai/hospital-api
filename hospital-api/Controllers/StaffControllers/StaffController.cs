using hospital_api.Models.StaffAggregate;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers;

[ApiController]
[Route("api/[controller]")]
public class StaffController : ControllerBase
{
    private readonly IStaffService _staffService;

    public StaffController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    // GET: api/Staff
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var staff = await _staffService.GetAllStaffAsync();
        return Ok(staff);
    }

    // GET: api/Staff/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var staff = await _staffService.GetStaffByIdAsync(id);
        if (staff == null)
            return NotFound();

        return Ok(staff);
    }

    // POST: api/Staff
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Staff staff)
    {
        try
        {
            await _staffService.AddStaffAsync(staff);
            return CreatedAtAction(nameof(Get), new { id = staff.Id }, staff);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/Staff/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Staff staff)
    {
        if (id != staff.Id)
            return BadRequest("ID mismatch.");

        try
        {
            await _staffService.UpdateStaffAsync(staff);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // DELETE: api/Staff/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _staffService.DeleteStaffAsync(id);
        return NoContent();
    }

    // GET: api/Staff/clinic/{clinicId}
    [HttpGet("clinic/{clinicId}")]
    public async Task<IActionResult> GetByClinic(int clinicId)
    {
        var staff = await _staffService.GetStaffByClinicIdAsync(clinicId);
        return Ok(staff);
    }

    // GET: api/Staff/hospital/{hospitalId}
    [HttpGet("hospital/{hospitalId}")]
    public async Task<IActionResult> GetByHospital(int hospitalId)
    {
        var staff = await _staffService.GetStaffByHospitalIdAsync(hospitalId);
        return Ok(staff);
    }

    // GET: api/Staff/experienced?minYears=5
    [HttpGet("experienced")]
    public async Task<IActionResult> GetExperienced([FromQuery] int minYears)
    {
        var staff = await _staffService.GetExperiencedStaffAsync(minYears);
        return Ok(staff);
    }

    // GET: api/Staff/{id}/annual-bonus
    [HttpGet("{id}/annual-bonus")]
    public async Task<IActionResult> GetAnnualBonus(int id)
    {
        try
        {
            var bonus = await _staffService.CalculateAnnualBonusAsync(id);
            return Ok(new { StaffId = id, AnnualBonus = bonus });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}