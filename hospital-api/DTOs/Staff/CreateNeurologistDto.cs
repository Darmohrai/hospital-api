using System.ComponentModel.DataAnnotations;
using hospital_api.Models.StaffAggregate;

namespace hospital_api.DTOs.Staff;

public class CreateNeurologistDto
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Range(0, 50)]
    public int WorkExperienceYears { get; set; }

    [Required]
    public AcademicDegree? AcademicDegree { get; set; }
    
    [Required]
    public AcademicTitle? AcademicTitle { get; set; }
    
    [Range(0, 100)]
    public int ExtendedVacationDays { get; set; }
}