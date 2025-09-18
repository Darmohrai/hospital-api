using hospital_api.Models.StaffAggregate;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    // GET: api/Doctor
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var doctors = await _doctorService.GetAllDoctorsAsync();
        return Ok(doctors);
    }

    // GET: api/Doctor/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var doctor = await _doctorService.GetDoctorByIdAsync(id);
        if (doctor == null)
            return NotFound();

        return Ok(doctor);
    }

    // POST: api/Doctor
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Doctor doctor)
    {
        try
        {
            await _doctorService.AddDoctorAsync(doctor);
            return CreatedAtAction(nameof(Get), new { id = doctor.Id }, doctor);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/Doctor/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Doctor doctor)
    {
        if (id != doctor.Id)
            return BadRequest("ID mismatch.");

        try
        {
            await _doctorService.UpdateDoctorAsync(doctor);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // DELETE: api/Doctor/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _doctorService.DeleteDoctorAsync(id);
        return NoContent();
    }

    // GET: api/Doctor/by-specialty?specialty=Cardiology
    [HttpGet("by-specialty")]
    public async Task<IActionResult> GetBySpecialty([FromQuery] string specialty)
    {
        var doctors = await _doctorService.GetDoctorsBySpecialtyAsync(specialty);
        return Ok(doctors);
    }

    // GET: api/Doctor/by-degree?degree=Doctor
    [HttpGet("by-degree")]
    public async Task<IActionResult> GetByDegree([FromQuery] AcademicDegree degree)
    {
        var doctors = await _doctorService.GetDoctorsByDegreeAsync(degree);
        return Ok(doctors);
    }

    // GET: api/Doctor/by-title?title=Professor
    [HttpGet("by-title")]
    public async Task<IActionResult> GetByTitle([FromQuery] AcademicTitle title)
    {
        var doctors = await _doctorService.GetDoctorsByTitleAsync(title);
        return Ok(doctors);
    }
}