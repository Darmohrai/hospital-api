using System.ComponentModel.DataAnnotations;
using hospital_api.Models.StaffAggregate;

namespace hospital_api.DTOs.Staff;

public class CreateSupportStaffDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Range(0, 60)]
    public int WorkExperienceYears { get; set; }

    [Required(ErrorMessage = "Role is required.")]
    public SupportRole Role { get; set; }
}