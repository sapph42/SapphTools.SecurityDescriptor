using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SapphTools.SecurityDescriptor.Classes;
internal static class SddlDictBuilder {
    public static void Build() {
        IEnumerable<Type> instances = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISddlRight<>))
                is not null &&
                t.GetConstructor(Type.EmptyTypes) is not null
            );
        foreach (Type t in instances) {
            RuntimeHelpers.RunClassConstructor(t.TypeHandle);
        }
    }
}
public interface ISddlRight<TSelf> where TSelf : ISddlRight<TSelf> {
    static abstract ObjectType Type { get; }
    static abstract ReadOnlyDictionary<uint, TSelf> ByVal { get; }
    static abstract ReadOnlyDictionary<string, TSelf> ByAbbr { get; }
    static ObjectType GetObjectType() => TSelf.Type;
    static ISddlRight<TSelf> ConvertTo(object obj) {
        if (obj is ISddlRight<TSelf> isr) {
            return isr;
        }
        throw new InvalidCastException("obj must be of type ISddlRight<TSelf>");
    }
}
public abstract class SddlRightValue : IEquatable<SddlRightValue> {
    protected static readonly Dictionary<ObjectType, ReadOnlyDictionary<uint, SddlRightValue>> byVal = [];
    protected static readonly Dictionary<ObjectType, ReadOnlyDictionary<string, SddlRightValue>> byAbbr = [];
    public uint Value { get; init; }
    public string? Abbr { get; init; }
    public static ReadOnlyDictionary<ObjectType, ReadOnlyDictionary<uint, SddlRightValue>> ByTypeAndVal => new(byVal);
    public static ReadOnlyDictionary<ObjectType, ReadOnlyDictionary<string, SddlRightValue>> ByTypeAndAbbr => new(byAbbr);
    protected SddlRightValue() { }
    protected SddlRightValue(uint value, string? abbr = null) { 
        Value = value; 
        Abbr = abbr; 
    }
    public static T Construct<T>(uint value) where T : SddlRightValue, ISddlRight<T> {
        if (Constuct(value, T.Type) as T is T r) {
            return r;
        }
        return (T)Activator.CreateInstance(typeof(T), value, null)!;
    }
    public static T Construct<T>(string abbr) where T : SddlRightValue, ISddlRight<T> {
        if (Construct(abbr, T.Type) as T is T r) {
            return r;
        }
        throw new ArgumentException("No such right exists for the given T.", nameof(abbr));
    }
    public static SddlRightValue? Constuct(uint value, ObjectType type) {
        if (byVal.TryGetValue(type, out ReadOnlyDictionary<uint, SddlRightValue>? dict)) {
            if (dict.TryGetValue(value, out SddlRightValue? right)) {
                return right;
            }
        }
        return null;
    }
    public static SddlRightValue? Construct(string abbr, ObjectType type) {
        if (byAbbr.TryGetValue(type, out ReadOnlyDictionary<string, SddlRightValue>? dict)) {
            if (dict.TryGetValue(abbr, out SddlRightValue? right)) {
                return right;
            }
        }
        return null;
    }
    public override bool Equals(object? obj) => Equals(obj as SddlRightValue);
    public bool Equals(SddlRightValue? other) {
        if (other is null) {
            return false;
        }
        if (!GetType().Equals(other.GetType())) {
            return false;
        }
        return Value == other.Value;
    }
    public override int GetHashCode() {
        HashCode hc = new();
        hc.Add(GetType());
        hc.Add(Value);
        return hc.ToHashCode();
    }
}
public class SddlRight : IEquatable<SddlRight> {
    protected readonly HashSet<SddlRightValue> Rights = [];

    static SddlRight() {
        SddlDictBuilder.Build();
    }

