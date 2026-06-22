# [Quest Target Info](https://steamcommunity.com/sharedfiles/filedetails/?id=3541678897)

Quest Target Info adds compact travel information for world-map targets in RimWorld.

The mod helps you quickly check route distance, transport range and estimated fuel requirements. It works with quest targets, selected world objects and selected world tiles through the world map interface.

---

## Features

### World target travel information

Quest Target Info can display travel information for:

* quest world targets;
* selected world objects;
* selected empty world tiles;
* the current map as the route origin.

When a quest with a world target is selected, the mod can show a dedicated travel information window in the quests tab.

When inspecting objects or tiles on the world map, the mod can add a `Travel` tab to the world inspect pane.

### Route information

The route section can show:

* estimated route distance in tiles;
* same-tile handling when the selected target is the current map;
* route layer context;
* optional layer-adjusted distance when planet layer modifiers affect travel range.

### Transport information

The mod can show travel sections for:

* Transport pods;
* Shuttles;
* Gravships.

Depending on the target, available DLC, current map state and transport availability, each section can show:

* availability status;
* route or target validation status;
* current distance and maximum range;
* estimated fuel required;
* outbound, return and total shuttle fuel;
* estimated shuttle launch count;
* invalid landing target warnings;
* signal jammer requirements;
* unavailable or out-of-range transport status.

### User interface

The UI is designed to stay compact and readable.

Supported UI options include:

* collapsible quest travel window;
* world inspect `Travel` tab;
* compact mode;
* optional unavailable transport sections;
* per-transport visibility toggles;
* optional ancient transport pod compatibility line;
* optional layer-adjusted distance display.

---

## Settings

Quest Target Info includes mod settings for controlling what is displayed.

Available settings include:

* show quest travel window;
* show world inspect `Travel` tab;
* show ancient pod compatibility line;
* show layer-adjusted distance;
* show unavailable transports;
* compact mode;
* show Transport pod;
* show Shuttle;
* show Gravship.

Transport visibility settings allow you to keep only the sections that are relevant to your current playthrough.

---

## Compatibility

* Target RimWorld version: `1.6`.
* Harmony is required.
* Odyssey-specific transport information is only shown when the Odyssey DLC is active and relevant.
* The mod focuses on world target travel information for route, range and flying transport checks.

---

## Requirements

* [Harmony](https://steamcommunity.com/workshop/filedetails/?id=2009463077)

---

## Localization

Quest Target Info currently includes localization files for:

* English;
* German;
* Russian;
* Ukrainian.

---

## Known limitations

Transport launch range and fuel values are centralized in the mod as constants and fallbacks. Runtime reading of those values from vanilla defs is intentionally not used, because the required values do not currently have a stable and reliable access path for this mod's use case.

The mod is focused on supported vanilla travel mechanics and may not provide complete support for every modded transport system or every modded world object behavior.

---

## Development build

1. Copy `docs/QuestTargetInfo.Local.props.example` to `src/QuestTargetInfo.Local.props`.
2. Set `RimWorldInstallDir` and `HarmonyDllPath` for your local machine.
3. Build `src/QuestTargetInfo.sln`.

By default, the compiled assembly is written to:

```text
mod/1.6/Assemblies/
```

You can override the output path by setting `ModAssembliesDir` in `QuestTargetInfo.Local.props`.

`QuestTargetInfo.Local.props` is a local machine-specific file and should not be committed.