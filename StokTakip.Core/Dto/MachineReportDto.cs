namespace StokTakip.Core.Dto;

public sealed class MachineConsumptionReportDto
{
    public string MachineCode { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}

public sealed class MachineCostReportDto
{
    public string MachineCode { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string ProjectCode { get; set; } = string.Empty;
    public decimal TotalCostTry { get; set; }
}

public sealed class ProjectMachineDistributionReportDto
{
    public string ProjectCode { get; set; } = string.Empty;
    public string MachineCode { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}

public sealed class MachineUsageSummaryReportDto
{
    public string MachineCode { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public int ProjectCount { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalCostTry { get; set; }
}