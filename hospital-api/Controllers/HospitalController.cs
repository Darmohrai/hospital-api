using Microsoft.AspNetCore.Mvc;
using hospital_api.Models.HospitalAggregate;
using hospital_api.Services.Interfaces.HospitalServices;

namespace hospital_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HospitalController : ControllerBase
{
    private readonly IHospitalService _hospitalService;

    public HospitalController(IHospitalService hospitalService)
    {
        _hospitalService = hospitalService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var hospitals = await _hospitalService.GetAllHospitalsAsync();
        return Ok(hospitals);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var hospital = await _hospitalService.GetHospitalByIdAsync(id);
        if (hospital == null)
            return NotFound();

        return Ok(hospital);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Hospital hospital)
    {
        await _hospitalService.CreateHospitalAsync(hospital);
        return CreatedAtAction(nameof(Get), new { id = hospital.Id }, hospital);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Hospital hospital)
    {
        if (id != hospital.Id)
            return BadRequest();

        await _hospitalService.UpdateHospitalAsync(hospital);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _hospitalService.DeleteHospitalAsync(id);
        return NoContent();
    }
}