using hospital_api.Models.StaffAggregate;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers.StaffControllers
{
    [ApiController]
    [Route("api/staff/doctors")] // Більш чіткий та REST-сумісний маршрут
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        /// <summary>
        /// Отримує список всіх лікарів (усіх спеціальностей).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var doctors = await _doctorService.GetAllAsync();
            return Ok(doctors);
        }

        /// <summary>
        /// Отримує лікаря за його ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var doctor = await _doctorService.GetByIdAsync(id);
            if (doctor == null)
                return NotFound();

            return Ok(doctor);
        }

        /// <summary>
        /// Отримує лікарів за вказаною спеціальністю.
        /// </summary>
        [HttpGet("by-specialty")]
        public async Task<IActionResult> GetBySpecialty([FromQuery] string specialty)
        {
            var doctors = await _doctorService.GetBySpecialtyAsync(specialty);
            return Ok(doctors);
        }

        /// <summary>
        /// Отримує лікарів за вказаним науковим ступенем.
        /// </summary>
        [HttpGet("by-degree")]
        public async Task<IActionResult> GetByDegree([FromQuery] AcademicDegree degree)
        {
            var doctors = await _doctorService.GetByDegreeAsync(degree);
            return Ok(doctors);
        }

        /// <summary>
        /// Отримує лікарів за вказаним вченим званням.
        /// </summary>
        [HttpGet("by-title")]
        public async Task<IActionResult> GetByTitle([FromQuery] AcademicTitle title)
        {
            var doctors = await _doctorService.GetByTitleAsync(title);
            return Ok(doctors);
        }

        // ❌ Методи Create, Update та Delete були видалені.
        // Це архітектурно правильне рішення, оскільки Doctor - абстрактний клас.
        // Створення відбувається через специфічні контролери (наприклад, POST /api/staff/surgeons),
        // а видалення - через загальний StaffController (DELETE /api/staff/{id}).
    }
}