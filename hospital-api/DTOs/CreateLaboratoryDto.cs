using System.ComponentModel.DataAnnotations;

namespace hospital_api.DTOs;

public class CreateLaboratoryDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
        
    public List<string> Profile { get; set; } = new();
    
    public List<int> HospitalIds { get; set; } = new();
    public List<int> ClinicIds { get; set; } = new();
}