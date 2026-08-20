namespace StokTakip.Core.Dto;

public sealed class ProjectListDto
{
    public int Id { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public decimal? OfferAmount { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? OfferAmountTry { get; set; }
    public DateTime? StartDate { get; set; }
    public bool IsActive { get; set; }
}