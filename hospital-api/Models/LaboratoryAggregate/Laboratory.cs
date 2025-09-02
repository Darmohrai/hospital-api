using System.ComponentModel.DataAnnotations;
using hospital_api.Models.ClinicAggregate;
using hospital_api.Models.HospitalAggregate;

namespace hospital_api.Models.LaboratoryAggregate;

public class Laboratory
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    // Якщо лабораторія може мати декілька профілів, краще використати колекцію,
    // але якщо один - то string підходить.
    [Required]
    public List<String> Profile { get; set; } = new ();

    // Зв’язок багато-до-багатьох з лікарнями та клініками.
    // EF Core 5+ підтримує автоматичне створення таблиць зв’язку.
    public List<Hospital> Hospitals { get; set; } = new();
    public List<Clinic> Clinics { get; set; } = new();
}