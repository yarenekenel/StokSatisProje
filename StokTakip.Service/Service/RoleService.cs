using StokTakip.Core.Dto;
using StokTakip.Core.Entity;
using StokTakip.Core.Result;
using StokTakip.Data.Repository;
using StokTakip.Service.Interface;

namespace StokTakip.Service.Service;

public sealed class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;

    public RoleService(IRoleRepository roleRepository, IPermissionRepository permissionRepository)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
    }

    public async Task<Result<IReadOnlyList<Role>>> GetAllAsync()
    {
        var roles = await _roleRepository.GetAllAsync();
        return Result.Success(roles);
    }

    public async Task<Result<IReadOnlyList<Permission>>> GetAllPermissionsAsync()
    {
        var permissions = await _permissionRepository.GetAllAsync();
        return Result.Success(permissions);
    }

    public async Task<Result<IReadOnlyList<RolePermissionDto>>> GetRolePermissionMatrixAsync()
    {
        var matrix = await _roleRepository.GetRolePermissionMatrixAsync();
        return Result.Success(matrix);
    }

    public async Task<Result<IReadOnlyList<int>>> GetPermissionIdsForRoleAsync(int roleId)
    {
        var ids = await _roleRepository.GetPermissionIdsForRoleAsync(roleId);
        return Result.Success(ids);
    }

    public async Task<Result<int>> CreateAsync(string name, IEnumerable<int> permissionIds)
    {
        name = (name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<int>(Error.Validation("Rol adı boş olamaz."));

        if (await _roleRepository.ExistsByNameAsync(name))
            return Result.Failure<int>(Error.Conflict("Bu isimde bir rol zaten mevcut."));

        var role = new Role { Name = name };
        var id = await _roleRepository.InsertAsync(role);

        await _roleRepository.SetRolePermissionsAsync(id, permissionIds);

        return Result.Success(id);
    }

    public async Task<Result> UpdateAsync(int id, string name, IEnumerable<int> permissionIds)
    {
        name = (name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("Rol adı boş olamaz."));

        var existing = await _roleRepository.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure(Error.NotFound("Rol bulunamadı."));

        if (await _roleRepository.ExistsByNameAsync(name, excludeId: id))
            return Result.Failure(Error.Conflict("Bu isimde bir rol zaten mevcut."));

        existing.Name = name;
        await _roleRepository.UpdateAsync(existing);
        await _roleRepository.SetRolePermissionsAsync(id, permissionIds);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var existing = await _roleRepository.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure(Error.NotFound("Rol bulunamadı."));

        if (await _roleRepository.HasUsersAsync(id))
            return Result.Failure(Error.Conflict("Bu role atanmış kullanıcılar var, silinemez."));

        await _roleRepository.DeleteAsync(id);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<string>>> GetPermissionCodesForUserAsync(int userId)
    {
        var codes = await _roleRepository.GetPermissionCodesForUserAsync(userId);
        return Result.Success(codes);
    }
}