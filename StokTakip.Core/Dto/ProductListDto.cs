namespace StokTakip.Core.Dto;

public sealed class ProductListDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ProductTypeName { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string StockTypeName { get; set; } = string.Empty;
    public string? AccountingStockCode { get; set; }
    public decimal? CriticalStockLevel { get; set; }
    public bool IsActive { get; set; }
}