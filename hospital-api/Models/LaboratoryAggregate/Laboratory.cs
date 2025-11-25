using System.ComponentModel.DataAnnotations;
using hospital_api.Models.ClinicAggregate;
using hospital_api.Models.HospitalAggregate;

namespace hospital_api.Models.LaboratoryAggregate;

public class Laboratory
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public List<String> Profile { get; set; } = new ();

    public List<Hospital> Hospitals { get; set; } = new();
    public List<Clinic> Clinics { get; set; } = new();
}