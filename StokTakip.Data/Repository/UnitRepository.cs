using StokTakip.Core.Entity;
using StokTakip.Data.Context;

namespace StokTakip.Data.Repository;

public sealed class UnitRepository : BaseRepository, IUnitRepository
{
    private const string SelectSql = "SELECT Id, Ad AS Name FROM lu_Birim";

    public UnitRepository(IDapperContext context) : base(context)
    {
    }

    public Task<IReadOnlyList<Unit>> GetAllAsync()
        => QueryAsync<Unit>(SelectSql + " ORDER BY Ad");

    public Task<Unit?> GetByIdAsync(int id)
        => QueryFirstOrDefaultAsync<Unit>(SelectSql + " WHERE Id = @Id", new { Id = id });

    public async Task<int> InsertAsync(Unit unit)
    {
        const string sql = """
            INSERT INTO lu_Birim (Ad)
            VALUES (@Name);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;
        return await ExecuteScalarAsync<int>(sql, new { unit.Name });
    }

    public async Task<bool> UpdateAsync(Unit unit)
    {
        const string sql = "UPDATE lu_Birim SET Ad = @Name WHERE Id = @Id";
        var affected = await ExecuteAsync(sql, new { unit.Id, unit.Name });
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM lu_Birim WHERE Id = @Id";
        var affected = await ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM lu_Birim
                WHERE LOWER(Ad) = LOWER(@Name)
                  AND (@ExcludeId IS NULL OR Id <> @ExcludeId)
            ) THEN 1 ELSE 0 END
            """;
        var exists = await ExecuteScalarAsync<int>(sql, new { Name = name, ExcludeId = excludeId });
        return exists == 1;
    }
}