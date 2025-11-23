using System.ComponentModel.DataAnnotations;
using hospital_api.Models.ClinicAggregate;
using hospital_api.Models.PatientAggregate;
using hospital_api.Models.StaffAggregate;

namespace hospital_api.Models.HospitalAggregate;

public class Hospital
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;
    
    public List<Building> Buildings { get; set; } = new();
    
    public List<Department> Departments { get; set; } = new();

    public List<Employment> Employments { get; set; } = new();

    public List<Patient> Patients { get; set; } = new();

    public List<Clinic> Clinics { get; set; } = new();
    
    public List<HospitalSpecialization> Specializations { get; set; } = new();

}

public enum HospitalSpecialization
{
    Surgeon,        // Хірург
    Neurologist,    // Невролог
    Ophthalmologist,// Окуліст
    Dentist,        // Стоматолог
    Radiologist,    // Рентгенолог
    Gynecologist,   // Гінеколог
    Cardiologist,   // Кардіолог
}
