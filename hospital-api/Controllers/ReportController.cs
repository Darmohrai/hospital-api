using hospital_api.DTOs.Reports;
using hospital_api.Models.StaffAggregate;
using hospital_api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hospital_api.Controllers;

[Authorize(Roles = "Authorized, Operator, Admin")]
[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// (Запит №1) Отримує кількість прийнятих пацієнтів за день.
    /// </summary>
    /// <param name="date">Дата (напр., '2025-10-20')</param>
    /// <param name="doctorId">Фільтр: ID конкретного лікаря</param>
    /// <param name="clinicId">Фільтр: ID поліклініки</param>
    /// <param name="specialty">Фільтр: Профіль лікаря (напр., 'Surgeon')</param>
    [HttpGet("daily-appointments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailyAppointmentCount(
        [FromQuery] DateTime date,
        [FromQuery] int? doctorId,
        [FromQuery] int? clinicId,
        [FromQuery] string? specialty)
    {
        // Встановлюємо час на початок дня, щоб врахувати всі візити за цю дату
        var reportDate = date.Date;

        var report = await _reportService.GetDailyAppointmentCountAsync(reportDate, doctorId, clinicId, specialty);
        return Ok(report);
    }

    /// <summary>
    /// (Запит №2) Отримує звіт про місткість лікарні (палати, ліжка).
    /// </summary>
    [HttpGet("hospital-capacity/{hospitalId}")]
    [ProducesResponseType(typeof(HospitalCapacityReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHospitalCapacity(int hospitalId)
    {
        try
        {
            var report = await _reportService.GetHospitalCapacityReportAsync(hospitalId);
            return Ok(report);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// (Запит №3) Звіт про середню кількість обстежень у лабораторіях.
    /// </summary>
    [HttpGet("laboratory-report")]
    [ProducesResponseType(typeof(LaboratoryReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLaboratoryReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? hospitalId,
        [FromQuery] int? clinicId)
    {
        if (startDate > endDate)
            return BadRequest("startDate не може бути пізнішою за endDate.");

        var report = await _reportService.GetLaboratoryReportAsync(startDate, endDate, hospitalId, clinicId);
        return Ok(report);
    }

    /// <summary>
    /// (Запит №5) Отримує список пацієнтів, яким проводили операції.
    /// </summary>
    [HttpGet("patient-operations")]
    [ProducesResponseType(typeof(IEnumerable<PatientOperationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientOperationReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? hospitalId,
        [FromQuery] int? clinicId,
        [FromQuery] int? doctorId)
    {
        if (startDate > endDate)
            return BadRequest("startDate не може бути пізнішою за endDate.");

        if (hospitalId.HasValue && clinicId.HasValue)
            return BadRequest("Вкажіть АБО hospitalId, АБО clinicId, але не обидва.");

        var report =
            await _reportService.GetPatientOperationReportAsync(startDate, endDate, hospitalId, clinicId, doctorId);
        return Ok(report);
    }

    /// <summary>
    /// (Запит №6) Отримує пацієнтів, які лікуються у лікаря вказаного профілю
    /// у вказаній поліклініці.
    /// </summary>
    [HttpGet("patients-by-clinic-specialty")]
    [ProducesResponseType(typeof(IEnumerable<PatientListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientsByClinicAndSpecialty(
        [FromQuery] int clinicId,
        [FromQuery] string specialty)
    {
        if (string.IsNullOrEmpty(specialty))
            return BadRequest("Параметр 'specialty' є обов'язковим.");

        var patients = await _reportService.GetPatientsByClinicAndSpecialtyAsync(clinicId, specialty);
        return Ok(patients);
    }

    /// <summary>
    /// (Запит №7) Отримує пацієнтів, які пройшли стаціонарне лікування
    /// (були виписані) за вказаний період.
    /// </summary>
    [HttpGet("inpatient-report")]
    [ProducesResponseType(typeof(IEnumerable<PatientListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInpatientReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? hospitalId,
        [FromQuery] int? doctorId)
    {
        if (startDate > endDate)
            return BadRequest("startDate не може бути пізнішою за endDate.");

        var patients = await _reportService.GetInpatientReportAsync(startDate, endDate, hospitalId, doctorId);
        return Ok(patients);
    }

    /// <summary>
    /// (Запит №8) Отримує звіт по лікарях з розширеними фільтрами.
    /// </summary>
    [HttpGet("doctor-report")]
    [ProducesResponseType(typeof(DoctorReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDoctorReport(
        [FromQuery] int? hospitalId,
        [FromQuery] int? clinicId,
        [FromQuery] string? specialty,
        [FromQuery] int? minWorkExperience,
        [FromQuery] AcademicDegree? degree,
        [FromQuery] AcademicTitle? title)
    {
        if (hospitalId.HasValue && clinicId.HasValue)
            return BadRequest("Вкажіть АБО hospitalId, АБО clinicId, але не обидва.");

        var report =
            await _reportService.GetDoctorReportAsync(hospitalId, clinicId, specialty, minWorkExperience, degree,
                title);
        return Ok(report);
    }

    /// <summary>
    /// (Запит №9) Отримує звіт по лікарях, які виконали
    /// певну кількість операцій.
    /// </summary>
    [HttpGet("doctor-operation-report")]
    [ProducesResponseType(typeof(DoctorReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDoctorOperationReport(
        [FromQuery] int minOperationCount,
        [FromQuery] string? specialty,
        [FromQuery] int? hospitalId,
        [FromQuery] int? clinicId)
    {
        if (hospitalId.HasValue && clinicId.HasValue)
            return BadRequest("Вкажіть АБО hospitalId, АБО clinicId, але не обидва.");

        var report =
            await _reportService.GetDoctorOperationReportAsync(minOperationCount, specialty, hospitalId, clinicId);
        return Ok(report);
    }

    /// <summary>
    /// (Запит №10) Отримує звіт по обслуговуючому персоналу.
    /// </summary>
    [HttpGet("support-staff-report")]
    [ProducesResponseType(typeof(SupportStaffReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSupportStaffReport(
        [FromQuery] SupportRole role, // Enum: 'Nurse', 'Orderly', 'Cleaner'
        [FromQuery] int? hospitalId,
        [FromQuery] int? clinicId)
    {
        if (hospitalId.HasValue && clinicId.HasValue)
            return BadRequest("Вкажіть АБО hospitalId, АБО clinicId, але не обидва.");

        var report = await _reportService.GetSupportStaffReportAsync(role, hospitalId, clinicId);
        return Ok(report);
    }
    
    /// <summary>
    /// (Запит №11) Отримує рейтинг лікарів за відсотком летальних операцій.
    /// </summary>
    [HttpGet("doctor-performance-report")]
    [ProducesResponseType(typeof(IEnumerable<DoctorPerformanceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDoctorPerformanceReport(
        [FromQuery] int? hospitalId,
        [FromQuery] int? clinicId)
    {
        if (hospitalId.HasValue && clinicId.HasValue)
            return BadRequest("Вкажіть АБО hospitalId, АБО clinicId, але не обидва.");

        var report = await _reportService.GetDoctorPerformanceReportAsync(hospitalId, clinicId);
        return Ok(report);
    }
    
    /// <summary>
    /// (Запит №13) Отримує звіт про завантаженість палат у відділенні.
    /// </summary>
    [HttpGet("department-occupancy/{departmentId}")]
    [ProducesResponseType(typeof(DepartmentOccupancyReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDepartmentOccupancy(int departmentId)
    {
        try
        {
            var report = await _reportService.GetDepartmentOccupancyReportAsync(departmentId);
            return Ok(report);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}