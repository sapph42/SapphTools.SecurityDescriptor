namespace SapphTools.SecurityDescriptor.Attributes;
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class RightMetaAttribute(ObjectType type, string abbr, string? desc = null) : MetaAttribute(abbr) {
    public string? Description { get; set; } = desc;
    public ObjectType Type { get; } = type;
}