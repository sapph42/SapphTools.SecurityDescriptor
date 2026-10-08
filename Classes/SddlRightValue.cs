using System.Collections.ObjectModel;
using System.Globalization;

namespace SapphTools.SecurityDescriptor.Classes;

// Initialization flows from this catalog to the concrete tables, never back again.
// The CLR initializes it once and publishes complete, read-only tables to every caller.
internal static class SddlDictBuilder {
    internal static readonly ReadOnlyDictionary<ObjectType, ReadOnlyDictionary<uint, SddlRightValue>> ByTypeAndVal;
    internal static readonly ReadOnlyDictionary<ObjectType, ReadOnlyDictionary<string, SddlRightValue>> ByTypeAndAbbr;
    internal static readonly ReadOnlyDictionary<string, SddlRightValue> ByAbbr;
    private static readonly Dictionary<ObjectType, Func<uint, SddlRightValue>> factories = [];

    static SddlDictBuilder() {
        Dictionary<ObjectType, ReadOnlyDictionary<uint, SddlRightValue>> byVal = [];
        Dictionary<ObjectType, ReadOnlyDictionary<string, SddlRightValue>> byAbbr = [];
        Dictionary<string, SddlRightValue> abbreviations = new(StringComparer.Ordinal);

        Register<GenericRight>();
        Register<StandardRight>();
        Register<DirectoryRight>();
        Register<FileRight>();
        Register<RegistryRight>();
        Register<MandatoryRight>();

        ByTypeAndVal = new(byVal);
        ByTypeAndAbbr = new(byAbbr);
        ByAbbr = new(abbreviations);

        void Register<T>() where T : SddlRightValue, ISddlRight<T> {
            byVal.Add(T.Type, new(T.ByVal.ToDictionary(kvp => kvp.Key, kvp => (SddlRightValue)kvp.Value)));
            byAbbr.Add(T.Type, new(T.ByAbbr.ToDictionary(kvp => kvp.Key, kvp => (SddlRightValue)kvp.Value, StringComparer.Ordinal)));
            factories.Add(T.Type, value => T.Create(value));
            foreach (KeyValuePair<string, T> entry in T.ByAbbr) {
                // Every domain has its own NONE; the empty aggregate needs no token.
                if (entry.Key.Length != 0) {
                    abbreviations.Add(entry.Key, entry.Value);
                }
            }
        }
    }

    internal static SddlRightValue? Create(uint value, ObjectType type) =>
        factories.TryGetValue(type, out Func<uint, SddlRightValue>? factory) ? factory(value) : null;
}

public interface ISddlRight<TSelf> where TSelf : ISddlRight<TSelf> {
    static abstract ObjectType Type { get; }
    static abstract ReadOnlyDictionary<uint, TSelf> ByVal { get; }
    static abstract ReadOnlyDictionary<string, TSelf> ByAbbr { get; }
    static abstract TSelf Create(uint value);
    static ObjectType GetObjectType() => TSelf.Type;
    static ISddlRight<TSelf> ConvertTo(object obj) {
        if (obj is ISddlRight<TSelf> isr) {
            return isr;
        }
        throw new InvalidCastException("obj must be of type ISddlRight<TSelf>");
    }
}

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

public class GenericRight : SddlRightValue, ISddlRight<GenericRight> {
    public static readonly GenericRight SDDL_NONE            = new(0x00000000, string.Empty, "None");
    public static readonly GenericRight SDDL_GENERIC_ALL     = new(0x10000000, "GA", "Full Control");
    public static readonly GenericRight SDDL_GENERIC_EXECUTE = new(0x20000000, "GX", "Execute");
    public static readonly GenericRight SDDL_GENERIC_WRITE   = new(0x40000000, "GW", "Write");
    public static readonly GenericRight SDDL_GENERIC_READ    = new(0x80000000, "GR", "Read");
    public static ReadOnlyDictionary<uint, GenericRight> ByVal { get; } = new(new Dictionary<uint, GenericRight>() {
        { SDDL_NONE.Value, SDDL_NONE },
        { SDDL_GENERIC_ALL.Value, SDDL_GENERIC_ALL },
        { SDDL_GENERIC_EXECUTE.Value, SDDL_GENERIC_EXECUTE },
        { SDDL_GENERIC_WRITE.Value, SDDL_GENERIC_WRITE },
        { SDDL_GENERIC_READ.Value, SDDL_GENERIC_READ }
    });
    public static ReadOnlyDictionary<string, GenericRight> ByAbbr { get; } = new(new Dictionary<string, GenericRight>(StringComparer.Ordinal) {
        { SDDL_NONE.Abbr!, SDDL_NONE },
        { SDDL_GENERIC_ALL.Abbr!, SDDL_GENERIC_ALL },
        { SDDL_GENERIC_EXECUTE.Abbr!, SDDL_GENERIC_EXECUTE },
        { SDDL_GENERIC_WRITE.Abbr!, SDDL_GENERIC_WRITE },
        { SDDL_GENERIC_READ.Abbr!, SDDL_GENERIC_READ }
    });
    public static ObjectType Type => ObjectType.Generic;
    protected GenericRight(uint value, string? abbr = null, string? description = null) : base(value, abbr, description) { }
    public override SddlRightValue Clone() =>
        new GenericRight(Value, Abbr, Description);
    public static GenericRight Create(uint value) => new(value);
}

