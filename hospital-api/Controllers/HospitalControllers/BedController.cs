using hospital_api.Models.HospitalAggregate;
using hospital_api.Services.Interfaces.HospitalServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.HospitalControllers;

[Authorize]
[ApiController]
[Route("api/hospital/[controller]")]
public class BedController : ControllerBase
{
    private readonly IBedService _bedService;

    public BedController(IBedService bedService)
    {
        _bedService = bedService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var beds = await _bedService.GetAllAsync();
        return Ok(beds);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var bed = await _bedService.GetByIdAsync(id);
        if (bed == null) return NotFound();
        return Ok(bed);
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Bed bed)
    {
        await _bedService.AddAsync(bed);
        return CreatedAtAction(nameof(Get), new { id = bed.Id }, bed);
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Bed bed)
    {
        if (id != bed.Id) return BadRequest();
        await _bedService.UpdateAsync(bed);
        return NoContent();
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _bedService.DeleteAsync(id);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable()
    {
        var beds = await _bedService.GetAvailableBedsAsync();
        return Ok(beds);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("occupied")]
    public async Task<IActionResult> GetOccupied()
    {
        var beds = await _bedService.GetOccupiedBedsAsync();
        return Ok(beds);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("room/{roomId}")]
    public async Task<IActionResult> GetByRoom(int roomId)
    {
        var beds = await _bedService.GetByRoomIdAsync(roomId);
        return Ok(beds);
    }
}