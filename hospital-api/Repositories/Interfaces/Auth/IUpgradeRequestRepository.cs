using hospital_api.Models.Auth;

namespace hospital_api.Repositories.Interfaces.Auth;

public interface IUpgradeRequestRepository : IRepository<UpgradeRequest>
{
    // Можливо, знадобляться специфічні методи, наприклад,
    // отримання запитів зі статусом Pending
    Task<IEnumerable<UpgradeRequest>> GetPendingRequestsAsync();
    Task<UpgradeRequest?> GetPendingRequestByUserIdAsync(string userId);
}