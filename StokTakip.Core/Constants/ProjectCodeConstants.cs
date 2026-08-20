namespace StokTakip.Core.Constants;

public static class ProjectCodeConstants
{
    public const string Prefix = "PR";
    public const int Digits = 6;

    public static string Format(int sequence) => $"{Prefix}{sequence.ToString().PadLeft(Digits, '0')}";
}