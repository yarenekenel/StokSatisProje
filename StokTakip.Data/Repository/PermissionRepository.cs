using StokTakip.Core.Entity;
using StokTakip.Data.Context;

namespace StokTakip.Data.Repository;

public sealed class PermissionRepository : BaseRepository, IPermissionRepository
{
    public PermissionRepository(IDapperContext context) : base(context)
    {
    }

    public Task<IReadOnlyList<Permission>> GetAllAsync()
    {
        const string sql = """
            SELECT Id, IzinKodu AS Code, IzinAdi AS Name
            FROM lu_Izin
            ORDER BY Id
            """;
        return QueryAsync<Permission>(sql);
    }
}