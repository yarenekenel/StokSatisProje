namespace StokTakip.Core.Entity;

public sealed class StockEntry
{
    public long Id { get; set; }
    public int ProductId { get; set; }
    public int? CompanyId { get; set; }
    public string? InvoiceNo { get; set; }
    public string? GoodsReceiptNo { get; set; }
    public string? ReceivedBy { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int CurrencyId { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal AmountForeign { get; init; }
    public decimal AmountTry { get; init; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
}


//NOT: TutarDoviz ve TutarTL kolonları SQL'de PERSISTED (yani veritabanı tarafından otomatik hesaplanıyor) olduğu için, C# tarafında bunlara asla değer yazmıyoruz init anahtar kelimesiyle "sadece okunabilir" işaretleyip yalnızca SELECT ile dolduruyoruz.