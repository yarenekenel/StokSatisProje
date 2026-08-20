using StokTakip.Core.Entity;
using StokTakip.Core.Result;
using StokTakip.Data.Repository;
using StokTakip.Service.Interface;

namespace StokTakip.Service.Service;

public sealed class ProductTypeService : IProductTypeService
{
    private readonly IProductTypeRepository _repository;

    public ProductTypeService(IProductTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<ProductType>>> GetAllAsync()
    {
        var productTypes = await _repository.GetAllAsync();
        return Result.Success(productTypes);
    }

    public async Task<Result<ProductType>> GetByIdAsync(int id)
    {
        var productType = await _repository.GetByIdAsync(id);
        if (productType is null)
            return Result.Failure<ProductType>(Error.NotFound("Ürün tipi bulunamadı."));

        return Result.Success(productType);
    }

    public async Task<Result<ProductType>> CreateAsync(string name, string codePrefix)
    {
        name = (name ?? string.Empty).Trim();
        codePrefix = (codePrefix ?? string.Empty).Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<ProductType>(Error.Validation("Ürün tipi adı boş olamaz."));

        if (string.IsNullOrWhiteSpace(codePrefix))
            return Result.Failure<ProductType>(Error.Validation("Kod ön eki boş olamaz."));

        if (await _repository.ExistsByNameAsync(name))
            return Result.Failure<ProductType>(Error.Conflict("Bu isimde bir ürün tipi zaten mevcut."));

        if (await _repository.ExistsByCodePrefixAsync(codePrefix))
            return Result.Failure<ProductType>(Error.Conflict("Bu kod ön eki zaten kullanılıyor."));

        var productType = new ProductType { Name = name, CodePrefix = codePrefix };
        productType.Id = await _repository.InsertAsync(productType);

        return Result.Success(productType);
    }

    public async Task<Result<ProductType>> UpdateAsync(int id, string name, string codePrefix)
    {
        name = (name ?? string.Empty).Trim();
        codePrefix = (codePrefix ?? string.Empty).Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<ProductType>(Error.Validation("Ürün tipi adı boş olamaz."));

        if (string.IsNullOrWhiteSpace(codePrefix))
            return Result.Failure<ProductType>(Error.Validation("Kod ön eki boş olamaz."));

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure<ProductType>(Error.NotFound("Ürün tipi bulunamadı."));

        if (await _repository.ExistsByNameAsync(name, excludeId: id))
            return Result.Failure<ProductType>(Error.Conflict("Bu isimde bir ürün tipi zaten mevcut."));

        if (await _repository.ExistsByCodePrefixAsync(codePrefix, excludeId: id))
            return Result.Failure<ProductType>(Error.Conflict("Bu kod ön eki zaten kullanılıyor."));

        existing.Name = name;
        existing.CodePrefix = codePrefix;
        await _repository.UpdateAsync(existing);

        return Result.Success(existing);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure(Error.NotFound("Ürün tipi bulunamadı."));

        await _repository.DeleteAsync(id);
        return Result.Success();
    }
}