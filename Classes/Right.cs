using System.Globalization;
using System.Text.RegularExpressions;

namespace SapphTools.SecurityDescriptor.Classes;
public class Right : IEquatable<Right> {
    public string Value { get; private set; }
    private Right(string value) {
        Value = value;
    }
    public static Right Construct(SddlRights sddlRights) {
        return new(sddlRights.GetAttributeOfType<MetaAttribute>()!.Abbr);
    }
    public static Right Construct(uint rightsMask) {
        return new(rightsMask.ToString("X8"));
    }
    public static Right Construct(string right) {
        if (right.StartsWith("0x") && uint.TryParse(right.AsSpan(2), NumberStyles.HexNumber, null, out uint hex)) {
            return Construct(hex);
        } else {
            Regex rightsPattern = new(string.Join("|", MetaExtensions.GetAllAbbr<SddlRights>()));
            MatchCollection rights = rightsPattern.Matches(right);
            if (rights.Count == 0) {
                throw new ArgumentException("Invalid rights string");
            }
            SddlRights? sddlRights = null;
            foreach (Match rightsMatch in rights) {
                if (!MetaExtensions.TryGetMetaAbbr(rightsMatch.Value, out SddlRights rightFlag)) {
                    throw new ArgumentException("Invalid SDDL right");
                }
                if (sddlRights is null) {
                    sddlRights = rightFlag;
                } else {
                    sddlRights |= rightFlag;
                }
            }
            return Construct(sddlRights!.Value);
        }
    }
    public uint GetHexValue() {
        if (Value.StartsWith("0x") && 
                uint.TryParse(
                    Value.AsSpan(2), 
                    NumberStyles.HexNumber, 
                    null, 
                    out uint hex)
                ) {
            return hex;
        }
        Regex rightsPattern = new(string.Join("|", MetaExtensions.GetAllAbbr<SddlRights>()));
        MatchCollection rights = rightsPattern.Matches(Value);
        SddlRights? sddlRights = null;
        foreach (Match rightsMatch in rights) {
            if (!MetaExtensions.TryGetMetaAbbr(rightsMatch.Value, out SddlRights rightFlag)) {
                throw new ArgumentException("Invalid SDDL right");
            }
            if (sddlRights is null) {
                sddlRights = rightFlag;
            } else {
                sddlRights |= rightFlag;
            }
        }
        if (sddlRights is null) {
            return 0;
        }
        return (uint)sddlRights;
    }
    public bool Equals(Right? other) {
        if (other is null) {
            return false;
        }
        if (Value.Equals(other.Value)) {
            return true;
        }
        return GetHexValue() == other.GetHexValue();
    }
    public override bool Equals(object? obj) {
        return Equals(obj as Right);
    }
    public override string ToString() {
        if (Value.StartsWith("0x")) {
            return $"0x{GetHexValue()}";
        } else {
            return Value;
        }
    }
    public override int GetHashCode() =>
        GetHexValue().GetHashCode();
}