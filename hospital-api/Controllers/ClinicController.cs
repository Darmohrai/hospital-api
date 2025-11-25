using hospital_api.DTOs;
using hospital_api.Models.ClinicAggregate;
using hospital_api.Models.HospitalAggregate;
using hospital_api.Models.PatientAggregate;
using hospital_api.Models.StaffAggregate;
using hospital_api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClinicController : ControllerBase
{
    private readonly IClinicService _clinicService;

    public ClinicController(IClinicService clinicService)
    {
        _clinicService = clinicService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var clinics = await _clinicService.GetAllDtosAsync();
        return Ok(clinics);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var clinic = await _clinicService.GetByIdAsync(id);
        if (clinic == null)
            return NotFound();

        return Ok(clinic);
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClinicDto dto)
    {
        var clinic = new Clinic
        {
            Name = dto.Name,
            Address = dto.Address,
            HospitalId = dto.HospitalId
        };

        await _clinicService.CreateAsync(clinic);
        return CreatedAtAction(nameof(GetById), new { id = clinic.Id }, clinic);
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Clinic clinic)
    {
        if (id != clinic.Id)
            return BadRequest("ID mismatch.");

        await _clinicService.UpdateAsync(clinic);
        return NoContent();
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _clinicService.DeleteAsync(id);
        return NoContent();
    }
    
    [Authorize(Roles = "Operator, Admin")]
    [HttpPost("{clinicId}/assign-hospital/{hospitalId}")]
    public async Task<IActionResult> AssignHospital(int clinicId, int hospitalId)
    {
        var result = await _clinicService.AssignHospitalAsync(clinicId, hospitalId);

        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPost("{clinicId}/staff/{staffId}")]
    public async Task<IActionResult> AddStaffToClinic(int clinicId, int staffId)
    {
        var result = await _clinicService.AddStaffToClinicAsync(clinicId, staffId);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPost("{clinicId}/patients")]
    public async Task<IActionResult> AddPatient(int clinicId, [FromBody] Patient patient)
    {
        var result = await _clinicService.AddPatientAsync(clinicId, patient);

        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return CreatedAtAction(nameof(AddPatient), new { id = result.Data!.Id }, result.Data);
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPost("refer-patient/{patientId}")]
    public async Task<IActionResult> ReferPatient(int patientId, [FromQuery] HospitalSpecialization specialization)
    {
        var result = await _clinicService.ReferPatientToHospitalAsync(patientId, specialization);

        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }
}