namespace Catapult.Core;

public static class StringExtensions
{
    public static bool IsNullOrWhiteSpace(this string str)
    {
        return string.IsNullOrWhiteSpace(str);
    }

    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    public static bool IsSet(this string str)
    {
        return !string.IsNullOrEmpty(str);
    }
}