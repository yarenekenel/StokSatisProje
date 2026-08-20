namespace StokTakip.Core.Dto;

public sealed class StockBalanceDto
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal TotalIn { get; set; }
    public decimal TotalOut { get; set; }
    public decimal RemainingStock { get; set; }
    public decimal? CriticalStockLevel { get; set; }
}