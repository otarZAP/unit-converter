# Unit Converter

A small Windows desktop app for converting between metric and imperial units — no online converter needed.

Pick a category, pick a "from" and "to" unit from the dropdowns, type a value, and the result updates live. A swap button flips the two units.

Categories:
- **Length**: mm, cm, m, km, in, ft, yd, mi
- **Weight**: mg, g, kg, oz, lb
- **Temperature**: °C, °F, K

## Build

Requires the in-box .NET Framework C# compiler (`csc.exe`, ships with Windows). No other dependencies.

```
build.cmd
```

Produces `UnitConverter.exe`.

## Run

The built `UnitConverter.exe` is not checked in — it is build output, and `.gitignore`
excludes it. Run `build.cmd` first, then double-click the `UnitConverter.exe` it
produces, or point a desktop shortcut at it.
