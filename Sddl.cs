using SapphTools.SecurityDescriptor.Classes;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SapphTools.SecurityDescriptor;
public class Sddl {
    private readonly static string TrusteePattern = @"S[0-9-]+|[A-Z]{2}";
    private const string GuidPattern = @"[a-f0-9]{8}-([a-f0-9]{4}-){3}[a-f0-9]{12}";
    private Regex? _acePattern;

    [JsonInclude]
    public string SddlString => ToString();
    [JsonInclude]
    public ObjectType Type { get; private set; }

    [JsonIgnore]
    public Trustee? Owner { get; set; }

    [JsonIgnore]
    public Trustee? Group { get; set; }

    [JsonIgnore]
    public SddlAclFlags? DaclFlags { get; set; }

    [JsonIgnore]
    public List<Ace>? DaclAces { get; private set; }

    [JsonIgnore]
    public SddlAclFlags? SaclFlags { get; set; }

    [JsonIgnore]
    public List<Ace>? SaclAces { get; private set; }

    static Sddl() {
        TrusteePattern = $@"S[0-9-]+|(?:{string.Join("|", MetaExtensions.GetAllAbbr<SidString>())})";
    }
    private Sddl() { }
    [JsonConstructor]
    public Sddl(string sddlString, ObjectType type) {
        Type = type;
        Match match = BuildPattern().Match(sddlString);
        if (!match.Success) {
            throw new ArgumentException("Invalid SDDL string");
        }
        Owner = match.Groups["owner"].Success ? Trustee.Construct(match.Groups["owner"].Value) : null;
        Group = match.Groups["group"].Success ? Trustee.Construct(match.Groups["group"].Value) : null;
        if (match.Groups["dacl_list"].Success) {
            foreach (Capture capture in match.Groups["dacl_flags"].Captures) {
                Regex daclflag = new(string.Join("|", MetaExtensions.GetAllAbbr<SddlAclFlags>()));
                MatchCollection daclMatches = daclflag.Matches(capture.Value);
                foreach (Match daclMatch in daclMatches) {
                    if (!MetaExtensions.TryGetMetaAbbr(daclMatch.Value, out SddlAclFlags daclFlag)) {
                        throw new ArgumentException("Invalid DACL flag");
                    }
                    if (DaclFlags is null) {
                        DaclFlags = daclFlag;
                    } else {
                        DaclFlags |= daclFlag;
                    }
                }
            }
            DaclAces = [];
            MatchCollection aces = _acePattern!.Matches(match.Groups["dacl_list"].Value);
            foreach (Match aceMatch in aces) {
                if (aceMatch.Success) {
                    DaclAces.Add(new Ace(
                        aceMatch.Groups["ace_type"].Value,
                        aceMatch.Groups["ace_flags"].Value,
                        aceMatch.Groups["rights"].Value,
                        aceMatch.Groups["object_guid"].Value,
                        aceMatch.Groups["object_inherit_guid"].Value,
                        aceMatch.Groups["trustee"].Value
                    ));
                }
            }
        }
        if (match.Groups["sacl_list"].Success) {
            foreach (Capture capture in match.Groups["sacl_flags"].Captures) {
                if (!MetaExtensions.TryGetMetaAbbr(capture.Value, out SddlAclFlags saclFlag)) {
                    throw new ArgumentException("Invalid SACL flag");
                }
                if (SaclFlags is null) {
                    SaclFlags = saclFlag;
                } else {
                    SaclFlags |= saclFlag;
                }
            }
            SaclAces = [];
            MatchCollection aces = _acePattern!.Matches(match.Groups["sacl_list"].Value);
            foreach (Match aceMatch in aces) {
                if (aceMatch.Success) {
                    SaclAces.Add(new Ace(
                        aceMatch.Groups["ace_type"].Value,
                        aceMatch.Groups["ace_flags"].Value,
                        aceMatch.Groups["rights"].Value,
                        aceMatch.Groups["object_guid"].Value,
                        aceMatch.Groups["object_inherit_guid"].Value,
                        aceMatch.Groups["trustee"].Value
                    ));
                }
            }
        }
    }
    public void AddDacl(Ace ace) {
        DaclAces ??= [];
        if (DaclAces.Contains(ace)) {
            return;
        }
        DaclAces.Add(ace);
    }
    public Sddl Clone() {
        return new() {
            Type = Type,
            Owner = Owner,
            Group = Group,
            DaclFlags = DaclFlags,
            DaclAces = DaclAces,
            SaclFlags = SaclFlags,
            SaclAces = SaclAces
        };
    }
    public void RemoveDacl(Ace ace) {
        if (DaclAces is null || !DaclAces.Contains(ace)) {
            return;
        }
        DaclAces.Remove(ace);
    }
    public void ReplaceDacl(Ace oldAce, Ace newAce) {
        if (DaclAces is null) {
            DaclAces = [];
            DaclAces.Add(newAce);
            return;
        }
        int oldIdx = DaclAces.IndexOf(oldAce);
        if (oldIdx < 0) {
            DaclAces.Add(newAce);
            return;
        }
        DaclAces[oldIdx] = newAce;
    }
    public FileSecurity ToFileSecurity() {
        return (FileSecurity)ToObjectSecurity();
    }
    public RegistrySecurity ToRegistrySecurity() {
        return (RegistrySecurity)ToObjectSecurity();
    }
    public ObjectSecurity ToObjectSecurity() {
        ObjectSecurity objSecurity = Type switch {
            ObjectType.RegistryKey => new RegistrySecurity(),
            ObjectType.File => new FileSecurity(),
            _ => throw new NotImplementedException(),
        };
        objSecurity.SetSecurityDescriptorSddlForm(ToString());
        return objSecurity;
    }
    public override string ToString() {
        StringBuilder sddl = new();
        if (Owner is not null) {
            sddl.Append($"O:{Owner.SddlSafe}");
        }
        if (Group is not null) {
            sddl.Append($"G:{Group.SddlSafe}");
        }
        if (DaclAces is not null) {
            sddl.Append("D:");
            if (DaclFlags is SddlAclFlags daclFlags) {
                foreach (SddlAclFlags flag in daclFlags.GetFlags()) {
                    sddl.Append(flag.GetAbbr());
                }
            }
            foreach (Ace ace in DaclAces) {
                sddl.Append(ace.ToString());
            }
        }
        if (SaclAces is not null) {
            sddl.Append("S:");
            if (SaclFlags is SddlAclFlags saclFlags) {
                foreach (SddlAclFlags flag in saclFlags.GetFlags()) {
                    sddl.Append(flag.GetAbbr());
                }
            }
            foreach (Ace ace in SaclAces) {
                sddl.Append(ace.ToString());
            }
        }
        return sddl.ToString();
    }
    private Regex BuildPattern() {
        StringBuilder pattern = new();
        pattern.Append($@"^((?:O:)(?<owner>{TrusteePattern}))?");
        pattern.Append($@"((?:G:)(?<group>{TrusteePattern}))?");
        pattern.Append(@"(?<dacl_list>(?:D:)"); //start dacl_list group
        pattern.Append($@"(?<dacl_flags>(?:{string.Join("|", MetaExtensions.GetAllAbbr<SddlAclFlags>())})*)");
        string acePattern = BuildAcePattern();
        _acePattern = new Regex(acePattern, RegexOptions.Compiled);
        pattern.Append(acePattern);
        pattern.Append(@"*)?"); //end dacl_list group
        pattern.Append(@"(?<sacl_list>(?:S:)"); //start sacl_list group
        pattern.Append($@"(?<sacl_flags>(?:{string.Join("|", MetaExtensions.GetAllAbbr<SddlAclFlags>())})*)");
        pattern.Append(acePattern);
        pattern.Append(@"*)?"); //end sacl_list group
        return new Regex(pattern.ToString(), RegexOptions.Compiled);
    }
    private string BuildAcePattern() {
        StringBuilder pattern = new();
        pattern.Append(@"(?<acl_ace>\("); //start acl_ace group
        pattern.Append($@"(?<ace_type>{string.Join("|", MetaExtensions.GetAllAbbr<SddlAceType>())});");
        pattern.Append($@"(?<ace_flags>(?:{string.Join("|", MetaExtensions.GetAllAbbr<SddlAceFlags>())})*);");
        pattern.Append($@"(?<rights>0x[0-9A-Fa-f]+|(?:{BuildRightsString()})+);");
        pattern.Append($@"(?<object_guid>{GuidPattern}|)?;");
        pattern.Append($@"(?<object_inherit_guid>{GuidPattern}|)?;");
        pattern.Append($@"(?<trustee>{TrusteePattern})");
        pattern.Append(@"\))"); //end acl_ace group
        return pattern.ToString();
    }
    private string BuildRightsString() {
        var parts = RightExtensions.GetAllAbbr(ObjectType.Generic)
            .Concat(RightExtensions.GetAllAbbr(ObjectType.Standard))
            .Concat(RightExtensions.GetAllAbbr(Type));
        return string.Join("|", parts);
    }
}