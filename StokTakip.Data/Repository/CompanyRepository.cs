using StokTakip.Core.Entity;
using StokTakip.Data.Context;

namespace StokTakip.Data.Repository;

public sealed class CompanyRepository : BaseRepository, ICompanyRepository
{
    private const string SelectSql = """
        SELECT Id,
               FirmaAdi AS CompanyName,
               VergiNo AS TaxNumber,
               Telefon AS Phone,
               Email,
               Adres AS Address,
               Aktif AS IsActive,
               OlusturmaTarihi AS CreatedAt
        FROM lu_Firma
        """;

    public CompanyRepository(IDapperContext context) : base(context)
    {
    }

    public Task<IReadOnlyList<Company>> GetAllAsync(bool? isActive = null)
    {
        const string sql = SelectSql + " WHERE (@IsActive IS NULL OR Aktif = @IsActive) ORDER BY FirmaAdi";
        return QueryAsync<Company>(sql, new { IsActive = isActive });
    }

    public Task<Company?> GetByIdAsync(int id)
        => QueryFirstOrDefaultAsync<Company>(SelectSql + " WHERE Id = @Id", new { Id = id });

    public async Task<int> InsertAsync(Company company)
    {
        const string sql = """
            INSERT INTO lu_Firma (FirmaAdi, VergiNo, Telefon, Email, Adres, Aktif)
            VALUES (@CompanyName, @TaxNumber, @Phone, @Email, @Address, @IsActive);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;
        return await ExecuteScalarAsync<int>(sql, new
        {
            company.CompanyName,
            company.TaxNumber,
            company.Phone,
            company.Email,
            company.Address,
            company.IsActive
        });
    }

    public async Task<bool> UpdateAsync(Company company)
    {
        const string sql = """
            UPDATE lu_Firma
            SET FirmaAdi = @CompanyName,
                VergiNo = @TaxNumber,
                Telefon = @Phone,
                Email = @Email,
                Adres = @Address,
                Aktif = @IsActive
            WHERE Id = @Id
            """;
        var affected = await ExecuteAsync(sql, new
        {
            company.Id,
            company.CompanyName,
            company.TaxNumber,
            company.Phone,
            company.Email,
            company.Address,
            company.IsActive
        });
        return affected > 0;
    }

    public async Task<bool> SetActiveAsync(int id, bool isActive)
    {
        const string sql = "UPDATE lu_Firma SET Aktif = @IsActive WHERE Id = @Id";
        var affected = await ExecuteAsync(sql, new { Id = id, IsActive = isActive });
        return affected > 0;
    }

    public async Task<bool> ExistsByTaxNumberAsync(string taxNumber, int? excludeId = null)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM lu_Firma
                WHERE VergiNo = @TaxNumber
                  AND (@ExcludeId IS NULL OR Id <> @ExcludeId)
            ) THEN 1 ELSE 0 END
            """;
        var exists = await ExecuteScalarAsync<int>(sql, new { TaxNumber = taxNumber, ExcludeId = excludeId });
        return exists == 1;
    }
}