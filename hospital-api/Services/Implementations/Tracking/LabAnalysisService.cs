using hospital_api.DTOs.Tracking;
using hospital_api.Models.Tracking;
using hospital_api.Repositories.Interfaces.Tracking;
using hospital_api.Services.Interfaces.Tracking;

namespace hospital_api.Services.Implementations.Tracking;

public class LabAnalysisService : ILabAnalysisService
{
    private readonly ILabAnalysisRepository _labAnalysisRepo;

    public LabAnalysisService(ILabAnalysisRepository labAnalysisRepo)
    {
        _labAnalysisRepo = labAnalysisRepo;
    }

    public Task<LabAnalysis?> GetByIdAsync(int id) => _labAnalysisRepo.GetByIdAsync(id);
    public Task<IEnumerable<LabAnalysis>> GetAllAsync() => _labAnalysisRepo.GetAllAsync();

    public async Task<LabAnalysis> CreateAsync(CreateLabAnalysisDto dto)
    {
        var labAnalysis = new LabAnalysis
        {
            AnalysisDate = dto.AnalysisDate,
            AnalysisType = dto.AnalysisType,
            PatientId = dto.PatientId,
            LaboratoryId = dto.LaboratoryId,
            ResultSummary = dto.ResultSummary
        };

        await _labAnalysisRepo.AddAsync(labAnalysis);
        return labAnalysis;
    }

    public async Task<bool> UpdateAsync(int id, CreateLabAnalysisDto dto)
    {
        var existing = await _labAnalysisRepo.GetByIdAsync(id);
        if (existing == null)
            return false;

        existing.AnalysisDate = dto.AnalysisDate;
        existing.AnalysisType = dto.AnalysisType;
        existing.PatientId = dto.PatientId;
        existing.LaboratoryId = dto.LaboratoryId;
        existing.ResultSummary = dto.ResultSummary;

        await _labAnalysisRepo.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _labAnalysisRepo.GetByIdAsync(id);
        if (existing == null)
            return false;

        await _labAnalysisRepo.DeleteAsync(id);
        return true;
    }
}