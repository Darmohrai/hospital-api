using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace hospital_api.Models.Auth;

public class UpgradeRequest
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = string.Empty;
    public IdentityUser User { get; set; } = null!;

    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    
    public String RequestedRole { get; set; }
    
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
}

public enum RequestStatus
{
    Pending,
    Approved,
    Rejected
}