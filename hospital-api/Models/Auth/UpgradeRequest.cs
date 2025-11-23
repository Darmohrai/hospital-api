using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity; // Потрібно для зв'язку з User

namespace hospital_api.Models.Auth;

public class UpgradeRequest
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = string.Empty; // ID користувача (з AspNetUsers)
    public IdentityUser User { get; set; } = null!; // Навігаційна властивість

    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    
    public String RequestedRole { get; set; }
    
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
}

public enum RequestStatus
{
    Pending,  // Очікує розгляду
    Approved, // Затверджено
    Rejected  // Відхилено
}