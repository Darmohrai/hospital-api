using hospital_api.Models.LaboratoryAggregate;

namespace hospital_api.Repositories.Interfaces;

public interface ILaboratoryRepository : IRepository<Laboratory>
{
    // Отримати лабораторії за певним профілем
    Task<IEnumerable<Laboratory>> GetByProfileAsync(string profile);

    // Отримати всі лабораторії, пов'язані з певною лікарнею
    Task<IEnumerable<Laboratory>> GetByHospitalIdAsync(int hospitalId);

    // Отримати всі лабораторії, пов'язані з певною клінікою
    Task<IEnumerable<Laboratory>> GetByClinicIdAsync(int clinicId);

    // Отримати лабораторію за назвою разом з усіма пов'язаними лікарнями та клініками
    Task<Laboratory?> GetByNameWithAssociationsAsync(string name);
}