public class StandardRight : SddlRightValue, ISddlRight<StandardRight> {
    public static readonly StandardRight SDDL_NONE            = new(0x00000000, string.Empty, "None");
    public static readonly StandardRight SDDL_STANDARD_DELETE = new(0x00010000, "SD", "Delete");
    public static readonly StandardRight SDDL_READ_CONTROL    = new(0x00020000, "RC", "Read permissions");
    public static readonly StandardRight SDDL_WRITE_DAC       = new(0x00040000, "WD", "Change permissions");
    public static readonly StandardRight SDDL_WRITE_OWNER     = new(0x00080000, "WO", "Take ownership");
    public static ReadOnlyDictionary<uint, StandardRight> ByVal { get; } = new(new Dictionary<uint, StandardRight>() {
        { SDDL_NONE.Value, SDDL_NONE },
        { SDDL_STANDARD_DELETE.Value, SDDL_STANDARD_DELETE },
        { SDDL_READ_CONTROL.Value, SDDL_READ_CONTROL },
        { SDDL_WRITE_DAC.Value, SDDL_WRITE_DAC },
        { SDDL_WRITE_OWNER.Value, SDDL_WRITE_OWNER }
    });
    public static ReadOnlyDictionary<string, StandardRight> ByAbbr { get; } = new(new Dictionary<string, StandardRight>(StringComparer.Ordinal) {
        { SDDL_NONE.Abbr!, SDDL_NONE },
        { SDDL_STANDARD_DELETE.Abbr!, SDDL_STANDARD_DELETE },
        { SDDL_READ_CONTROL.Abbr!, SDDL_READ_CONTROL },
        { SDDL_WRITE_DAC.Abbr!, SDDL_WRITE_DAC },
        { SDDL_WRITE_OWNER.Abbr!, SDDL_WRITE_OWNER }
    });
    public static ObjectType Type => ObjectType.Standard;
    protected StandardRight(uint value, string? abbr = null, string? description = null) : base(value, abbr, description) { }
    public override SddlRightValue Clone() =>
        new StandardRight(Value, Abbr, Description);
    public static StandardRight Create(uint value) => new(value);
}

