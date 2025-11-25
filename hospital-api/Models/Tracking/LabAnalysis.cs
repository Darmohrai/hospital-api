using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using hospital_api.Models.LaboratoryAggregate;
using hospital_api.Models.PatientAggregate;

namespace hospital_api.Models.Tracking;

public class LabAnalysis
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime AnalysisDate { get; set; }

    [Required]
    public string AnalysisType { get; set; } = string.Empty;

    [Required]
    [ForeignKey(nameof(Patient))]
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(Laboratory))]
    public int LaboratoryId { get; set; }
    public Laboratory Laboratory { get; set; } = null!;
    
    public string ResultSummary { get; set; } = string.Empty;
}