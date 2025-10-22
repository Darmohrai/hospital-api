using hospital_api.DTOs.Reports;
using hospital_api.Models.StaffAggregate;

namespace hospital_api.Services.Interfaces;

public interface IReportService
{
    /// <summary>
    /// Отримує кількість пацієнтів, прийнятих за вказаний день,
    /// згідно з заданими фільтрами. (Запит №1)
    /// </summary>
    /// <param name="date">Дата звіту</param>
    /// <param name="doctorId">ID конкретного лікаря (опціонально)</param>
    /// <param name="clinicId">ID поліклініки (опціонально)</param>
    /// <param name="specialty">Профіль лікаря (опціонально)</param>
    Task<AppointmentCountDto> GetDailyAppointmentCountAsync(DateTime date, int? doctorId, int? clinicId,
        string? specialty);

    Task<HospitalCapacityReportDto> GetHospitalCapacityReportAsync(int hospitalId);

    Task<LaboratoryReportDto> GetLaboratoryReportAsync(DateTime startDate, DateTime endDate, int? hospitalId,
        int? clinicId);

    Task<IEnumerable<PatientOperationDto>> GetPatientOperationReportAsync(
        DateTime startDate,
        DateTime endDate,
        int? hospitalId,
        int? clinicId,
        int? doctorId);

    Task<IEnumerable<PatientListDto>> GetPatientsByClinicAndSpecialtyAsync(int clinicId, string specialty);

    Task<IEnumerable<PatientListDto>> GetInpatientReportAsync(
        DateTime startDate,
        DateTime endDate,
        int? hospitalId,
        int? doctorId);

    Task<DoctorReportDto> GetDoctorReportAsync(
        int? hospitalId,
        int? clinicId,
        string? specialty,
        int? minWorkExperience,
        AcademicDegree? degree,
        AcademicTitle? title
    );

    Task<DoctorReportDto> GetDoctorOperationReportAsync(
        int minOperationCount,
        string? specialty,
        int? hospitalId,
        int? clinicId
    );

    Task<SupportStaffReportDto> GetSupportStaffReportAsync(
        SupportRole role,
        int? hospitalId,
        int? clinicId
    );

    Task<IEnumerable<DoctorPerformanceDto>> GetDoctorPerformanceReportAsync(int? hospitalId, int? clinicId);
    
    Task<DepartmentOccupancyReportDto> GetDepartmentOccupancyReportAsync(int departmentId);
}