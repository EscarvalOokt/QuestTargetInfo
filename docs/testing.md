# Quest Target Info testing policy

This document describes regression boundaries and in-game validation for Quest Target Info. Build, test and deployment commands are in [README](../README.md#development).

## Testing principle

Automated tests protect project-owned arithmetic, state transitions and formatting decisions. Package checks establish the files and version mapping being delivered. RimWorld runs establish patch integration, actual travel comparisons, UI behavior and save lifecycle. None of these substitutes for the others.

A scenario below is a procedure, not evidence that it has passed. Changes to the historical 1.6 binary are prohibited during ordinary current-version work; shared content still requires checks in both supported versions.

## Automated test boundary

The NUnit project references the production project. Tests cover project-owned fuel/range arithmetic and upward display rounding, gravship fuel-per-tile/layer/connection inputs, shuttle outbound/return results, cache refresh and geometry identity, and viewport-aware window layout. Test-host references may be copied into test output; production deployment is disabled for the dependency build and those assemblies do not belong in the package.

Deterministic inputs verify what the mod does with those inputs. They do not prove the real game's planet-layer connectivity, destination validity, Harmony patch success, widget layout or save safety. No test should construct a large imitation of RimWorld solely to avoid a meaningful in-game scenario.

## Build and package checks

Package validation belongs to [ModDeployment.targets](../build/ModDeployment.targets) and runs automatically with `DeployToGame=true`, before destination cleanup. The same deployment prepares the installed mod or a clean external staging package. It validates supported-version declarations, exact load mappings, the allowed source assembly inventory and historical hashes, together with package identity and filesystem safety checks.

When changing these rules, exercise the scenarios below in an isolated source copy and disposable external destination with the normal commands from README. Record the checked candidate and outcomes; the scenario list itself is not evidence of execution.

1. **Ordinary build/test:** leave deployment disabled and omit its destination property. Verify the current DLL is produced in `bin/`; `mod/`, the installed copy and the historical SHA remain unchanged.
2. **Clean package:** deploy into a clean staging directory. Verify shared About/languages, `LoadFolders.xml`, `1.6/Assemblies/QuestTargetInfo.dll` and `Current/Assemblies/QuestTargetInfo.dll`. Compare the current DLL to build output and historical DLL to the pinned SHA. Confirm that only the intended two DLLs are present and PDB symbols are excluded, including a locally seeded historical PDB in a disposable source copy.
3. **Update:** seed obsolete managed content and old assembly layouts, then redeploy. Verify managed content replacement, obsolete-layout removal and preservation of a file outside the managed directories. Test source-supplied Workshop ID precedence and, in an isolated source fixture without that file, preservation of an existing local ID. Do not expect recovery when no ID exists.
4. **Invalid inputs:** verify rejection before destination writes for an empty/relative destination, incorrect suffix/casing, another package ID, missing content, missing or mismatched historical DLL, malformed SHA and a current version declared as historical. Remove the historical declaration while leaving 1.6 in metadata; also test missing, extra or duplicate version/mapping entries, incorrect mapping order and an unexpected source DLL. Every case must fail in the deployment pipeline while retaining the existing destination. For actual repository/game/filesystem paths, invoke only the validation target; destructive targets belong solely in disposable fixtures. Test symbolic links/junctions, dangling links and a directory occupying `LoadFolders.xml` in isolated fixtures when the environment supports them.
5. **Localization:** parse all four XML files, check matching keys/placeholders and preserve every key consumed by the historical assembly. Confirm the return/direct-flight warning keys are present in each locale. XML validation does not establish visual fit or translation quality.

## Runtime acceptance setup

Identify the source revision and uncommitted candidate changes, deployed DLL SHA, full game version, Harmony build, OS, date, DLC, ordered mod list, settings, language, resolution and UI scale. Confirm only one copy of the mod is enabled. Keep relevant player logs and observations with the project evidence for that candidate; do not link private evidence from this public policy.

Run 1.7 initially with Core, compatible Harmony and Quest Target Info, then with Odyssey and other DLC required by a scenario. Use disposable save copies. Run 1.6 separately with its compatible dependencies and the same full package to check historical assembly selection and shared translations. A pass in one environment does not imply a pass in another.

Install the complete candidate package with RimWorld closed. Keep existing mod settings for one startup to check resolution of the persisted `QuestTargetInfo.QuestTargetInfoSettings` class name; do not erase them before that check. Initialization must not report a missing settings class or a Harmony patch failure.

Record each scenario as passed, failed or not run. For a failure, retain the scenario ID, game/DLC/mod configuration, language, resolution/UI scale, exact steps, expected and actual behavior, a screenshot and the relevant player-log excerpt. A scenario that cannot be arranged, such as a directional layer connection, stays not run rather than being inferred from another scenario.

## Runtime scenarios

| ID | Setup and action | Expected result |
| --- | --- | --- |
| RT-01 | Start each supported game version with the complete candidate package; load a disposable colony. | Exactly the intended version-specific DLL loads; initialization and Harmony patches produce no mod errors. Shared texts resolve. |
| RT-02 | Select quests with zero, one and several targets; include a valid world target and an invalid target. Switch Available/Active/Historical categories, dismiss the selected quest, choose another quest and close Quests. | The travel window follows the actual selected quest, closes when selection is cleared or invalid, and reopens for a supported single-target selection. It never retains a previously selected quest after a category switch or dismissal, and closes with Quests. |
| RT-03 | Select one world object, an empty tile, multiple objects, then clear selection. Repeat with no usable current map. | Travel is available only for a valid request and never appears twice. Missing context clears cached state. |
| RT-04 | Compare same-layer and cross-layer destinations, directional connections and unreachable layers against vanilla travel. Include impassable intermediate terrain. | Distances and restrictions match flight semantics; intermediate impassability does not masquerade as an invalid destination. |
| RT-05 | Select a pod destination inside, exactly at and beyond maximum range; include fractional fuel calculations. Compare to a vanilla launch check. | Boundary status is correct and displayed required fuel rounds upward. The ancient-pod compatibility line uses its own reference range. |
| RT-06 | Test shuttles with both legs valid, outbound valid/return unavailable, outbound valid/return out of range and outbound out of range. Repeat in compact mode. | Outbound status stays meaningful. Return warnings remain visible; unknown return/total fuel is not shown as zero or a valid total. Direct-flight estimates do not imply a staged itinerary. |
| RT-07 | With Odyssey, inspect gravship travel with no engine, a packed engine, insufficient fuel, range boundaries, blocked landing and jammer requirements. Test same-tile travel without and with a grav anchor. | A packed engine is not treated as an available vehicle. Engine and destination restrictions match the game's launch checks; same-tile travel without the required anchor is rejected. |
| RT-08 | Keep a target open while spawning/despawning an engine, refuelling, changing engine fuel-use modifiers/range/jammer state, moving the target or switching the current map. Change applicable layer connections and repeat with the game paused. | Information refreshes within about 0.5 seconds without reselecting the target, uses current spawned-engine state, fuel-per-tile and connection costs, and does not reuse an obsolete estimate or successful result. |
| RT-09 | Toggle each setting, compact mode and all individual transport switches. Close settings, reopen the display, restart and reload. | Visibility changes apply immediately and persist. The route remains usable when transports are hidden; essential warnings remain in compact mode. |
| RT-10 | Use English, German, Russian and Ukrainian at representative small/large resolutions and supported UI scales. Expand/collapse the quest window and scroll long content. | Text, warnings and controls remain readable; the window stays on screen and scrolling exposes all content without duplicate tabs. |
| RT-11 | Save/load, return to the menu and open another world with similar tile/object IDs. Switch maps with a target open. | Cached requests and models do not leak across maps or games; new information reflects the current world. |
| RT-12 | Add the mod to a copy of an existing save. Separately save with it, disable/remove it, and reload a copy. | Record each operation independently, including settings persistence and logs. Do not infer removal safety from an add-to-save pass. |

For 1.6, run RT-01 and check existing quest/world travel, persisted settings and shared translations in all four languages. The historical DLL must remain unchanged. The quest-window synchronization and transport behavior specified for 1.7 in RT-02 and RT-05 through RT-08 are not acceptance requirements for the frozen 1.6 implementation.

### Quest-window synchronization

After RT-01 on 1.7, use this sequence for RT-02. Prepare two quests with different valid world targets so stale information is easy to recognize. Keep the game paused:

1. Select quest A. Exactly one travel window must appear and identify A's target.
2. Switch between Available, Active and Historical categories without selecting another quest. When the main pane clears selection, the travel window must close rather than retain A.
3. Select quest B. The window must show B's target and calculations; A's content must not remain.
4. Dismiss the selected quest through the actual UI. If the game selects another supported quest, the travel window must follow that quest. If selection becomes empty or unsupported, the window must close.
5. Select a quest without a world target or with multiple targets. The travel window must close. Return to a supported single-target quest and confirm that it reopens once.
6. Keep a supported quest selected, expand the window, and scroll long content. Wait at least two seconds and interact with the same quest without changing selection. Periodic refresh and redraw must not reset scrolling, collapse state or create a second window.
7. Change the selected quest. The target must change immediately; scrolling may reset for the new selection. Collapse and expand the window and confirm that all content remains accessible.
8. Close Quests and open another main tab. The travel window must close with its owner. Reopening Quests must show information only for its actual supported selection.

Repeat steps 1–8 with Odyssey enabled and the small viewport and long translations from RT-10. Use RT-08 for dynamic refresh and RT-09 for immediate settings updates.

## Release candidate validation

Build and deploy to a clean external staging directory. Record current and historical hashes, inspect the package, then run the applicable scenarios against a copy of that exact package. Do not silently rebuild after acceptance; a changed binary or content requires checks appropriate to the change.

Release readiness requires successful build/automated and package checks plus recorded runtime acceptance for the supported versions. An unrun required scenario remains pending. Prepared files, metadata or a successful deployment do not establish Workshop publication. Publication is a separate operation against the reviewed package.

## Adding tests

Add an automated regression when a project-owned contract can be tested with deterministic inputs, preferably covering the failing boundary and the intended adjacent behavior. Extend a numbered runtime scenario when the result depends on actual game state, patch integration, UI or save lifecycle. Preserve scenario IDs and identify the candidate when recording execution. Avoid tests that merely mirror a helper's implementation or assert that the test runner starts.