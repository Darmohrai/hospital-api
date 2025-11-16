namespace hospital_api.DTOs.Clinic;

public class ClinicDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    
    public int? HospitalId { get; set; }
}