using System.Numerics;
using System.Text.RegularExpressions;

namespace Toolkit.Core;

public sealed class ReleaseVersion : IComparable<ReleaseVersion>
{
    private static readonly Regex Pattern = new(@"\Av?(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?(?:\+[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?\z", RegexOptions.CultureInvariant);
    private readonly BigInteger[] numbers;
    private readonly string[] pre;
    public string Text { get; }
    public bool IsPrerelease => pre.Length != 0;
    private ReleaseVersion(string text, BigInteger[] values, string[] identifiers) { Text = text; numbers = values; pre = identifiers; }

    public static bool TryParse(string? text, out ReleaseVersion? result)
    {
        result = null;
        if (string.IsNullOrEmpty(text)) return false;
        var m = Pattern.Match(text);
        if (!m.Success) return false;
        string[] pre = m.Groups[4].Success ? m.Groups[4].Value.Split('.') : [];
        if (pre.Any(x => x.Length > 1 && x[0] == '0' && x.All(char.IsAsciiDigit))) return false;
        result = new(text, [BigInteger.Parse(m.Groups[1].Value), BigInteger.Parse(m.Groups[2].Value), BigInteger.Parse(m.Groups[3].Value)], pre);
        return true;
    }

    public int CompareTo(ReleaseVersion? other)
    {
        if (other is null) return 1;
        for (int i = 0; i < 3; i++) { int c = numbers[i].CompareTo(other.numbers[i]); if (c != 0) return c; }
        if (pre.Length == 0 || other.pre.Length == 0) return pre.Length == other.pre.Length ? 0 : pre.Length == 0 ? 1 : -1;
        for (int i = 0; i < Math.Min(pre.Length, other.pre.Length); i++)
        {
            bool an = pre[i].All(char.IsAsciiDigit), bn = other.pre[i].All(char.IsAsciiDigit);
            int c = an && bn ? BigInteger.Parse(pre[i]).CompareTo(BigInteger.Parse(other.pre[i])) : an != bn ? (an ? -1 : 1) : string.CompareOrdinal(pre[i], other.pre[i]);
            if (c != 0) return c;
        }
        return pre.Length.CompareTo(other.pre.Length);
    }
}
