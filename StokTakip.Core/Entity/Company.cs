namespace StokTakip.Core.Entity;

public sealed class Company
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; } //zorunlu değil
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}