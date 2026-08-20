namespace StokTakip.Core.Constants;

public static class MachineCodeFormatter
{
    public static string Format(DateTime date, int projectId, int sequence)
    {
        if (projectId <= 0)
            throw new ArgumentOutOfRangeException(nameof(projectId));

        if (sequence <= 0)
            throw new ArgumentOutOfRangeException(nameof(sequence));

        return $"{date:yyMMdd}/{projectId:D4}{sequence:D3}";
    }
}