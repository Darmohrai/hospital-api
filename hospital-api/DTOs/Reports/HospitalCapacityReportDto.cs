using System.Collections.Generic;

namespace hospital_api.DTOs.Reports;

// DTO для Запиту №2
public class HospitalCapacityReportDto
{
    public int HospitalId { get; set; }
    public string HospitalName { get; set; } = string.Empty;
    public int TotalRoomCount { get; set; }
    public int TotalBedCount { get; set; }
    public List<DepartmentCapacityDto> Departments { get; set; } = new();
}

public class DepartmentCapacityDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    
    // Кількість палат у відділенні
    public int RoomCount { get; set; }
    
    // Кількість ліжок у відділенні
    public int BedCount { get; set; }
    
    // Кількість вільних ліжок у відділенні
    public int FreeBedCount { get; set; }
    
    // Кількість повністю вільних палат
    public int FullyFreeRoomCount { get; set; }
}