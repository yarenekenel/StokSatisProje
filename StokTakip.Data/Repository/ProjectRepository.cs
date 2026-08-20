using StokTakip.Core.Constants;
using StokTakip.Core.Dto;
using StokTakip.Core.Entity;
using StokTakip.Data.Context;

namespace StokTakip.Data.Repository;

public sealed class ProjectRepository : BaseRepository, IProjectRepository
{
    private const string SelectSql = """
        SELECT Id,
               ProjeKodu AS ProjectCode,
               FirmaId AS CompanyId,
               TeklifTutari AS OfferAmount,
               ParaBirimiId AS CurrencyId,
               Kur AS ExchangeRate,
               TeklifTutariTL AS OfferAmountTry,
               BaslangicTarihi AS StartDate,
               Aktif AS IsActive,
               Aciklama AS Description
        FROM dt_Proje
        """;

    public ProjectRepository(IDapperContext context) : base(context)
    {
    }

    public Task<IReadOnlyList<Project>> GetAllAsync(bool? isActive = null)
    {
        const string sql = SelectSql + " WHERE (@IsActive IS NULL OR Aktif = @IsActive) ORDER BY ProjeKodu";
        return QueryAsync<Project>(sql, new { IsActive = isActive });
    }

    public Task<Project?> GetByIdAsync(int id)
        => QueryFirstOrDefaultAsync<Project>(SelectSql + " WHERE Id = @Id", new { Id = id });

    public Task<IReadOnlyList<ProjectListDto>> GetListAsync(bool? isActive = null)
    {
        const string sql = """
            SELECT p.Id,
                   p.ProjeKodu AS ProjectCode,
                   f.FirmaAdi AS CompanyName,
                   p.TeklifTutari AS OfferAmount,
                   c.Kod AS CurrencyCode,
                   p.TeklifTutariTL AS OfferAmountTry,
                   p.BaslangicTarihi AS StartDate,
                   p.Aktif AS IsActive
            FROM dt_Proje p
            JOIN lu_Firma f ON f.Id = p.FirmaId
            LEFT JOIN lu_ParaBirimi c ON c.Id = p.ParaBirimiId
            WHERE (@IsActive IS NULL OR p.Aktif = @IsActive)
            ORDER BY p.ProjeKodu
            """;
        return QueryAsync<ProjectListDto>(sql, new { IsActive = isActive });
    }

    public async Task<int> InsertAsync(Project project)
    {
        const string sql = """
            INSERT INTO dt_Proje
            (ProjeKodu, FirmaId, TeklifTutari, ParaBirimiId, Kur, BaslangicTarihi, Aktif, Aciklama)
            VALUES
            (@ProjectCode, @CompanyId, @OfferAmount, @CurrencyId, @ExchangeRate, @StartDate, @IsActive, @Description);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;
        return await ExecuteScalarAsync<int>(sql, new
        {
            project.ProjectCode,
            project.CompanyId,
            project.OfferAmount,
            project.CurrencyId,
            project.ExchangeRate,
            project.StartDate,
            project.IsActive,
            project.Description
        });
    }

    public async Task<bool> UpdateAsync(Project project)
    {
        const string sql = """
            UPDATE dt_Proje
            SET FirmaId = @CompanyId,
                TeklifTutari = @OfferAmount,
                ParaBirimiId = @CurrencyId,
                Kur = @ExchangeRate,
                BaslangicTarihi = @StartDate,
                Aktif = @IsActive,
                Aciklama = @Description
            WHERE Id = @Id
            """;
        var affected = await ExecuteAsync(sql, new
        {
            project.Id,
            project.CompanyId,
            project.OfferAmount,
            project.CurrencyId,
            project.ExchangeRate,
            project.StartDate,
            project.IsActive,
            project.Description
        });
        return affected > 0;
    }

    public async Task<bool> SetActiveAsync(int id, bool isActive)
    {
        const string sql = "UPDATE dt_Proje SET Aktif = @IsActive WHERE Id = @Id";
        var affected = await ExecuteAsync(sql, new { Id = id, IsActive = isActive });
        return affected > 0;
    }

    public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dt_Proje
                WHERE ProjeKodu = @Code
                  AND (@ExcludeId IS NULL OR Id <> @ExcludeId)
            ) THEN 1 ELSE 0 END
            """;
        var exists = await ExecuteScalarAsync<int>(sql, new { Code = code, ExcludeId = excludeId });
        return exists == 1;
    }

    public async Task<int> GetMaxCodeSequenceAsync()
    {
        const string sql = """
            SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(ProjeKodu, 3, 20) AS INT)), 0)
            FROM dt_Proje
            WHERE ProjeKodu LIKE @Pattern
              AND LEN(ProjeKodu) = @CodeLength
              AND SUBSTRING(ProjeKodu, 3, 20) NOT LIKE '%[^0-9]%'
            """;
        return await ExecuteScalarAsync<int>(sql, new
        {
            Pattern = ProjectCodeConstants.Prefix + "%",
            CodeLength = ProjectCodeConstants.Prefix.Length + ProjectCodeConstants.Digits
        });
    }

    public async Task<bool> HasStockIssuesAsync(int projectId)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1
                FROM tx_StokCikis c
                JOIN tx_ProjeMakina pm ON pm.Id = c.ProjeMakinaId
                WHERE pm.ProjeId = @Id
            ) THEN 1 ELSE 0 END
            """;
        var exists = await ExecuteScalarAsync<int>(sql, new { Id = projectId });
        return exists == 1;
    }
}