using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers;

[ApiController]
[Route("api/staff/[controller]")]
public class DentistController : ControllerBase
{
    private readonly IDentistService _dentistService;

    public DentistController(IDentistService dentistService)
    {
        _dentistService = dentistService;
    }

    // GET: api/Dentist
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var dentists = await _dentistService.GetAllDentistsAsync();
        return Ok(dentists);
    }

    // GET: api/Dentist/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var dentist = await _dentistService.GetDentistByIdAsync(id);
        if (dentist == null)
            return NotFound();

        return Ok(dentist);
    }

    // POST: api/Dentist
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Dentist dentist)
    {
        try
        {
            await _dentistService.AddDentistAsync(dentist);
            return CreatedAtAction(nameof(Get), new { id = dentist.Id }, dentist);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/Dentist/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Dentist dentist)
    {
        if (id != dentist.Id)
            return BadRequest("ID mismatch.");

        try
        {
            await _dentistService.UpdateDentistAsync(dentist);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // DELETE: api/Dentist/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _dentistService.DeleteDentistAsync(id);
        return NoContent();
    }

    // GET: api/Dentist/top-performing?minOperationCount=10
    [HttpGet("top-performing")]
    public async Task<IActionResult> GetTopPerforming([FromQuery] int minOperationCount)
    {
        var dentists = await _dentistService.GetTopPerformingDentistsAsync(minOperationCount);
        return Ok(dentists);
    }

    // GET: api/Dentist/high-hazard?minCoefficient=0.1
    [HttpGet("high-hazard")]
    public async Task<IActionResult> GetHighHazardDentists([FromQuery] float minCoefficient)
    {
        var dentists = await _dentistService.GetDentistsWithHighHazardPayAsync(minCoefficient);
        return Ok(dentists);
    }

    // GET: api/Dentist/{id}/summary
    [HttpGet("{id}/summary")]
    public async Task<IActionResult> GetSummary(int id)
    {
        var summary = await _dentistService.GetDentistSummaryAsync(id);
        if (summary == "Dentist not found.")
            return NotFound(summary);

        return Ok(summary);
    }
}