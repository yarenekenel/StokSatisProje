using StokTakip.Core.Entity;
using StokTakip.Core.Result;
using StokTakip.Data.Repository;
using StokTakip.Service.Interface;

namespace StokTakip.Service.Service;

public sealed class StockTypeService : IStockTypeService
{
    private readonly IStockTypeRepository _repository;

    public StockTypeService(IStockTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<StockType>>> GetAllAsync()
    {
        var stockTypes = await _repository.GetAllAsync();
        return Result.Success(stockTypes);
    }

    public async Task<Result<StockType>> GetByIdAsync(int id)
    {
        var stockType = await _repository.GetByIdAsync(id);
        if (stockType is null)
            return Result.Failure<StockType>(Error.NotFound("Stok tipi bulunamadı."));

        return Result.Success(stockType);
    }

    public async Task<Result<StockType>> CreateAsync(string name)
    {
        name = (name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<StockType>(Error.Validation("Stok tipi adı boş olamaz."));

        if (await _repository.ExistsByNameAsync(name))
            return Result.Failure<StockType>(Error.Conflict("Bu isimde bir stok tipi zaten mevcut."));

        var stockType = new StockType { Name = name };
        stockType.Id = await _repository.InsertAsync(stockType);

        return Result.Success(stockType);
    }

    public async Task<Result<StockType>> UpdateAsync(int id, string name)
    {
        name = (name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<StockType>(Error.Validation("Stok tipi adı boş olamaz."));

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure<StockType>(Error.NotFound("Stok tipi bulunamadı."));

        if (await _repository.ExistsByNameAsync(name, excludeId: id))
            return Result.Failure<StockType>(Error.Conflict("Bu isimde bir stok tipi zaten mevcut."));

        existing.Name = name;
        await _repository.UpdateAsync(existing);

        return Result.Success(existing);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure(Error.NotFound("Stok tipi bulunamadı."));

        await _repository.DeleteAsync(id);
        return Result.Success();
    }
}