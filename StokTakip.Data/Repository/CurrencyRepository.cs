using StokTakip.Core.Entity;
using StokTakip.Data.Context;

namespace StokTakip.Data.Repository;

public sealed class CurrencyRepository : BaseRepository, ICurrencyRepository
{
    private const string SelectSql = "SELECT Id, Kod AS Code, Ad AS Name FROM lu_ParaBirimi";

    public CurrencyRepository(IDapperContext context) : base(context)
    {
    }

    public Task<IReadOnlyList<Currency>> GetAllAsync()
        => QueryAsync<Currency>(SelectSql + " ORDER BY Kod");

    public Task<Currency?> GetByIdAsync(int id)
        => QueryFirstOrDefaultAsync<Currency>(SelectSql + " WHERE Id = @Id", new { Id = id });

    public async Task<int> InsertAsync(Currency currency)
    {
        const string sql = """
            INSERT INTO lu_ParaBirimi (Kod, Ad)
            VALUES (@Code, @Name);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;
        return await ExecuteScalarAsync<int>(sql, new { currency.Code, currency.Name });
    }

    public async Task<bool> UpdateAsync(Currency currency)
    {
        const string sql = "UPDATE lu_ParaBirimi SET Kod = @Code, Ad = @Name WHERE Id = @Id";
        var affected = await ExecuteAsync(sql, new { currency.Id, currency.Code, currency.Name });
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM lu_ParaBirimi WHERE Id = @Id";
        var affected = await ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM lu_ParaBirimi
                WHERE LOWER(Kod) = LOWER(@Code)
                  AND (@ExcludeId IS NULL OR Id <> @ExcludeId)
            ) THEN 1 ELSE 0 END
            """;
        var exists = await ExecuteScalarAsync<int>(sql, new { Code = code, ExcludeId = excludeId });
        return exists == 1;
    }
}