namespace StokTakip.Core.Constants;

public static class ProductCodeConstants
{
    public const string Prefix = "UR";
    public const int Digits = 6;

    public static string Format(int sequence) => $"{Prefix}{sequence.ToString().PadLeft(Digits, '0')}";
}