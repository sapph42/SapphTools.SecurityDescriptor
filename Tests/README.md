# Rights regression tests

Run with .NET 8:

```sh
dotnet test Tests/SecurityDescriptor.Tests.csproj
```

The tests cover the replacement rights model, including domain collisions, KR/KX
token identity, immutable cached tables, typed factories, mixed-domain parsing,
raw masks, strict input validation, and equality/hash consistency.

The existing `Right`/`Ace` APIs still use the old enum; integrating the replacement
into those APIs is a separate migration.

Text input preserves symbolic identity. Repeated tokens are deduplicated and
formatted in ordinal abbreviation order. Numeric input stays hexadecimal, even
when its value matches a named right. `ByVal` is a canonical metadata lookup, with
KR chosen for the shared KR/KX mask; both spellings remain available in `ByAbbr`.
`SddlRightValue` equality compares concrete type, mask, and abbreviation.
`SddlRight` equality compares its token set or its raw mask, preserving the
difference between symbolic and numeric input. Use `ToValue()` explicitly when
comparing access masks alone.

The non-generic aggregate parser accepts all supported abbreviations, including
combinations of generic, standard, and object-specific rights. The generic
aggregate overloads forward to the same parser for compatibility; the type
parameter does not restrict aggregate tokens. Individual
`SddlRightValue.Construct<T>(string)` factories still require T's own tokens.