    public uint ToValue() {
        uint val = 0;
        foreach (SddlRightValue value in Rights) {
            val |= value.Value;
        }
        return val;
    }
    public override string ToString() {
        if (Rights.Any(r => string.IsNullOrWhiteSpace(r.Abbr))) {
            return "0x" + ToValue().ToString("X8");
        }
        return string.Join("", Rights.Select(r => r.Abbr));
    }
    public static SddlRight Construct<T>(string? rights) where T : SddlRightValue, ISddlRight<T> {
        SddlRight ret = new();
        if (string.IsNullOrWhiteSpace(rights)) {
            if (SddlRightValue.Construct(string.Empty, T.Type) is SddlRightValue val) {
                ret.Rights.Add(val);
            }
            return ret;
        }
        if (rights.Length % 2 != 0) {
            throw new ArgumentException("An invalid rights string was presented.", nameof(rights));
        }
        Span<char> rightsSpan = new(rights.ToCharArray());
        for (int i = 0; i < rightsSpan.Length; i++) {
            string right = new (rightsSpan.Slice(i*2, 2));
            if (SddlRightValue.Construct(right, T.Type) is SddlRightValue val) {
                ret.Rights.Add(val);
            } else {
                throw new ArgumentException($"One or more rights were not recognized for the provided type ({right}) ", nameof(rights));
            }
        }
        return ret;
    }
    public static SddlRight Construct<T>(uint rights) where T : SddlRightValue, ISddlRight<T> {
        SddlRight ret = new();
        if (rights == 0) {
            if (SddlRightValue.Construct(string.Empty, T.Type) is SddlRightValue val) {
                ret.Rights.Add(val);
            }
            return ret;
        }
        foreach (uint val in T.ByVal.Keys) {
            if ((rights & val) == val) {
                ret.Rights.Add(T.ByVal[val]);
            }
        }
        if (ret.ToValue() == rights) {
            return ret;
        }
        ret.Rights.Clear();
        ret.Rights.Add(SddlRightValue.Construct<T>(rights));
        return ret;
    }

    public override bool Equals(object? obj) => Equals(obj as SddlRight);
    public bool Equals(SddlRight? other) {
        if (other is null) {
            return false;
        }
        return other.GetType().Equals(GetType()) && ToString().Equals(other.ToString()) && ToValue()==other.ToValue();
    }
    public override int GetHashCode() {
        HashCode hc = new();
        foreach (SddlRightValue right in Rights) {
            hc.Add(right.GetHashCode());
        }
        return hc.ToHashCode();
    }
}
public class GenericRight : SddlRightValue, ISddlRight<GenericRight> {
    public static readonly GenericRight SDDL_NONE            = new(0x00000000);
    public static readonly GenericRight SDDL_GENERIC_ALL     = new(0x10000000, "GA");
    public static readonly GenericRight SDDL_GENERIC_EXECUTE = new(0x20000000, "GX");
    public static readonly GenericRight SDDL_GENERIC_WRITE   = new(0x40000000, "GW");
    public static readonly GenericRight SDDL_GENERIC_READ    = new(0x80000000, "GR");
    public static ReadOnlyDictionary<uint, SddlRightValue> ByVal => new(new Dictionary<uint, SddlRightValue>() {
        { SDDL_GENERIC_ALL.Value, SDDL_GENERIC_ALL },
        { SDDL_GENERIC_EXECUTE.Value, SDDL_GENERIC_EXECUTE },
        { SDDL_GENERIC_WRITE.Value, SDDL_GENERIC_WRITE },
        { SDDL_GENERIC_READ.Value, SDDL_GENERIC_READ }
    });
    public static ReadOnlyDictionary<string, SddlRightValue> ByAbbr => new(new Dictionary<string, SddlRightValue>() {
        { string.Empty, SDDL_NONE },
        { SDDL_GENERIC_ALL.Abbr!, SDDL_GENERIC_ALL },
        { SDDL_GENERIC_EXECUTE.Abbr!, SDDL_GENERIC_EXECUTE },
        { SDDL_GENERIC_WRITE.Abbr!, SDDL_GENERIC_WRITE },
        { SDDL_GENERIC_READ.Abbr!, SDDL_GENERIC_READ }
    });
    public static ObjectType Type => ObjectType.Generic;
    static ReadOnlyDictionary<uint, GenericRight> ISddlRight<GenericRight>.ByVal => new(ByVal.Select(kvp => new KeyValuePair<uint, GenericRight>(kvp.Key, (GenericRight)kvp.Value)).ToDictionary());
    static ReadOnlyDictionary<string, GenericRight> ISddlRight<GenericRight>.ByAbbr => new(ByAbbr.Select(kvp => new KeyValuePair<string, GenericRight>(kvp.Key, (GenericRight)kvp.Value)).ToDictionary());

