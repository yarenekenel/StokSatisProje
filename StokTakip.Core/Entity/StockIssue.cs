namespace StokTakip.Core.Entity;

public sealed class StockIssue
{
    public long Id { get; set; }
    public int ProjectMachineId { get; set; }
    public long StockEntryId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal AmountTry { get; init; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
}

//NOT: proje.sql'de ProjeId yazsa da gerçek DB'de bu kolon yok — çıkış artık doğrudan projeye değil, ProjectMachineId (proje-makina atamasına) bağlanıyor, projeye erişim buradan dolaylı yapılıyor.