using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers;

[ApiController]
[Route("api/staff/[controller]")]
public class CardiologistController : ControllerBase
{
    private readonly ICardiologistService _cardiologistService;

    public CardiologistController(ICardiologistService cardiologistService)
    {
        _cardiologistService = cardiologistService;
    }

    // GET: api/Cardiologist
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var cardiologists = await _cardiologistService.GetAllCardiologistsAsync();
        return Ok(cardiologists);
    }

    // GET: api/Cardiologist/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var cardiologist = await _cardiologistService.GetCardiologistByIdAsync(id);
        if (cardiologist == null)
            return NotFound();

        return Ok(cardiologist);
    }

    // POST: api/Cardiologist
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Cardiologist cardiologist)
    {
        try
        {
            await _cardiologistService.AddCardiologistAsync(cardiologist);
            return CreatedAtAction(nameof(Get), new { id = cardiologist.Id }, cardiologist);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/Cardiologist/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Cardiologist cardiologist)
    {
        if (id != cardiologist.Id)
            return BadRequest("ID mismatch.");

        try
        {
            await _cardiologistService.UpdateCardiologistAsync(cardiologist);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // DELETE: api/Cardiologist/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _cardiologistService.DeleteCardiologistAsync(id);
        return NoContent();
    }

    // GET: api/Cardiologist/top-surgeons?minOperations=10
    [HttpGet("top-surgeons")]
    public async Task<IActionResult> GetTopSurgeons([FromQuery] int minOperations)
    {
        var topSurgeons = await _cardiologistService.GetTopSurgeonsByOperationsAsync(minOperations);
        return Ok(topSurgeons);
    }

    // GET: api/Cardiologist/fatal-operations
    [HttpGet("fatal-operations")]
    public async Task<IActionResult> GetCardiologistsWithFatalOperations()
    {
        var cardiologists = await _cardiologistService.GetCardiologistsWithFatalOperationsAsync();
        return Ok(cardiologists);
    }

    // GET: api/Cardiologist/{id}/profile-summary
    [HttpGet("{id}/profile-summary")]
    public async Task<IActionResult> GetProfileSummary(int id)
    {
        var summary = await _cardiologistService.GetCardiologistProfileSummaryAsync(id);
        if (summary == "Cardiologist not found.")
            return NotFound(summary);

        return Ok(summary);
    }
}