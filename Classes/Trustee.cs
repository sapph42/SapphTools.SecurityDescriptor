using SapphTools.SecurityDescriptor.Converters;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SapphTools.SecurityDescriptor.Classes;
[JsonConverter(typeof(TrusteeConverter))]
public partial class Trustee : IEquatable<Trustee> {
    public enum TrusteeDisplayType {
        User,
        Group,
        WellKnown,
        Unknown
    }
    public string Sid { get; private set; }
    public Bitmap DisplayImage {
        get {
            return GetDisplayType() switch {
                TrusteeDisplayType.User => Properties.Resources.user,
                TrusteeDisplayType.Group => Properties.Resources.group,
                TrusteeDisplayType.WellKnown => Properties.Resources.wellknown,
                _ => Properties.Resources.sid
            };
        }
    }
    public required string SddlSafe { get; init; }
    public required SecurityIdentifier NativeSid { get; init; }

    public string DisplayString { get; init; }
    private Trustee(string sid) {
        Sid = sid;
        DisplayString = GetDisplay();
    }
    public Trustee Clone() {
        return new(Sid) {
            SddlSafe = SddlSafe,
            NativeSid = NativeSid
        };
    }
    public string GetDisplay() {
        try {
            return new SecurityIdentifier(Sid).Translate(typeof(NTAccount)).Value;
        } catch (Exception ex) when (ex is IdentityNotMappedException or ArgumentException) {
            return Sid;
        }
    }
    public TrusteeDisplayType GetDisplayType() {
        SecurityIdentifier sid = new(Sid);
        string sidVal = sid.Value;
        if (WellKnownPattern().IsMatch(sidVal)) {
            return TrusteeDisplayType.WellKnown;
        }
        if (!TryLookupSid(sid, out string? name, out SidNameUse use)) {
            return TrusteeDisplayType.Unknown;
        }
        return use switch {
            SidNameUse.User =>
                name?.EndsWith('$') == true ?
                    TrusteeDisplayType.Group :
                    TrusteeDisplayType.User,
            SidNameUse.Group or 
                SidNameUse.Alias or 
                SidNameUse.WellKnownGroup => 
                    TrusteeDisplayType.Group,
            _ when sid.IsWellKnown(WellKnownSidType.LocalSystemSid) ||
                sid.IsWellKnown(WellKnownSidType.LocalServiceSid) ||
                sid.IsWellKnown(WellKnownSidType.NetworkServiceSid) =>
                    TrusteeDisplayType.WellKnown,
            _ => TrusteeDisplayType.Unknown
        };
    }
    public static IEnumerable<string> EnumerateWellKnownPrincipals() {
        foreach (SidString s in Enum.GetValues(typeof(SidString))) {
            if (s.GetFull() is string name) { 
                yield return name;
            }
        }
    }

    public static Trustee Construct(SecurityIdentifier sid) {
        return new(sid.ToString()) {
            SddlSafe = sid.ToString(),
            NativeSid = sid
        };
    }
    public static Trustee Construct(string sid) {
        if (MetaExtensions.TryGetMetaAbbr<SidString>(sid, out _)) {
            return new(sid) {
                SddlSafe = sid,
                NativeSid = new(sid)
            };
        } else if (MetaExtensions.TryGetMetaFull(sid, out SidString ss1)) {
            string abbr = ss1.GetAbbr();
            if (TryParseSid(abbr, out SecurityIdentifier? securityIdentifier)) {
                return new(abbr) {
                    SddlSafe = securityIdentifier.ToString(),
                    NativeSid = securityIdentifier
                };
            } else {
                throw new ArgumentException("Argument could not be resolved to a SecurityIdentifier", nameof(sid));
            }
        } else if (MetaExtensions.TryGetMetaExpanded(sid, out SidString ss2)) {
            string abbr = ss2.GetAbbr();
            if (TryParseSid(abbr, out SecurityIdentifier? securityIdentifier)) {
                return new(abbr) {
                    SddlSafe = abbr,
                    NativeSid = securityIdentifier
                };
            } else {
                throw new ArgumentException("Argument could not be resolved to a SecurityIdentifier", nameof(sid));
            }
        } else if (TryParseSid(sid, out SecurityIdentifier? securityIdentifier)) {
            return Construct(securityIdentifier);
        } else {
            throw new ArgumentException("Invalid SID");
        }
    }
    private static bool TryLookupSid(SecurityIdentifier sid, out string? accountName, out SidNameUse use) {
        byte[] sidBytes = new byte[sid.BinaryLength];
        sid.GetBinaryForm(sidBytes, 0);
        uint nameLength = 0;
        uint domainLength = 0;
        LookupAccountSid(null, sidBytes, null, ref nameLength, null, ref domainLength, out _);
        int error = Marshal.GetLastWin32Error();
        if (error != 122) { 
            accountName = null;
            use = SidNameUse.Unknown;
            return false;
        }
        StringBuilder name = new((int)nameLength);
        StringBuilder domain = new((int)domainLength);
        if (!LookupAccountSid(
            null,
            sidBytes,
            name,
            ref nameLength,
            domain,
            ref domainLength,
            out use
        )) {
            accountName = null;
            use = SidNameUse.Unknown;
            return false;
        }
        accountName = name.ToString();
        return true;
    }
    private static bool TryParseSid(string value, [NotNullWhen(true)] out SecurityIdentifier? sid) {
        try {
            sid = new SecurityIdentifier(value);
            return true;
        } catch (ArgumentException) {
            sid = null;
            return false;
        }
    }

    public bool Equals(Trustee? other) {
        if (other is null) {
            return false;
        }
        if (TryParseSid(Sid, out SecurityIdentifier? thisSid) &&
                TryParseSid(other.Sid, out SecurityIdentifier? otherSid)
            ) {
            return thisSid.Equals(otherSid);
        }
        return Sid.Equals(other.Sid, StringComparison.OrdinalIgnoreCase);
    }
    public override bool Equals(object? obj) {
        return Equals(obj as Trustee);
    }
    public override int GetHashCode() {
        if (TryParseSid(Sid, out SecurityIdentifier? thisSid)) {
            return thisSid.GetHashCode();
        }
        return Sid.GetHashCode();
    }

    private enum SidNameUse {
        User = 1,
        Group,
        Domain,
        Alias,
        WellKnownGroup,
        DeletedAccount,
        Invalid,
        Unknown,
        Computer,
        Label,
        LogonSession
    }
    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool LookupAccountSid(
        string? lpSystemName,
        byte[] Sid,
        StringBuilder? name,
        ref uint cchName,
        StringBuilder? ReferenceDomainName,
        ref uint cchReferencedDomainName,
        out SidNameUse peUse
    );

    [GeneratedRegex(@"S-1-(3|5-80|15|16)-.*")]
    private partial Regex WellKnownPattern();
}