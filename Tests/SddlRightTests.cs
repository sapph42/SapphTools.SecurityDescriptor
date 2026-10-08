using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using SapphTools.SecurityDescriptor.Attributes;
using SapphTools.SecurityDescriptor.Classes;
using SapphTools.SecurityDescriptor.Enums;
using SapphTools.SecurityDescriptor.Extensions;

namespace SapphTools.SecurityDescriptor.Tests;

[TestClass]
public class SddlRightTests {
    [TestMethod]
    public void NamedDescriptionsPreserveEachOriginalEnumFieldsMetadata() {
        int checkedRights = 0;
        // Read fields directly: enum values cannot distinguish CC/NW or KR/KX.
        foreach (FieldInfo field in typeof(SddlRights).GetFields(BindingFlags.Public | BindingFlags.Static)) {
            RightMetaAttribute? meta = field.GetCustomAttribute<RightMetaAttribute>();
            if (meta is null) {
                continue;
            }
            SddlRightValue right = SddlRightValue.ByAbbreviation[meta.Abbr];
            Assert.AreEqual(meta.Description, right.Description, field.Name);
            Assert.AreEqual(meta.Description, right.GetDescription(), field.Name);
            checkedRights++;
        }
        Assert.AreEqual(28, checkedRights);
    }

    [TestMethod]
    public void DescriptionsAreAvailableThroughTypedFactoriesAndCatalogs() {
        Assert.AreEqual("No write up", SddlRightValue.Construct<MandatoryRight>("NW").Description);
        Assert.AreEqual("Create child objects", SddlRightValue.Construct<DirectoryRight>("CC").Description);
        foreach (ObjectType type in Enum.GetValues<ObjectType>()) {
            Assert.AreEqual("None", SddlRightValue.Construct(string.Empty, type)!.Description);
            Assert.AreEqual("Special", SddlRightValue.Construct(0xDEADBEEFu, type)!.Description);
        }
    }

    [TestMethod]
    public void CatalogContainsEveryDomainBeforeDomainSpecificLookup() {
        Assert.AreEqual(6, SddlRightValue.ByTypeAndVal.Count);
        Assert.AreEqual(6, SddlRightValue.ByTypeAndAbbr.Count);
        Assert.AreEqual(ObjectType.DirectoryService, DirectoryRight.Type);
        Assert.AreEqual(ObjectType.File, FileRight.Type);
        Assert.AreEqual(ObjectType.RegistryKey, RegistryRight.Type);
        Assert.AreEqual(ObjectType.Mandatory, MandatoryRight.Type);
        foreach (ObjectType type in Enum.GetValues<ObjectType>()) {
            Assert.IsTrue(SddlRightValue.ByTypeAndVal.ContainsKey(type));
            Assert.IsTrue(SddlRightValue.ByTypeAndAbbr.ContainsKey(type));
            Assert.IsNotNull(SddlRightValue.Construct(string.Empty, type));
        }
    }

    [TestMethod]
    public void LookupTablesAreCachedAndReadOnly() {
        Assert.AreSame(GenericRight.ByVal, GenericRight.ByVal);
        Assert.AreSame(StandardRight.ByAbbr, StandardRight.ByAbbr);
        Assert.AreSame(DirectoryRight.ByVal, DirectoryRight.ByVal);
        Assert.AreSame(FileRight.ByAbbr, FileRight.ByAbbr);
        Assert.AreSame(RegistryRight.ByVal, RegistryRight.ByVal);
        Assert.AreSame(MandatoryRight.ByAbbr, MandatoryRight.ByAbbr);
        Assert.AreSame(SddlRightValue.ByTypeAndVal, SddlRightValue.ByTypeAndVal);
        Assert.AreSame(SddlRightValue.ByTypeAndAbbr, SddlRightValue.ByTypeAndAbbr);
        Assert.AreSame(SddlRightValue.ByAbbreviation, SddlRightValue.ByAbbreviation);
        Assert.ThrowsException<NotSupportedException>(() =>
            ((IDictionary<string, RegistryRight>)RegistryRight.ByAbbr).Add("ZZ", RegistryRight.SDDL_KEY_READ));
    }

    [TestMethod]
    public void AllSymbolicRightsRoundTripThroughTheGlobalParser() {
        Assert.AreEqual(28, SddlRightValue.ByAbbreviation.Count);
        foreach (var entry in SddlRightValue.ByAbbreviation) {
            SddlRight right = SddlRight.Construct(entry.Key);
            Assert.AreEqual(entry.Key, right.ToString());
            Assert.AreEqual(entry.Value.Value, right.ToValue());
            Assert.AreEqual(entry.Key, entry.Value.ToString());
        }
    }