public class DirectoryRight : SddlRightValue, ISddlRight<DirectoryRight> {
    public static readonly DirectoryRight SDDL_NONE            = new(0x00000000, string.Empty, "None");
    public static readonly DirectoryRight SDDL_CREATE_CHILD    = new(0x00000001, "CC", "Create child objects");
    public static readonly DirectoryRight SDDL_DELETE_CHILD    = new(0x00000002, "DC", "Delete child objects");
    public static readonly DirectoryRight SDDL_LIST_CHILDREN   = new(0x00000004, "LC", "List contents");
    public static readonly DirectoryRight SDDL_SELF_WRITE      = new(0x00000008, "SW", "Validated write");
    public static readonly DirectoryRight SDDL_READ_PROPERTY   = new(0x00000010, "RP", "Read properties");
    public static readonly DirectoryRight SDDL_WRITE_PROPERTY  = new(0x00000020, "WP", "Write properties");
    public static readonly DirectoryRight SDDL_DELETE_TREE     = new(0x00000040, "DT", "Delete subtree");
    public static readonly DirectoryRight SDDL_LIST_OBJECT     = new(0x00000080, "LO", "List object");
    public static readonly DirectoryRight SDDL_CONTROL_ACCESS  = new(0x00000100, "CR", "Extended rights");
    public static ReadOnlyDictionary<uint, DirectoryRight> ByVal { get; } = new(new Dictionary<uint, DirectoryRight>() {
        { SDDL_NONE.Value, SDDL_NONE },
        { SDDL_CREATE_CHILD.Value, SDDL_CREATE_CHILD },
        { SDDL_DELETE_CHILD.Value, SDDL_DELETE_CHILD },
        { SDDL_LIST_CHILDREN.Value, SDDL_LIST_CHILDREN },
        { SDDL_SELF_WRITE.Value, SDDL_SELF_WRITE },
        { SDDL_READ_PROPERTY.Value, SDDL_READ_PROPERTY },
        { SDDL_WRITE_PROPERTY.Value, SDDL_WRITE_PROPERTY },
        { SDDL_DELETE_TREE.Value, SDDL_DELETE_TREE },
        { SDDL_LIST_OBJECT.Value, SDDL_LIST_OBJECT },
        { SDDL_CONTROL_ACCESS.Value, SDDL_CONTROL_ACCESS }
    });
    public static ReadOnlyDictionary<string, DirectoryRight> ByAbbr { get; } = new(new Dictionary<string, DirectoryRight>(StringComparer.Ordinal) {
        { SDDL_NONE.Abbr!, SDDL_NONE },
        { SDDL_CREATE_CHILD.Abbr!, SDDL_CREATE_CHILD },
        { SDDL_DELETE_CHILD.Abbr!, SDDL_DELETE_CHILD },
        { SDDL_LIST_CHILDREN.Abbr!, SDDL_LIST_CHILDREN },
        { SDDL_SELF_WRITE.Abbr!, SDDL_SELF_WRITE },
        { SDDL_READ_PROPERTY.Abbr!, SDDL_READ_PROPERTY },
        { SDDL_WRITE_PROPERTY.Abbr!, SDDL_WRITE_PROPERTY },
        { SDDL_DELETE_TREE.Abbr!, SDDL_DELETE_TREE },
        { SDDL_LIST_OBJECT.Abbr!, SDDL_LIST_OBJECT },
        { SDDL_CONTROL_ACCESS.Abbr!, SDDL_CONTROL_ACCESS }
    });
    public static ObjectType Type => ObjectType.DirectoryService;
    protected DirectoryRight(uint value, string? abbr = null, string? description = null) : base(value, abbr, description) { }
    public override SddlRightValue Clone() =>
        new DirectoryRight(Value, Abbr, Description);
    public static DirectoryRight Create(uint value) => new(value);
}

public class FileRight : SddlRightValue, ISddlRight<FileRight> {
    public static readonly FileRight SDDL_NONE         = new(0x00000000, string.Empty, "None");
    public static readonly FileRight SDDL_FILE_ALL     = new(0x001F01FF, "FA", "Full Control");
    public static readonly FileRight SDDL_FILE_READ    = new(0x00120089, "FR", "Read");
    public static readonly FileRight SDDL_FILE_WRITE   = new(0x00120116, "FW", "Write");
    public static readonly FileRight SDDL_FILE_EXECUTE = new(0x001200A0, "FX", "Execute");
    public static ReadOnlyDictionary<uint, FileRight> ByVal { get; } = new(new Dictionary<uint, FileRight>() {
        { SDDL_NONE.Value, SDDL_NONE },
        { SDDL_FILE_ALL.Value, SDDL_FILE_ALL },
        { SDDL_FILE_READ.Value, SDDL_FILE_READ },
        { SDDL_FILE_WRITE.Value, SDDL_FILE_WRITE },
        { SDDL_FILE_EXECUTE.Value, SDDL_FILE_EXECUTE }
    });
    public static ReadOnlyDictionary<string, FileRight> ByAbbr { get; } = new(new Dictionary<string, FileRight>(StringComparer.Ordinal) {
        { SDDL_NONE.Abbr!, SDDL_NONE },
        { SDDL_FILE_ALL.Abbr!, SDDL_FILE_ALL },
        { SDDL_FILE_READ.Abbr!, SDDL_FILE_READ },
        { SDDL_FILE_WRITE.Abbr!, SDDL_FILE_WRITE },
        { SDDL_FILE_EXECUTE.Abbr!, SDDL_FILE_EXECUTE }
    });
    public static ObjectType Type => ObjectType.File;
    protected FileRight(uint value, string? abbr = null, string? description = null) : base(value, abbr, description) { }
    public override SddlRightValue Clone() =>
        new FileRight(Value, Abbr, Description);
    public static FileRight Create(uint value) => new(value);
}

