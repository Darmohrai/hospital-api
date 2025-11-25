using hospital_api.Models.Auth;

namespace hospital_api.Repositories.Interfaces.Auth;

public interface IUpgradeRequestRepository : IRepository<UpgradeRequest>
{
    Task<IEnumerable<UpgradeRequest>> GetPendingRequestsAsync();
    Task<UpgradeRequest?> GetPendingRequestByUserIdAsync(string userId);
}