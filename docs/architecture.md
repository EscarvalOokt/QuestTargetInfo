# Quest Target Info architecture

This document describes the 2.1.0 source implementation for RimWorld 1.7 and the package's preserved 1.6 assembly. The 1.6 assembly is frozen and is not rebuilt from these sources.

User instructions and development commands are in [README](../README.md). Validation procedures are in [Testing policy](testing.md).

## Runtime and package

The current project targets `net10.0` and references RimWorld's `Assembly-CSharp.dll`, `UnityStubs.dll` and a compatible Harmony assembly. Game and Harmony references use `Private=false` in production. Harmony is a separate player dependency; there is no shared UI library or additional runtime library shipped by this project.

The product identity remains `escarval.questtargetinfo`, with assembly/root namespace `QuestTargetInfo` and Workshop ID `3541678897`. Mod version 2.1.0 is independent of the selected game version.

[About.xml](../mod/About/About.xml) declares the supported game versions. [LoadFolders.xml](../mod/LoadFolders.xml) loads the shared root plus `1.6` for RimWorld 1.6, or the shared root plus `Current` for 1.7. The shared root contains metadata and translations. The preserved assembly at `mod/1.6/Assemblies/QuestTargetInfo.dll` has SHA-256 `E03761CA6254364D5D165F5EB58767D842059940701A2D4FF38A4FEC0A944D72`; its checksum is recorded in the build configuration.

The source tree excludes the current generated assembly. Ordinary builds write it to `src/QuestTargetInfo/bin/Release/`, leaving `mod/` and the installed game unchanged. The full deployed package adds `Current/Assemblies/QuestTargetInfo.dll`. Deployment excludes PDB debug symbols from source content and copies only the current DLL from build output; ignored local historical symbols do not become release content. Consequently, source archives and the source `mod/` directory are not complete current-version installations.

[ModDeployment.targets](../build/ModDeployment.targets) runs only with `DeployToGame=true`. `RimWorldModAssembliesDir` names an absolute destination ending in `Current/Assemblies`, but deployment replaces the complete managed package rooted two levels above that directory. The same pipeline prepares an installed mod or a clean external staging package. `DeployModToGame` depends on `ValidateModDeployment`, so validation completes before cleanup.

Validation checks destination boundaries, path casing, package identity, source content and historical checksums. The versions in `About.xml` and the version entries in `LoadFolders.xml` must match the current game version plus the declared historical versions; each load entry must select the shared root and its intended assembly directory. Source DLLs must belong to the declared historical assembly inventory. Source-repository locations and ancestors, game/filesystem roots, Git checkout roots, paths through symbolic links or junctions, and a directory occupying `LoadFolders.xml` are rejected.

The managed replacement boundary is `About`, `Defs`, `Patches`, `Languages`, `Textures`, `Sounds`, `Resources`, `Common`, declared historical versions and `Current`. The obsolete root `Assemblies` and numbered current-game directory are removed. Unrelated files outside this boundary are retained. A destination's local `About/PublishedFileId.txt` is saved and restored only if the source does not supply it; this project supplies its own ID. Missing local IDs are not reconstructed. The targets validate before cleanup but do not provide rollback or atomic replacement after a write failure.

## Implementation map

- [QuestTargetInfoMod](../src/QuestTargetInfo/QuestTargetInfoMod.cs) owns settings initialization and Harmony registration. [Settings](../src/QuestTargetInfo/QuestTargetInfoSettings.cs) owns persisted display options and their change notification.
- [Quest selection patch](../src/QuestTargetInfo/Patch_Quests_Select.cs) and [quest pane patch](../src/QuestTargetInfo/Patch_Quests_DoWindowContents.cs) synchronize the quest travel window through [window utility](../src/QuestTargetInfo/WorldTargetInfoWindowUtility.cs). [World inspection patch](../src/QuestTargetInfo/Patch_WorldInspectPane_CurTabs.cs) supplies the Travel tab; [selection utility](../src/QuestTargetInfo/WorldTargetInfoSelectionUtility.cs) resolves its request.
- [Route calculator](../src/QuestTargetInfo/Info/WorldTargetRouteCalculator.cs) and [transport calculator](../src/QuestTargetInfo/Info/WorldTargetFlyingTransportCalculator.cs) own route and transport evaluation. [Transport constants](../src/QuestTargetInfo/Info/WorldTargetTransportConstants.cs) owns vanilla reference values.
- [Model builder](../src/QuestTargetInfo/Info/WorldTargetInfoModelBuilder.cs) converts results into localized sections; [drawer](../src/QuestTargetInfo/UI/WorldTargetInfoDrawer.cs), [quest window](../src/QuestTargetInfo/Window_QuestTargetInfo.cs) and [world tab](../src/QuestTargetInfo/WITab_WorldTargetInfo.cs) own presentation.
- [Runtime context](../src/QuestTargetInfo/Runtime/WorldTargetInfoContext.cs) resolves live requests and geometry identity; [runtime cache](../src/QuestTargetInfo/Runtime/WorldTargetInfoCache.cs) separates route reuse from mutable model refresh.
- [Production project](../src/QuestTargetInfo/QuestTargetInfo.csproj), [deployment targets](../build/ModDeployment.targets), metadata and load mapping own build and package configuration. [Tests](../tests/QuestTargetInfo.Tests/QuestTargetInfo.Tests.csproj) exercise the project-owned regression boundary.

## Runtime flow

