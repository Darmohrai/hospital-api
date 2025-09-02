using hospital_api.Models.OperationsAggregate;

namespace hospital_api.Repositories.Interfaces;

public interface IOperationRepository : IRepository<Operation>
{
    Task<IEnumerable<Operation>> GetByPatientIdAsync(int patientId);
    Task<IEnumerable<Operation>> GetByDoctorIdAsync(int doctorId);
    Task<IEnumerable<Operation>> GetByHospitalIdAsync(int hospitalId);
    Task<IEnumerable<Operation>> GetByClinicIdAsync(int clinicId);
    Task<IEnumerable<Operation>> GetFatalOperationsAsync();
    Task<IEnumerable<Operation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}
