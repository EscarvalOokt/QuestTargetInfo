# [Quest Target Info](https://steamcommunity.com/sharedfiles/filedetails/?id=3541678897)

Adds an additional UI window to quest details, displaying the estimated distance to the target and fuel costs for different transport types.

When a quest with a world target is selected, an info window automatically appears on the right side of the quests tab.

---

## Features

- Estimated distance to the quest target (in tiles)
- Shuttle fuel cost (requires *Odyssey DLC*)
- Transport Pod fuel cost
- Gravship fuel cost (requires *Odyssey DLC*)
- Collapsible UI window for minimal screen clutter

---

## Compatibility

Designed primarily for the *Odyssey DLC*, but works correctly with any DLC or mod configuration.

---

## Requirements

- [Harmony](https://steamcommunity.com/workshop/filedetails/?id=2009463077)

## Development build

1. Copy `docs/QuestTargetInfo.Local.props.example` to `src/QuestTargetInfo.Local.props`.
2. Set `RimWorldInstallDir` and `HarmonyDllPath` for your local machine.
3. Build `src/QuestTargetInfo.sln`.

By default, the compiled assembly is written to:

`mod/Current/Assemblies/`

You can override the output path by setting `ModAssembliesDir` in `QuestTargetInfo.Local.props`.