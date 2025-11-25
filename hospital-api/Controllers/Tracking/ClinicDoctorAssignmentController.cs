using hospital_api.DTOs.Tracking;
using hospital_api.Services.Interfaces.Tracking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.Tracking;

[Authorize]
[ApiController]
[Route("api/clinic-assignment")]
public class ClinicDoctorAssignmentController : ControllerBase
{
    private readonly IClinicDoctorAssignmentService _service;

    public ClinicDoctorAssignmentController(IClinicDoctorAssignmentService service)
    {
        _service = service;
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("by-patient/{patientId}")]
    public async Task<IActionResult> GetByPatient(int patientId)
    {
        return Ok(await _service.GetAssignmentsForPatientAsync(patientId));
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("by-doctor/{doctorId}")]
    public async Task<IActionResult> GetByDoctor(int doctorId)
    {
        return Ok(await _service.GetAssignmentsForDoctorAsync(doctorId));
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ClinicDoctorAssignmentDto dto)
    {
        try
        {
            var assignment = await _service.CreateAsync(dto);
            return Ok(assignment);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "Operator, Admin")]
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