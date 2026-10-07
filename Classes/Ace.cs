using System.Text;

namespace SapphTools.SecurityDescriptor.Classes;
public class Ace : IEquatable<Ace> {
    public SddlAceType Type { get; set; }
    public SddlAceFlags? Flags { get; set; }
    public SddlRight Right { get; set; }
    public Guid? ObjectType { get; set; }
    public Guid? ObjectInheritType { get; set; }
    public Trustee Trustee { get; set; }
    public Ace(SddlAceType ace, SddlAceFlags? flags, SddlRight right, Guid? objectType, Guid? objectInheritType, Trustee trustee) {
        Type = ace;
        Flags = flags;
        Right = right;
        ObjectType = objectType;
        ObjectInheritType = objectInheritType;
        Trustee = trustee;
    }
    public Ace(string type, string flags, string rights, string? objectType, string? objectInheritType, string trustee) {
        if (!MetaExtensions.TryGetMetaAbbr(type, out SddlAceType aceType)) {
            throw new ArgumentException("Invalid ACE type");
        }
        Type = aceType;
        Flags = null;
        for (int i = 0; i < flags.Length; i += 2) {
            string flag = flags.Substring(i, 2);
            if (!MetaExtensions.TryGetMetaAbbr(flag, out SddlAceFlags aceFlag)) {
                throw new ArgumentException("Invalid ACE flag");
            }
            if (Flags is null) {
                Flags = aceFlag;
            } else {
                Flags |= aceFlag;
            }
        }
        Right = SddlRight.Construct(rights);
        _ = Guid.TryParse(objectType, out Guid o);
        ObjectType = o != Guid.Empty ? o : null;
        _ = Guid.TryParse(objectInheritType, out Guid oi);
        ObjectInheritType = oi != Guid.Empty ? oi : null;
        Trustee = Trustee.Construct(trustee);
    }
    public override string ToString() {
        StringBuilder ace = new();
        ace.Append('(');
        ace.Append(Type.GetAbbr());
        ace.Append(';');
        if (Flags is SddlAceFlags aceFlags) {
            foreach (SddlAceFlags flag in aceFlags.GetFlags()) {
                ace.Append(flag.GetAbbr());
            }
        }
        ace.Append(';');
        ace.Append($"{Right}");
        ace.Append(';');
        ace.Append(ObjectType?.ToString("D"));
        ace.Append(';');
        ace.Append(ObjectInheritType?.ToString("D"));
        ace.Append(';');
        ace.Append(Trustee.SddlSafe);
        ace.Append(')');
        return ace.ToString();
    }

    public Ace Clone() {
        return new(Type, Flags, Right.Clone(), ObjectType, ObjectInheritType, Trustee.Clone());
    }
    public bool Equals(Ace? other) {
        if (other is null) {
            return false;
        }
        return Type == other.Type &&
            Flags == other.Flags &&
            Right.Equals(other.Right) &&
            ObjectType.Equals(other.ObjectType) &&
            ObjectInheritType.Equals(other.ObjectInheritType) &&
            Trustee.Equals(other.Trustee);
    }
    public override bool Equals(object? obj) {
        return Equals(obj as Ace);
    }
    public override int GetHashCode() {
        HashCode code = new();
        code.Add(Type);
        code.Add(Flags);
        code.Add(Right);
        code.Add(ObjectType);
        code.Add(ObjectInheritType);
        code.Add(Trustee);
        return code.ToHashCode();
    }
}