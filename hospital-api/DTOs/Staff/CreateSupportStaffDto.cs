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
    
    public int? HospitalId { get; set; }
    public int? ClinicId { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (HospitalId.HasValue && ClinicId.HasValue)
        {
            yield return new ValidationResult(
                "Staff cannot be assigned to both a Hospital and a Clinic at the same time.",
                new[] { nameof(HospitalId), nameof(ClinicId) });
        }
    }
}