    static GenericRight() {
        SddlDictBuilder.Build();
        byVal.Add(Type, ByVal);
        byAbbr.Add(Type, ByAbbr);
    }
    protected GenericRight() : base() { }
    protected GenericRight(uint value, string? abbr = null) : base(value, abbr) { }
    public static ObjectType GetObjectType<GenericRight>() => Type;
    public override bool Equals(object? obj) {
        if (obj is null) {
            return false;
        }
        if (obj is GenericRight mr) {
            return Equals(mr);
        }
        return base.Equals(obj);
    }
    public bool Equals(GenericRight? other) {
        if (other is null) {
            return false;
        }
        return string.Equals(Abbr, other.Abbr) && Value == other.Value;
    }
    public override int GetHashCode() {
        HashCode hc = new();
        hc.Add(Value);
        hc.Add(Abbr);
        return hc.ToHashCode();
    }
}
public class StandardRight : SddlRightValue, ISddlRight<StandardRight> {
    public static readonly StandardRight SDDL_NONE            = new(0x00000000);
    public static readonly StandardRight SDDL_STANDARD_DELETE = new(0x00010000, "SD");
    public static readonly StandardRight SDDL_READ_CONTROL    = new(0x00020000, "RC");
    public static readonly StandardRight SDDL_WRITE_DAC       = new(0x00040000, "WD");
    public static readonly StandardRight SDDL_WRITE_OWNER     = new(0x00080000, "WO");
    public static ReadOnlyDictionary<uint, SddlRightValue> ByVal => new(new Dictionary<uint, SddlRightValue>() {
        { SDDL_STANDARD_DELETE.Value, SDDL_STANDARD_DELETE },
        { SDDL_READ_CONTROL.Value, SDDL_READ_CONTROL },
        { SDDL_WRITE_DAC.Value, SDDL_WRITE_DAC },
        { SDDL_WRITE_OWNER.Value, SDDL_WRITE_OWNER }
    });
    public static ReadOnlyDictionary<string, SddlRightValue> ByAbbr => new(new Dictionary<string, SddlRightValue>() {
        { string.Empty, SDDL_NONE },
        { SDDL_STANDARD_DELETE.Abbr!, SDDL_STANDARD_DELETE },
        { SDDL_READ_CONTROL.Abbr!, SDDL_READ_CONTROL },
        { SDDL_WRITE_DAC.Abbr!, SDDL_WRITE_DAC },
        { SDDL_WRITE_OWNER.Abbr!, SDDL_WRITE_OWNER }
    });
    public static ObjectType Type => ObjectType.Standard;
    static ReadOnlyDictionary<uint, StandardRight> ISddlRight<StandardRight>.ByVal => new(ByVal.Select(kvp => new KeyValuePair<uint, StandardRight>(kvp.Key, (StandardRight)kvp.Value)).ToDictionary());
    static ReadOnlyDictionary<string, StandardRight> ISddlRight<StandardRight>.ByAbbr => new(ByAbbr.Select(kvp => new KeyValuePair<string, StandardRight>(kvp.Key, (StandardRight)kvp.Value)).ToDictionary());

