using StokTakip.Core.Dto;
using StokTakip.Core.Entity;
using StokTakip.Core.Result;
using StokTakip.Data.Repository;
using StokTakip.Service.Interface;

namespace StokTakip.Service.Service;

public sealed class StockIssueService : IStockIssueService
{
    private readonly IStockIssueRepository _stockIssueRepository;
    private readonly IStockEntryRepository _stockEntryRepository;
    private readonly IProjectMachineRepository _projectMachineRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IProductRepository _productRepository;

    public StockIssueService(
        IStockIssueRepository stockIssueRepository,
        IStockEntryRepository stockEntryRepository,
        IProjectMachineRepository projectMachineRepository,
        IProjectRepository projectRepository,
        IProductRepository productRepository)
    {
        _stockIssueRepository = stockIssueRepository;
        _stockEntryRepository = stockEntryRepository;
        _projectMachineRepository = projectMachineRepository;
        _projectRepository = projectRepository;
        _productRepository = productRepository;
    }

    public async Task<Result<IReadOnlyList<StockIssueListDto>>> GetListAsync()
    {
        var list = await _stockIssueRepository.GetListAsync();
        return Result.Success(list);
    }

    public async Task<Result<IReadOnlyList<StockLotLookupDto>>> GetOpenLotsLookupAsync()
    {
        var lots = await _stockEntryRepository.GetOpenLotsLookupAsync();
        return Result.Success(lots);
    }

    public async Task<Result<int>> CreateLineAsync(
        int projectMachineId, int productId, decimal quantity, DateTime date, string? description, long? stockEntryId)
    {
        if (quantity <= 0)
            return Result.Failure<int>(Error.Validation("Miktar sıfırdan büyük olmalıdır."));

        var projectMachine = await _projectMachineRepository.GetByIdAsync(projectMachineId);
        if (projectMachine is null)
            return Result.Failure<int>(Error.NotFound("Proje-makina ataması bulunamadı."));

        var project = await _projectRepository.GetByIdAsync(projectMachine.ProjectId);
        if (project is null)
            return Result.Failure<int>(Error.NotFound("Proje bulunamadı."));

        if (!project.IsActive)
            return Result.Failure<int>(Error.Failure("Pasif projeye stok çıkışı yapılamaz."));

        var product = await _productRepository.GetByIdAsync(productId);
        if (product is null)
            return Result.Failure<int>(Error.NotFound("Ürün bulunamadı."));

        List<StockIssue> lines;

        if (stockEntryId.HasValue)
        {
            var lot = await _stockEntryRepository.GetOpenLotByIdAsync(stockEntryId.Value);
            var line = AllocateFromLot(lot, productId, projectMachineId, quantity, date, description);
            if (line is null)
                return Result.Failure<int>(Error.Conflict("Seçilen mal kabul kaydında yetersiz stok."));

            lines = new List<StockIssue> { line };
        }
        else
        {
            var lots = await _stockEntryRepository.GetOpenLotsByProductIdAsync(productId);
            var allocated = AllocateFifo(lots, projectMachineId, quantity, date, description);
            if (allocated is null)
                return Result.Failure<int>(Error.Conflict("Yetersiz stok."));

            lines = allocated;
        }

        foreach (var line in lines)
        {
            await _stockIssueRepository.InsertAsync(line);
        }

        return Result.Success(lines.Count);
    }

    private static StockIssue? AllocateFromLot(
        StockEntryOpenLotDto? lot, int productId, int projectMachineId, decimal quantity, DateTime date, string? description)
    {
        if (lot is null || quantity <= 0)
            return null;

        if (lot.ProductId != productId)
            return null;

        if (lot.RemainingQuantity < quantity || lot.Quantity <= 0)
            return null;

        var unitPriceTry = lot.AmountTry / lot.Quantity;

        return new StockIssue
        {
            ProjectMachineId = projectMachineId,
            StockEntryId = lot.Id,
            Quantity = quantity,
            UnitPrice = unitPriceTry,
            Date = date,
            Description = description
        };
    }

    private static List<StockIssue>? AllocateFifo(
        IReadOnlyList<StockEntryOpenLotDto> lots, int projectMachineId, decimal quantity, DateTime date, string? description)
    {
        if (quantity <= 0 || lots.Count == 0)
            return null;

        var available = lots.Sum(x => x.RemainingQuantity);
        if (available < quantity)
            return null;

        var remainingToIssue = quantity;
        var lines = new List<StockIssue>();

        foreach (var lot in lots)
        {
            if (remainingToIssue <= 0)
                break;

            if (lot.RemainingQuantity <= 0 || lot.Quantity <= 0)
                continue;

            var take = Math.Min(lot.RemainingQuantity, remainingToIssue);
            var unitPriceTry = lot.AmountTry / lot.Quantity;

            lines.Add(new StockIssue
            {
                ProjectMachineId = projectMachineId,
                StockEntryId = lot.Id,
                Quantity = take,
                UnitPrice = unitPriceTry,
                Date = date,
                Description = description
            });

            remainingToIssue -= take;
        }

        return remainingToIssue > 0 ? null : lines;
    }
}