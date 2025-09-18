using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers;

[ApiController]
[Route("api/staff/[controller]")]
public class SurgeonController : ControllerBase
{
    private readonly ISurgeonService _surgeonService;

    public SurgeonController(ISurgeonService surgeonService)
    {
        _surgeonService = surgeonService;
    }

    // Отримати всіх хірургів
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var surgeons = await _surgeonService.GetAllSurgeonsAsync();
        return Ok(surgeons);
    }

    // Отримати хірурга за ID
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var surgeon = await _surgeonService.GetSurgeonByIdAsync(id);
        if (surgeon == null)
            return NotFound();

        return Ok(surgeon);
    }

    // Додати нового хірурга
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Surgeon surgeon)
    {
        await _surgeonService.AddSurgeonAsync(surgeon);
        return CreatedAtAction(nameof(Get), new { id = surgeon.Id }, surgeon);
    }

    // Оновити хірурга
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Surgeon surgeon)
    {
        if (id != surgeon.Id)
            return BadRequest();

        await _surgeonService.UpdateSurgeonAsync(surgeon);
        return NoContent();
    }

    // Видалити хірурга
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _surgeonService.DeleteSurgeonAsync(id);
        return NoContent();
    }

    // Отримати хірургів з мінімальною кількістю операцій
    [HttpGet("min-operations/{count}")]
    public async Task<IActionResult> GetByOperationCount(int count)
    {
        var surgeons = await _surgeonService.GetByOperationCountAsync(count);
        return Ok(surgeons);
    }

    // Отримати всіх хірургів разом з операціями
    [HttpGet("with-operations")]
    public async Task<IActionResult> GetAllWithOperations()
    {
        var surgeons = await _surgeonService.GetAllWithOperationsAsync();
        return Ok(surgeons);
    }

    // Профіль хірурга у вигляді текстового звіту
    [HttpGet("{id}/profile-summary")]
    public async Task<IActionResult> GetProfileSummary(int id)
    {
        var summary = await _surgeonService.GetSurgeonProfileSummaryAsync(id);
        return Ok(summary);
    }
}