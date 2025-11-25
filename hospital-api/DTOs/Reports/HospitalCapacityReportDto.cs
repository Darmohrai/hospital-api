namespace hospital_api.DTOs.Reports;

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
    
    public int RoomCount { get; set; }
    
    public int BedCount { get; set; }
    
    public int FreeBedCount { get; set; }
    
    public int FullyFreeRoomCount { get; set; }
}