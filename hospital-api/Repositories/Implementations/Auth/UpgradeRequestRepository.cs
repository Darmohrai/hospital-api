using hospital_api.Data;
using hospital_api.Models.Auth;
using hospital_api.Repositories.Interfaces.Auth;
using Microsoft.EntityFrameworkCore; // Потрібен для Include та Where
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hospital_api.Repositories.Implementations.Auth;

public class UpgradeRequestRepository : GenericRepository<UpgradeRequest>, IUpgradeRequestRepository
{
    public UpgradeRequestRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<UpgradeRequest>> GetPendingRequestsAsync()
    {
        return await _context.UpgradeRequests
            .Include(r => r.User) // Завантажуємо дані користувача
            .Where(r => r.Status == RequestStatus.Pending)
            .OrderBy(r => r.RequestDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<UpgradeRequest?> GetPendingRequestByUserIdAsync(string userId)
    {
        return await _context.UpgradeRequests
            .FirstOrDefaultAsync(r => r.UserId == userId && r.Status == RequestStatus.Pending);
    }
}