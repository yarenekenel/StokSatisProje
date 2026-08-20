namespace StokTakip.Core.Dto;

public sealed class ProductLookupDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public decimal TotalIn { get; set; }
    public decimal TotalOut { get; set; }
    public decimal RemainingStock { get; set; }
    public decimal? CriticalStockLevel { get; set; }
}