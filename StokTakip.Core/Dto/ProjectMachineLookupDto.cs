namespace StokTakip.Core.Dto;

public sealed class ProjectMachineLookupDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int MachineId { get; set; }
    public string MachineCode { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
}