using hospital_api.Models.HospitalAggregate;
using hospital_api.Services.Interfaces.HospitalServices;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.HospitalControllers;

[ApiController]
[Route("api/hospital/[controller]")]
public class BuildingController : ControllerBase
{
    private readonly IBuildingService _buildingService;

    public BuildingController(IBuildingService buildingService)
    {
        _buildingService = buildingService;
    }

    // Отримати всі корпуси
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var buildings = await _buildingService.GetAllAsync();
        return Ok(buildings);
    }

    // Отримати корпус за ID
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var building = await _buildingService.GetByIdAsync(id);
        if (building == null) return NotFound();
        return Ok(building);
    }

    // Додати новий корпус
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Building building)
    {
        await _buildingService.AddAsync(building);
        return CreatedAtAction(nameof(Get), new { id = building.Id }, building);
    }

    // Оновити корпус
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Building building)
    {
        if (id != building.Id) return BadRequest();
        await _buildingService.UpdateAsync(building);
        return NoContent();
    }

    // Видалити корпус
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _buildingService.DeleteAsync(id);
        return NoContent();
    }

    // Отримати корпус за назвою
    [HttpGet("name/{name}")]
    public async Task<IActionResult> GetByName(string name)
    {
        var building = await _buildingService.GetByNameAsync(name);
        if (building == null) return NotFound();
        return Ok(building);
    }

    // Отримати корпуси за ID лікарні
    [HttpGet("hospital/{hospitalId}")]
    public async Task<IActionResult> GetByHospital(int hospitalId)
    {
        var buildings = await _buildingService.GetByHospitalIdAsync(hospitalId);
        return Ok(buildings);
    }

    // Отримати всі корпуси з відділеннями
    [HttpGet("with-departments")]
    public async Task<IActionResult> GetAllWithDepartments()
    {
        var buildings = await _buildingService.GetAllWithDepartmentsAsync();
        return Ok(buildings);
    }
}