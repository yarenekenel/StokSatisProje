namespace StokTakip.Core.Entity;

public sealed class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int ProductTypeId { get; set; }
    public int UnitId { get; set; }
    public int StockTypeId { get; set; }
    public string? AccountingStockCode { get; set; }
    public decimal? CriticalStockLevel { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}