using StokTakip.Core.Dto;
using StokTakip.Core.Entity;
using StokTakip.Data.Context;

namespace StokTakip.Data.Repository;

public sealed class ProjectMachineRepository : BaseRepository, IProjectMachineRepository
{
    public ProjectMachineRepository(IDapperContext context) : base(context)
    {
    }

    public Task<ProjectMachine?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT Id,
                   ProjeId AS ProjectId,
                   MakinaId AS MachineId,
                   AtamaTarihi AS AssignedAt
            FROM tx_ProjeMakina
            WHERE Id = @Id
            """;
        return QueryFirstOrDefaultAsync<ProjectMachine>(sql, new { Id = id });
    }

    public Task<IReadOnlyList<ProjectMachineListDto>> GetListAsync(int? projectId = null)
    {
        const string sql = """
            SELECT pm.Id,
                   pm.ProjeId AS ProjectId,
                   p.ProjeKodu AS ProjectCode,
                   pm.MakinaId AS MachineId,
                   m.[kod] AS MachineCode,
                   m.[ad] AS MachineName,
                   pm.AtamaTarihi AS AssignedAt
            FROM tx_ProjeMakina pm
            JOIN dt_Proje p ON p.Id = pm.ProjeId
            JOIN dt_Makina m ON m.[id] = pm.MakinaId
            WHERE (@ProjectId IS NULL OR pm.ProjeId = @ProjectId)
            ORDER BY p.ProjeKodu, m.[kod]
            """;
        return QueryAsync<ProjectMachineListDto>(sql, new { ProjectId = projectId });
    }

    public Task<IReadOnlyList<ProjectMachineLookupDto>> GetLookupByProjectIdAsync(int projectId)
    {
        const string sql = """
            SELECT pm.Id,
                   pm.ProjeId AS ProjectId,
                   pm.MakinaId AS MachineId,
                   m.[kod] AS MachineCode,
                   m.[ad] AS MachineName
            FROM tx_ProjeMakina pm
            JOIN dt_Makina m ON m.[id] = pm.MakinaId
            WHERE pm.ProjeId = @ProjectId
              AND ISNULL(m.[durum], 0) = 1
            ORDER BY m.[kod]
            """;
        return QueryAsync<ProjectMachineLookupDto>(sql, new { ProjectId = projectId });
    }

    public async Task<int> InsertAsync(ProjectMachine projectMachine)
    {
        const string sql = """
            INSERT INTO tx_ProjeMakina (ProjeId, MakinaId, AtamaTarihi)
            VALUES (@ProjectId, @MachineId, SYSUTCDATETIME());
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;
        return await ExecuteScalarAsync<int>(sql, new { projectMachine.ProjectId, projectMachine.MachineId });
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM tx_ProjeMakina WHERE Id = @Id";
        var affected = await ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    public async Task<bool> ExistsAsync(int projectId, int machineId, int? excludeId = null)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM tx_ProjeMakina
                WHERE ProjeId = @ProjectId
                  AND MakinaId = @MachineId
                  AND (@ExcludeId IS NULL OR Id <> @ExcludeId)
            ) THEN 1 ELSE 0 END
            """;
        var exists = await ExecuteScalarAsync<int>(sql, new { ProjectId = projectId, MachineId = machineId, ExcludeId = excludeId });
        return exists == 1;
    }

    public async Task<bool> HasStockIssuesAsync(int projectMachineId)
    {
        const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM tx_StokCikis WHERE ProjeMakinaId = @Id) THEN 1 ELSE 0 END";
        var exists = await ExecuteScalarAsync<int>(sql, new { Id = projectMachineId });
        return exists == 1;
    }
}