using hospital_api.Models.StaffAggregate.DoctorAggregate;

namespace hospital_api.Services.Interfaces.StaffServices;

public interface IOphthalmologistService
{
    Task<IEnumerable<Ophthalmologist>> GetAllOphthalmologistsAsync();
    Task<Ophthalmologist?> GetOphthalmologistByIdAsync(int id);
    Task AddOphthalmologistAsync(Ophthalmologist ophthalmologist);
    Task UpdateOphthalmologistAsync(Ophthalmologist ophthalmologist);
    Task DeleteOphthalmologistAsync(int id);

    Task<IEnumerable<Ophthalmologist>> GetOphthalmologistsByExtendedVacationDaysAsync(int minDays);
    Task<string> GetOphthalmologistProfileSummaryAsync(int ophthalmologistId);
}
