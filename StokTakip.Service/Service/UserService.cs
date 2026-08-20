using StokTakip.Core.Entity;
using StokTakip.Core.Result;
using StokTakip.Data.Repository;
using StokTakip.Service.Interface;

namespace StokTakip.Service.Service;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<User>> LoginAsync(string username, string password)
    {
        username = (username ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return Result.Failure<User>(Error.Validation("Kullanıcı adı ve şifre boş olamaz."));

        var user = await _repository.GetByUsernameAsync(username);
        if (user is null || !user.IsActive)
            return Result.Failure<User>(Error.Validation("Kullanıcı adı veya şifre hatalı."));

        var passwordMatches = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (!passwordMatches)
            return Result.Failure<User>(Error.Validation("Kullanıcı adı veya şifre hatalı."));

        return Result.Success(user);
    }

    public async Task<Result<IReadOnlyList<User>>> GetAllAsync()
    {
        var users = await _repository.GetAllAsync();
        return Result.Success(users);
    }

    public async Task<Result<User>> CreateAsync(string username, string password, string? fullName)
    {
        username = (username ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(username))
            return Result.Failure<User>(Error.Validation("Kullanıcı adı boş olamaz."));

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return Result.Failure<User>(Error.Validation("Şifre en az 6 karakter olmalıdır."));

        if (await _repository.ExistsByUsernameAsync(username))
            return Result.Failure<User>(Error.Conflict("Bu kullanıcı adı zaten kullanılıyor."));

        var user = new User
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim(),
            IsActive = true
        };

        user.Id = await _repository.InsertAsync(user);
        return Result.Success(user);
    }

    public async Task<Result> UpdateAsync(int id, string? fullName, string? newPassword)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure(Error.NotFound("Kullanıcı bulunamadı."));

        existing.FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim();
        await _repository.UpdateAsync(existing);

        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            if (newPassword.Length < 6)
                return Result.Failure(Error.Validation("Şifre en az 6 karakter olmalıdır."));

            var hash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _repository.UpdatePasswordAsync(id, hash);
        }

        return Result.Success();
    }

    public async Task<Result> SetActiveAsync(int id, bool isActive)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure(Error.NotFound("Kullanıcı bulunamadı."));

        await _repository.SetActiveAsync(id, isActive);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure(Error.NotFound("Kullanıcı bulunamadı."));

        await _repository.DeleteAsync(id);
        return Result.Success();
    }

    public async Task<Result<User>> CreateAsync(string username, string password, string? fullName, int? roleId)
    {
        username = (username ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(username))
            return Result.Failure<User>(Error.Validation("Kullanıcı adı boş olamaz."));

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return Result.Failure<User>(Error.Validation("Şifre en az 6 karakter olmalıdır."));

        if (await _repository.ExistsByUsernameAsync(username))
            return Result.Failure<User>(Error.Conflict("Bu kullanıcı adı zaten kullanılıyor."));

        var user = new User
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim(),
            IsActive = true,
            RoleId = roleId
        };

        user.Id = await _repository.InsertAsync(user);
        return Result.Success(user);
    }
    public async Task<Result> UpdateAsync(int id, string? fullName, string? newPassword, int? roleId)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure(Error.NotFound("Kullanıcı bulunamadı."));

        existing.FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim();
        existing.RoleId = roleId;
        await _repository.UpdateAsync(existing);

        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            if (newPassword.Length < 6)
                return Result.Failure(Error.Validation("Şifre en az 6 karakter olmalıdır."));

            var hash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _repository.UpdatePasswordAsync(id, hash);
        }

        return Result.Success();
    }
}