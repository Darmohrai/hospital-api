namespace hospital_api.DTOs.Reports;

public class LaboratoryReportDto
{
    public string FilterDescription { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDaysInPeriod { get; set; }
    public int TotalAnalysesConducted { get; set; }
    public double AverageAnalysesPerDay { get; set; }

    public List<DailyLabCountDto> DailyCounts { get; set; } = new();
}

public class DailyLabCountDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}