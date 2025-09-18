using hospital_api.Models.HospitalAggregate;
using hospital_api.Services.Interfaces.HospitalServices;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.HospitalControllers;

[ApiController]
[Route("api/hospital/[controller]")]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    // Отримати всі відділення
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var departments = await _departmentService.GetAllAsync();
        return Ok(departments);
    }

    // Отримати відділення за ID
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var department = await _departmentService.GetByIdAsync(id);
        if (department == null) return NotFound();
        return Ok(department);
    }

    // Додати нове відділення
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Department department)
    {
        await _departmentService.AddAsync(department);
        return CreatedAtAction(nameof(Get), new { id = department.Id }, department);
    }

    // Оновити відділення
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Department department)
    {
        if (id != department.Id) return BadRequest();
        await _departmentService.UpdateAsync(department);
        return NoContent();
    }

    // Видалити відділення
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _departmentService.DeleteAsync(id);
        return NoContent();
    }

    // Отримати відділення за назвою
    [HttpGet("name/{name}")]
    public async Task<IActionResult> GetByName(string name)
    {
        var department = await _departmentService.GetByNameAsync(name);
        if (department == null) return NotFound();
        return Ok(department);
    }

    // Отримати відділення за спеціалізацією
    [HttpGet("specialization/{specialization}")]
    public async Task<IActionResult> GetBySpecialization(string specialization)
    {
        var departments = await _departmentService.GetBySpecializationAsync(specialization);
        return Ok(departments);
    }

    // Отримати відділення за ID будівлі разом із кімнатами
    [HttpGet("building/{buildingId}/with-rooms")]
    public async Task<IActionResult> GetByBuildingWithRooms(int buildingId)
    {
        var departments = await _departmentService.GetByBuildingIdWithRoomsAsync(buildingId);
        return Ok(departments);
    }
}