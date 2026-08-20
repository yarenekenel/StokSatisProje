using StokTakip.Core.Entity;

namespace StokTakip.Data.Repository;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByIdAsync(int id);
    Task<IReadOnlyList<User>> GetAllAsync();
    Task<int> InsertAsync(User user);
    Task<bool> UpdateAsync(User user);
    Task<bool> UpdatePasswordAsync(int id, string passwordHash);
    Task<bool> SetActiveAsync(int id, bool isActive);
    Task<bool> ExistsByUsernameAsync(string username, int? excludeId = null);
    Task<bool> DeleteAsync(int id);
}