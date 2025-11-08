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

    // --- CRUD Операції ---

    /// <summary>
    /// Отримує список всіх поліклінік.
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // ✅ ВИПРАВЛЕНО: Викликаємо новий метод DTO
        var clinics = await _clinicService.GetAllDtosAsync();
        return Ok(clinics);
    }

    /// <summary>
    /// Отримує поліклініку за її ID.
    /// </summary>
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var clinic = await _clinicService.GetByIdAsync(id);
        if (clinic == null)
            return NotFound();

        return Ok(clinic);
    }

    /// <summary>
    /// Створює нову поліклініку.
    /// </summary>
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

    /// <summary>
    /// Оновлює існуючу поліклініку.
    /// </summary>
    [Authorize(Roles = "Operator, Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Clinic clinic)
    {
        if (id != clinic.Id)
            return BadRequest("ID mismatch.");

        await _clinicService.UpdateAsync(clinic);
        return NoContent(); // Стандартна відповідь для успішного оновлення
    }

    /// <summary>
    /// Видаляє поліклініку за її ID.
    /// </summary>
    [Authorize(Roles = "Operator, Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _clinicService.DeleteAsync(id);
        return NoContent();
    }

    // --- Бізнес-логіка ---

    /// <summary>
    /// Призначає поліклініку до лікарні.
    /// </summary>
    [Authorize(Roles = "Operator, Admin")]
    [HttpPost("{clinicId}/assign-hospital/{hospitalId}")]
    public async Task<IActionResult> AssignHospital(int clinicId, int hospitalId)
    {
        var result = await _clinicService.AssignHospitalAsync(clinicId, hospitalId);

        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Працевлаштовує співробітника в поліклініку.
    /// </summary>
    [Authorize(Roles = "Operator, Admin")]
    [HttpPost("{clinicId}/staff/{staffId}")] // ✅ REST-сумісний маршрут
    public async Task<IActionResult> AddStaffToClinic(int clinicId, int staffId)
    {
        var result = await _clinicService.AddStaffToClinicAsync(clinicId, staffId);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data); // Повертаємо створений об'єкт Employment
    }

    /// <summary>
    /// Реєструє пацієнта в поліклініці.
    /// </summary>
    [Authorize(Roles = "Operator, Admin")]
    [HttpPost("{clinicId}/patients")]
    public async Task<IActionResult> AddPatient(int clinicId, [FromBody] Patient patient)
    {
        var result = await _clinicService.AddPatientAsync(clinicId, patient);

        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        // Повертаємо 201 Created з посиланням на нового пацієнта (потрібен PatientController)
        return CreatedAtAction(nameof(AddPatient), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Направляє пацієнта до лікарні з потрібною спеціалізацією.
    /// </summary>
    [Authorize(Roles = "Operator, Admin")]
    [HttpPost("refer-patient/{patientId}")]
    public async Task<IActionResult> ReferPatient(int patientId, [FromQuery] HospitalSpecialization specialization)
    {
        var result = await _clinicService.ReferPatientToHospitalAsync(patientId, specialization);

        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage }); // Наприклад, "Лікарню не знайдено"

        return Ok(result.Data);
    }
}