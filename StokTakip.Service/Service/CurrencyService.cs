using StokTakip.Core.Entity;
using StokTakip.Core.Result;
using StokTakip.Data.Repository;
using StokTakip.Service.Interface;

namespace StokTakip.Service.Service;

public sealed class CurrencyService : ICurrencyService
{
    private readonly ICurrencyRepository _repository;

    public CurrencyService(ICurrencyRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<Currency>>> GetAllAsync()
    {
        var currencies = await _repository.GetAllAsync();
        return Result.Success(currencies);
    }

    public async Task<Result<Currency>> GetByIdAsync(int id)
    {
        var currency = await _repository.GetByIdAsync(id);
        if (currency is null)
            return Result.Failure<Currency>(Error.NotFound("Para birimi bulunamadı."));

        return Result.Success(currency);
    }

    public async Task<Result<Currency>> CreateAsync(string code, string name)
    {
        code = (code ?? string.Empty).Trim().ToUpperInvariant();
        name = (name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<Currency>(Error.Validation("Para birimi kodu boş olamaz."));

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Currency>(Error.Validation("Para birimi adı boş olamaz."));

        if (await _repository.ExistsByCodeAsync(code))
            return Result.Failure<Currency>(Error.Conflict("Bu kodda bir para birimi zaten mevcut."));

        var currency = new Currency { Code = code, Name = name };
        currency.Id = await _repository.InsertAsync(currency);

        return Result.Success(currency);
    }

    public async Task<Result<Currency>> UpdateAsync(int id, string code, string name)
    {
        code = (code ?? string.Empty).Trim().ToUpperInvariant();
        name = (name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<Currency>(Error.Validation("Para birimi kodu boş olamaz."));

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Currency>(Error.Validation("Para birimi adı boş olamaz."));

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure<Currency>(Error.NotFound("Para birimi bulunamadı."));

        if (await _repository.ExistsByCodeAsync(code, excludeId: id))
            return Result.Failure<Currency>(Error.Conflict("Bu kodda bir para birimi zaten mevcut."));

        existing.Code = code;
        existing.Name = name;
        await _repository.UpdateAsync(existing);

        return Result.Success(existing);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure(Error.NotFound("Para birimi bulunamadı."));

        await _repository.DeleteAsync(id);
        return Result.Success();
    }
}