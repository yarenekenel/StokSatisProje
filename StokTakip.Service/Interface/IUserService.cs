using StokTakip.Core.Entity;
using StokTakip.Core.Result;

namespace StokTakip.Service.Interface;

public interface IUserService
{
    Task<Result<User>> LoginAsync(string username, string password);
    Task<Result<IReadOnlyList<User>>> GetAllAsync();
    Task<Result<User>> CreateAsync(string username, string password, string? fullName, int? roleId);
    Task<Result> UpdateAsync(int id, string? fullName, string? newPassword, int? roleId);
    Task<Result> SetActiveAsync(int id, bool isActive);
    Task<Result> DeleteAsync(int id);
}