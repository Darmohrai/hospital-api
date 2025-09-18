using hospital_api.Models.HospitalAggregate;
using hospital_api.Services.Interfaces.HospitalServices;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.HospitalControllers;

[ApiController]
[Route("api/hospital/[controller]")]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    // --- CRUD ---

    // Отримати всі кімнати
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var rooms = await _roomService.GetAllAsync();
        return Ok(rooms);
    }

    // Отримати кімнату за ID
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var room = await _roomService.GetByIdAsync(id);
        if (room == null) return NotFound();
        return Ok(room);
    }

    // Додати нову кімнату
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Room room)
    {
        await _roomService.AddAsync(room);
        return CreatedAtAction(nameof(Get), new { id = room.Id }, room);
    }

    // Оновити кімнату
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Room room)
    {
        if (id != room.Id) return BadRequest();
        await _roomService.UpdateAsync(room);
        return NoContent();
    }

    // Видалити кімнату
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _roomService.DeleteAsync(id);
        return NoContent();
    }

    // --- Специфічні методи ---

    // Отримати кімнату за номером
    [HttpGet("number/{roomNumber}")]
    public async Task<IActionResult> GetByNumber(string roomNumber)
    {
        var room = await _roomService.GetByNumberAsync(roomNumber);
        if (room == null) return NotFound();
        return Ok(room);
    }

    // Отримати кімнати за ID відділення
    [HttpGet("department/{departmentId}")]
    public async Task<IActionResult> GetByDepartment(int departmentId)
    {
        var rooms = await _roomService.GetByDepartmentIdAsync(departmentId);
        return Ok(rooms);
    }

    // Отримати кімнати за місткістю
    [HttpGet("capacity/{capacity}")]
    public async Task<IActionResult> GetByCapacity(int capacity)
    {
        var rooms = await _roomService.GetByCapacityAsync(capacity);
        return Ok(rooms);
    }

    // Отримати всі кімнати разом із ліжками та відділенням
    [HttpGet("with-beds-department")]
    public async Task<IActionResult> GetAllWithBedsAndDepartment()
    {
        var rooms = await _roomService.GetAllWithBedsAndDepartmentAsync();
        return Ok(rooms);
    }
}