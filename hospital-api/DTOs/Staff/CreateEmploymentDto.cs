using System.ComponentModel.DataAnnotations;

namespace hospital_api.DTOs.Staff;

public class CreateEmploymentDto
{
    [Required]
    public int StaffId { get; set; }

    public int? HospitalId { get; set; }
    public int? ClinicId { get; set; }
}