The `Mod` constructor loads `QuestTargetInfoSettings` and applies Harmony patches under the existing package ID. The exact `MainTabWindow_Quests.Select(Quest)` postfix uses the selected quest argument for immediate updates, including selection during `PreOpen`. A second postfix on `DoWindowContents(Rect)` receives the pane's actual `selected` field through Harmony field injection. It catches category changes and dismissal actions that assign the field directly without calling `Select`, without repeated reflection lookup.

Both quest hooks use the same synchronization method. A cleared or unsupported selection closes the travel window; a supported selection opens or replaces it. The same owner and quest retain the existing window and scroll state. A quest must have one valid world target and a valid current map. World inspection resolves one selected world object first, or an empty tile when no world objects are selected. Both entry points use the current map as origin.

A request carries origin and target tiles, the target label/object and its UI source. Route and transport results are converted into section and line view models. The shared drawer renders those models in the quest window or scrollable world tab. Display settings control inclusion and detail; they do not change game travel rules.

## Cache and window lifecycle

The shared runtime cache refreshes the model every 0.5 seconds using a monotonic clock, including while the game is paused. Settings, selection or target-label changes invalidate presentation immediately. Route geometry is cached separately, keyed by the world/map and layer objects, origin/target tile IDs and a structural snapshot of the directional layer graph. Connectivity, layer reachability or graph fuel-cost changes invalidate that geometry even when the selection stays unchanged.

The quest window retains its quest and owning Quests window, while the pane's draw postfix reconciles that binding with the actual selection. The window resolves the live target and current map again on each UI request. Moving targets and map changes therefore do not keep the original request frozen. Invalid context, destroyed/unspawned targets and leaving the game close the quest display or hide the world tab and clear cached state. Switching world, map or target resets scrolling; dynamic, settings and topology refreshes preserve it.

The quest window is placed beside the owner's actual window rectangle, using the right side when it fits, otherwise the left, otherwise a bounded overlay. Its rectangle is clamped to the scaled UI viewport. Expanded content scrolls; collapse uses a 60-unit preferred height within the viewport. Requests, models and window state are transient and are not written into a save.

## Route and transport boundaries

Flight traversal uses `passImpassable: true`: impassable intermediate tiles do not block a flight. A destination can still be invalid. Planet-layer connections, directional traversal and layer range factors come from the game. Route distance is not a caravan path, walking time or a promise that the destination is launchable.

Pod and shuttle maximum ranges, fuel-per-tile values and minimum fuel remain centralized constants/fallbacks. They are intentionally not read from runtime definitions. These sections estimate vanilla reference vehicles rather than selecting or inspecting actual loaded vehicles. Modified transport definitions may therefore produce different launch behavior.

Shuttle outbound availability is evaluated independently from the return trip. A missing return route leaves return and total fuel unknown; an out-of-range return flight produces a separate warning. These warnings survive compact display. Fuel values estimate direct flights, while the launch-count arithmetic does not search for intermediate stops or sum minimum fuel charges for multiple legs. Displayed fuel requirements are rounded upward to avoid understating the estimate.

The gravship section requires Odyssey and a spawned grav engine on the captured origin map. It queries that map's spawned engines directly, avoiding the game's tick-based lookup cache and excluding packed or contained engines; spawning or despawning while paused can therefore reach the next model refresh. Its path adapter follows the inspected 1.7 layer-path and tile-projection rules, then computes fuel from the current engine's fuel-per-tile value, destination layer factor and connection costs, with the vanilla minimum. It deliberately avoids the game's endpoint-only fuel cache so changing fuel-use modifiers, worlds or layer connections cannot reuse an obsolete estimate. The engine supplies range, total fuel and signal-jammer state. Landing validation follows the relevant game checks; relocating on the same tile requires the applicable grav anchor. The section reports conditions but does not perform launches, reserve fuel or mutate the vehicle.

## Configuration

[QuestTargetInfoSettings](../src/QuestTargetInfo/QuestTargetInfoSettings.cs) persists display options through `Scribe_Values`, using keys compatible with the 1.6 version. Defaults are all visibility switches enabled and compact mode disabled. A transient settings version invalidates cached display models when an option changes. Compact mode suppresses secondary details while keeping essential status and return warnings.

The settings class keeps the full name `QuestTargetInfo.QuestTargetInfoSettings`. Scribe stores this name in the XML `Class` attribute; changing the namespace breaks direct resolution of existing settings.

Machine configuration belongs to the ignored `src/QuestTargetInfo.Local.props`. Its portable [example](QuestTargetInfo.Local.props.example) documents game/Harmony references and optional deployment. Build commands and their effects are maintained in README.

## Persistence and save lifecycle

The mod defines no `Thing`, world object, game component, map component or custom saved definition. Its persisted state is limited to mod settings; requests, cached models, window positions, scrolling and collapse state are session state. It does not rewrite quest data or store travel calculations in the save.

This source-level boundary does not substitute for adding/removing the mod in game. Save/load, switching worlds, map changes and removal from a resaved copy have separate acceptance scenarios. Historical 1.6 and current 1.7 behavior must be validated in their own game versions.

## Project boundaries

Quest Target Info supplies information and presentation. RimWorld owns traversal, transport launch/landing, world selection and save loading; Harmony owns patch application. The mod exposes no supported integration API for other mods. Custom transport systems, a caravan planner, multiplayer guarantees and automatic save conversion are outside the current supported behavior. Compatibility statements distinguish these limits from tested failures and untested combinations.