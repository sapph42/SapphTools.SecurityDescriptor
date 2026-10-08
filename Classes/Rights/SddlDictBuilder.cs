using System.Collections.ObjectModel;

namespace SapphTools.SecurityDescriptor.Classes.Rights;
internal static class SddlDictBuilder {
    internal static readonly ReadOnlyDictionary<ObjectType, ReadOnlyDictionary<uint, SddlRightValue>> ByTypeAndVal;
    internal static readonly ReadOnlyDictionary<ObjectType, ReadOnlyDictionary<string, SddlRightValue>> ByTypeAndAbbr;
    internal static readonly ReadOnlyDictionary<string, SddlRightValue> ByAbbr;
    private static readonly Dictionary<ObjectType, Func<uint, SddlRightValue>> factories = [];

    static SddlDictBuilder() {
        Dictionary<ObjectType, ReadOnlyDictionary<uint, SddlRightValue>> byVal = [];
        Dictionary<ObjectType, ReadOnlyDictionary<string, SddlRightValue>> byAbbr = [];
        Dictionary<string, SddlRightValue> abbreviations = new(StringComparer.Ordinal);

        Register<GenericRight>();
        Register<StandardRight>();
        Register<DirectoryRight>();
        Register<FileRight>();
        Register<RegistryRight>();
        Register<MandatoryRight>();

        ByTypeAndVal = new(byVal);
        ByTypeAndAbbr = new(byAbbr);
        ByAbbr = new(abbreviations);

        void Register<T>() where T : SddlRightValue, ISddlRight<T> {
            byVal.Add(T.Type, new(T.ByVal.ToDictionary(kvp => kvp.Key, kvp => (SddlRightValue)kvp.Value)));
            byAbbr.Add(T.Type, new(T.ByAbbr.ToDictionary(kvp => kvp.Key, kvp => (SddlRightValue)kvp.Value, StringComparer.Ordinal)));
            factories.Add(T.Type, value => T.Create(value));
            foreach (KeyValuePair<string, T> entry in T.ByAbbr) {
                // Every domain has its own NONE; the empty aggregate needs no token.
                if (entry.Key.Length != 0) {
                    abbreviations.Add(entry.Key, entry.Value);
                }
            }
        }
    }

    internal static SddlRightValue? Create(uint value, ObjectType type) =>
        factories.TryGetValue(type, out Func<uint, SddlRightValue>? factory) ? factory(value) : null;
}