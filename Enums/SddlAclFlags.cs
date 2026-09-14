namespace SapphTools.SecurityDescriptor.Enums;
[Flags]
public enum SddlAclFlags {
    [Meta("P")]
    SDDL_PROTECTED = 0x01,

    [Meta("AR")]
    SDDL_AUTO_INHERIT_REQ = 0x02,

    [Meta("AI")]
    SDDL_AUTO_INHERITED = 0x04,

    [Meta("NO_ACCESS_CONTROL")]
    NO_ACCESS_CONTROL = 0x08
}