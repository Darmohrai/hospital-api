namespace hospital_api.DTOs.Reports;

public class AppointmentCountDto
{
    public string FilterDescription { get; set; } = string.Empty;

    public DateTime ReportDate { get; set; }

    public int PatientCount { get; set; }

    public List<DoctorAppointmentCountDto> CountByDoctor { get; set; } = new();
}

public class DoctorAppointmentCountDto
{
    public int DoctorId { get; set; }
    public string DoctorFullName { get; set; } = string.Empty;
    public int PatientCount { get; set; }
}