using hospital_api.DTOs.Tracking;
using hospital_api.Services.Interfaces.Tracking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.Tracking;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AdmissionController : ControllerBase
{
    private readonly IAdmissionService _service;

    public AdmissionController(IAdmissionService service)
    {
        _service = service;
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var admission = await _service.GetByIdAsync(id);
        if (admission == null)
            return NotFound();
        return Ok(admission);
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAdmissionDto dto)
    {
        try
        {
            var newAdmission = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = newAdmission.Id }, newAdmission);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPut("{id}/discharge")]
    public async Task<IActionResult> Discharge(int id, [FromQuery] DateTime? dischargeDate)
    {
        var date = dischargeDate ?? DateTime.UtcNow; 
        try
        {
            var admission = await _service.DischargeAsync(id, date);
            if (admission == null)
                return NotFound();
            
            return Ok(admission);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}