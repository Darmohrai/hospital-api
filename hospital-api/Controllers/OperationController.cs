﻿using hospital_api.Models.OperationsAggregate;
using hospital_api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers;

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

    [Authorize(Roles = "Operator, Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Operation operation)
    {
        await _operationService.AddAsync(operation);
        return CreatedAtAction(nameof(Get), new { id = operation.Id }, operation);
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Operation operation)
    {
        if (id != operation.Id) return BadRequest();
        await _operationService.UpdateAsync(operation);
        return NoContent();
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
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