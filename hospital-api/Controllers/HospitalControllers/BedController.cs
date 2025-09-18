using hospital_api.Models.HospitalAggregate;
using hospital_api.Services.Interfaces.HospitalServices;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.HospitalControllers;

[ApiController]
[Route("api/hospital/[controller]")]
public class BedController : ControllerBase
{
    private readonly IBedService _bedService;

    public BedController(IBedService bedService)
    {
        _bedService = bedService;
    }

    // Отримати всі ліжка
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var beds = await _bedService.GetAllAsync();
        return Ok(beds);
    }

    // Отримати ліжко за ID
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var bed = await _bedService.GetByIdAsync(id);
        if (bed == null) return NotFound();
        return Ok(bed);
    }

    // Додати нове ліжко
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Bed bed)
    {
        await _bedService.AddAsync(bed);
        return CreatedAtAction(nameof(Get), new { id = bed.Id }, bed);
    }

    // Оновити ліжко
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Bed bed)
    {
        if (id != bed.Id) return BadRequest();
        await _bedService.UpdateAsync(bed);
        return NoContent();
    }

    // Видалити ліжко
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _bedService.DeleteAsync(id);
        return NoContent();
    }

    // Отримати доступні ліжка
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable()
    {
        var beds = await _bedService.GetAvailableBedsAsync();
        return Ok(beds);
    }

    // Отримати зайняті ліжка
    [HttpGet("occupied")]
    public async Task<IActionResult> GetOccupied()
    {
        var beds = await _bedService.GetOccupiedBedsAsync();
        return Ok(beds);
    }

    // Отримати ліжка за ID палати
    [HttpGet("room/{roomId}")]
    public async Task<IActionResult> GetByRoom(int roomId)
    {
        var beds = await _bedService.GetByRoomIdAsync(roomId);
        return Ok(beds);
    }
}