# Fork of saul/demofile-net

This fork carries one fix on top of an upstream release until upstream ships it.

| Branch | What |
| --- | --- |
| `fix/fixed-width-integer-encoders` | The fix alone, based on upstream `v0.44.1`, ready for an upstream pull request |
| `release/v0.44.1-fix.1` | The fix plus this file and the package version `0.44.1-fix.1`; tagged `v0.44.1-fix.1` |

## Difference from upstream

`src/DemoFile/FieldDecode.cs` honours the `fixed8`, `fixed16` and `fixed32` var encoders in the
byte, sbyte, int16, uint16, int32, uint32 and enum decoders: the field is read as that many raw bits
(signed types sign-extended) instead of a varint. `src/DemoFile.Test/Unit/FieldDecodeTest.cs` tests it.

## Why

Since CS2 demo patch 14197 (the game update of 2026-11-11), the send tables give every 8-bit
networked field (`uint8`, `int8`, 8-bit enums and their arrays) the `fixed8` encoder. Upstream
0.44.1 reads those fields as varints, so the entity stream desyncs at the first one and parsing a
post-update demo fails (for example `ArgumentOutOfRangeException` at tick 1). Other parsers made the
same change: LaihoE/demoparser#375 and markus-wa/demoinfocs-golang#764.

## Packages

The `v0.44.1-fix.1` GitHub release has the `DemoFile` and `DemoFile.Game.Cs` packages
(`dotnet pack -c Release -p:PublicRelease=true` at the tag). Nothing is pushed to nuget.org.

## Exit

Once an upstream release decodes fixed-width integer fields, use that release and retire this fork.
