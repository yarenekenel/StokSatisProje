using StokTakip.Core.Dto;
using StokTakip.Core.Entity;

namespace StokTakip.Data.Repository;

public interface IStockIssueRepository
{
    Task<StockIssue?> GetByIdAsync(long id);
    Task<IReadOnlyList<StockIssue>> GetAllAsync();
    Task<IReadOnlyList<StockIssue>> GetByProjectIdAsync(int projectId);
    Task<IReadOnlyList<StockIssueListDto>> GetListAsync();
    Task<long> InsertAsync(StockIssue stockIssue);
}