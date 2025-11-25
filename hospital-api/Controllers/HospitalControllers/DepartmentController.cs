using hospital_api.Models.HospitalAggregate;
using hospital_api.Services.Interfaces.HospitalServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.HospitalControllers;

[Authorize]
[ApiController]
[Route("api/hospital/[controller]")]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var departments = await _departmentService.GetAllAsync();
        return Ok(departments);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var department = await _departmentService.GetByIdAsync(id);
        if (department == null) return NotFound();
        return Ok(department);
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Department department)
    {
        await _departmentService.AddAsync(department);
        return CreatedAtAction(nameof(Get), new { id = department.Id }, department);
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Department department)
    {
        if (id != department.Id) return BadRequest();
        await _departmentService.UpdateAsync(department);
        return NoContent();
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _departmentService.DeleteAsync(id);
        return NoContent();
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("name/{name}")]
    public async Task<IActionResult> GetByName(string name)
    {
        var department = await _departmentService.GetByNameAsync(name);
        if (department == null) return NotFound();
        return Ok(department);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("specialization/{specialization}")]
    public async Task<IActionResult> GetBySpecialization(string specialization)
    {
        var departments = await _departmentService.GetBySpecializationAsync(specialization);
        return Ok(departments);
    }

    [Authorize(Roles = "Authorized, Operator, Admin")]
    [HttpGet("building/{buildingId}/with-rooms")]
    public async Task<IActionResult> GetByBuildingWithRooms(int buildingId)
    {
        var departments = await _departmentService.GetByBuildingIdWithRoomsAsync(buildingId);
        return Ok(departments);
    }
}