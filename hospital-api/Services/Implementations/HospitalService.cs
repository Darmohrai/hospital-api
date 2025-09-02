using hospital_api.Models.HospitalAggregate;
using hospital_api.Repositories.Interfaces.HospitalRepo;
using hospital_api.Services.Interfaces;

namespace hospital_api.Services.Implementations;

public class HospitalService : IHospitalService
{
    private readonly IHospitalRepository _hospitalRepository;

    public HospitalService(IHospitalRepository hospitalRepository)
    {
        _hospitalRepository = hospitalRepository;
    }

    public async Task<IEnumerable<Hospital>> GetAllHospitalsAsync()
    {
        return await _hospitalRepository.GetAllAsync();
    }

    public async Task<Hospital?> GetHospitalByIdAsync(int id)
    {
        return await _hospitalRepository.GetByIdAsync(id);
    }

    public async Task CreateHospitalAsync(Hospital hospital)
    {
        await _hospitalRepository.AddAsync(hospital);
    }

    public async Task UpdateHospitalAsync(Hospital hospital)
    {
        await _hospitalRepository.UpdateAsync(hospital);
    }

    public async Task DeleteHospitalAsync(int id)
    {
        await _hospitalRepository.DeleteAsync(id);
    }
}