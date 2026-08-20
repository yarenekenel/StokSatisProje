namespace StokTakip.Core.Dto;

public sealed class StockEntryOpenLotDto
{
    public long Id { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal AmountTry { get; set; }
    public decimal RemainingQuantity { get; set; }
    public DateTime Date { get; set; }
    public string? InvoiceNo { get; set; }
    public string? GoodsReceiptNo { get; set; }
}