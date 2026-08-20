namespace StokTakip.Core.Dto;

public sealed class StockLotLookupDto
{
    public long StockEntryId { get; set; }
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string? GoodsReceiptNo { get; set; }
    public string? InvoiceNo { get; set; }
    public DateTime Date { get; set; }
    public decimal RemainingQuantity { get; set; }
}