using System.Collections.Generic;

namespace hospital_api.DTOs.Reports;

// DTO для Запиту №13
public class DepartmentOccupancyReportDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;

    // Загальна статистика по відділенню
    public int TotalRooms { get; set; }
    public int TotalCapacity { get; set; } // Сума місткості всіх палат
    public int TotalOccupiedBeds { get; set; }
    public double OverallOccupancyPercent { get; set; }
    
    // Деталізація по палатах
    public List<RoomOccupancyDto> Rooms { get; set; } = new();
}

public class RoomOccupancyDto
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    
    // Місткість палати (з моделі Room.Capacity)
    public int RoomCapacity { get; set; } 
    
    // Кількість зайнятих ліжок
    public int OccupiedBeds { get; set; } 
    
    // % завантаженості (OccupiedBeds / RoomCapacity)
    public double OccupancyPercent { get; set; } 
}