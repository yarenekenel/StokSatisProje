using StokTakip.Core.Dto;
using StokTakip.Core.Entity;

namespace StokTakip.Data.Repository;

public interface IRoleRepository
{
    Task<IReadOnlyList<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(int id);
    Task<int> InsertAsync(Role role);
    Task<bool> UpdateAsync(Role role);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<IReadOnlyList<RolePermissionDto>> GetRolePermissionMatrixAsync();
    Task<IReadOnlyList<int>> GetPermissionIdsForRoleAsync(int roleId);
    Task SetRolePermissionsAsync(int roleId, IEnumerable<int> permissionIds);
    Task<bool> HasUsersAsync(int roleId);
    Task<IReadOnlyList<string>> GetPermissionCodesForUserAsync(int userId);
}