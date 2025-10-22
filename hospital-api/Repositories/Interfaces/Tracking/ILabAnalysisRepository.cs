using hospital_api.Models.Tracking;

namespace hospital_api.Repositories.Interfaces.Tracking;

public interface ILabAnalysisRepository : IRepository<LabAnalysis>
{
    Task<IEnumerable<LabAnalysis>> GetAllWithAssociationsAsync();
}