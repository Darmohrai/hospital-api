using hospital_api.Models.StaffAggregate.DoctorAggregate;

namespace hospital_api.Repositories.Interfaces.StaffRepo;

public interface IOphthalmologistRepository : IRepository<Ophthalmologist>
{
    // Отримати офтальмологів, які мають певну мінімальну кількість додаткових днів відпустки
    Task<IEnumerable<Ophthalmologist>> GetByExtendedVacationDaysAsync(int minDays);
}