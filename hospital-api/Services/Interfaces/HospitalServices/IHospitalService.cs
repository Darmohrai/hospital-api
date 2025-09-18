using hospital_api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using hospital_api.Models.HospitalAggregate;

namespace hospital_api.Services.Interfaces.HospitalServices;

public interface IHospitalService
{
    Task<IEnumerable<Hospital>> GetAllHospitalsAsync();
    Task<Hospital?> GetHospitalByIdAsync(int id);
    Task CreateHospitalAsync(Hospital hospital);
    Task UpdateHospitalAsync(Hospital hospital);
    Task DeleteHospitalAsync(int id);
}