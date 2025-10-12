using System.ComponentModel.DataAnnotations;

namespace hospital_api.DTOs;

public class CreateClinicDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(250)]
    public string Address { get; set; } = string.Empty;

    public int? HospitalId { get; set; }
}