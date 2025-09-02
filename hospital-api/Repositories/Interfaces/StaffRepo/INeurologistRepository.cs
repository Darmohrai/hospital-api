using hospital_api.Models.StaffAggregate.DoctorAggregate;

namespace hospital_api.Repositories.Interfaces.StaffRepo;

public interface INeurologistRepository : IRepository<Neurologist>
{
    // Отримати неврологів, які мають певну мінімальну кількість додаткових днів відпустки
    Task<IEnumerable<Neurologist>> GetByExtendedVacationDaysAsync(int minDays);
}