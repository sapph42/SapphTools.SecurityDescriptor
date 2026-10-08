using System.Collections.ObjectModel;

namespace SapphTools.SecurityDescriptor.Classes.Rights;

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
