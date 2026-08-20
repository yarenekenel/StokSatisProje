using StokTakip.Core.Dto;
using StokTakip.Core.Entity;
using StokTakip.Core.Result;
using StokTakip.Data.Repository;
using StokTakip.Service.Interface;

namespace StokTakip.Service.Service;

public sealed class ProjectMachineService : IProjectMachineService
{
    private readonly IProjectMachineRepository _repository;
    private readonly IProjectRepository _projectRepository;
    private readonly IMachineRepository _machineRepository;

    public ProjectMachineService(
        IProjectMachineRepository repository,
        IProjectRepository projectRepository,
        IMachineRepository machineRepository)
    {
        _repository = repository;
        _projectRepository = projectRepository;
        _machineRepository = machineRepository;
    }

    public async Task<Result<IReadOnlyList<ProjectMachineListDto>>> GetListAsync(int? projectId = null)
    {
        var list = await _repository.GetListAsync(projectId);
        return Result.Success(list);
    }

    public async Task<Result<IReadOnlyList<ProjectMachineLookupDto>>> GetLookupByProjectIdAsync(int projectId)
    {
        var list = await _repository.GetLookupByProjectIdAsync(projectId);
        return Result.Success(list);
    }

    public async Task<Result<int>> AssignAsync(int projectId, int machineId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project is null)
            return Result.Failure<int>(Error.NotFound("Proje bulunamadı."));

        var machine = await _machineRepository.GetByIdAsync(machineId);
        if (machine is null)
            return Result.Failure<int>(Error.NotFound("Makina bulunamadı."));

        if (await _repository.ExistsAsync(projectId, machineId))
            return Result.Failure<int>(Error.Conflict("Bu makina zaten bu projeye atanmış."));

        var projectMachine = new ProjectMachine
        {
            ProjectId = projectId,
            MachineId = machineId
        };

        var id = await _repository.InsertAsync(projectMachine);
        return Result.Success(id);
    }

    public async Task<Result> UnassignAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure(Error.NotFound("Atama bulunamadı."));

        if (await _repository.HasStockIssuesAsync(id))
            return Result.Failure(Error.Conflict("Bu atamaya bağlı stok çıkışı var, kaldırılamaz."));

        await _repository.DeleteAsync(id);
        return Result.Success();
    }
}