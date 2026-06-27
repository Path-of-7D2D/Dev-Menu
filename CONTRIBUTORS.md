# Contributor Guide

This repository contains the source project and the deployable mod folder for
Dev Menu.

Dev Menu targets **7 Days to Die V3.0 "Dead Hot Summer"**. It uses V3
`XUi_InGame` XML, a net48 C# DLL, and Harmony patches loaded through the game's
`0_TFP_Harmony` mod.

## Project Layout

```text
1A-DevMenu/                         deployable mod folder
  ModInfo.xml                       mod manifest
  DevMenu.dll                       built mod assembly
  Config/Localization.csv           control labels, tooltips, buff text
  Config/buffs.xml                  unlimited-ammo buff definition
  Config/XUi_InGame/xui.xml         window group registration
  Config/XUi_InGame/windows.xml     Dev Menu window and XML appends
  Config/XUi_InGame/templates.xml   reusable list row templates

src/DevMenu/                        C# source project
  DevMenu.csproj                    net48 project and build/deploy targets
  DevMenuModApi.cs                  IModApi entry point and Harmony bootstrap
  DevMenuHotkeyPatches.cs           rebindable / hotkey and open handling
  DevMenuLootRerollService.cs       loot-container reroll/reset logic
  DevMenuCatalogs.cs                item, cheat, and tile-entity catalogs
  DevMenuEntries.cs                 catalog entry models
  DevMenuServices.cs                item spawn, cheat, and tile spawn services
  XUiC_DevMenuWindow.cs             main tabbed window controller
  XUiC_DevMenuLists.cs              list and row controllers
  Commands/ConsoleCmdDevMenu.cs     devmenu and p7dev console command
  Patches/                          Harmony patches for vanilla controllers
```

The `1A-` prefix controls load order. 7 Days to Die loads mod folders
alphabetically.

## Build Requirements

- .NET SDK
- Local 7 Days to Die V3.0 install
- Game reference assemblies from `7DaysToDie_Data/Managed`
- Harmony from `Mods/0_TFP_Harmony/0Harmony.dll`

The project defaults to the standard Steam path:

```text
C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die
```

Override the path with `Game7D2D` or the `GAME_7D2D` environment variable.

## Build Commands

Build from the repository root:

```powershell
dotnet build .\src\DevMenu\DevMenu.csproj -c Release
```

Use a non-default game path:

```powershell
dotnet build .\src\DevMenu\DevMenu.csproj -c Release -p:Game7D2D="D:\SteamLibrary\steamapps\common\7 Days To Die"
```

Skip refreshing the live game install:

```powershell
dotnet build .\src\DevMenu\DevMenu.csproj -c Release -p:InstallToGame=false
```

A successful build copies `DevMenu.dll` and source config files into
`1A-DevMenu/`. If the game `Mods` folder exists, the build also refreshes the
live install at:

```text
7 Days To Die/Mods/1A-DevMenu/
```

## Architecture Notes

`DevMenuModApi` patches the assembly with Harmony and registers the live input
hotkey. `PlayerActionsLocal` is created before mods load, so the hotkey code
must update the already-running input set as well as patch future constructors.

The main UI is defined in `Config/XUi_InGame/windows.xml` and controlled by
`XUiC_DevMenuWindow`. It uses V3 `TabSelector` elements with three tab
controllers:

- `DevMenuItemCategoryList` and `DevMenuItemList`
- `DevMenuCheatList`
- `LootableTileEntityList`

Catalogs are built from live game definitions at runtime:

- Items come from the loaded `ItemClass` catalog.
- Tile entities come from lootable composite blocks that use
  `TEFeatureStorage` and a `LootList`.
- Cheats are defined in code in `DevMenuCheatCatalog`.

Service classes perform the actual work. Keep UI controllers thin and route
behavior through services where practical.

## XUi And Config Notes

Use V3 paths:

```text
Config/XUi_InGame/
Config/Localization.csv
```

Do not use older `Config/XUi/`, `Localization.txt`, or `controls.xml` patterns
unless the game version changes back to those names.

Source config lives under `src/DevMenu/Config`. Build output copies those files
into `1A-DevMenu/Config` and the live game install.

## Feature Notes

Item spawning supports Q1-Q6 quality selection. The quality is applied only when
the target item supports quality.

Tile entity spawning places lootable blocks with no owner and resets them to
untouched state so game loot is generated when opened.

Loot reroll resets the currently targeted lootable tile entity to untouched and
clears generated contents. It intentionally skips `bPlayerStorage` containers to
avoid wiping player-owned storage.

The header `Reroll Loot` button is injected only into `windowLooting`, not
backpack, bag storage, workstation output, or dew collector windows.

## Release Process

Releases are created by the manual GitHub Actions workflow in
`.github/workflows/release.yml`.

The workflow:

- Accepts a manual `version_tag`, such as `0.1.0` or `v0.1.0`.
- Verifies `1A-DevMenu/ModInfo.xml` and `1A-DevMenu/DevMenu.dll` exist.
- Zips `1A-DevMenu`.
- Generates release notes through `Path-of-7D2D/Changelog-Generator`.
- Publishes a GitHub release with `softprops/action-gh-release`.

Run a Release build before publishing so the committed deployable DLL and config
are current.

Use conventional commit subjects for useful generated release notes:

```text
feat: add loot reroll button
fix: avoid rerolling player storage
docs: document dev menu hotkey
```

## Validation Checklist

Before publishing a release:

- Build in `Release` configuration.
- Confirm `1A-DevMenu/DevMenu.dll` is current.
- Confirm `1A-DevMenu/Config` matches `src/DevMenu/Config`.
- Start the game with EasyAntiCheat off.
- Confirm `/` opens and closes the Dev Menu.
- Confirm `Dev Menu` appears under `Options > Controls > Menus`.
- Confirm the inventory/crafting header icon opens the menu.
- Confirm item search, category filtering, and Q1-Q6 spawning work.
- Confirm major cheat actions work in singleplayer.
- Confirm tile entity spawning creates an untouched lootable container.
- Confirm `Reroll Loot` closes a loot container and rerolls it on reopen.
- Confirm player-owned storage is not rerolled.
- If multiplayer behavior changed, test with both server and client installed.

## Code Style

- Keep changes scoped to the feature being worked on.
- Prefer existing service and catalog patterns over new abstractions.
- Use live V3 game XML and decompiled types as the source of truth.
- Keep UI XML, localization, source services, and deployable config in sync.
- Avoid broad refactors unless they directly reduce risk or support the feature.
