using hospital_api.Models.HospitalAggregate;

namespace hospital_api.Repositories.Interfaces.HospitalRepo;

public interface IBedRepository : IRepository<Bed>
{
    // Отримати всі вільні ліжка.
    Task<IEnumerable<Bed>> GetAvailableBedsAsync();

    // Отримати всі зайняті ліжка.
    Task<IEnumerable<Bed>> GetOccupiedBedsAsync();

    // Отримати ліжка за ідентифікатором кімнати, включаючи дані про саму кімнату.
    Task<IEnumerable<Bed>> GetByRoomIdAsync(int roomId);
}