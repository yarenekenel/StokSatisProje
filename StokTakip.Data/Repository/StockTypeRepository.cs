using StokTakip.Core.Entity;
using StokTakip.Data.Context;

namespace StokTakip.Data.Repository;

public sealed class StockTypeRepository : BaseRepository, IStockTypeRepository
{
    private const string SelectSql = "SELECT Id, Ad AS Name FROM lu_StokTipi";

    public StockTypeRepository(IDapperContext context) : base(context)
    {
    }

    public Task<IReadOnlyList<StockType>> GetAllAsync()
        => QueryAsync<StockType>(SelectSql + " ORDER BY Ad");

    public Task<StockType?> GetByIdAsync(int id)
        => QueryFirstOrDefaultAsync<StockType>(SelectSql + " WHERE Id = @Id", new { Id = id });

    public async Task<int> InsertAsync(StockType stockType)
    {
        const string sql = """
            INSERT INTO lu_StokTipi (Ad)
            VALUES (@Name);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;
        return await ExecuteScalarAsync<int>(sql, new { stockType.Name });
    }

    public async Task<bool> UpdateAsync(StockType stockType)
    {
        const string sql = "UPDATE lu_StokTipi SET Ad = @Name WHERE Id = @Id";
        var affected = await ExecuteAsync(sql, new { stockType.Id, stockType.Name });
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM lu_StokTipi WHERE Id = @Id";
        var affected = await ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM lu_StokTipi
                WHERE LOWER(Ad) = LOWER(@Name)
                  AND (@ExcludeId IS NULL OR Id <> @ExcludeId)
            ) THEN 1 ELSE 0 END
            """;
        var exists = await ExecuteScalarAsync<int>(sql, new { Name = name, ExcludeId = excludeId });
        return exists == 1;
    }
}