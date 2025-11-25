namespace hospital_api.DTOs.Reports;

public class DepartmentOccupancyReportDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;

    public int TotalRooms { get; set; }
    public int TotalCapacity { get; set; }
    public int TotalOccupiedBeds { get; set; }
    public double OverallOccupancyPercent { get; set; }
    
    public List<RoomOccupancyDto> Rooms { get; set; } = new();
}

public class RoomOccupancyDto
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    
    public int RoomCapacity { get; set; } 
    
    public int OccupiedBeds { get; set; } 
    
    public double OccupancyPercent { get; set; } 
}