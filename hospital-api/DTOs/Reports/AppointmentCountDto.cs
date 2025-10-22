namespace hospital_api.DTOs.Reports;

public class AppointmentCountDto
{
    // Опис фільтра (напр., "Лікар: Іванов І.І." або "Поліклініка №5, Хірурги")
    public string FilterDescription { get; set; } = string.Empty;

    public DateTime ReportDate { get; set; }

    public int PatientCount { get; set; }

    // (Опціонально) Можна додати список по лікарях, якщо запит "для всіх"
    public List<DoctorAppointmentCountDto> CountByDoctor { get; set; } = new();
}

public class DoctorAppointmentCountDto
{
    public int DoctorId { get; set; }
    public string DoctorFullName { get; set; } = string.Empty;
    public int PatientCount { get; set; }
}