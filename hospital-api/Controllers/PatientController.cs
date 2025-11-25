using hospital_api.DTOs.Patient;
using hospital_api.DTOs.Reports;
using hospital_api.Models.PatientAggregate;
using hospital_api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PatientController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var patients = await _patientService.GetAllWithAssociationsAsync();
        return Ok(patients);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var patient = await _patientService.GetByIdAsync(id);
        if (patient == null) return NotFound();
        return Ok(patient);
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Patient patient)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        await _patientService.AddAsync(patient);
        return CreatedAtAction(nameof(Get), new { id = patient.Id }, patient);
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Patient patient)
    {
        if (id != patient.Id) return BadRequest();
        await _patientService.UpdateAsync(patient);
        return NoContent();
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _patientService.DeleteAsync(id);
        return NoContent();
    }
    
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("name")]
    public async Task<IActionResult> GetByFullName([FromQuery] string fullName)
    {
        var patients = await _patientService.GetByFullNameAsync(fullName);
        return Ok(patients);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("status")]
    public async Task<IActionResult> GetByHealthStatus([FromQuery] string status)
    {
        var patients = await _patientService.GetByHealthStatusAsync(status);
        return Ok(patients);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("clinic/{clinicId}")]
    public async Task<IActionResult> GetByClinic(int clinicId)
    {
        var patients = await _patientService.GetByClinicIdAsync(clinicId);
        return Ok(patients);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("hospital/{hospitalId}")]
    public async Task<IActionResult> GetByHospital(int hospitalId)
    {
        var patients = await _patientService.GetByHospitalIdAsync(hospitalId);
        return Ok(patients);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("doctor/{doctorId}")]
    public async Task<IActionResult> GetByAssignedDoctor(int doctorId)
    {
        var patients = await _patientService.GetByAssignedDoctorIdAsync(doctorId);
        return Ok(patients);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("all-with-associations")]
    public async Task<IActionResult> GetAllWithAssociations()
    {
        var patients = await _patientService.GetAllWithAssociationsAsync();
        return Ok(patients);
    }
    
    [Authorize(Roles = "Operator, Admin")]
    [HttpPost("{patientId}/assign-bed/{bedId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignBed(int patientId, int bedId)
    {
        try
        {
            await _patientService.AssignPatientToBedAsync(patientId, bedId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPost("{patientId}/unassign-bed")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnassignBed(int patientId)
    {
        try
        {
            await _patientService.UnassignPatientFromBedAsync(patientId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("list-by-location")]
    [ProducesResponseType(typeof(IEnumerable<PatientDetailsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientListByLocation(
        [FromQuery] int hospitalId, 
        [FromQuery] int? departmentId, 
        [FromQuery] int? roomId)
    {
        var patients = await _patientService.GetPatientListAsync(hospitalId, departmentId, roomId);
        return Ok(patients);
    }
    
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("{patientId}/history")]
    [ProducesResponseType(typeof(PatientHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatientHistory(int patientId)
    {
        try
        {
            var history = await _patientService.GetPatientHistoryAsync(patientId);
            return Ok(history);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    [Authorize(Roles = "Operator, Admin")]
    [HttpPost("{patientId}/assign-doctor/{doctorId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignDoctor(int patientId, int doctorId)
    {
        try
        {
            await _patientService.AssignDoctorAsync(patientId, doctorId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPost("{patientId}/remove-doctor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveDoctor(int patientId)
    {
        try
        {
            await _patientService.RemoveDoctorAsync(patientId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}