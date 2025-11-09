using hospital_api.Data;
using hospital_api.DTOs;
using hospital_api.Models.LaboratoryAggregate;
using hospital_api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LaboratoryController : ControllerBase
{
    private readonly ILaboratoryService _laboratoryService;
    private readonly ApplicationDbContext _context; 

    public LaboratoryController(ILaboratoryService laboratoryService, ApplicationDbContext context)
    {
        _laboratoryService = laboratoryService;
        _context = context;
    }

    /// <summary>
    /// Отримує список всіх лабораторій...
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var laboratories = await _context.Laboratories
            .Include(l => l.Hospitals)
            .Include(l => l.Clinics)
            .Select(l => new 
            {
                l.Id,
                l.Name,
                l.Profile, // <-- Повертаємо як є (List<string>)
                Hospitals = l.Hospitals.Select(h => new { h.Id, h.Name }).ToList(),
                Clinics = l.Clinics.Select(c => new { c.Id, c.Name }).ToList()
            })
            .ToListAsync();
            
        return Ok(laboratories);
    }

    /// <summary>
    /// Отримує лабораторію за ID.
    /// </summary>
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var laboratory = await _context.Laboratories
            .Include(l => l.Hospitals)
            .Include(l => l.Clinics)
            .Select(l => new 
            {
                l.Id,
                l.Name,
                // ✅ ОНОВЛЕНО:
                // Більше не об'єднуємо в рядок. Повертаємо список.
                Profile = l.Profile, 
                HospitalIds = l.Hospitals.Select(h => h.Id).ToList(),
                ClinicIds = l.Clinics.Select(c => c.Id).ToList()
            })
            .FirstOrDefaultAsync(l => l.Id == id);

        if (laboratory == null)
            return NotFound();

        return Ok(laboratory);
    }

    /// <summary>
    /// Створює нову лабораторію.
    /// </summary>
    [Authorize(Roles = "Operator, Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLaboratoryDto dto)
    {
        var laboratory = new Laboratory
        {
            Name = dto.Name,
            // ✅ ОНОВЛЕНО:
            // Просто присвоюємо список. Більше ніяких .Split()
            Profile = dto.Profile 
        };

        // ... (решта коду для Hospitals та Clinics без змін)
        if (dto.HospitalIds.Any())
        {
            var hospitals = await _context.Hospitals
                .Where(h => dto.HospitalIds.Contains(h.Id)).ToListAsync();
            laboratory.Hospitals = hospitals;
        }
        if (dto.ClinicIds.Any())
        {
            var clinics = await _context.Clinics
                .Where(c => dto.ClinicIds.Contains(c.Id)).ToListAsync();
            laboratory.Clinics = clinics;
        }

        await _laboratoryService.AddAsync(laboratory);
        
        return CreatedAtAction(nameof(GetById), new { id = laboratory.Id }, laboratory);
    }

    /// <summary>
    /// Оновлює існуючу лабораторію.
    /// </summary>
    [Authorize(Roles = "Operator, Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateLaboratoryDto dto)
    {
        var entityToUpdate = await _context.Laboratories
            .Include(l => l.Hospitals)
            .Include(l => l.Clinics)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (entityToUpdate == null)
            return NotFound();

        // Оновлюємо прості властивості
        entityToUpdate.Name = dto.Name;
        // ✅ ОНОВЛЕНО:
        // Просто присвоюємо список.
        entityToUpdate.Profile = dto.Profile;

        // ... (решта коду для Hospitals та Clinics без змін)
        var selectedHospitals = await _context.Hospitals
            .Where(h => dto.HospitalIds.Contains(h.Id)).ToListAsync();
        entityToUpdate.Hospitals = selectedHospitals;
        var selectedClinics = await _context.Clinics
            .Where(c => dto.ClinicIds.Contains(c.Id)).ToListAsync();
        entityToUpdate.Clinics = selectedClinics;

        await _laboratoryService.UpdateAsync(entityToUpdate); 
        
        return NoContent(); // Успіх
    }

    /// <summary>
    /// Видаляє лабораторію...
    /// </summary>
    [Authorize(Roles = "Operator, Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _laboratoryService.DeleteAsync(id);
        return NoContent();
    }
}