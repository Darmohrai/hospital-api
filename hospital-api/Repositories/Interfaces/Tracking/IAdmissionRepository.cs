using hospital_api.Models.Tracking;

namespace hospital_api.Repositories.Interfaces.Tracking;

public interface IAdmissionRepository : IRepository<Admission>
{
    Task<IEnumerable<Admission>> GetAllWithAssociationsAsync();
}