    static StandardRight() {
        SddlDictBuilder.Build();
        byVal.Add(Type, ByVal);
        byAbbr.Add(Type, ByAbbr);
    }
    protected StandardRight() : base() { }
    protected StandardRight(uint value, string? abbr = null) : base(value, abbr) { }
    public static ObjectType GetObjectType<GenericRight>() => Type;
    public override bool Equals(object? obj) {
        if (obj is null) {
            return false;
        }
        if (obj is StandardRight mr) {
            return Equals(mr);
        }
        return base.Equals(obj);
    }
    public bool Equals(StandardRight? other) {
        if (other is null) {
            return false;
        }
        return string.Equals(Abbr, other.Abbr) && Value == other.Value;
    }
    public override int GetHashCode() {
        HashCode hc = new();
        hc.Add(Value);
        hc.Add(Abbr);
        return hc.ToHashCode();
    }
}
public class DirectoryRight : SddlRightValue, ISddlRight<DirectoryRight> {
    public static readonly DirectoryRight SDDL_NONE            = new(0x00000000);
    public static readonly DirectoryRight SDDL_CREATE_CHILD    = new(0x00000001, "CC");
    public static readonly DirectoryRight SDDL_DELETE_CHILD    = new(0x00000002, "DC");
    public static readonly DirectoryRight SDDL_LIST_CHILDREN   = new(0x00000004, "LC");
    public static readonly DirectoryRight SDDL_SELF_WRITE      = new(0x00000008, "SW");
    public static readonly DirectoryRight SDDL_READ_PROPERTY   = new(0x00000010, "RP");
    public static readonly DirectoryRight SDDL_WRITE_PROPERTY  = new(0x00000020, "WP");
    public static readonly DirectoryRight SDDL_DELETE_TREE     = new(0x00000040, "DT");
    public static readonly DirectoryRight SDDL_LIST_OBJECT     = new(0x00000080, "LO");
    public static readonly DirectoryRight SDDL_CONTROL_ACCESS  = new(0x00000100, "CR");
    public static ReadOnlyDictionary<uint, SddlRightValue> ByVal => new(new Dictionary<uint, SddlRightValue>() {
        { SDDL_CREATE_CHILD.Value, SDDL_CREATE_CHILD },
        { SDDL_DELETE_CHILD.Value, SDDL_DELETE_CHILD },
        { SDDL_LIST_CHILDREN.Value, SDDL_LIST_CHILDREN },
        { SDDL_SELF_WRITE.Value, SDDL_SELF_WRITE },
        { SDDL_READ_PROPERTY.Value, SDDL_READ_PROPERTY },
        { SDDL_WRITE_PROPERTY.Value, SDDL_WRITE_PROPERTY },
        { SDDL_DELETE_TREE.Value, SDDL_DELETE_TREE },
        { SDDL_LIST_OBJECT.Value, SDDL_LIST_OBJECT },
        { SDDL_CONTROL_ACCESS.Value, SDDL_CONTROL_ACCESS },
    });
    public static ReadOnlyDictionary<string, SddlRightValue> ByAbbr => new(new Dictionary<string, SddlRightValue>() {
        { string.Empty, SDDL_NONE },
        { SDDL_CREATE_CHILD.Abbr!, SDDL_CREATE_CHILD },
        { SDDL_DELETE_CHILD.Abbr!, SDDL_DELETE_CHILD },
        { SDDL_LIST_CHILDREN.Abbr!, SDDL_LIST_CHILDREN },
        { SDDL_SELF_WRITE.Abbr!, SDDL_SELF_WRITE },
        { SDDL_READ_PROPERTY.Abbr!, SDDL_READ_PROPERTY },
        { SDDL_WRITE_PROPERTY.Abbr!, SDDL_WRITE_PROPERTY },
        { SDDL_DELETE_TREE.Abbr!, SDDL_DELETE_TREE },
        { SDDL_LIST_OBJECT.Abbr!, SDDL_LIST_OBJECT },
        { SDDL_CONTROL_ACCESS.Abbr!, SDDL_CONTROL_ACCESS },
    });
    public static ObjectType Type => ObjectType.Standard;
    static ReadOnlyDictionary<uint, DirectoryRight> ISddlRight<DirectoryRight>.ByVal => new(ByVal.Select(kvp => new KeyValuePair<uint, DirectoryRight>(kvp.Key, (DirectoryRight)kvp.Value)).ToDictionary());
    static ReadOnlyDictionary<string, DirectoryRight> ISddlRight<DirectoryRight>.ByAbbr => new(ByAbbr.Select(kvp => new KeyValuePair<string, DirectoryRight>(kvp.Key, (DirectoryRight)kvp.Value)).ToDictionary());

