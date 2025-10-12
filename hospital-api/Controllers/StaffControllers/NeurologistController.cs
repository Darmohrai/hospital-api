using hospital_api.DTOs.Staff;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers;

[ApiController]
[Route("api/staff/[controller]")]
public class NeurologistController : ControllerBase
{
    private readonly INeurologistService _neurologistService;

    public NeurologistController(INeurologistService neurologistService)
    {
        _neurologistService = neurologistService;
    }

    // Отримати всіх невропатологів
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var neurologists = await _neurologistService.GetAllNeurologistsAsync();
        return Ok(neurologists);
    }

    // Отримати невропатолога за ID
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var neurologist = await _neurologistService.GetNeurologistByIdAsync(id);
        if (neurologist == null)
            return NotFound();

        return Ok(neurologist);
    }

    // Додати нового невропатолога
    [HttpPost("{hospitalId}")]
    public async Task<IActionResult> Create(int hospitalId, [FromBody] CreateNeurologistDto neurologistDto)
    {
        var result = await _neurologistService.AddNeurologistToHospitalAsync(hospitalId, neurologistDto);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return CreatedAtAction(nameof(Get), new { id = result.Data.Id }, result.Data);
    }

// Оновити невропатолога
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Neurologist neurologist)
    {
        if (id != neurologist.Id)
            return BadRequest();

        await _neurologistService.UpdateNeurologistAsync(neurologist);
        return NoContent();
    }

// Видалити невропатолога
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _neurologistService.DeleteNeurologistAsync(id);
        return NoContent();
    }

// Отримати невропатологів з розширеною відпусткою більше заданих днів
    [HttpGet("extended-vacation/{minDays}")]
    public async Task<IActionResult> GetByExtendedVacationDays(int minDays)
    {
        var result = await _neurologistService.GetNeurologistsByExtendedVacationDaysAsync(minDays);
        return Ok(result);
    }

// Отримати профіль невропатолога у вигляді текстового звіту
    [HttpGet("{id}/profile-summary")]
    public async Task<IActionResult> GetProfileSummary(int id)
    {
        var summary = await _neurologistService.GetNeurologistProfileSummaryAsync(id);
        return Ok(summary);
    }
}