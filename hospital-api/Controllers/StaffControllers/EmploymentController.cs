using hospital_api.DTOs.Staff;
using hospital_api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers;

[Authorize(Roles = "Admin, Operator")]
[ApiController]
[Route("api/employment")]
public class EmploymentController : ControllerBase
{
    private readonly IEmploymentService _employmentService;

    public EmploymentController(IEmploymentService employmentService)
    {
        _employmentService = employmentService;
    }
    
    [HttpGet("staff/{staffId}")]
    public async Task<IActionResult> GetByStaffId(int staffId)
    {
        var response = await _employmentService.GetEmploymentsByStaffIdAsync(staffId);
        
        if (!response.IsSuccess)
        {
            return NotFound(response.ErrorMessage);
        }

        return Ok(response.Data);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateEmployment([FromBody] CreateEmploymentDto dto)
    {
        var response = await _employmentService.CreateEmploymentAsync(dto);

        if (!response.IsSuccess)
        {
            return BadRequest(response.ErrorMessage);
        }

        return Ok(response.Data);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployment(int id)
    {
        var response = await _employmentService.DeleteEmploymentAsync(id);

        if (!response.IsSuccess)
        {
            return NotFound(response.ErrorMessage);
        }

        return NoContent();
    }
}