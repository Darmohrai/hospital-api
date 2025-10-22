using hospital_api.Models.Tracking;

namespace hospital_api.Repositories.Interfaces.Tracking;

public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<IEnumerable<Appointment>> GetAllWithAssociationsAsync();
}