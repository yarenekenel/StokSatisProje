namespace StokTakip.Core.Dto;

public sealed class StockEntryListDto
{
    public long Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? InvoiceNo { get; set; }
    public string? GoodsReceiptNo { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal AmountTry { get; set; }
    public DateTime Date { get; set; }
}