public class RegistryRight : SddlRightValue, ISddlRight<RegistryRight> {
    public static readonly RegistryRight SDDL_NONE        = new(0x00000000, string.Empty, "None");
    public static readonly RegistryRight SDDL_KEY_ALL     = new(0x000F003F, "KA", "Full Control");
    public static readonly RegistryRight SDDL_KEY_READ    = new(0x00020019, "KR", "Read");
    public static readonly RegistryRight SDDL_KEY_WRITE   = new(0x00020006, "KW", "Write");
    public static readonly RegistryRight SDDL_KEY_EXECUTE = new(0x00020019, "KX", "Read");
    // KR is the canonical numeric lookup; KX remains distinct in ByAbbr.
    public static ReadOnlyDictionary<uint, RegistryRight> ByVal { get; } = new(new Dictionary<uint, RegistryRight>() {
        { SDDL_NONE.Value, SDDL_NONE },
        { SDDL_KEY_ALL.Value, SDDL_KEY_ALL },
        { SDDL_KEY_READ.Value, SDDL_KEY_READ },
        { SDDL_KEY_WRITE.Value, SDDL_KEY_WRITE }
    });
    public static ReadOnlyDictionary<string, RegistryRight> ByAbbr { get; } = new(new Dictionary<string, RegistryRight>(StringComparer.Ordinal) {
        { SDDL_NONE.Abbr!, SDDL_NONE },
        { SDDL_KEY_ALL.Abbr!, SDDL_KEY_ALL },
        { SDDL_KEY_READ.Abbr!, SDDL_KEY_READ },
        { SDDL_KEY_WRITE.Abbr!, SDDL_KEY_WRITE },
        { SDDL_KEY_EXECUTE.Abbr!, SDDL_KEY_EXECUTE }
    });
    public static ObjectType Type => ObjectType.RegistryKey;
    protected RegistryRight(uint value, string? abbr = null, string? description = null) : base(value, abbr, description) { }
    public override SddlRightValue Clone() =>
        new RegistryRight(Value, Abbr, Description);
    public static RegistryRight Create(uint value) => new(value);
}

public class MandatoryRight : SddlRightValue, ISddlRight<MandatoryRight> {
    public static readonly MandatoryRight SDDL_NONE          = new(0x00000000, string.Empty, "None");
    public static readonly MandatoryRight SDDL_NO_WRITE_UP   = new(0x00000001, "NW", "No write up");
    public static readonly MandatoryRight SDDL_NO_READ_UP    = new(0x00000002, "NR", "No read up");
    public static readonly MandatoryRight SDDL_NO_EXECUTE_UP = new(0x00000004, "NX", "No execute up");
    public static ReadOnlyDictionary<uint, MandatoryRight> ByVal { get; } = new(new Dictionary<uint, MandatoryRight>() {
        { SDDL_NONE.Value, SDDL_NONE },
        { SDDL_NO_WRITE_UP.Value, SDDL_NO_WRITE_UP },
        { SDDL_NO_READ_UP.Value, SDDL_NO_READ_UP },
        { SDDL_NO_EXECUTE_UP.Value, SDDL_NO_EXECUTE_UP }
    });
    public static ReadOnlyDictionary<string, MandatoryRight> ByAbbr { get; } = new(new Dictionary<string, MandatoryRight>(StringComparer.Ordinal) {
        { SDDL_NONE.Abbr!, SDDL_NONE },
        { SDDL_NO_WRITE_UP.Abbr!, SDDL_NO_WRITE_UP },
        { SDDL_NO_READ_UP.Abbr!, SDDL_NO_READ_UP },
        { SDDL_NO_EXECUTE_UP.Abbr!, SDDL_NO_EXECUTE_UP }
    });
    public static ObjectType Type => ObjectType.Mandatory;
    protected MandatoryRight(uint value, string? abbr = null, string? description = null) : base(value, abbr, description) { }
    public override SddlRightValue Clone() =>
        new MandatoryRight(Value, Abbr, Description);
    public static MandatoryRight Create(uint value) => new(value);
}