    static DirectoryRight() {
        SddlDictBuilder.Build();
        byVal.Add(Type, ByVal);
        byAbbr.Add(Type, ByAbbr);
    }
    protected DirectoryRight() : base() { }
    protected DirectoryRight(uint value, string? abbr = null) : base(value, abbr) { }
    public static ObjectType GetObjectType<GenericRight>() => Type;
    public override bool Equals(object? obj) {
        if (obj is null) {
            return false;
        }
        if (obj is DirectoryRight mr) {
            return Equals(mr);
        }
        return base.Equals(obj);
    }
    public bool Equals(DirectoryRight? other) {
        if (other is null) {
            return false;
        }
        return string.Equals(Abbr, other.Abbr) && Value == other.Value;
    }
    public override int GetHashCode() {
        HashCode hc = new();
        hc.Add(Value);
        hc.Add(Abbr);
        return hc.ToHashCode();
    }
}
public class FileRight : SddlRightValue, ISddlRight<FileRight> {
    public static readonly FileRight SDDL_NONE         = new(0x00000000);
    public static readonly FileRight SDDL_FILE_ALL     = new(0x001F01FF, "FA");
    public static readonly FileRight SDDL_FILE_READ    = new(0x00120089, "FR");
    public static readonly FileRight SDDL_FILE_WRITE   = new(0x00120116, "FW");
    public static readonly FileRight SDDL_FILE_EXECUTE = new(0x001200A0, "FX");
    public static ReadOnlyDictionary<uint, SddlRightValue> ByVal => new(new Dictionary<uint, SddlRightValue>() {
        { SDDL_FILE_ALL.Value, SDDL_FILE_ALL },
        { SDDL_FILE_READ.Value, SDDL_FILE_READ },
        { SDDL_FILE_WRITE.Value, SDDL_FILE_WRITE },
        { SDDL_FILE_EXECUTE.Value, SDDL_FILE_EXECUTE }
    });
    public static ReadOnlyDictionary<string, SddlRightValue> ByAbbr => new(new Dictionary<string, SddlRightValue>() {
        { string.Empty, SDDL_NONE },
        { SDDL_FILE_ALL.Abbr!, SDDL_FILE_ALL },
        { SDDL_FILE_READ.Abbr!, SDDL_FILE_READ },
        { SDDL_FILE_WRITE.Abbr!, SDDL_FILE_WRITE },
        { SDDL_FILE_EXECUTE.Abbr!, SDDL_FILE_EXECUTE }
    });
    public static ObjectType Type => ObjectType.Standard;
    static ReadOnlyDictionary<uint, FileRight> ISddlRight<FileRight>.ByVal => new(ByVal.Select(kvp => new KeyValuePair<uint, FileRight>(kvp.Key, (FileRight)kvp.Value)).ToDictionary());
    static ReadOnlyDictionary<string, FileRight> ISddlRight<FileRight>.ByAbbr => new(ByAbbr.Select(kvp => new KeyValuePair<string, FileRight>(kvp.Key, (FileRight)kvp.Value)).ToDictionary());

    static FileRight() {
        SddlDictBuilder.Build();
        byVal.Add(Type, ByVal);
        byAbbr.Add(Type, ByAbbr);
    }
    protected FileRight() : base() { }
    protected FileRight(uint value, string? abbr = null) : base(value, abbr) { }
    public static ObjectType GetObjectType<GenericRight>() => Type;
    public override bool Equals(object? obj) {
        if (obj is null) {
            return false;
        }
        if (obj is FileRight mr) {
            return Equals(mr);
        }
        return base.Equals(obj);
    }
    public bool Equals(FileRight? other) {
        if (other is null) {
            return false;
        }
        return string.Equals(Abbr, other.Abbr) && Value == other.Value;
    }
    public override int GetHashCode() {
        HashCode hc = new();
        hc.Add(Value);
        hc.Add(Abbr);
        return hc.ToHashCode();
    }
}
public class RegistryRight : SddlRightValue, ISddlRight<RegistryRight> {
    public static readonly RegistryRight SDDL_NONE        = new(0x00000000);
    public static readonly RegistryRight SDDL_KEY_ALL     = new(0x000F003F, "KA");
    public static readonly RegistryRight SDDL_KEY_READ    = new(0x00020019, "KR");
    public static readonly RegistryRight SDDL_KEY_WRITE   = new(0x00020006, "KW");
    public static readonly RegistryRight SDDL_KEY_EXECUTE = new(0x00020019, "KX");
    public static ReadOnlyDictionary<uint, SddlRightValue> ByVal => new(new Dictionary<uint, SddlRightValue>() {
        { SDDL_KEY_ALL.Value, SDDL_KEY_ALL },
        { SDDL_KEY_READ.Value, SDDL_KEY_READ },
        { SDDL_KEY_WRITE.Value, SDDL_KEY_WRITE },
        { SDDL_KEY_EXECUTE.Value, SDDL_KEY_EXECUTE }
    });
    public static ReadOnlyDictionary<string, SddlRightValue> ByAbbr => new(new Dictionary<string, SddlRightValue>() {
        { string.Empty, SDDL_NONE },
        { SDDL_KEY_ALL.Abbr!, SDDL_KEY_ALL },
        { SDDL_KEY_READ.Abbr!, SDDL_KEY_READ },
        { SDDL_KEY_WRITE.Abbr!, SDDL_KEY_WRITE },
        { SDDL_KEY_EXECUTE.Abbr!, SDDL_KEY_EXECUTE }
    });
    public static ObjectType Type => ObjectType.Standard;
    static ReadOnlyDictionary<uint, RegistryRight> ISddlRight<RegistryRight>.ByVal => new(ByVal.Select(kvp => new KeyValuePair<uint, RegistryRight>(kvp.Key, (RegistryRight)kvp.Value)).ToDictionary());
    static ReadOnlyDictionary<string, RegistryRight> ISddlRight<RegistryRight>.ByAbbr => new(ByAbbr.Select(kvp => new KeyValuePair<string, RegistryRight>(kvp.Key, (RegistryRight)kvp.Value)).ToDictionary());

