namespace StokTakip.Core.Dto;

public sealed class StockTypeDistributionDto
{
    public string StockTypeName { get; set; } = string.Empty;
    public decimal TotalRemainingStock { get; set; }
}