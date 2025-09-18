using hospital_api.Models.LaboratoryAggregate;

namespace hospital_api.Services.Interfaces;

public interface ILaboratoryService
{
    // CRUD
    Task<Laboratory?> GetByIdAsync(int id);
    Task<IEnumerable<Laboratory>> GetAllAsync();
    Task AddAsync(Laboratory laboratory);
    Task UpdateAsync(Laboratory laboratory);
    Task DeleteAsync(int id);

    // Специфічні методи
    Task<IEnumerable<Laboratory>> GetByProfileAsync(string profile);
    Task<IEnumerable<Laboratory>> GetByHospitalIdAsync(int hospitalId);
    Task<IEnumerable<Laboratory>> GetByClinicIdAsync(int clinicId);
    Task<Laboratory?> GetByNameWithAssociationsAsync(string name);
}
