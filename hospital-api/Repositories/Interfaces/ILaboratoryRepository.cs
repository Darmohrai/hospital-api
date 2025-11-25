using hospital_api.Models.LaboratoryAggregate;

namespace hospital_api.Repositories.Interfaces;

public interface ILaboratoryRepository : IRepository<Laboratory>
{
    Task<IEnumerable<Laboratory>> GetByProfileAsync(string profile);

    Task<IEnumerable<Laboratory>> GetByHospitalIdAsync(int hospitalId);

    Task<IEnumerable<Laboratory>> GetByClinicIdAsync(int clinicId);

    Task<Laboratory?> GetByNameWithAssociationsAsync(string name);
}