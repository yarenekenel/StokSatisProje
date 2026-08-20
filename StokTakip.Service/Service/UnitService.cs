using StokTakip.Core.Entity;
using StokTakip.Core.Result;
using StokTakip.Data.Repository;
using StokTakip.Service.Interface;

namespace StokTakip.Service.Service;

public sealed class UnitService : IUnitService
{
    private readonly IUnitRepository _repository;

    public UnitService(IUnitRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<Unit>>> GetAllAsync()
    {
        var units = await _repository.GetAllAsync();
        return Result.Success(units);
    }

    public async Task<Result<Unit>> GetByIdAsync(int id)
    {
        var unit = await _repository.GetByIdAsync(id);
        if (unit is null)
            return Result.Failure<Unit>(Error.NotFound("Birim bulunamadı."));

        return Result.Success(unit);
    }

    public async Task<Result<Unit>> CreateAsync(string name)
    {
        name = (name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Unit>(Error.Validation("Birim adı boş olamaz."));

        if (await _repository.ExistsByNameAsync(name))
            return Result.Failure<Unit>(Error.Conflict("Bu isimde bir birim zaten mevcut."));

        var unit = new Unit { Name = name };
        unit.Id = await _repository.InsertAsync(unit);

        return Result.Success(unit);
    }

    public async Task<Result<Unit>> UpdateAsync(int id, string name)
    {
        name = (name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Unit>(Error.Validation("Birim adı boş olamaz."));

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure<Unit>(Error.NotFound("Birim bulunamadı."));

        if (await _repository.ExistsByNameAsync(name, excludeId: id))
            return Result.Failure<Unit>(Error.Conflict("Bu isimde bir birim zaten mevcut."));

        existing.Name = name;
        await _repository.UpdateAsync(existing);

        return Result.Success(existing);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure(Error.NotFound("Birim bulunamadı."));

        await _repository.DeleteAsync(id);
        return Result.Success();
    }
}