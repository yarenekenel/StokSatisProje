using StokTakip.Core.Constants;
using StokTakip.Core.Dto;
using StokTakip.Core.Entity;
using StokTakip.Data.Context;

namespace StokTakip.Data.Repository;

public sealed class ProductRepository : BaseRepository, IProductRepository
{
    private const string SelectSql = """
        SELECT Id,
               Kod AS Code,
               Ad AS Name,
               UrunTipiId AS ProductTypeId,
               BirimId AS UnitId,
               StokTipiId AS StockTypeId,
               MuhasebeStokKodu AS AccountingStockCode,
               KritikStokSeviyesi AS CriticalStockLevel,
               Aciklama AS Description,
               Aktif AS IsActive,
               OlusturmaTarihi AS CreatedAt
        FROM dt_Urun
        """;

    public ProductRepository(IDapperContext context) : base(context)
    {
    }

    public Task<IReadOnlyList<Product>> GetAllAsync(bool? isActive = null)
    {
        const string sql = SelectSql + " WHERE (@IsActive IS NULL OR Aktif = @IsActive) ORDER BY Kod";
        return QueryAsync<Product>(sql, new { IsActive = isActive });
    }

    public Task<Product?> GetByIdAsync(int id)
        => QueryFirstOrDefaultAsync<Product>(SelectSql + " WHERE Id = @Id", new { Id = id });

    public Task<IReadOnlyList<ProductListDto>> GetListAsync(bool? isActive = null)
    {
        const string sql = """
            SELECT u.Id,
                   u.Kod AS Code,
                   u.Ad AS Name,
                   t.Ad AS ProductTypeName,
                   b.Ad AS UnitName,
                   s.Ad AS StockTypeName,
                   u.MuhasebeStokKodu AS AccountingStockCode,
                   u.KritikStokSeviyesi AS CriticalStockLevel,
                   u.Aktif AS IsActive
            FROM dt_Urun u
            JOIN lu_UrunTipi t ON t.Id = u.UrunTipiId
            JOIN lu_Birim b ON b.Id = u.BirimId
            JOIN lu_StokTipi s ON s.Id = u.StokTipiId
            WHERE (@IsActive IS NULL OR u.Aktif = @IsActive)
            ORDER BY u.Kod
            """;
        return QueryAsync<ProductListDto>(sql, new { IsActive = isActive });
    }

    public Task<IReadOnlyList<ProductLookupDto>> GetLookupAsync()
    {
        const string sql = """
            SELECT u.Id,
                   u.Kod AS Code,
                   u.Ad AS Name,
                   ISNULL(v.Birim, b.Ad) AS UnitName,
                   ISNULL(v.ToplamGiris, 0) AS TotalIn,
                   ISNULL(v.ToplamCikis, 0) AS TotalOut,
                   ISNULL(v.KalanStok, 0) AS RemainingStock,
                   ISNULL(v.KritikStokSeviyesi, u.KritikStokSeviyesi) AS CriticalStockLevel
            FROM dt_Urun u
            JOIN lu_Birim b ON b.Id = u.BirimId
            LEFT JOIN vw_StokBakiye v ON v.UrunId = u.Id
            WHERE u.Aktif = 1
            ORDER BY u.Kod
            """;
        return QueryAsync<ProductLookupDto>(sql);
    }

    public async Task<int> InsertAsync(Product product)
    {
        const string sql = """
            INSERT INTO dt_Urun
            (Kod, Ad, UrunTipiId, BirimId, StokTipiId, MuhasebeStokKodu, KritikStokSeviyesi, Aciklama, Aktif)
            VALUES
            (@Code, @Name, @ProductTypeId, @UnitId, @StockTypeId, @AccountingStockCode, @CriticalStockLevel, @Description, @IsActive);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;
        return await ExecuteScalarAsync<int>(sql, new
        {
            product.Code,
            product.Name,
            product.ProductTypeId,
            product.UnitId,
            product.StockTypeId,
            product.AccountingStockCode,
            product.CriticalStockLevel,
            product.Description,
            product.IsActive
        });
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        const string sql = """
            UPDATE dt_Urun
            SET Ad = @Name,
                UrunTipiId = @ProductTypeId,
                BirimId = @UnitId,
                StokTipiId = @StockTypeId,
                MuhasebeStokKodu = @AccountingStockCode,
                KritikStokSeviyesi = @CriticalStockLevel,
                Aciklama = @Description,
                Aktif = @IsActive
            WHERE Id = @Id
            """;
        var affected = await ExecuteAsync(sql, new
        {
            product.Id,
            product.Name,
            product.ProductTypeId,
            product.UnitId,
            product.StockTypeId,
            product.AccountingStockCode,
            product.CriticalStockLevel,
            product.Description,
            product.IsActive
        });
        return affected > 0;
    }

    public async Task<bool> SetActiveAsync(int id, bool isActive)
    {
        const string sql = "UPDATE dt_Urun SET Aktif = @IsActive WHERE Id = @Id";
        var affected = await ExecuteAsync(sql, new { Id = id, IsActive = isActive });
        return affected > 0;
    }

    public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dt_Urun
                WHERE Kod = @Code
                  AND (@ExcludeId IS NULL OR Id <> @ExcludeId)
            ) THEN 1 ELSE 0 END
            """;
        var exists = await ExecuteScalarAsync<int>(sql, new { Code = code, ExcludeId = excludeId });
        return exists == 1;
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dt_Urun
                WHERE LOWER(Ad) = LOWER(@Name)
                  AND (@ExcludeId IS NULL OR Id <> @ExcludeId)
            ) THEN 1 ELSE 0 END
            """;
        var exists = await ExecuteScalarAsync<int>(sql, new { Name = name, ExcludeId = excludeId });
        return exists == 1;
    }

    public async Task<bool> HasStockMovementsAsync(int productId)
    {
        const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM tx_StokGiris WHERE UrunId = @Id) THEN 1 ELSE 0 END";
        var exists = await ExecuteScalarAsync<int>(sql, new { Id = productId });
        return exists == 1;
    }

    public async Task<int> GetMaxCodeSequenceAsync()
    {
        const string sql = """
            SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(Kod, 3, 20) AS INT)), 0)
            FROM dt_Urun
            WHERE Kod LIKE @Pattern
              AND LEN(Kod) = @CodeLength
              AND SUBSTRING(Kod, 3, 20) NOT LIKE '%[^0-9]%'
            """;
        return await ExecuteScalarAsync<int>(sql, new
        {
            Pattern = ProductCodeConstants.Prefix + "%",
            CodeLength = ProductCodeConstants.Prefix.Length + ProductCodeConstants.Digits
        });
    }
}