    static RegistryRight() {
        SddlDictBuilder.Build();
        byVal.Add(Type, ByVal);
        byAbbr.Add(Type, ByAbbr);
    }
    protected RegistryRight() : base() { }
    protected RegistryRight(uint value, string? abbr = null) : base(value, abbr) { }
    public static ObjectType GetObjectType<GenericRight>() => Type;
    public override bool Equals(object? obj) {
        if (obj is null) {
            return false;
        }
        if (obj is RegistryRight mr) {
            return Equals(mr);
        }
        return base.Equals(obj);
    }
    public bool Equals(RegistryRight? other) {
        if (other is null) {
            return false;
        }
        return string.Equals(Abbr, other.Abbr) && Value == other.Value;
    }
    public override int GetHashCode() {
        HashCode hc = new();
        hc.Add(Value);
        hc.Add(Abbr);
        return hc.ToHashCode();
    }
}
public class MandatoryRight : SddlRightValue, ISddlRight<MandatoryRight> {
    public static readonly MandatoryRight SDDL_NONE          = new(0x00000000);
    public static readonly MandatoryRight SDDL_NO_WRITE_UP   = new(0x00000001, "NW");
    public static readonly MandatoryRight SDDL_NO_READ_UP    = new(0x00000002, "NR");
    public static readonly MandatoryRight SDDL_NO_EXECUTE_UP = new(0x00000004, "NX");
    public static ReadOnlyDictionary<uint, SddlRightValue> ByVal => new(new Dictionary<uint, SddlRightValue>() {
        { SDDL_NO_WRITE_UP.Value, SDDL_NO_WRITE_UP },
        { SDDL_NO_READ_UP.Value, SDDL_NO_READ_UP },
        { SDDL_NO_EXECUTE_UP.Value, SDDL_NO_EXECUTE_UP }
    });
    public static ReadOnlyDictionary<string, SddlRightValue> ByAbbr => new(new Dictionary<string, SddlRightValue>() {
        { string.Empty, SDDL_NONE },
        { SDDL_NO_WRITE_UP.Abbr!, SDDL_NO_WRITE_UP },
        { SDDL_NO_READ_UP.Abbr!, SDDL_NO_READ_UP },
        { SDDL_NO_EXECUTE_UP.Abbr!, SDDL_NO_EXECUTE_UP }
    });
    public static ObjectType Type => ObjectType.Mandatory;
    static ReadOnlyDictionary<uint, MandatoryRight> ISddlRight<MandatoryRight>.ByVal => new (ByVal.Select(kvp => new KeyValuePair<uint, MandatoryRight>(kvp.Key, (MandatoryRight)kvp.Value)).ToDictionary());
    static ReadOnlyDictionary<string, MandatoryRight> ISddlRight<MandatoryRight>.ByAbbr => new(ByAbbr.Select(kvp => new KeyValuePair<string, MandatoryRight>(kvp.Key, (MandatoryRight)kvp.Value)).ToDictionary());

    static MandatoryRight() {
        SddlDictBuilder.Build();
        byVal.Add(Type, ByVal);
        byAbbr.Add(Type, ByAbbr);
    }
    protected MandatoryRight() : base() { }
    protected MandatoryRight(uint value, string? abbr = null) : base(value, abbr) { }
    public static ObjectType GetObjectType<MandatoryRight>() => Type;
    public override bool Equals(object? obj) {
        if (obj is null) {
            return false;
        }
        if (obj is MandatoryRight mr) {
            return Equals(mr);
        }
        return base.Equals(obj);
    }
    public bool Equals(MandatoryRight? other) {
        if (other is null) {
            return false;
        }
        return string.Equals(Abbr, other.Abbr) && Value == other.Value;
    }
    public override int GetHashCode() {
        HashCode hc = new();
        hc.Add(Value);
        hc.Add(Abbr);
        return hc.ToHashCode();
    }
}
