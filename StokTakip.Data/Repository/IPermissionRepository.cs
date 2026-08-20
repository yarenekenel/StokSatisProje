using StokTakip.Core.Entity;

namespace StokTakip.Data.Repository;

public interface IPermissionRepository
{
    Task<IReadOnlyList<Permission>> GetAllAsync();
}