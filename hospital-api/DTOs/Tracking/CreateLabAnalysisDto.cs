using System.ComponentModel.DataAnnotations;

namespace hospital_api.DTOs.Tracking;

public class CreateLabAnalysisDto
{
    [Required]
    public DateTime AnalysisDate { get; set; }

    [Required]
    public string AnalysisType { get; set; } = string.Empty;

    [Required]
    public int PatientId { get; set; }

    [Required]
    public int LaboratoryId { get; set; }
    
    public string ResultSummary { get; set; } = string.Empty;
}