namespace SapphTools.SecurityDescriptor.Enums;
public enum SidString {
    [Meta("AA", @"BUILTIN\Access Control Assistance Operators")]
    SDDL_ACCESS_CONTROL_ASSISTANCE_OPS,
    [Meta("AC", @"APPLICATION PACKAGE AUTHORITY\ALL APPLICATION PACKAGES")]
    SDDL_ALL_APP_PACKAGES,
    [Meta("AN", @"NT AUTHORITY\ANONYMOUS LOGON")]
    SDDL_ANONYMOUS,
    [Meta("AO")]
    SDDL_ACCOUNT_OPERATORS,
    [Meta("AU", @"NT AUTHORITY\Authenticated Users")]
    SDDL_AUTHENTICATED_USERS,
    [Meta("BA", @"BUILTIN\Administrators")]
    SDDL_BUILTIN_ADMINISTRATORS,
    [Meta("BG", @"BUILTIN\Guests")]
    SDDL_BUILTIN_GUESTS,
    [Meta("BO", @"BUILTIN\Backup Operators")]
    SDDL_BACKUP_OPERATORS,
    [Meta("BU", @"BUILTIN\Users")]
    SDDL_BUILTIN_USERS,
    [Meta("CA", @"MHS\Cert Publishers")]
    SDDL_CERT_SERV_ADMINISTRATORS,
    [Meta("CD")]
    SDDL_CERTSVC_DCOM_ACCESS,
    [Meta("CG", @"CREATOR GROUP")]
    SDDL_CREATOR_GROUP,
    [Meta("CO", @"CREATOR OWNER")]
    SDDL_CREATOR_OWNER,
    [Meta("CY", @"BUILTIN\Cryptographic Operators")]
    SDDL_CRYPTO_OPERATORS,
    [Meta("DA", @"MHS\Domain Admins")]
    SDDL_DOMAIN_ADMINISTRATORS,
    [Meta("DC", @"MHS\Domain Computers")]
    SDDL_DOMAIN_COMPUTERS,
    [Meta("DD", @"MHS\Domain Controllers")]
    SDDL_DOMAIN_DOMAIN_CONTROLLERS,
    [Meta("DG", @"MHS\Domain Guests")]
    SDDL_DOMAIN_GUESTS,
    [Meta("DU", @"MHS\Domain Users")]
    SDDL_DOMAIN_USERS,
    [Meta("EA", @"MHS\Enterprise Admins")]
    SDDL_ENTERPRISE_ADMINS,
    [Meta("ED", @"NT AUTHORITY\ENTERPRISE DOMAIN CONTROLLERS")]
    SDDL_ENTERPRISE_DOMAIN_CONTROLLERS,
    [Meta("EK", @"MHS\Enterprise Key Admins")]
    SDDL_ENTERPRISE_KEY_ADMINS,
    [Meta("ER", @"BUILTIN\Event Log Readers")]
    SDDL_EVENT_LOG_READERS,
    [Meta("ES")]
    SDDL_RDS_ENDPOINT_SERVERS,
    [Meta("HA", @"BUILTIN\Hyper-V Administrators")]
    SDDL_HYPER_V_ADMINS,
    [Meta("HI")]
    SDDL_ML_HIGH,
    [Meta("HO", @"BUILTIN\User Mode Hardware Operators")]
    SDDL_USER_MODE_HARDWARE_OPERATORS,
    [Meta("IS", @"BUILTIN\IIS_IUSRS")]
    SDDL_IIS_USERS,
    [Meta("IU", @"NT AUTHORITY\INTERACTIVE")]
    SDDL_INTERACTIVE,
    [Meta("KA", @"MHS\Key Admins")]
    SDDL_KEY_ADMINS,
    [Meta("LA", @"EAMCWKGDDM704\xAdministrator")]
    SDDL_LOCAL_ADMIN,
    [Meta("LG", @"EAMCWKGDDM704\xGuest")]
    SDDL_LOCAL_GUEST,
    [Meta("LS", @"NT AUTHORITY\LOCAL SERVICE")]
    SDDL_LOCAL_SERVICE,
    [Meta("LU", @"BUILTIN\Performance Log Users")]
    SDDL_PERFLOG_USERS,
    [Meta("LW")]
    SDDL_ML_LOW,
    [Meta("ME")]
    SDDL_ML_MEDIUM,
    [Meta("MP")]
    SDDL_ML_MEDIUM_PLUS,
    [Meta("MU", @"BUILTIN\Performance Monitor Users")]
    SDDL_PERFMON_USERS,
    [Meta("NO", @"BUILTIN\Network Configuration Operators")]
    SDDL_NETWORK_CONFIGURATION_OPS,
    [Meta("NS", @"NT AUTHORITY\NETWORK SERVICE")]
    SDDL_NETWORK_SERVICE,
    [Meta("NU", @"NT AUTHORITY\NETWORK")]
    SDDL_NETWORK,
    [Meta("OW", @"OWNER RIGHTS")]
    SDDL_OWNER_RIGHTS,
    [Meta("PA", @"MHS\Group Policy Creator Owners")]
    SDDL_GROUP_POLICY_ADMINS,
    [Meta("PO")]
    SDDL_PRINTER_OPERATORS,
    [Meta("PS", @"NT AUTHORITY\SELF")]
    SDDL_PERSONAL_SELF,
    [Meta("PU", @"BUILTIN\Power Users")]
    SDDL_POWER_USERS,
    [Meta("RA")]
    SDDL_RDS_REMOTE_ACCESS_SERVERS,
    [Meta("RC", @"NT AUTHORITY\RESTRICTED")]
    SDDL_RESTRICTED_CODE,
    [Meta("RD", @"BUILTIN\Remote Desktop Users")]
    SDDL_REMOTE_DESKTOP,
    [Meta("RE", @"BUILTIN\Replicator")]
    SDDL_REPLICATOR,
    [Meta("RM", @"BUILTIN\Remote Management Users")]
    SDDL_RMS__SERVICE_OPERATORS,
    [Meta("RO", @"MHS\Enterprise Read-only Domain Controllers")]
    SDDL_ENTERPRISE_RO_DCs,
    [Meta("RS", @"MHS\RAS and IAS Servers")]
    SDDL_RAS_SERVERS,
    [Meta("RU")]
    SDDL_ALIAS_PREW2KCOMPACC,
    [Meta("SA", @"MHS\Schema Admins")]
    SDDL_SCHEMA_ADMINISTRATORS,
    [Meta("SH", @"BUILTIN\OpenSSH Users")]
    SDDL_OPENSSH_USERS,
    [Meta("SI")]
    SDDL_ML_SYSTEM,
    [Meta("SO")]
    SDDL_SERVER_OPERATORS,
    [Meta("SS", @"Service asserted identity")]
    SDDL_SERVICE_ASSERTED,
    [Meta("SU", @"NT AUTHORITY\SERVICE")]
    SDDL_SERVICE,
    [Meta("SY", @"NT AUTHORITY\SYSTEM")]
    SDDL_LOCAL_SYSTEM,
    [Meta("UD", @"NT AUTHORITY\USER MODE DRIVERS")]
    SDDL_USER_MODE_DRIVERS,
    [Meta("WD", @"Everyone")]
    SDDL_EVERYONE,
    [Meta("WR", @"NT AUTHORITY\WRITE RESTRICTED")]
    SDDL_WRITE_RESTRICTED_CODE
}