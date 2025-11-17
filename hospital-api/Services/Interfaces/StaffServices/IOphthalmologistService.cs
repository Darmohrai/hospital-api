using hospital_api.Models.StaffAggregate.DoctorAggregate;

namespace hospital_api.Services.Interfaces.StaffServices;

public interface IOphthalmologistService
{
    Task<IEnumerable<Ophthalmologist>> GetAllAsync();
    Task<Ophthalmologist?> GetByIdAsync(int id);
    Task<ServiceResponse<Ophthalmologist>> CreateAsync(Ophthalmologist ophthalmologist);
    Task<ServiceResponse<Ophthalmologist>> UpdateAsync(Ophthalmologist ophthalmologist);
    Task<ServiceResponse<bool>> DeleteAsync(int id);
        
    Task<IEnumerable<Ophthalmologist>> GetByExtendedVacationDaysAsync(int minDays);
        
    Task<string> GetProfileSummaryAsync(int ophthalmologistId);
    
    Task<int> GetOphthalmologistExtendedVacationDaysAsync(int ophthalmologistId);

}
