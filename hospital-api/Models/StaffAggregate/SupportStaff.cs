namespace hospital_api.Models.StaffAggregate;

public class SupportStaff : Staff
{
    public SupportRole Role { get; set; } = SupportRole.None;
}

public enum SupportRole
{
    None = 0,
    Nurse = 1,             // Медсестра
    Orderly = 2,           // Санітар
    Technician = 3,        // Технік
    LabAssistant = 4,      // Лаборант
    Receptionist = 5,      // Реєстратор
    Administrator = 6,     // Адміністратор
    Cleaner = 7,           // Прибиральник
    Porter = 8,             // Кур'єр/носильник
    Other = 9
}
