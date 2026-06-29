# Dev Menu

Dev Menu is an in-game developer toolbox for **7 Days to Die V3.0 "Dead Hot
Summer"**.

It adds one menu for spawning items and entities, applying buffs/debuffs,
running common developer cheats, spawning lootable tile entities, and rerolling
loot containers while testing.

## Features

- Tabbed in-game Dev Menu with `Items`, `Entities`, `Buffs/Debuffs`, `Cheats`, and `Tile Entities` tabs.
- Rebindable hotkey in `Options > Controls > Menus`; default key is `/`.
- Inventory/crafting header icon for opening the menu from the normal UI.
- `devmenu` and `p7dev` console commands.
- Categorized item browser with search, category filtering, and Q1-Q6 item tier
  selection.
- Categorized entity browser with search and category filtering for zombies,
  animals, NPCs, vehicles, and utility entities.
- Categorized buff/debuff browser with search, active status, duration presets,
  and add/remove actions for testing loaded vanilla and modded buffs.
- Common cheat toggles and actions for development/testing.
- Lootable tile entity spawning in front of the player.
- `Reroll Loot` button on lootable containers to close the container and reset
  it to untouched so loot is generated again on next open.

## Installation

1. Download a release zip or build the mod locally.
2. Copy the `1A-DevMenu` folder into your game `Mods` folder.

The final folder should look like this:

```text
7 Days To Die/Mods/1A-DevMenu/ModInfo.xml
7 Days To Die/Mods/1A-DevMenu/DevMenu.dll
7 Days To Die/Mods/1A-DevMenu/Config/
```

Keep the game's `0_TFP_Harmony` mod installed. Dev Menu uses Harmony patches for
the hotkey and loot-container controls.

## Opening The Menu

Use any of these options:

- Press `/` in game.
- Rebind `Dev Menu` in `Options > Controls > Menus`.
- Click the Dev Menu icon in the inventory/crafting header.
- Open the console and run `devmenu` or `p7dev`.

## Items Tab

The Items tab lets you browse and spawn loaded item definitions.

- Select a category first, such as tools, ammo, weapons, armor, blocks, or
  resources.
- Use search to narrow the current list.
- Toggle `Filter by category` when searching across categories.
- Choose Q1-Q6 before spawning tiered items.
- Spawn one or ten of the selected item.

## Entities Tab

The Entities tab lets you browse and spawn loaded entity definitions.

- Select a category first, such as zombies, hostile animals, passive animals,
  NPCs/traders, drones, vehicles, or loot/utility entities.
- Use search to narrow the current list.
- Toggle `Filter by category` when searching across categories.
- Spawn one or five of the selected entity.

## Buffs/Debuffs Tab

The Buffs/Debuffs tab lists loaded buff definitions from the runtime buff
catalog, including modded buffs.

- Select a category first, such as injuries, disease/poison, status debuffs,
  drugs/candy, food/drink, healing, equipment/set, environmental, debug/admin,
  or hidden/utility.
- Use search to narrow the current list.
- Toggle `Filter by category` when searching across categories.
- Choose 30 seconds, 1 minute, 5 minutes, 10 minutes, or 60 minutes.
- Add the selected buff/debuff to the player or remove it if it is active.

## Cheats Tab

The Cheats tab includes quick toggles and one-shot actions for common testing
tasks:

- God mode
- Unlimited ammo
- Noclip
- No aggro
- Heal and fill needs
- Clear debuffs
- Repair gear
- Unlock recipes
- Add XP or skill points
- Teleport to crosshair
- Set time of day
- Change weather

## Tile Entities Tab

The Tile Entities tab lists lootable tile entities discovered from the loaded
block catalog. Spawning one places it in front of the player as an untouched
world loot container, so the game generates loot when it is opened.

## Reroll Loot

Lootable containers get a `Reroll Loot` button in the container header. Pressing
it closes the container, clears the generated contents, and marks the container
as untouched. Reopen the container to generate a new loot roll.

Player-owned storage is ignored by the reroll action to avoid wiping personal
storage.

## Console Examples

```text
devmenu
p7dev
devmenu item gunHandgunT1Pistol
devmenu item gunHandgunT1Pistol 1 6
devmenu item ammo9mmBulletBall 100
devmenu entities zombie
devmenu entity zombieArlene
devmenu entity animalWolf 3
devmenu buffs injury
devmenu buff add buffInjuryAbrasion 300
devmenu buff remove buffInjuryAbrasion
devmenu cheat god
devmenu cheat ammo
devmenu cheat noclip
devmenu cheat noaggro
devmenu cheat healneeds
devmenu cheat repairgear
devmenu cheat unlockrecipes
devmenu cheat teleportcrosshair
devmenu cheat timenight
devmenu cheat weatherstorm
devmenu tile lootChestHero
devmenu rerollloot 100 64 100
```

## Multiplayer

Dev Menu is an admin/testing mod. Install it on both the server and client for
multiplayer testing.

The UI runs client-side. Item spawning, entity spawning, buff/debuff actions,
tile entity spawning, cheat actions, and loot rerolls are routed to the server
when needed.

## EasyAntiCheat

This mod includes a custom DLL and Harmony patches. Launch the game with
EasyAntiCheat disabled.

## Contributing

Codebase notes, build steps, validation guidance, and release details are in
[CONTRIBUTORS.md](CONTRIBUTORS.md).

## License

This project is licensed under the [MIT License](LICENSE).
