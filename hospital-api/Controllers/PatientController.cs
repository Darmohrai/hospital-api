using hospital_api.Models.PatientAggregate;
using hospital_api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    // --- CRUD ---

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var patients = await _patientService.GetAllAsync();
        return Ok(patients);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var patient = await _patientService.GetByIdAsync(id);
        if (patient == null) return NotFound();
        return Ok(patient);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Patient patient)
    {
        await _patientService.AddAsync(patient);
        return CreatedAtAction(nameof(Get), new { id = patient.Id }, patient);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Patient patient)
    {
        if (id != patient.Id) return BadRequest();
        await _patientService.UpdateAsync(patient);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _patientService.DeleteAsync(id);
        return NoContent();
    }

    // --- Специфічні методи ---

    [HttpGet("name")]
    public async Task<IActionResult> GetByFullName([FromQuery] string fullName)
    {
        var patients = await _patientService.GetByFullNameAsync(fullName);
        return Ok(patients);
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetByHealthStatus([FromQuery] string status)
    {
        var patients = await _patientService.GetByHealthStatusAsync(status);
        return Ok(patients);
    }

    [HttpGet("clinic/{clinicId}")]
    public async Task<IActionResult> GetByClinic(int clinicId)
    {
        var patients = await _patientService.GetByClinicIdAsync(clinicId);
        return Ok(patients);
    }

    [HttpGet("hospital/{hospitalId}")]
    public async Task<IActionResult> GetByHospital(int hospitalId)
    {
        var patients = await _patientService.GetByHospitalIdAsync(hospitalId);
        return Ok(patients);
    }

    [HttpGet("doctor/{doctorId}")]
    public async Task<IActionResult> GetByAssignedDoctor(int doctorId)
    {
        var patients = await _patientService.GetByAssignedDoctorIdAsync(doctorId);
        return Ok(patients);
    }

    [HttpGet("all-with-associations")]
    public async Task<IActionResult> GetAllWithAssociations()
    {
        var patients = await _patientService.GetAllWithAssociationsAsync();
        return Ok(patients);
    }
}