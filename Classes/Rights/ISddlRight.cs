using System.Collections.ObjectModel;

namespace SapphTools.SecurityDescriptor.Classes.Rights;
public interface ISddlRight<TSelf> where TSelf : ISddlRight<TSelf> {
    static abstract ObjectType Type { get; }
    static abstract ReadOnlyDictionary<uint, TSelf> ByVal { get; }
    static abstract ReadOnlyDictionary<string, TSelf> ByAbbr { get; }
    static abstract TSelf Create(uint value);
    static ObjectType GetObjectType() => TSelf.Type;
    static ISddlRight<TSelf> ConvertTo(object obj) {
        if (obj is ISddlRight<TSelf> isr) {
            return isr;
        }
        throw new InvalidCastException("obj must be of type ISddlRight<TSelf>");
    }
}