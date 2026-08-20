namespace StokTakip.Core.Entity;

public sealed class Currency
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}