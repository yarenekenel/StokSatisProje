namespace StokTakip.Core.Entity;

public sealed class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? RoleId { get; set; }
    public string? RoleName { get; set; }
}