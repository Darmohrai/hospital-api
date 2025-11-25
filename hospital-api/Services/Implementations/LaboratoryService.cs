using hospital_api.Models.LaboratoryAggregate;
using hospital_api.Repositories.Interfaces;
using hospital_api.Services.Interfaces;

namespace hospital_api.Services.Implementations;

public class LaboratoryService : ILaboratoryService
{
    private readonly ILaboratoryRepository _laboratoryRepository;

    public LaboratoryService(ILaboratoryRepository laboratoryRepository)
    {
        _laboratoryRepository = laboratoryRepository;
    }

    public async Task<Laboratory?> GetByIdAsync(int id)
    {
        return await _laboratoryRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Laboratory>> GetAllAsync()
    {
        return await _laboratoryRepository.GetAllAsync();
    }

    public async Task AddAsync(Laboratory laboratory)
    {
        if (string.IsNullOrWhiteSpace(laboratory.Name))
        {
            throw new ArgumentException("Laboratory name is required.");
        }

        await _laboratoryRepository.AddAsync(laboratory);
    }

    public async Task UpdateAsync(Laboratory laboratory)
    {
        var existingLab = await _laboratoryRepository.GetByIdAsync(laboratory.Id);
        if (existingLab == null)
        {
            throw new InvalidOperationException("Laboratory not found.");
        }

        await _laboratoryRepository.UpdateAsync(laboratory);
    }

    public async Task DeleteAsync(int id)
    {
        await _laboratoryRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Laboratory>> GetByProfileAsync(string profile)
    {
        return await _laboratoryRepository.GetByProfileAsync(profile);
    }

    public async Task<IEnumerable<Laboratory>> GetByHospitalIdAsync(int hospitalId)
    {
        return await _laboratoryRepository.GetByHospitalIdAsync(hospitalId);
    }

    public async Task<IEnumerable<Laboratory>> GetByClinicIdAsync(int clinicId)
    {
        return await _laboratoryRepository.GetByClinicIdAsync(clinicId);
    }

    public async Task<Laboratory?> GetByNameWithAssociationsAsync(string name)
    {
        return await _laboratoryRepository.GetByNameWithAssociationsAsync(name);
    }
}
