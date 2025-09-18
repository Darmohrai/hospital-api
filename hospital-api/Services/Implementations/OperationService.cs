using hospital_api.Models.OperationsAggregate;
using hospital_api.Repositories.Interfaces;
using hospital_api.Services.Interfaces;

namespace hospital_api.Services.Implementations;

public class OperationService : IOperationService
{
    private readonly IOperationRepository _operationRepository;

    public OperationService(IOperationRepository operationRepository)
    {
        _operationRepository = operationRepository;
    }

    // --- CRUD ---
    public async Task<Operation?> GetByIdAsync(int id)
    {
        return await _operationRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Operation>> GetAllAsync()
    {
        return await _operationRepository.GetAllAsync();
    }

    public async Task AddAsync(Operation operation)
    {
        if (operation.Date == default)
        {
            throw new ArgumentException("Operation date is required.");
        }

        await _operationRepository.AddAsync(operation);
    }

    public async Task UpdateAsync(Operation operation)
    {
        var existingOperation = await _operationRepository.GetByIdAsync(operation.Id);
        if (existingOperation == null)
        {
            throw new InvalidOperationException("Operation not found.");
        }

        await _operationRepository.UpdateAsync(operation);
    }

    public async Task DeleteAsync(int id)
    {
        await _operationRepository.DeleteAsync(id);
    }

    // --- Специфічні методи ---
    public async Task<IEnumerable<Operation>> GetByPatientIdAsync(int patientId)
    {
        return await _operationRepository.GetByPatientIdAsync(patientId);
    }

    public async Task<IEnumerable<Operation>> GetByDoctorIdAsync(int doctorId)
    {
        return await _operationRepository.GetByDoctorIdAsync(doctorId);
    }

    public async Task<IEnumerable<Operation>> GetByHospitalIdAsync(int hospitalId)
    {
        return await _operationRepository.GetByHospitalIdAsync(hospitalId);
    }

    public async Task<IEnumerable<Operation>> GetByClinicIdAsync(int clinicId)
    {
        return await _operationRepository.GetByClinicIdAsync(clinicId);
    }

    public async Task<IEnumerable<Operation>> GetFatalOperationsAsync()
    {
        return await _operationRepository.GetFatalOperationsAsync();
    }

    public async Task<IEnumerable<Operation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _operationRepository.GetByDateRangeAsync(startDate, endDate);
    }
}
