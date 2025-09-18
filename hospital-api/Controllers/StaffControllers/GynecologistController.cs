using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers;

[ApiController]
[Route("api/staff/[controller]")]
public class GynecologistController : ControllerBase
{
    private readonly IGynecologistService _gynecologistService;

    public GynecologistController(IGynecologistService gynecologistService)
    {
        _gynecologistService = gynecologistService;
    }

    // Отримати всіх гінекологів
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var gynecologists = await _gynecologistService.GetAllGynecologistsAsync();
        return Ok(gynecologists);
    }

    // Отримати гінеколога за ID
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var gynecologist = await _gynecologistService.GetGynecologistByIdAsync(id);
        if (gynecologist == null)
            return NotFound();

        return Ok(gynecologist);
    }

    // Додати нового гінеколога
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Gynecologist gynecologist)
    {
        await _gynecologistService.AddGynecologistAsync(gynecologist);
        return CreatedAtAction(nameof(Get), new { id = gynecologist.Id }, gynecologist);
    }

    // Оновити гінеколога
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Gynecologist gynecologist)
    {
        if (id != gynecologist.Id)
            return BadRequest();

        await _gynecologistService.UpdateGynecologistAsync(gynecologist);
        return NoContent();
    }

    // Видалити гінеколога
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _gynecologistService.DeleteGynecologistAsync(id);
        return NoContent();
    }

    // Отримати топ-хірургів за кількістю операцій
    [HttpGet("top-surgeons/{minOperations}")]
    public async Task<IActionResult> GetTopSurgeons(int minOperations)
    {
        var result = await _gynecologistService.GetTopSurgeonsByOperationsAsync(minOperations);
        return Ok(result);
    }

    // Отримати всіх гінекологів з операціями
    [HttpGet("with-operations")]
    public async Task<IActionResult> GetAllWithOperations()
    {
        var result = await _gynecologistService.GetAllWithOperationsAsync();
        return Ok(result);
    }

    // Отримати профіль гінеколога у вигляді текстового звіту
    [HttpGet("{id}/profile-summary")]
    public async Task<IActionResult> GetProfileSummary(int id)
    {
        var summary = await _gynecologistService.GetGynecologistProfileSummaryAsync(id);
        return Ok(summary);
    }
}