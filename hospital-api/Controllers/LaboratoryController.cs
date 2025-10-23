using hospital_api.Models.LaboratoryAggregate;
using hospital_api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LaboratoryController : ControllerBase
{
    private readonly ILaboratoryService _laboratoryService;

    public LaboratoryController(ILaboratoryService laboratoryService)
    {
        _laboratoryService = laboratoryService;
    }

    // --- CRUD ---

    // Отримати всі лабораторії
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var labs = await _laboratoryService.GetAllAsync();
        return Ok(labs);
    }

    // Отримати лабораторію за ID
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var lab = await _laboratoryService.GetByIdAsync(id);
        if (lab == null) return NotFound();
        return Ok(lab);
    }

    // Додати лабораторію
    [Authorize(Roles = "Operator, Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Laboratory laboratory)
    {
        await _laboratoryService.AddAsync(laboratory);
        return CreatedAtAction(nameof(Get), new { id = laboratory.Id }, laboratory);
    }

    // Оновити лабораторію
    [Authorize(Roles = "Operator, Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Laboratory laboratory)
    {
        if (id != laboratory.Id) return BadRequest();
        await _laboratoryService.UpdateAsync(laboratory);
        return NoContent();
    }

    // Видалити лабораторію
    [Authorize(Roles = "Operator, Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _laboratoryService.DeleteAsync(id);
        return NoContent();
    }

    // --- Специфічні методи ---

    // Отримати лабораторії за профілем
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("profile/{profile}")]
    public async Task<IActionResult> GetByProfile(string profile)
    {
        var labs = await _laboratoryService.GetByProfileAsync(profile);
        return Ok(labs);
    }

    // Отримати лабораторії за ID лікарні
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("hospital/{hospitalId}")]
    public async Task<IActionResult> GetByHospital(int hospitalId)
    {
        var labs = await _laboratoryService.GetByHospitalIdAsync(hospitalId);
        return Ok(labs);
    }

    // Отримати лабораторії за ID клініки
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("clinic/{clinicId}")]
    public async Task<IActionResult> GetByClinic(int clinicId)
    {
        var labs = await _laboratoryService.GetByClinicIdAsync(clinicId);
        return Ok(labs);
    }

    // Отримати лабораторію з асоціаціями за назвою
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("name/{name}/with-associations")]
    public async Task<IActionResult> GetByNameWithAssociations(string name)
    {
        var lab = await _laboratoryService.GetByNameWithAssociationsAsync(name);
        if (lab == null) return NotFound();
        return Ok(lab);
    }
}