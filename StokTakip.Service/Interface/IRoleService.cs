using StokTakip.Core.Dto;
using StokTakip.Core.Entity;
using StokTakip.Core.Result;

namespace StokTakip.Service.Interface;

public interface IRoleService
{
    Task<Result<IReadOnlyList<Role>>> GetAllAsync();
    Task<Result<IReadOnlyList<Permission>>> GetAllPermissionsAsync();
    Task<Result<IReadOnlyList<RolePermissionDto>>> GetRolePermissionMatrixAsync();
    Task<Result<IReadOnlyList<int>>> GetPermissionIdsForRoleAsync(int roleId);
    Task<Result<int>> CreateAsync(string name, IEnumerable<int> permissionIds);
    Task<Result> UpdateAsync(int id, string name, IEnumerable<int> permissionIds);
    Task<Result> DeleteAsync(int id);
    Task<Result<IReadOnlyList<string>>> GetPermissionCodesForUserAsync(int userId);
}