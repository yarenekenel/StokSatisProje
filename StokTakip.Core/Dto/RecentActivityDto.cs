using StokTakip.Core.Enum;

namespace StokTakip.Core.Dto;

public sealed class RecentActivityDto
{
    public StockMovementType Type { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public DateTime Date { get; set; }
}