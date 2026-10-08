using System.Globalization;

namespace SapphTools.SecurityDescriptor.Classes.Rights;
public class SddlRight : IEquatable<SddlRight> {
    private static readonly System.Buffers.SearchValues<char> HexChars = System.Buffers.SearchValues.Create("0123456789ABCDEFabcdef");
    protected readonly HashSet<SddlRightValue> Rights = [];
    private readonly uint? rawMask;

    public SddlRight() { }
    private SddlRight(uint rights) {
        rawMask = rights;
    }

    public SddlRight Clone() {
        SddlRight clone = rawMask.HasValue ? new(rawMask.Value) : new();
        foreach (SddlRightValue value in Rights) {
            clone.Rights.Add(value.Clone());
        }
        return clone;
    }
    public uint ToValue() {
        uint val = rawMask ?? 0;
        foreach (SddlRightValue value in Rights) {
            val |= value.Value;
        }
        return val;
    }
    public override string ToString() {
        if (rawMask.HasValue || Rights.Any(r => r.Abbr is null)) {
            return "0x" + ToValue().ToString("X8", CultureInfo.InvariantCulture);
        }
        // Formatting and equality must not depend on HashSet insertion order.
        return string.Concat(Rights.Select(r => r.Abbr).OrderBy(abbr => abbr, StringComparer.Ordinal));
    }

    public static SddlRight Construct(string? rights) {
        if (string.IsNullOrWhiteSpace(rights)) {
            return new();
        }
        if (rights.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) {
            ReadOnlySpan<char> hex = rights.AsSpan(2);
            if (hex.IsEmpty || hex.Length > 8 || hex.IndexOfAnyExcept(HexChars) >= 0 ||
                !uint.TryParse(hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out uint mask)) {
                throw new ArgumentException("An invalid hexadecimal rights mask was presented.", nameof(rights));
            }
            return Construct(mask);
        }
        if (rights.Length % 2 != 0) {
            throw new ArgumentException("An invalid rights string was presented.", nameof(rights));
        }
        SddlRight ret = new();
        for (int i = 0; i < rights.Length; i += 2) {
            string abbr = rights.Substring(i, 2);
            if (!SddlRightValue.ByAbbreviation.TryGetValue(abbr, out SddlRightValue? right)) {
                throw new ArgumentException($"An unrecognized SDDL right was presented ({abbr}).", nameof(rights));
            }
            ret.Rights.Add(right);
        }
        return ret;
    }
    public static SddlRight Construct(uint rights) => new(rights);
    // Compatibility overloads: aggregate text may span multiple rights domains.
    public static SddlRight Construct<T>(string? rights) where T : SddlRightValue, ISddlRight<T> => Construct(rights);
    public static SddlRight Construct<T>(uint rights) where T : SddlRightValue, ISddlRight<T> => Construct(rights);

    public override bool Equals(object? obj) => Equals(obj as SddlRight);
    public bool Equals(SddlRight? other) =>
        other is not null && other is not null && ToValue() == other.ToValue();
    public override int GetHashCode() => ToValue().GetHashCode();
}