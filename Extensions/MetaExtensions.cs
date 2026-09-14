using System.Diagnostics.CodeAnalysis;

namespace SapphTools.SecurityDescriptor.Extensions;
public static class MetaExtensions {
    public static bool TryGetMetaAbbr<T>(string abbr, [NotNullWhen(true)] out T? enumVal) where T : Enum {
        foreach (T e in Enum.GetValues(typeof(T))) {
            MetaAttribute? meta = e.GetAttributeOfType<MetaAttribute>();
            if (meta is null) {
                continue;
            }
            if (meta.Abbr == abbr) {
                enumVal = e;
                return true;
            }
        }
        enumVal = default;
        return false;
    }
    public static bool TryGetMetaExpanded<T>(string expanded, [NotNullWhen(true)] out T? enumVal) where T : Enum {
        foreach (T e in Enum.GetValues(typeof(T))) {
            MetaAttribute? meta = e.GetAttributeOfType<MetaAttribute>();
            if (meta is null) {
                continue;
            }
            if (meta.Expanded == expanded) {
                enumVal = e;
                return true;
            }
        }
        enumVal = default;
        return false;
    }
    public static bool TryGetMetaFull<T>(string full, [NotNullWhen(true)] out T? enumVal) where T : Enum {
        foreach (T e in Enum.GetValues(typeof(T))) {
            MetaAttribute? meta = e.GetAttributeOfType<MetaAttribute>();
            if (meta is null) {
                continue;
            }
            if (meta.Full == full) {
                enumVal = e;
                return true;
            }
        }
        enumVal = default;
        return false;
    }
    public static string GetAbbr(this Enum enumVal) {
        MetaAttribute? meta = enumVal.GetAttributeOfType<MetaAttribute>();
        return meta is null ? throw new InvalidOperationException("Enum value does not have a MetaAttribute") : meta.Abbr;
    }
    public static string? GetExpanded(this Enum enumVal) {
        MetaAttribute? meta = enumVal.GetAttributeOfType<MetaAttribute>();
        return meta is null ? throw new InvalidOperationException("Enum value does not have a MetaAttribute") : meta.Expanded;
    }
    public static string? GetFull(this Enum enumVal) {
        MetaAttribute? meta = enumVal.GetAttributeOfType<MetaAttribute>();
        return meta is null ? throw new InvalidOperationException("Enum value does not have a MetaAttribute") : meta.Full;
    }
    public static IEnumerable<string> GetAllAbbr<T>() where T : Enum {
        foreach (T e in Enum.GetValues(typeof(T))) {
            MetaAttribute? meta = e.GetAttributeOfType<MetaAttribute>();
            if (meta is null) {
                continue;
            }
            yield return meta.Abbr;
        }
    }
    public static IEnumerable<string> GetAllExpanded<T>() where T : Enum {
        foreach (T e in Enum.GetValues(typeof(T))) {
            MetaAttribute? meta = e.GetAttributeOfType<MetaAttribute>();
            if (meta is null) {
                continue;
            }
            if (meta.Expanded is not null) {
                yield return meta.Expanded;
            }
        }
    }
    public static IEnumerable<string> GetAllFull<T>() where T : Enum {
        foreach (T e in Enum.GetValues(typeof(T))) {
            MetaAttribute? meta = e.GetAttributeOfType<MetaAttribute>();
            if (meta is null) {
                continue;
            }
            if (meta.Full is not null) {
                yield return meta.Full;
            }
        }
    }
}