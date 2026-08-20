using StokTakip.Core.Entity;
using StokTakip.Data.Context;

namespace StokTakip.Data.Repository;

public sealed class ProductTypeRepository : BaseRepository, IProductTypeRepository
{
    private const string SelectSql = "SELECT Id, Ad AS Name, KodOnEki AS CodePrefix FROM lu_UrunTipi";

    public ProductTypeRepository(IDapperContext context) : base(context)
    {
    }

    public Task<IReadOnlyList<ProductType>> GetAllAsync()
        => QueryAsync<ProductType>(SelectSql + " ORDER BY Ad");

    public Task<ProductType?> GetByIdAsync(int id)
        => QueryFirstOrDefaultAsync<ProductType>(SelectSql + " WHERE Id = @Id", new { Id = id });

    public async Task<int> InsertAsync(ProductType productType)
    {
        const string sql = """
            INSERT INTO lu_UrunTipi (Ad, KodOnEki)
            VALUES (@Name, @CodePrefix);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;
        return await ExecuteScalarAsync<int>(sql, new { productType.Name, productType.CodePrefix });
    }

    public async Task<bool> UpdateAsync(ProductType productType)
    {
        const string sql = "UPDATE lu_UrunTipi SET Ad = @Name, KodOnEki = @CodePrefix WHERE Id = @Id";
        var affected = await ExecuteAsync(sql, new { productType.Id, productType.Name, productType.CodePrefix });
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM lu_UrunTipi WHERE Id = @Id";
        var affected = await ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM lu_UrunTipi
                WHERE LOWER(Ad) = LOWER(@Name)
                  AND (@ExcludeId IS NULL OR Id <> @ExcludeId)
            ) THEN 1 ELSE 0 END
            """;
        var exists = await ExecuteScalarAsync<int>(sql, new { Name = name, ExcludeId = excludeId });
        return exists == 1;
    }

    public async Task<bool> ExistsByCodePrefixAsync(string codePrefix, int? excludeId = null)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM lu_UrunTipi
                WHERE LOWER(KodOnEki) = LOWER(@CodePrefix)
                  AND (@ExcludeId IS NULL OR Id <> @ExcludeId)
            ) THEN 1 ELSE 0 END
            """;
        var exists = await ExecuteScalarAsync<int>(sql, new { CodePrefix = codePrefix, ExcludeId = excludeId });
        return exists == 1;
    }
}