namespace StokTakip.Core.Dto;

public sealed class CriticalStockDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal RemainingStock { get; set; }
    public decimal CriticalStockLevel { get; set; }
    public string UnitName { get; set; } = string.Empty;
}