# Rights regression tests

Run with .NET 8:

```sh
dotnet test Tests/SecurityDescriptor.Tests.csproj
```

The tests cover the replacement rights model, including domain collisions, KR/KX
token identity, immutable cached tables, typed factories, mixed-domain parsing,
raw masks, strict input validation, equality/hash consistency, and UI descriptions.

Named values expose `Description` with the original enum field's UI text. The
`GetDescription()` extension also accepts the new value objects. NONE values use
"None"; raw masks use the existing "Special" fallback. Descriptions are metadata
and do not participate in equality or hashing.

`Ace.Right` now uses `SddlRight`; the old `Right` class has been retired.

Text input preserves symbolic identity. Repeated tokens are deduplicated and
formatted in ordinal abbreviation order. Numeric input stays hexadecimal, even
when its value matches a named right. `ByVal` is a canonical metadata lookup, with
KR chosen for the shared KR/KX mask; both spellings remain available in `ByAbbr`.
`SddlRightValue` equality compares concrete type, mask, and abbreviation.
`SddlRight` equality and hashing compare the combined access mask, so equivalent
symbolic and numeric representations compare equal. Equality does not rewrite
the stored tokens or hexadecimal representation. Clone tests verify that both
representations, domain identities, and UI descriptions survive copying.

The non-generic aggregate parser accepts all supported abbreviations, including
combinations of generic, standard, and object-specific rights. The generic
aggregate overloads forward to the same parser for compatibility; the type
parameter does not restrict aggregate tokens. Individual
`SddlRightValue.Construct<T>(string)` factories still require T's own tokens.
