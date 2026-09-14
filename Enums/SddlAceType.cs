namespace SapphTools.SecurityDescriptor.Enums;
public enum SddlAceType {
    [Meta("A")]
    SDDL_ACCESS_ALLOWED = 0x00001,
    [Meta("D")]
    SDDL_ACCESS_DENIED = 0x00002,
    [Meta("OA")]
    SDDL_OBJECT_ACCESS_ALLOWED = 0x00004,
    [Meta("OD")]
    SDDL_OBJECT_ACCESS_DENIED = 0x00008,
    [Meta("AU")]
    SDDL_AUDIT = 0x00010,
    [Meta("AL")]
    SDDL_ALARM = 0x00020,
    [Meta("OU")]
    SDDL_OBJECT_AUDIT = 0x00040,
    [Meta("OL")]
    SDDL_OBJECT_ALARM = 0x00080,
    [Meta("ML")]
    SDDL_MANDATORY_LABEL = 0x00100,
    [Meta("XA")]
    SDDL_CALLBACK_ACCESS_ALLOWED = 0x00200,
    [Meta("XD")]
    SDDL_CALLBACK_ACCESS_DENIED = 0x00400,
    [Meta("RA")]
    SDDL_RESOURCE_ATTRIBUTE = 0x00800,
    [Meta("SP")]
    SDDL_SCOPED_POLICY_ID = 0x01000,
    [Meta("XU")]
    SDDL_CALLBACK_AUDIT = 0x02000,
    [Meta("ZA")]
    SDDL_CALLBACK_OBJECT_ACCESS_ALLOWED = 0x04000,
    [Meta("TL")]
    SDDL_PROCESS_TRUST_LABEL = 0x08000,
    [Meta("FL")]
    SDDL_ACCESS_FILTER = 0x10000
}