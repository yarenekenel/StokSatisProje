using StokTakip.Core.Dto;
using StokTakip.Core.Result;

namespace StokTakip.Service.Interface;

public interface IStockIssueService
{
    Task<Result<IReadOnlyList<StockIssueListDto>>> GetListAsync();
    Task<Result<IReadOnlyList<StockLotLookupDto>>> GetOpenLotsLookupAsync();
    Task<Result<int>> CreateLineAsync(int projectMachineId, int productId, decimal quantity, DateTime date, string? description, long? stockEntryId);
}