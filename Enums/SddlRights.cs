namespace SapphTools.SecurityDescriptor.Enums;
[Flags]
public enum SddlRights : uint {
    [RightMeta(ObjectType.Generic, "GA", "Full Control")]
    SDDL_GENERIC_ALL = 0x10000000,

    [RightMeta(ObjectType.Generic, "GR", "Read")]
    SDDL_GENERIC_READ = 0x80000000,

    [RightMeta(ObjectType.Generic, "GW", "Write")]
    SDDL_GENERIC_WRITE = 0x40000000,

    [RightMeta(ObjectType.Generic, "GX", "Execute")]
    SDDL_GENERIC_EXECUTE = 0x20000000,


    [RightMeta(ObjectType.Standard, "RC", "Read permissions")]
    SDDL_READ_CONTROL = 0x00020000,

    [RightMeta(ObjectType.Standard, "SD", "Delete")]
    SDDL_STANDARD_DELETE = 0x00010000,

    [RightMeta(ObjectType.Standard, "WD", "Change permissions")]
    SDDL_WRITE_DAC = 0x00040000,

    [RightMeta(ObjectType.Standard, "WO", "Take ownership")]
    SDDL_WRITE_OWNER = 0x00080000,


    [RightMeta(ObjectType.DirectoryService, "RP", "Read properties")]
    SDDL_READ_PROPERTY = 0x00000010,

    [RightMeta(ObjectType.DirectoryService, "WP", "Write properties")]
    SDDL_WRITE_PROPERTY = 0x00000020,

    [RightMeta(ObjectType.DirectoryService, "CC", "Create child objects")]
    SDDL_CREATE_CHILD = 0x00000001,

    [RightMeta(ObjectType.DirectoryService, "DC", "Delete child objects")]
    SDDL_DELETE_CHILD = 0x00000002,

    [RightMeta(ObjectType.DirectoryService, "LC", "List contents")]
    SDDL_LIST_CHILDREN = 0x00000004,

    [RightMeta(ObjectType.DirectoryService, "SW", "Validated write")]
    SDDL_SELF_WRITE = 0x00000008,

    [RightMeta(ObjectType.DirectoryService, "LO", "List object")]
    SDDL_LIST_OBJECT = 0x00000080,

    [RightMeta(ObjectType.DirectoryService, "DT", "Delete subtree")]
    SDDL_DELETE_TREE = 0x00000040,

    [RightMeta(ObjectType.DirectoryService, "CR", "Extended rights")]
    SDDL_CONTROL_ACCESS = 0x00000100,


    [RightMeta(ObjectType.File, "FA", "Full Control")]
    SDDL_FILE_ALL = 0x001F01FF,

    [RightMeta(ObjectType.File, "FR", "Read")]
    SDDL_FILE_READ = 0x00120089,

    [RightMeta(ObjectType.File, "FW", "Write")]
    SDDL_FILE_WRITE = 0x00120116,

    [RightMeta(ObjectType.File, "FX", "Execute")]
    SDDL_FILE_EXECUTE = 0x001200A0,


    [RightMeta(ObjectType.RegistryKey, "KA", "Full Control")]
    SDDL_KEY_ALL = 0x000F003F,

    [RightMeta(ObjectType.RegistryKey, "KR", "Read")]
    SDDL_KEY_READ = 0x00020019,

    [RightMeta(ObjectType.RegistryKey, "KW", "Write")]
    SDDL_KEY_WRITE = 0x00020006,

    [RightMeta(ObjectType.RegistryKey, "KX", "Read")]
    SDDL_KEY_EXECUTE = 0x00020019,


    [RightMeta(ObjectType.Mandatory, "NR", "No read up")]
    SDDL_NO_READ_UP = 0x00000002,

    [RightMeta(ObjectType.Mandatory, "NW", "No write up")]
    SDDL_NO_WRITE_UP = 0x00000001,

    [RightMeta(ObjectType.Mandatory, "NX", "No execute up")]
    SDDL_NO_EXECUTE_UP = 0x00000004,
}