    [TestMethod]
    public void AssignmentPreservesMandatoryTokenRatherThanDirectoryAlias() {
        MandatoryRight original = MandatoryRight.SDDL_NO_WRITE_UP;
        MandatoryRight copy = original;
        Assert.AreEqual("NW", copy.Abbr);
        Assert.AreEqual("NW", copy.ToString());
        Assert.AreEqual(DirectoryRight.SDDL_CREATE_CHILD.Value, copy.Value);
        Assert.IsFalse(copy.Equals(DirectoryRight.SDDL_CREATE_CHILD));
        Assert.AreEqual(2, new HashSet<SddlRightValue> { copy, DirectoryRight.SDDL_CREATE_CHILD }.Count);
    }

    [TestMethod]
    public void RegistryAliasesRemainDistinctWithACanonicalNumericLookup() {
        RegistryRight read = SddlRightValue.Construct<RegistryRight>("KR");
        RegistryRight execute = SddlRightValue.Construct<RegistryRight>("KX");
        Assert.AreSame(RegistryRight.SDDL_KEY_READ, read);
        Assert.AreSame(RegistryRight.SDDL_KEY_EXECUTE, execute);
        Assert.AreEqual(read.Value, execute.Value);
        Assert.IsFalse(read.Equals(execute));
        Assert.IsFalse(((object)read).Equals(execute));
        Assert.AreEqual(2, new HashSet<SddlRightValue> { read, execute }.Count);
        Assert.AreSame(read, RegistryRight.ByVal[read.Value]);
        Assert.AreSame(read, SddlRightValue.ByTypeAndVal[ObjectType.RegistryKey][read.Value]);
        Assert.AreEqual("KRKX", SddlRight.Construct("KRKX").ToString());
        Assert.AreEqual(read.Value, SddlRight.Construct("KRKX").ToValue());
        Assert.IsFalse(SddlRight.Construct("KR").Equals(SddlRight.Construct("KX")));
    }

    [TestMethod]
    public void EqualValueObjectsHaveEqualHashesAcrossEveryEqualityOverload() {
        RegistryRight first = RegistryRight.Create(0x00020019);
        RegistryRight second = RegistryRight.Create(0x00020019);
        Assert.IsTrue(first.Equals(second));
        Assert.IsTrue(((object)first).Equals(second));
        Assert.IsTrue(((IEquatable<SddlRightValue>)first).Equals(second));
        Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
        Assert.AreEqual(1, new HashSet<SddlRightValue> { first, second }.Count);
        Assert.IsFalse(first.Equals(RegistryRight.SDDL_KEY_READ));
        Assert.IsFalse(first.Equals(null));
        Assert.IsFalse(first.Equals(new object()));
    }

    [TestMethod]
    public void GenericFactoriesConstructUnknownMasksWithoutPublicConstructors() {
        const uint mask = 0xDEADBEEF;
        AssertRaw(SddlRightValue.Construct<GenericRight>(mask), mask);
        AssertRaw(SddlRightValue.Construct<StandardRight>(mask), mask);
        AssertRaw(SddlRightValue.Construct<DirectoryRight>(mask), mask);
        AssertRaw(SddlRightValue.Construct<FileRight>(mask), mask);
        AssertRaw(SddlRightValue.Construct<RegistryRight>(mask), mask);
        AssertRaw(SddlRightValue.Construct<MandatoryRight>(mask), mask);
        foreach (ObjectType type in Enum.GetValues<ObjectType>()) {
            AssertRaw(SddlRightValue.Construct(mask, type)!, mask);
        }
        Assert.IsNull(SddlRightValue.Construct(mask, (ObjectType)99));
    }

    [TestMethod]
    public void KnownNumericValuesStillHaveNoInventedSpelling() {
        AssertRaw(SddlRightValue.Construct<MandatoryRight>(1), 1);
        AssertRaw(SddlRightValue.Construct<RegistryRight>(0x00020019), 0x00020019);
        AssertRaw(SddlRightValue.Construct<FileRight>(0x001F01FF), 0x001F01FF);
        AssertRaw(SddlRightValue.Construct<GenericRight>(0), 0);
    }

    [TestMethod]
    public void TypedTokenFactoriesUseTheirOwnDomain() {
        Assert.AreSame(MandatoryRight.SDDL_NO_WRITE_UP, SddlRightValue.Construct<MandatoryRight>("NW"));
        Assert.AreSame(DirectoryRight.SDDL_CREATE_CHILD, SddlRightValue.Construct<DirectoryRight>("CC"));
        Assert.AreSame(GenericRight.SDDL_NONE, SddlRightValue.Construct<GenericRight>(string.Empty));
        Assert.ThrowsException<ArgumentException>(() => SddlRightValue.Construct<MandatoryRight>("CC"));
        Assert.ThrowsException<ArgumentNullException>(() => SddlRightValue.Construct<MandatoryRight>((string)null!));
        Assert.IsNull(SddlRightValue.Construct("CC", ObjectType.Mandatory));
        Assert.IsNull(SddlRightValue.Construct("NW", (ObjectType)99));
        Assert.ThrowsException<ArgumentNullException>(() => SddlRightValue.Construct((string)null!, ObjectType.Mandatory));
    }

