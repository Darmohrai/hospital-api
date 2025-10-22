using hospital_api.Data;
using hospital_api.Models.Tracking;
using hospital_api.Repositories.Interfaces.Tracking;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations.Tracking;

public class LabAnalysisRepository : GenericRepository<LabAnalysis>, ILabAnalysisRepository
{
    public LabAnalysisRepository(ApplicationDbContext context) : base(context)
    {
    }
    
    public async Task<IEnumerable<LabAnalysis>> GetAllWithAssociationsAsync()
    {
        return await _context.LabAnalyses
            .Include(a => a.Laboratory)
            .AsNoTracking()
            .ToListAsync();
    }
}