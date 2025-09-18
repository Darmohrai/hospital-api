using hospital_api.Models.PatientAggregate;
using hospital_api.Repositories.Interfaces;
using hospital_api.Services.Interfaces;

namespace hospital_api.Services.Implementations;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepository;

    public PatientService(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    // --- CRUD ---
    public async Task<Patient?> GetByIdAsync(int id)
    {
        return await _patientRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Patient>> GetAllAsync()
    {
        return await _patientRepository.GetAllAsync();
    }

    public async Task AddAsync(Patient patient)
    {
        if (string.IsNullOrWhiteSpace(patient.FullName))
        {
            throw new ArgumentException("Patient full name is required.");
        }

        await _patientRepository.AddAsync(patient);
    }

    public async Task UpdateAsync(Patient patient)
    {
        var existingPatient = await _patientRepository.GetByIdAsync(patient.Id);
        if (existingPatient == null)
        {
            throw new InvalidOperationException("Patient not found.");
        }

        await _patientRepository.UpdateAsync(patient);
    }

    public async Task DeleteAsync(int id)
    {
        await _patientRepository.DeleteAsync(id);
    }

    // --- Специфічні методи ---
    public async Task<IEnumerable<Patient>> GetByFullNameAsync(string fullName)
    {
        return await _patientRepository.GetByFullNameAsync(fullName);
    }

    public async Task<IEnumerable<Patient>> GetByHealthStatusAsync(string status)
    {
        return await _patientRepository.GetByHealthStatusAsync(status);
    }

    public async Task<IEnumerable<Patient>> GetByClinicIdAsync(int clinicId)
    {
        return await _patientRepository.GetByClinicIdAsync(clinicId);
    }

    public async Task<IEnumerable<Patient>> GetByHospitalIdAsync(int hospitalId)
    {
        return await _patientRepository.GetByHospitalIdAsync(hospitalId);
    }

    public async Task<IEnumerable<Patient>> GetByAssignedDoctorIdAsync(int doctorId)
    {
        return await _patientRepository.GetByAssignedDoctorIdAsync(doctorId);
    }

    public async Task<IEnumerable<Patient>> GetAllWithAssociationsAsync()
    {
        return await _patientRepository.GetAllWithAssociationsAsync();
    }
}
