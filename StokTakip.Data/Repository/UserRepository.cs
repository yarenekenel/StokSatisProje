using StokTakip.Core.Entity;
using StokTakip.Data.Context;

namespace StokTakip.Data.Repository;

public sealed class UserRepository : BaseRepository, IUserRepository
{
    private const string SelectSql = """
    SELECT k.Id,
           k.KullaniciAdi AS Username,
           k.SifreHash AS PasswordHash,
           k.AdSoyad AS FullName,
           k.Aktif AS IsActive,
           k.OlusturmaTarihi AS CreatedAt,
           k.RolId AS RoleId,
           r.RolAdi AS RoleName
    FROM lu_Kullanici k
    LEFT JOIN lu_Rol r ON r.Id = k.RolId
    """;

    public UserRepository(IDapperContext context) : base(context)
    {
    }

    public Task<User?> GetByUsernameAsync(string username)
       => QueryFirstOrDefaultAsync<User>(SelectSql + " WHERE k.KullaniciAdi = @Username", new { Username = username });
    public Task<User?> GetByIdAsync(int id)
        => QueryFirstOrDefaultAsync<User>(SelectSql + " WHERE k.Id = @Id", new { Id = id });

    public Task<IReadOnlyList<User>> GetAllAsync()
        => QueryAsync<User>(SelectSql + " ORDER BY k.KullaniciAdi");

    public async Task<int> InsertAsync(User user)
    {
        const string sql = """
        INSERT INTO lu_Kullanici (KullaniciAdi, SifreHash, AdSoyad, Aktif, OlusturmaTarihi, RolId)
        VALUES (@Username, @PasswordHash, @FullName, @IsActive, SYSUTCDATETIME(), @RoleId);
        SELECT CAST(SCOPE_IDENTITY() AS INT);
        """;
        return await ExecuteScalarAsync<int>(sql, new { user.Username, user.PasswordHash, user.FullName, user.IsActive, user.RoleId });
    }

    public async Task<bool> UpdateAsync(User user)
    {
        const string sql = """
        UPDATE lu_Kullanici
        SET AdSoyad = @FullName,
            RolId = @RoleId
        WHERE Id = @Id
        """;
        var affected = await ExecuteAsync(sql, new { user.Id, user.FullName, user.RoleId });
        return affected > 0;
    }

    public async Task<bool> UpdatePasswordAsync(int id, string passwordHash)
    {
        const string sql = "UPDATE lu_Kullanici SET SifreHash = @PasswordHash WHERE Id = @Id";
        var affected = await ExecuteAsync(sql, new { Id = id, PasswordHash = passwordHash });
        return affected > 0;
    }

    public async Task<bool> SetActiveAsync(int id, bool isActive)
    {
        const string sql = "UPDATE lu_Kullanici SET Aktif = @IsActive WHERE Id = @Id";
        var affected = await ExecuteAsync(sql, new { Id = id, IsActive = isActive });
        return affected > 0;
    }

    public async Task<bool> ExistsByUsernameAsync(string username, int? excludeId = null)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM lu_Kullanici
                WHERE LOWER(KullaniciAdi) = LOWER(@Username)
                  AND (@ExcludeId IS NULL OR Id <> @ExcludeId)
            ) THEN 1 ELSE 0 END
            """;
        var exists = await ExecuteScalarAsync<int>(sql, new { Username = username, ExcludeId = excludeId });
        return exists == 1;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM lu_Kullanici WHERE Id = @Id";
        var affected = await ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }
}