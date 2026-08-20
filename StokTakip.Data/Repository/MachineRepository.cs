using StokTakip.Core.Entity;
using StokTakip.Data.Context;


namespace StokTakip.Data.Repository;

public sealed class MachineRepository : BaseRepository, IMachineRepository
{
    // dt_Makina kolonları küçük harf: id/kod/ad/durum/tarih — köşeli parantez şart
    private const string SelectSql = """
        SELECT [id] AS Id,
               [kod] AS Code,
               [ad] AS Name,
               [durum] AS IsActive,
               [tarih] AS CreatedAt
        FROM dt_Makina
        """;

    public MachineRepository(IDapperContext context) : base(context)
    {
    }

    public Task<IReadOnlyList<Machine>> GetAllAsync(bool? isActive = null)
    {
        const string sql = SelectSql + " WHERE (@IsActive IS NULL OR [durum] = @IsActive) ORDER BY [kod]";
        return QueryAsync<Machine>(sql, new { IsActive = isActive });
    }

    public Task<Machine?> GetByIdAsync(int id)
        => QueryFirstOrDefaultAsync<Machine>(SelectSql + " WHERE [id] = @Id", new { Id = id });

    public async Task<int> InsertAsync(Machine machine)
    {
        const string sql = """
            INSERT INTO dt_Makina ([kod], [ad], [durum], [tarih])
            VALUES (@Code, @Name, @IsActive, SYSUTCDATETIME());
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;
        return await ExecuteScalarAsync<int>(sql, new { machine.Code, machine.Name, machine.IsActive });
    }

    public async Task<bool> UpdateAsync(Machine machine)
    {
        const string sql = """
            UPDATE dt_Makina
            SET [kod] = @Code,
                [ad] = @Name,
                [durum] = @IsActive
            WHERE [id] = @Id
            """;
        var affected = await ExecuteAsync(sql, new { machine.Id, machine.Code, machine.Name, machine.IsActive });
        return affected > 0;
    }

    public async Task<bool> SetActiveAsync(int id, bool isActive)
    {
        const string sql = "UPDATE dt_Makina SET [durum] = @IsActive WHERE [id] = @Id";
        var affected = await ExecuteAsync(sql, new { Id = id, IsActive = isActive });
        return affected > 0;
    }

    public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dt_Makina
                WHERE LOWER([kod]) = LOWER(@Code)
                  AND (@ExcludeId IS NULL OR [id] <> @ExcludeId)
            ) THEN 1 ELSE 0 END
            """;
        var exists = await ExecuteScalarAsync<int>(sql, new { Code = code, ExcludeId = excludeId });
        return exists == 1;
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dt_Makina
                WHERE LOWER([ad]) = LOWER(@Name)
                  AND (@ExcludeId IS NULL OR [id] <> @ExcludeId)
            ) THEN 1 ELSE 0 END
            """;
        var exists = await ExecuteScalarAsync<int>(sql, new { Name = name, ExcludeId = excludeId });
        return exists == 1;
    }

    public async Task<int> GetNextSequenceForProjectAsync(int projectId)
    {
        const string maxFromCodesSql = """
            SELECT ISNULL(MAX(TRY_CAST(RIGHT(m.[kod], 3) AS INT)), 0)
            FROM dt_Makina m
            WHERE CHARINDEX('/', m.[kod]) > 0
              AND LEN(m.[kod]) - CHARINDEX('/', m.[kod]) = 7
              AND SUBSTRING(m.[kod], CHARINDEX('/', m.[kod]) + 1, 4) = @ProjectPart
            """;

        const string assignmentCountSql = "SELECT COUNT(1) FROM tx_ProjeMakina WHERE ProjeId = @ProjectId";

        var projectPart = projectId.ToString("D4");
        var maxFromCodes = await ExecuteScalarAsync<int>(maxFromCodesSql, new { ProjectPart = projectPart });
        var assignmentCount = await ExecuteScalarAsync<int>(assignmentCountSql, new { ProjectId = projectId });

        return Math.Max(maxFromCodes, assignmentCount) + 1;
    }

    public async Task<bool> HasProjectLinksAsync(int machineId)
    {
        const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM tx_ProjeMakina WHERE MakinaId = @Id) THEN 1 ELSE 0 END";
        var exists = await ExecuteScalarAsync<int>(sql, new { Id = machineId });
        return exists == 1;
    }
}