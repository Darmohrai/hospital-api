using System;

namespace hospital_api.DTOs.Reports;

// DTO для Запиту №5
public class PatientOperationDto
{
    public int PatientId { get; set; }
    public string PatientFullName { get; set; } = string.Empty;
    public int OperationId { get; set; }
    public string OperationType { get; set; } = string.Empty;
    public DateTime OperationDate { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty; // Назва лікарні або клініки
}