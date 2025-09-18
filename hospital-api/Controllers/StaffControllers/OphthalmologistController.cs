using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers;

[ApiController]
[Route("api/staff/[controller]")]
public class OphthalmologistController : ControllerBase
{
    private readonly IOphthalmologistService _ophthalmologistService;

    public OphthalmologistController(IOphthalmologistService ophthalmologistService)
    {
        _ophthalmologistService = ophthalmologistService;
    }

    // Отримати всіх офтальмологів
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var ophthalmologists = await _ophthalmologistService.GetAllOphthalmologistsAsync();
        return Ok(ophthalmologists);
    }

    // Отримати офтальмолога за ID
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var ophthalmologist = await _ophthalmologistService.GetOphthalmologistByIdAsync(id);
        if (ophthalmologist == null)
            return NotFound();

        return Ok(ophthalmologist);
    }

    // Додати нового офтальмолога
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Ophthalmologist ophthalmologist)
    {
        await _ophthalmologistService.AddOphthalmologistAsync(ophthalmologist);
        return CreatedAtAction(nameof(Get), new { id = ophthalmologist.Id }, ophthalmologist);
    }

    // Оновити офтальмолога
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Ophthalmologist ophthalmologist)
    {
        if (id != ophthalmologist.Id)
            return BadRequest();

        await _ophthalmologistService.UpdateOphthalmologistAsync(ophthalmologist);
        return NoContent();
    }

    // Видалити офтальмолога
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _ophthalmologistService.DeleteOphthalmologistAsync(id);
        return NoContent();
    }

    // Отримати офтальмологів з розширеною відпусткою більше заданих днів
    [HttpGet("extended-vacation/{minDays}")]
    public async Task<IActionResult> GetByExtendedVacationDays(int minDays)
    {
        var result = await _ophthalmologistService.GetOphthalmologistsByExtendedVacationDaysAsync(minDays);
        return Ok(result);
    }

    // Отримати профіль офтальмолога у вигляді текстового звіту
    [HttpGet("{id}/profile-summary")]
    public async Task<IActionResult> GetProfileSummary(int id)
    {
        var summary = await _ophthalmologistService.GetOphthalmologistProfileSummaryAsync(id);
        return Ok(summary);
    }
}