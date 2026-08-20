using Microsoft.VisualBasic;
using StokTakip.Core.Constants;
using StokTakip.Core.Entity;
using StokTakip.Core.Result;
using StokTakip.Data.Repository;
using StokTakip.Service.Interface;

namespace StokTakip.Service.Service;

public sealed class MachineService : IMachineService
{
    private readonly IMachineRepository _repository;
    private readonly IProjectRepository _projectRepository;

    public MachineService(IMachineRepository repository, IProjectRepository projectRepository)
    {
        _repository = repository;
        _projectRepository = projectRepository;
    }

    public async Task<Result<IReadOnlyList<Machine>>> GetAllAsync(bool? isActive = null)
    {
        var machines = await _repository.GetAllAsync(isActive);
        return Result.Success(machines);
    }

    public async Task<Result<Machine>> GetByIdAsync(int id)
    {
        var machine = await _repository.GetByIdAsync(id);
        if (machine is null)
            return Result.Failure<Machine>(Error.NotFound("Makina bulunamadı."));

        return Result.Success(machine);
    }

    public async Task<Result<Machine>> CreateForProjectAsync(int projectId, string name)
    {
        name = (name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Machine>(Error.Validation("Makina adı boş olamaz."));

        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project is null)
            return Result.Failure<Machine>(Error.NotFound("Proje bulunamadı."));

        if (await _repository.ExistsByNameAsync(name))
            return Result.Failure<Machine>(Error.Conflict("Bu isimde bir makina zaten mevcut."));

        var sequence = await _repository.GetNextSequenceForProjectAsync(projectId);
        var code = MachineCodeFormatter.Format(DateTime.Now, projectId, sequence);

        var machine = new Machine
        {
            Code = code,
            Name = name,
            IsActive = true
        };

        machine.Id = await _repository.InsertAsync(machine);
        return Result.Success(machine);
    }

    public async Task<Result> UpdateAsync(int id, string name)
    {
        name = (name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("Makina adı boş olamaz."));

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure(Error.NotFound("Makina bulunamadı."));

        if (await _repository.ExistsByNameAsync(name, excludeId: id))
            return Result.Failure(Error.Conflict("Bu isimde bir makina zaten mevcut."));

        existing.Name = name;
        await _repository.UpdateAsync(existing);
        return Result.Success();
    }

    public async Task<Result> SetActiveAsync(int id, bool isActive)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure(Error.NotFound("Makina bulunamadı."));

        if (!isActive && await _repository.HasProjectLinksAsync(id))
            return Result.Failure(Error.Conflict("Bu makina bir projeye atanmış, pasife alınamaz."));

        await _repository.SetActiveAsync(id, isActive);
        return Result.Success();
    }
}