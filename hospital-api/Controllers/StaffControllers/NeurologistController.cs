using hospital_api.DTOs.Staff;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers;

[Authorize]
[ApiController]
[Route("api/staff/[controller]")]
public class NeurologistController : ControllerBase
{
    private readonly INeurologistService _neurologistService;

    public NeurologistController(INeurologistService neurologistService)
    {
        _neurologistService = neurologistService;
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var neurologists = await _neurologistService.GetAllNeurologistsAsync();
        return Ok(neurologists);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var neurologist = await _neurologistService.GetNeurologistByIdAsync(id);
        if (neurologist == null)
            return NotFound();

        return Ok(neurologist);
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNeurologistDto neurologistDto)
    {
        var result = await _neurologistService.AddNeurologistToHospitalAsync(null, neurologistDto);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return CreatedAtAction(nameof(Get), new { id = result.Data.Id }, result.Data);
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Neurologist neurologist)
    {
        if (id != neurologist.Id)
            return BadRequest();

        await _neurologistService.UpdateNeurologistAsync(neurologist);
        return NoContent();
    }
    
    [Authorize(Roles = "Operator, Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _neurologistService.DeleteNeurologistAsync(id);
        return NoContent();
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("extended-vacation/{minDays}")]
    public async Task<IActionResult> GetByExtendedVacationDays(int minDays)
    {
        var result = await _neurologistService.GetNeurologistsByExtendedVacationDaysAsync(minDays);
        return Ok(result);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("{id}/profile-summary")]
    public async Task<IActionResult> GetProfileSummary(int id)
    {
        var summary = await _neurologistService.GetNeurologistProfileSummaryAsync(id);
        return Ok(summary);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("vacation/{neurologistId}")]
    public async Task<IActionResult> GetExtendVacationDays(int neurologistId)
    {
        var days = await _neurologistService.GetNeurologistExtendedVacationDaysAsync(neurologistId);

        return Ok(days);
    }
}