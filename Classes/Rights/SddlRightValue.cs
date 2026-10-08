using System.Collections.ObjectModel;
using System.Globalization;

namespace SapphTools.SecurityDescriptor.Classes.Rights;
public abstract class SddlRightValue : IEquatable<SddlRightValue> {
    public uint Value { get; }
    public string? Abbr { get; }
    public string Description { get; }
    public static ReadOnlyDictionary<ObjectType, ReadOnlyDictionary<uint, SddlRightValue>> ByTypeAndVal => SddlDictBuilder.ByTypeAndVal;
    public static ReadOnlyDictionary<ObjectType, ReadOnlyDictionary<string, SddlRightValue>> ByTypeAndAbbr => SddlDictBuilder.ByTypeAndAbbr;
    public static ReadOnlyDictionary<string, SddlRightValue> ByAbbreviation => SddlDictBuilder.ByAbbr;

    protected SddlRightValue(uint value, string? abbr = null, string? description = null) {
        Value = value;
        Abbr = abbr;
        Description = description ?? "Special";
    }

    // A mask cannot reveal which spelling (for example KR or KX) produced it.
    public static T Construct<T>(uint value) where T : SddlRightValue, ISddlRight<T> => T.Create(value);
    public static T Construct<T>(string abbr) where T : SddlRightValue, ISddlRight<T> {
        ArgumentNullException.ThrowIfNull(abbr);
        if (T.ByAbbr.TryGetValue(abbr, out T? right)) {
            return right;
        }
        throw new ArgumentException("No such right exists for the given T.", nameof(abbr));
    }
    public static SddlRightValue? Construct(uint value, ObjectType type) => SddlDictBuilder.Create(value, type);
    public static SddlRightValue? Construct(string abbr, ObjectType type) {
        ArgumentNullException.ThrowIfNull(abbr);
        if (ByTypeAndAbbr.TryGetValue(type, out ReadOnlyDictionary<string, SddlRightValue>? dict) &&
            dict.TryGetValue(abbr, out SddlRightValue? right)) {
            return right;
        }
        return null;
    }

    public abstract SddlRightValue Clone();

    public override bool Equals(object? obj) => Equals(obj as SddlRightValue);
    public bool Equals(SddlRightValue? other) =>
        other is not null && GetType() == other.GetType() && Value == other.Value &&
        string.Equals(Abbr, other.Abbr, StringComparison.Ordinal);
    public override int GetHashCode() => HashCode.Combine(GetType(), Value, Abbr);
    public override string ToString() => Abbr ?? "0x" + Value.ToString("X8", CultureInfo.InvariantCulture);
}
