using Microsoft.AspNetCore.Mvc;
using hospital_api.Models.HospitalAggregate;
using hospital_api.Services.Interfaces.HospitalServices;
using Microsoft.AspNetCore.Authorization;

namespace hospital_api.Controllers.HospitalControllers;

[Authorize]
[ApiController]
[Route("api/hospital")]
public class HospitalController : ControllerBase
{
    private readonly IHospitalService _hospitalService;

    public HospitalController(IHospitalService hospitalService)
    {
        _hospitalService = hospitalService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var hospitals = await _hospitalService.GetAllDtosAsync();
        return Ok(hospitals);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var hospital = await _hospitalService.GetHospitalByIdAsync(id);
        if (hospital == null)
            return NotFound();

        return Ok(hospital);
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Hospital hospital)
    {
        await _hospitalService.CreateHospitalAsync(hospital);
        return CreatedAtAction(nameof(Get), new { id = hospital.Id }, hospital);
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Hospital hospital)
    {
        if (id != hospital.Id)
            return BadRequest();

        await _hospitalService.UpdateHospitalAsync(hospital);
        return NoContent();
    }

    // [AllowAnonymous]
    [Authorize(Roles = "Operator, Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _hospitalService.DeleteHospitalAsync(id);
        return NoContent();
    }
}