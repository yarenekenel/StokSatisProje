namespace StokTakip.Core.Dto;

public sealed class ProductStockChartDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal RemainingStock { get; set; }
}