namespace StokTakip.Core.Dto;

public sealed class DashboardSummaryDto
{
    public int ActiveProductCount { get; set; }
    public int ActiveCompanyCount { get; set; }
    public int ActiveProjectCount { get; set; }
    public int ActiveMachineCount { get; set; }
    public decimal TotalRemainingStock { get; set; }
    public int BelowCriticalStockCount { get; set; }
}