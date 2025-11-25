using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers;

[Authorize]
[ApiController]
[Route("api/staff")]
public class StaffController : ControllerBase
{
    private readonly IStaffService _staffService;

    public StaffController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var staff = await _staffService.GetAllAsync();
        return Ok(staff);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var staff = await _staffService.GetByIdAsync(id);
        if (staff == null)
            return NotFound();

        return Ok(staff);
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _staffService.DeleteAsync(id);
        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return NoContent();
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("clinic/{clinicId}")]
    public async Task<IActionResult> GetByClinic(int clinicId)
    {
        var staff = await _staffService.GetByClinicAsync(clinicId);
        return Ok(staff);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("hospital/{hospitalId}")]
    public async Task<IActionResult> GetByHospital(int hospitalId)
    {
        var staff = await _staffService.GetByHospitalAsync(hospitalId);
        return Ok(staff);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("experienced")]
    public async Task<IActionResult> GetExperienced([FromQuery] int minYears)
    {
        var staff = await _staffService.GetExperiencedStaffAsync(minYears);
        return Ok(staff);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("{id}/annual-bonus")]
    public async Task<IActionResult> GetAnnualBonus(int id)
    {
        var result = await _staffService.CalculateAnnualBonusAsync(id);

        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(new { StaffId = id, AnnualBonus = result.Data });
    }
}