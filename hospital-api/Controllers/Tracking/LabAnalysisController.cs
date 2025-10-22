using hospital_api.DTOs.Tracking;
using hospital_api.Services.Interfaces.Tracking;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.Tracking;

[ApiController]
[Route("api/[controller]")]
public class LabAnalysisController : ControllerBase
{
    private readonly ILabAnalysisService _service;

    public LabAnalysisController(ILabAnalysisService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var analysis = await _service.GetByIdAsync(id);
        if (analysis == null)
            return NotFound();
        return Ok(analysis);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLabAnalysisDto dto)
    {
        var newAnalysis = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = newAnalysis.Id }, newAnalysis);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateLabAnalysisDto dto)
    {
        var success = await _service.UpdateAsync(id, dto);
        if (!success)
            return NotFound();
        
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success)
            return NotFound();
        
        return NoContent();
    }
}