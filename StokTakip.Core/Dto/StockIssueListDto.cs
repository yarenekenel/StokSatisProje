namespace StokTakip.Core.Dto;

public sealed class StockIssueListDto
{
    public long Id { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string MachineCode { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public long StockEntryId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal AmountTry { get; set; }
    public DateTime Date { get; set; }
    public string? InvoiceNo { get; set; }
    public string? GoodsReceiptNo { get; set; }
}