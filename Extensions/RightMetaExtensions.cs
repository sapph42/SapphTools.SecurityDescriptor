namespace SapphTools.SecurityDescriptor.Extensions;
public static class RightExtensions {
    public static string GetDescription(this SddlRights enumVal) {
        RightMetaAttribute? meta = enumVal.GetAttributeOfType<RightMetaAttribute>() ?? 
            throw new InvalidOperationException("Enum value does not have a RightMetaAttribute");
        return meta.Description ?? "Special";
    }

    public static bool TryGetRight(string abbr, ObjectType type, out SddlRights? right) {
        foreach (SddlRights r in Enum.GetValues(typeof(SddlRights))) {
            RightMetaAttribute? meta = r.GetAttributeOfType<RightMetaAttribute>();
            if (meta is null) {
                continue;
            }
            if (meta.Type == type && meta.Abbr == abbr) {
                right = r;
                return true;
            }
        }
        right = null;
        return false;
    }
    public static IEnumerable<string> GetAllAbbr(ObjectType type) {
        foreach (SddlRights right in Enum.GetValues(typeof(SddlRights))) {
            RightMetaAttribute? meta = right.GetAttributeOfType<RightMetaAttribute>();
            if (meta is null) {
                continue;
            }
            if (meta.Type == type) {
                yield return meta.Abbr;
            }
        }
    }
}