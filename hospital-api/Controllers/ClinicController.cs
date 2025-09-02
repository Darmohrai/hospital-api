using hospital_api.Models.ClinicAggregate;
using hospital_api.Models.HospitalAggregate;
using hospital_api.Models.PatientAggregate;
using hospital_api.Models.StaffAggregate;
using hospital_api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClinicController : ControllerBase
{
    private readonly IClinicService _clinicService;

    public ClinicController(IClinicService clinicService)
    {
        _clinicService = clinicService;
    }

    // ----------------- CRUD -----------------

    // GET: api/clinics
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var clinics = await _clinicService.GetAllClinicsAsync();
        return Ok(clinics);
    }

    // GET: api/clinics/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var clinic = await _clinicService.GetClinicByIdAsync(id);
        if (clinic == null) return NotFound();
        return Ok(clinic);
    }

    // POST: api/clinics
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Clinic clinic)
    {
        var createdClinic = await _clinicService.CreateClinicAsync(clinic);
        return CreatedAtAction(nameof(GetById), new { id = createdClinic.Id }, createdClinic);
    }

    // PUT: api/clinics/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Clinic clinic)
    {
        if (id != clinic.Id) return BadRequest();

        var updatedClinic = await _clinicService.UpdateClinicAsync(clinic);
        return Ok(updatedClinic);
    }

    // DELETE: api/clinics/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _clinicService.DeleteClinicAsync(id);
        return NoContent();
    }

    // ----------------- Бізнес-логіка -----------------

    // POST: api/clinics/5/assign-hospital/2
    [HttpPost("{clinicId}/assign-hospital/{hospitalId}")]
    public async Task<IActionResult> AssignHospital(int clinicId, int hospitalId)
    {
        await _clinicService.AssignHospitalAsync(clinicId, hospitalId);
        return NoContent();
    }

    // POST: api/clinics/5/add-staff
    [HttpPost("{clinicId}/add-staff")]
    public async Task<IActionResult> AddStaff(int clinicId, [FromBody] Staff staff)
    {
        await _clinicService.AddStaffToClinicAsync(clinicId, staff);
        return NoContent();
    }

    // POST: api/clinics/5/add-patient
    [HttpPost("{clinicId}/add-patient")]
    public async Task<IActionResult> AddPatient(int clinicId, [FromBody] Patient patient)
    {
        await _clinicService.AddPatientAsync(clinicId, patient);
        return NoContent();
    }

    // POST: api/clinics/admit-patient/10?specialization=Cardiologist
    [HttpPost("admit-patient/{patientId}")]
    public async Task<IActionResult> AdmitPatient(int patientId, [FromQuery] HospitalSpecialization specialization)
    {
        var hospital = await _clinicService.ReferPatientToHospitalAsync(patientId, specialization);
        if (hospital == null) return NotFound("No hospital with the required specialization found.");
        return Ok(hospital);
    }
}
