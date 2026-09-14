namespace SapphTools.SecurityDescriptor.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class MetaAttribute(string abbr) : Attribute {
    public string Abbr { get; } = abbr;
    public string? Expanded { get; private set; }
    public string? Full { get; init; }
    public MetaAttribute(string abbr, string full) : this(abbr) {
        Full = full;
        Expanded = !full.Contains('\\') ?
            full :
            full.Split('\\')[1];
    }
}