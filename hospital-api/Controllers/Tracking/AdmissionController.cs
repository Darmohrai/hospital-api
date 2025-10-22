using hospital_api.DTOs.Tracking;
using hospital_api.Services.Interfaces.Tracking;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.Tracking;

[ApiController]
[Route("api/[controller]")]
public class AdmissionController : ControllerBase
{
    private readonly IAdmissionService _service;

    public AdmissionController(IAdmissionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var admission = await _service.GetByIdAsync(id);
        if (admission == null)
            return NotFound();
        return Ok(admission);
    }

    /// <summary>
    /// Госпіталізувати пацієнта.
    /// </summary>
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

    /// <summary>
    /// Виписати пацієнта (встановити дату виписки).
    /// </summary>
    [HttpPut("{id}/discharge")]
    public async Task<IActionResult> Discharge(int id, [FromQuery] DateTime? dischargeDate)
    {
        // Якщо дату не вказано, використовуємо поточний час
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

    /// <summary>
    /// Видалити запис про госпіталізацію (тільки для вже виписаних).
    /// </summary>
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