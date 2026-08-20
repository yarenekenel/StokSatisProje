namespace StokTakip.Core.Entity;

public sealed class ProjectMachine
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int MachineId { get; set; }
    public DateTime AssignedAt { get; set; }
}