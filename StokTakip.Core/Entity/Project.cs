namespace StokTakip.Core.Entity;

public sealed class Project
{
    public int Id { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public decimal? OfferAmount { get; set; }
    public int? CurrencyId { get; set; }
    public decimal? ExchangeRate { get; set; }
    public decimal? OfferAmountTry { get; init; }
    public DateTime? StartDate { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
}