    [DataTestMethod]
    [DataRow("RCRP", "RCRP", 0x00020010u)]
    [DataRow("RPGASD", "GARPSD", 0x10010010u)]
    [DataRow("NWNRNX", "NRNWNX", 7u)]
    [DataRow("KXKR", "KRKX", 0x00020019u)]
    [DataRow("NWNW", "NW", 1u)]
    [DataRow("CCNW", "CCNW", 1u)]
    [DataRow("FRWD", "FRWD", 0x00160089u)]
    public void AggregateParsesCompleteStringsAndMixedDomains(string input, string expected, uint mask) {
        SddlRight result = SddlRight.Construct(input);
        Assert.AreEqual(expected, result.ToString());
        Assert.AreEqual(mask, result.ToValue());
        Assert.IsTrue(result.Equals(SddlRight.Construct(result.ToString())));
        Assert.IsTrue(result.Equals(SddlRight.Construct<DirectoryRight>(input)));
    }

    [DataTestMethod]
    [DataRow(0u)]
    [DataRow(1u)]
    [DataRow(3u)]
    [DataRow(7u)]
    [DataRow(0x001F01FFu)]
    [DataRow(0x00020019u)]
    [DataRow(0x80000000u)]
    [DataRow(uint.MaxValue)]
    public void AggregateNumericInputStaysNumericAndKeepsAllBits(uint mask) {
        SddlRight result = SddlRight.Construct(mask);
        Assert.AreEqual(mask, result.ToValue());
        Assert.AreEqual($"0x{mask:X8}", result.ToString());
        Assert.IsTrue(result.Equals(SddlRight.Construct(result.ToString())));
        Assert.IsTrue(result.Equals(SddlRight.Construct<FileRight>(mask)));
        Assert.AreEqual(result.GetHashCode(), SddlRight.Construct(result.ToString()).GetHashCode());
    }

    [DataTestMethod]
    [DataRow("0x1", "0x00000001", 1u)]
    [DataRow("0Xdeadbeef", "0xDEADBEEF", 0xDEADBEEFu)]
    [DataRow("0x00020019", "0x00020019", 0x00020019u)]
    public void HexadecimalTextParsesAsARawMask(string input, string expected, uint mask) {
        SddlRight result = SddlRight.Construct(input);
        Assert.AreEqual(expected, result.ToString());
        Assert.AreEqual(mask, result.ToValue());
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public void EmptyTextHasNoRightsAndFormatsAsEmpty(string? input) {
        SddlRight result = SddlRight.Construct(input);
        Assert.AreEqual(0u, result.ToValue());
        Assert.AreEqual(string.Empty, result.ToString());
        Assert.IsTrue(result.Equals(new SddlRight()));
        Assert.AreEqual(result.GetHashCode(), new SddlRight().GetHashCode());
    }

    [DataTestMethod]
    [DataRow("N")]
    [DataRow("NWZ")]
    [DataRow("NWRPZZ")]
    [DataRow("nw")]
    [DataRow("NW NR")]
    [DataRow("NW  ")]
    [DataRow("  NW")]
    [DataRow("0x")]
    [DataRow("0xGG")]
    [DataRow("0x100000000")]
    [DataRow("0x 1")]
    [DataRow("0x1 ")]
    [DataRow("0x+1")]
    public void InvalidTextIsRejectedRatherThanPartiallyParsed(string input) {
        Assert.ThrowsException<ArgumentException>(() => SddlRight.Construct(input));
    }

    [TestMethod]
    public void AggregateEqualityAndHashingAreOrderIndependentButPreserveTokenIdentity() {
        SddlRight first = SddlRight.Construct("NWNRNX");
        SddlRight second = SddlRight.Construct("NXNRNW");
        Assert.IsTrue(first.Equals(second));
        Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
        Assert.AreEqual(1, new HashSet<SddlRight> { first, second }.Count);
        Assert.IsTrue(SddlRight.Construct("NWNW").Equals(SddlRight.Construct("NW")));
        Assert.IsFalse(SddlRight.Construct("NW").Equals(SddlRight.Construct("CC")));
        Assert.IsFalse(SddlRight.Construct("NW").Equals(SddlRight.Construct(1u)));
        Assert.IsFalse(SddlRight.Construct(0u).Equals(SddlRight.Construct(string.Empty)));
        Assert.IsFalse(first.Equals(null));
        Assert.IsFalse(first.Equals(new object()));
    }

    [TestMethod]
    public void ConcurrentLookupsReturnCompleteConsistentTables() {
        Parallel.For(0, 100, _ => {
            Assert.AreEqual(6, SddlRightValue.ByTypeAndVal.Count);
            Assert.AreEqual(28, SddlRightValue.ByAbbreviation.Count);
            Assert.AreEqual("KRKXNWNXRC", SddlRight.Construct("RCKXNWKRNX").ToString());
        });
    }

    private static void AssertRaw(SddlRightValue right, uint mask) {
        Assert.IsNotNull(right);
        Assert.AreEqual(mask, right.Value);
        Assert.IsNull(right.Abbr);
        Assert.AreEqual("Special", right.Description);
        Assert.AreEqual($"0x{mask:X8}", right.ToString());
    }
}
