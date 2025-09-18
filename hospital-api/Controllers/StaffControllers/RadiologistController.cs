using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers;

[ApiController]
[Route("api/staff/[controller]")]
public class RadiologistController : ControllerBase
{
    private readonly IRadiologistService _radiologistService;

    public RadiologistController(IRadiologistService radiologistService)
    {
        _radiologistService = radiologistService;
    }

    // Отримати всіх радіологів
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var radiologists = await _radiologistService.GetAllRadiologistsAsync();
        return Ok(radiologists);
    }

    // Отримати радіолога за ID
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var radiologist = await _radiologistService.GetRadiologistByIdAsync(id);
        if (radiologist == null)
            return NotFound();

        return Ok(radiologist);
    }

    // Додати нового радіолога
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Radiologist radiologist)
    {
        await _radiologistService.AddRadiologistAsync(radiologist);
        return CreatedAtAction(nameof(Get), new { id = radiologist.Id }, radiologist);
    }

    // Оновити радіолога
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Radiologist radiologist)
    {
        if (id != radiologist.Id)
            return BadRequest();

        await _radiologistService.UpdateRadiologistAsync(radiologist);
        return NoContent();
    }

    // Видалити радіолога
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _radiologistService.DeleteRadiologistAsync(id);
        return NoContent();
    }

    // Фільтрація за коефіцієнтом шкідливості
    [HttpGet("hazard-pay/{minCoefficient}")]
    public async Task<IActionResult> GetByHazardPayCoefficient(float minCoefficient)
    {
        var result = await _radiologistService.GetByHazardPayCoefficientAsync(minCoefficient);
        return Ok(result);
    }

    // Фільтрація за розширеною відпусткою
    [HttpGet("extended-vacation/{minDays}")]
    public async Task<IActionResult> GetByExtendedVacationDays(int minDays)
    {
        var result = await _radiologistService.GetByExtendedVacationDaysAsync(minDays);
        return Ok(result);
    }

    // Фільтрація одночасно за коефіцієнтом шкідливості та додатковими днями відпустки
    [HttpGet("hazard-and-vacation")]
    public async Task<IActionResult> GetByHazardAndVacation([FromQuery] float minCoefficient, [FromQuery] int minDays)
    {
        var result = await _radiologistService.GetByHazardPayAndVacationAsync(minCoefficient, minDays);
        return Ok(result);
    }

    // Профіль радіолога у вигляді текстового звіту
    [HttpGet("{id}/profile-summary")]
    public async Task<IActionResult> GetProfileSummary(int id)
    {
        var summary = await _radiologistService.GetRadiologistProfileSummaryAsync(id);
        return Ok(summary);
    }
}