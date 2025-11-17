using hospital_api.DTOs.Staff;
using hospital_api.Models.StaffAggregate.DoctorAggregate;

namespace hospital_api.Services.Interfaces.StaffServices;

public interface INeurologistService
{
    Task<IEnumerable<Neurologist>> GetAllNeurologistsAsync();
    Task<Neurologist?> GetNeurologistByIdAsync(int id);
    Task<ServiceResponse<Neurologist>> AddNeurologistToHospitalAsync(int? hospitalId,
        CreateNeurologistDto neurologistDto);
    Task UpdateNeurologistAsync(Neurologist neurologist);
    Task DeleteNeurologistAsync(int id);

    Task<IEnumerable<Neurologist>> GetNeurologistsByExtendedVacationDaysAsync(int minDays);
    Task<string> GetNeurologistProfileSummaryAsync(int neurologistId);
    
    Task<int> GetNeurologistExtendedVacationDaysAsync(int neurologistId);
}
