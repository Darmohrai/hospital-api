using hospital_api.DTOs.Tracking;
using hospital_api.Models.Tracking;

namespace hospital_api.Services.Interfaces.Tracking;

public interface ILabAnalysisService
{
    Task<LabAnalysis?> GetByIdAsync(int id);
    Task<IEnumerable<LabAnalysis>> GetAllAsync();
    Task<LabAnalysis> CreateAsync(CreateLabAnalysisDto dto);
    Task<bool> UpdateAsync(int id, CreateLabAnalysisDto dto);
    Task<bool> DeleteAsync(int id);
}