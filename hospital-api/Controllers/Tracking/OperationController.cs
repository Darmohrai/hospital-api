using hospital_api.DTOs.Tracking;
using hospital_api.Models.OperationsAggregate;
using hospital_api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.Tracking;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OperationController : ControllerBase
{
    private readonly IOperationService _operationService;

    public OperationController(IOperationService operationService)
    {
        _operationService = operationService;
    }

    // --- CRUD ---

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var operations = await _operationService.GetAllAsync();
        return Ok(operations);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var operation = await _operationService.GetByIdAsync(id);
        if (operation == null) return NotFound();
        return Ok(operation);
    }

    // ✅ ОНОВЛЕНО: Використання CreateOperationDto
    [Authorize(Roles = "Operator, Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOperationDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Мапінг DTO на модель
        var operation = new Operation
        {
            Date = dto.Date,
            Type = dto.Type,
            IsFatal = dto.IsFatal,
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            HospitalId = dto.HospitalId,
            ClinicId = dto.ClinicId
        };

        await _operationService.AddAsync(operation);
        // Повертаємо повний об'єкт операції, який був доданий (включаючи Id)
        return CreatedAtAction(nameof(Get), new { id = operation.Id }, operation);
    }

    // ✅ ОНОВЛЕНО: Використання CreateOperationDto
    [Authorize(Roles = "Operator, Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateOperationDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // 1. Отримуємо існуючу операцію
        var existingOperation = await _operationService.GetByIdAsync(id);
        if (existingOperation == null)
        {
            return NotFound();
        }

        // 2. Оновлюємо поля з DTO
        existingOperation.Date = dto.Date;
        existingOperation.Type = dto.Type;
        existingOperation.IsFatal = dto.IsFatal;
        existingOperation.PatientId = dto.PatientId;
        existingOperation.DoctorId = dto.DoctorId;
        existingOperation.HospitalId = dto.HospitalId;
        existingOperation.ClinicId = dto.ClinicId;

        // 3. Зберігаємо оновлену модель
        await _operationService.UpdateAsync(existingOperation);
        return NoContent();
    }


    [Authorize(Roles = "Operator, Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Перевіряємо, чи існує запис, перш ніж видаляти
        var existingOperation = await _operationService.GetByIdAsync(id);
        if (existingOperation == null)
        {
            return NotFound();
        }

        await _operationService.DeleteAsync(id);
        return NoContent();
    }

    // --- Специфічні методи ---

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetByPatient(int patientId)
    {
        var operations = await _operationService.GetByPatientIdAsync(patientId);
        return Ok(operations);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("doctor/{doctorId}")]
    public async Task<IActionResult> GetByDoctor(int doctorId)
    {
        var operations = await _operationService.GetByDoctorIdAsync(doctorId);
        return Ok(operations);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("hospital/{hospitalId}")]
    public async Task<IActionResult> GetByHospital(int hospitalId)
    {
        var operations = await _operationService.GetByHospitalIdAsync(hospitalId);
        return Ok(operations);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("clinic/{clinicId}")]
    public async Task<IActionResult> GetByClinic(int clinicId)
    {
        var operations = await _operationService.GetByClinicIdAsync(clinicId);
        return Ok(operations);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("fatal")]
    public async Task<IActionResult> GetFatalOperations()
    {
        var operations = await _operationService.GetFatalOperationsAsync();
        return Ok(operations);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("daterange")]
    public async Task<IActionResult> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var operations = await _operationService.GetByDateRangeAsync(startDate, endDate);
        return Ok(operations);
    }
}