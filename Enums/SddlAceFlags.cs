namespace SapphTools.SecurityDescriptor.Enums;
[Flags]
public enum SddlAceFlags {
    [Meta("CI")]
    SDDL_CONTAINER_INHERIT = 0x0001,
    [Meta("OI")]
    SDDL_OBJECT_INHERIT = 0x0002,
    [Meta("NP")]
    SDDL_NO_PROPAGATE = 0x0004,
    [Meta("IO")]
    SDDL_INHERIT_ONLY = 0x0008,
    [Meta("ID")]
    SDDL_INHERITED = 0x0010,
    [Meta("SA")]
    SDDL_AUDIT_SUCCESS = 0x0020,
    [Meta("FA")]
    SDDL_AUDIT_FAILURE = 0x0040,
    [Meta("TP")]
    SDDL_TRUST_PROTECTED_FILTER = 0x0080,
    [Meta("CR")]
    SDDL_CRITICAL = 0x0100
}