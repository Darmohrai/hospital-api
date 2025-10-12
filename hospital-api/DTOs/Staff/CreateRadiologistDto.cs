using System.ComponentModel.DataAnnotations;
using hospital_api.Models.StaffAggregate;

namespace hospital_api.DTOs.Staff;

public class CreateRadiologistDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Range(0, 60)]
    public int WorkExperienceYears { get; set; }

    [Required]
    public AcademicDegree? AcademicDegree { get; set; }

    [Required]
    public AcademicTitle? AcademicTitle { get; set; }

    [Range(0, float.MaxValue)]
    public float HazardPayCoefficient { get; set; }

    [Range(0, int.MaxValue)]
    public int ExtendedVacationDays { get; set; }
}