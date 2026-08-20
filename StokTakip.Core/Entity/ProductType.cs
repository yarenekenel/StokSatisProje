namespace StokTakip.Core.Entity;

public sealed class ProductType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CodePrefix { get; set; } = string.Empty;
}