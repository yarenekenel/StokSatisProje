using StokTakip.Core.Dto;
using StokTakip.Core.Entity;
using StokTakip.Data.Context;

namespace StokTakip.Data.Repository;

public sealed class RoleRepository : BaseRepository, IRoleRepository
{
    public RoleRepository(IDapperContext context) : base(context)
    {
    }

    public Task<IReadOnlyList<Role>> GetAllAsync()
    {
        const string sql = "SELECT Id, RolAdi AS Name FROM lu_Rol ORDER BY RolAdi";
        return QueryAsync<Role>(sql);
    }

    public Task<Role?> GetByIdAsync(int id)
        => QueryFirstOrDefaultAsync<Role>("SELECT Id, RolAdi AS Name FROM lu_Rol WHERE Id = @Id", new { Id = id });

    public async Task<int> InsertAsync(Role role)
    {
        const string sql = """
            INSERT INTO lu_Rol (RolAdi)
            VALUES (@Name);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;
        return await ExecuteScalarAsync<int>(sql, new { role.Name });
    }

    public async Task<bool> UpdateAsync(Role role)
    {
        const string sql = "UPDATE lu_Rol SET RolAdi = @Name WHERE Id = @Id";
        var affected = await ExecuteAsync(sql, new { role.Id, role.Name });
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM lu_Rol WHERE Id = @Id";
        var affected = await ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM lu_Rol
                WHERE LOWER(RolAdi) = LOWER(@Name)
                  AND (@ExcludeId IS NULL OR Id <> @ExcludeId)
            ) THEN 1 ELSE 0 END
            """;
        var exists = await ExecuteScalarAsync<int>(sql, new { Name = name, ExcludeId = excludeId });
        return exists == 1;
    }

    public Task<IReadOnlyList<RolePermissionDto>> GetRolePermissionMatrixAsync()
    {
        const string sql = """
            SELECT r.Id AS RoleId,
                   r.RolAdi AS RoleName,
                   i.Id AS PermissionId,
                   i.IzinKodu AS PermissionCode,
                   i.IzinAdi AS PermissionName,
                   CASE WHEN ri.RolId IS NULL THEN 0 ELSE 1 END AS IsGranted
            FROM lu_Rol r
            CROSS JOIN lu_Izin i
            LEFT JOIN tx_RolIzin ri ON ri.RolId = r.Id AND ri.IzinId = i.Id
            ORDER BY r.RolAdi, i.Id
            """;
        return QueryAsync<RolePermissionDto>(sql);
    }

    public Task<IReadOnlyList<int>> GetPermissionIdsForRoleAsync(int roleId)
    {
        const string sql = "SELECT IzinId FROM tx_RolIzin WHERE RolId = @RoleId";
        return QueryAsync<int>(sql, new { RoleId = roleId });
    }

    public async Task SetRolePermissionsAsync(int roleId, IEnumerable<int> permissionIds)
    {
        await ExecuteAsync("DELETE FROM tx_RolIzin WHERE RolId = @RoleId", new { RoleId = roleId });

        foreach (var permissionId in permissionIds)
        {
            await ExecuteAsync(
                "INSERT INTO tx_RolIzin (RolId, IzinId) VALUES (@RoleId, @PermissionId)",
                new { RoleId = roleId, PermissionId = permissionId });
        }
    }

    public async Task<bool> HasUsersAsync(int roleId)
    {
        const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM lu_Kullanici WHERE RolId = @RoleId) THEN 1 ELSE 0 END";
        var exists = await ExecuteScalarAsync<int>(sql, new { RoleId = roleId });
        return exists == 1;
    }

    public Task<IReadOnlyList<string>> GetPermissionCodesForUserAsync(int userId)
    {
        const string sql = """
            SELECT i.IzinKodu
            FROM lu_Kullanici k
            JOIN tx_RolIzin ri ON ri.RolId = k.RolId
            JOIN lu_Izin i ON i.Id = ri.IzinId
            WHERE k.Id = @UserId
            """;
        return QueryAsync<string>(sql, new { UserId = userId });
    }
}