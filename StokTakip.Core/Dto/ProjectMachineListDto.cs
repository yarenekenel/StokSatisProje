namespace StokTakip.Core.Dto;

public sealed class ProjectMachineListDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public int MachineId { get; set; }
    public string MachineCode { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}