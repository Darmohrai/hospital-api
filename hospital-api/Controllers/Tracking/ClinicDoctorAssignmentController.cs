using hospital_api.DTOs.Tracking;
using hospital_api.Services.Interfaces.Tracking;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.Tracking;

[ApiController]
[Route("api/clinic-assignment")]
public class ClinicDoctorAssignmentController : ControllerBase
{
    private readonly IClinicDoctorAssignmentService _service;

    public ClinicDoctorAssignmentController(IClinicDoctorAssignmentService service)
    {
        _service = service;
    }

    /// <summary>
    /// Отримує всі призначення (лікарів) для пацієнта.
    /// </summary>
    [HttpGet("by-patient/{patientId}")]
    public async Task<IActionResult> GetByPatient(int patientId)
    {
        return Ok(await _service.GetAssignmentsForPatientAsync(patientId));
    }

    /// <summary>
    /// Отримує всі призначення (пацієнтів) для лікаря.
    /// </summary>
    [HttpGet("by-doctor/{doctorId}")]
    public async Task<IActionResult> GetByDoctor(int doctorId)
    {
        return Ok(await _service.GetAssignmentsForDoctorAsync(doctorId));
    }

    /// <summary>
    /// Призначає лікаря пацієнту в клініці.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ClinicDoctorAssignmentDto dto)
    {
        try
        {
            var assignment = await _service.CreateAsync(dto);
            return Ok(assignment); // Повертаємо 200 OK замість 201, оскільки немає "GetById"
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Видаляє призначення лікаря пацієнту в клініці.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> Delete(
        [FromQuery] int patientId, 
        [FromQuery] int doctorId, 
        [FromQuery] int clinicId)
    {
        try
        {
            await _service.DeleteAsync(patientId, doctorId, clinicId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}