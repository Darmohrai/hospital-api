using hospital_api.DTOs.Staff;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers;

[Authorize]
[ApiController]
[Route("api/staff/radiologists")]
public class RadiologistController : ControllerBase
{
    private readonly IRadiologistService _radiologistService;

    public RadiologistController(IRadiologistService radiologistService)
    {
        _radiologistService = radiologistService;
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var radiologists = await _radiologistService.GetAllAsync();
        return Ok(radiologists);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var radiologist = await _radiologistService.GetByIdAsync(id);
        if (radiologist == null)
            return NotFound();

        return Ok(radiologist);
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRadiologistDto dto)
    {
        var radiologist = new Radiologist
        {
            FullName = dto.FullName,
            WorkExperienceYears = dto.WorkExperienceYears,
            AcademicDegree = dto.AcademicDegree,
            AcademicTitle = dto.AcademicTitle,
            HazardPayCoefficient = dto.HazardPayCoefficient,
            ExtendedVacationDays = dto.ExtendedVacationDays
        };

        var result = await _radiologistService.CreateAsync(radiologist);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Radiologist radiologist)
    {
        if (id != radiologist.Id)
            return BadRequest("ID mismatch.");

        var result = await _radiologistService.UpdateAsync(radiologist);

        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return NoContent();
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _radiologistService.DeleteAsync(id);

        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return NoContent();
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("hazard-pay")]
    public async Task<IActionResult> GetByHazardPay([FromQuery] float minCoefficient)
    {
        var result = await _radiologistService.GetByHazardPayCoefficientAsync(minCoefficient);
        return Ok(result);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("extended-vacation")]
    public async Task<IActionResult> GetByExtendedVacation([FromQuery] int minDays)
    {
        var result = await _radiologistService.GetByExtendedVacationDaysAsync(minDays);
        return Ok(result);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("hazard-and-vacation")]
    public async Task<IActionResult> GetByHazardAndVacation([FromQuery] float minCoefficient, [FromQuery] int minDays)
    {
        var result = await _radiologistService.GetByHazardPayAndVacationAsync(minCoefficient, minDays);
        return Ok(result);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("{id}/profile-summary")]
    public async Task<IActionResult> GetProfileSummary(int id)
    {
        var summary = await _radiologistService.GetProfileSummaryAsync(id);

        if (summary.Contains("not found"))
            return NotFound(summary);

        return Ok(summary);
    }
    
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("vacation/{radiologistId}")]
    public async Task<IActionResult> GetExtendVacationDays(int radiologistId)
    {
        var days = await _radiologistService.GetRadiologistExtendedVacationDaysAsync(radiologistId);

        return Ok(days);
    }
    
    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("hazard/{radiologistId}")]
    public async Task<IActionResult> GetRadiologistHazardPayCoefficient(int radiologistId)
    {
        var days = await _radiologistService.GetRadiologistHazardPayCoefficientAsync(radiologistId);

        return Ok(days);
    }
}