using System;
using System.Collections.Generic;
using System.Globalization;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

namespace DevMenu.Commands
{
    [Preserve]
    public class ConsoleCmdDevMenu : ConsoleCmdAbstract
    {
        public override bool IsExecuteOnClient => true;

        public override DeviceFlag AllowedDeviceTypesClient =>
            DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX;

        public override string[] getCommands()
        {
            return new[] { "devmenu", "p7dev" };
        }

        public override string getDescription()
        {
            return "Opens the Dev Menu or runs Dev Menu item, entity, buff, cheat, and tile entity actions.";
        }

        public override string getHelp()
        {
            return "Usage:\n" +
                "  devmenu\n" +
                "  devmenu open\n" +
                "  devmenu items [filter]\n" +
                "  devmenu item <itemName> [count] [quality 1-6]\n" +
                "  devmenu entities [filter]\n" +
                "  devmenu entity <entityName> [count]\n" +
                "  devmenu entity <entityName> <x> <y> <z> [count] [yaw]\n" +
                "  devmenu buffs [filter]\n" +
                "  devmenu buff add <buffName> [durationSeconds]\n" +
                "  devmenu buff remove <buffName>\n" +
                "  devmenu cheats\n" +
                "  devmenu cheat <key>\n" +
                "  devmenu tiles [filter]\n" +
                "  devmenu tile <blockName> [x y z] [rotation]\n" +
                "  devmenu rerollloot <x> <y> <z>\n" +
                "  devmenu reload\n" +
                "\n" +
                "Examples:\n" +
                "  devmenu\n" +
                "  devmenu item gunHandgunT1Pistol\n" +
                "  devmenu item ammo9mmBulletBall 100\n" +
                "  devmenu item gunHandgunT1Pistol 1 6\n" +
                "  devmenu entity zombieArlene\n" +
                "  devmenu entity animalWolf 3\n" +
                "  devmenu buff add buffInjuryAbrasion 300\n" +
                "  devmenu buff remove buffInjuryAbrasion\n" +
                "  devmenu cheat noaggro\n" +
                "  devmenu tile lootChestHero";
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (_params.Count == 0 || IsSubcommand(_params[0], "open"))
            {
                OpenWindow();
                return;
            }

            if (IsSubcommand(_params[0], "help"))
            {
                Output(getHelp());
                return;
            }

            if (IsSubcommand(_params[0], "reload"))
            {
                DevMenuItemCatalog.Reload();
                DevMenuEntityCatalog.Reload();
                DevMenuBuffCatalog.Reload();
                LootableTileEntityCatalog.Reload();
                Output("Reloaded catalogs. Found " + DevMenuItemCatalog.Entries.Count + " items, " + DevMenuEntityCatalog.Entries.Count + " entities, " + DevMenuBuffCatalog.Entries.Count + " buffs, and " + LootableTileEntityCatalog.Entries.Count + " tile entities.");
                return;
            }

            if (IsSubcommand(_params[0], "items"))
            {
                ListItems(_params);
                return;
            }

            if (IsSubcommand(_params[0], "item"))
            {
                GiveItem(_params, _senderInfo);
                return;
            }

            if (IsSubcommand(_params[0], "entities"))
            {
                ListEntities(_params);
                return;
            }

            if (IsSubcommand(_params[0], "entity"))
            {
                SpawnEntity(_params, _senderInfo);
                return;
            }

            if (IsSubcommand(_params[0], "buffs"))
            {
                ListBuffs(_params);
                return;
            }

            if (IsSubcommand(_params[0], "buff"))
            {
                RunBuffAction(_params, _senderInfo);
                return;
            }

            if (IsSubcommand(_params[0], "cheats"))
            {
                ListCheats();
                return;
            }

            if (IsSubcommand(_params[0], "cheat"))
            {
                ToggleCheat(_params, _senderInfo);
                return;
            }

            if (IsSubcommand(_params[0], "tiles"))
            {
                ListTileEntities(_params);
                return;
            }

            if (IsSubcommand(_params[0], "tile"))
            {
                SpawnTileEntity(_params, _senderInfo);
                return;
            }

            if (IsSubcommand(_params[0], "rerollloot"))
            {
                RerollLoot(_params);
                return;
            }

            Output("Unknown subcommand: " + _params[0]);
            Output(getHelp());
        }

        private static void OpenWindow()
        {
            if (GameManager.IsDedicatedServer)
            {
                Output("The menu can only be opened on a client.");
                return;
            }

            LocalPlayerUI playerUi = LocalPlayerUI.GetUIForPrimaryPlayer();
            if (playerUi == null || playerUi.windowManager == null)
            {
                Output("No local player UI is available.");
                return;
            }

            playerUi.windowManager.Open(XUiC_DevMenuWindow.WindowGroupName, _bModal: true);
        }

        private static void ListItems(List<string> parameters)
        {
            string filter = parameters.Count > 1 ? string.Join(" ", parameters.GetRange(1, parameters.Count - 1).ToArray()) : null;
            int shown = 0;

            Output("Items: " + DevMenuItemCatalog.Entries.Count);
            foreach (DevMenuItemEntry entry in DevMenuItemCatalog.Entries)
            {
                if (!string.IsNullOrEmpty(filter) && !entry.MatchesSearch(filter))
                {
                    continue;
                }

                Output(entry.ItemName + " - " + entry.DisplayName + FormatSuffix(entry.Category));
                shown++;
                if (shown >= 80)
                {
                    Output("Output capped at 80 entries. Refine the filter to narrow the list.");
                    break;
                }
            }
        }

        private static void GiveItem(List<string> parameters, CommandSenderInfo senderInfo)
        {
            if (parameters.Count < 2)
            {
                Output("Usage: devmenu item <itemName> [count]");
                return;
            }

            int count = 1;
            if (parameters.Count >= 3 && !int.TryParse(parameters[2], out count))
            {
                Output("Invalid count. Usage: devmenu item <itemName> [count] [quality 1-6]");
                return;
            }

            int quality = 0;
            if (parameters.Count >= 4 && (!int.TryParse(parameters[3], out quality) || quality < 1 || quality > 6))
            {
                Output("Invalid quality. Usage: devmenu item <itemName> [count] [quality 1-6]");
                return;
            }

            EntityAlive entity = GetSenderEntity(senderInfo);
            if (entity == null && !GameManager.IsDedicatedServer)
            {
                entity = GameManager.Instance?.World?.GetPrimaryPlayer();
            }

            DevMenuItemSpawnService.TryGiveToPlayer(entity, parameters[1], count, quality, out string message);
            Output(message);
        }

        private static void ListEntities(List<string> parameters)
        {
            string filter = parameters.Count > 1 ? string.Join(" ", parameters.GetRange(1, parameters.Count - 1).ToArray()) : null;
            int shown = 0;

            Output("Entities: " + DevMenuEntityCatalog.Entries.Count);
            foreach (DevMenuEntityEntry entry in DevMenuEntityCatalog.Entries)
            {
                if (!string.IsNullOrEmpty(filter) && !entry.MatchesSearch(filter))
                {
                    continue;
                }

                Output(entry.EntityName + " - " + entry.DisplayName + FormatSuffix(entry.Category) + FormatSuffix(entry.EntityType));
                shown++;
                if (shown >= 80)
                {
                    Output("Output capped at 80 entries. Refine the filter to narrow the list.");
                    break;
                }
            }
        }

        private static void SpawnEntity(List<string> parameters, CommandSenderInfo senderInfo)
        {
            if (parameters.Count < 2)
            {
                Output("Usage: devmenu entity <entityName> [count]");
                return;
            }

            string entityName = parameters[1];
            if (parameters.Count >= 5)
            {
                if (!TryParseVector3(parameters, 2, out Vector3 spawnPosition))
                {
                    Output("Invalid position. Usage: devmenu entity <entityName> <x> <y> <z> [count] [yaw]");
                    return;
                }

                int count = 1;
                if (parameters.Count >= 6 && !int.TryParse(parameters[5], out count))
                {
                    Output("Invalid count. Usage: devmenu entity <entityName> <x> <y> <z> [count] [yaw]");
                    return;
                }

                float yaw = 0f;
                if (parameters.Count >= 7 && !float.TryParse(parameters[6], NumberStyles.Float, CultureInfo.InvariantCulture, out yaw))
                {
                    Output("Invalid yaw. Use a numeric Y rotation in degrees.");
                    return;
                }

                EntityAlive entity = GetSenderEntity(senderInfo);
                DevMenuEntitySpawnService.TrySpawnAt(entityName, spawnPosition, new Vector3(0f, yaw, 0f), count, entity, out string spawnMessage);
                Output(spawnMessage);
                return;
            }

            int frontCount = 1;
            if (parameters.Count >= 3 && !int.TryParse(parameters[2], out frontCount))
            {
                Output("Invalid count. Usage: devmenu entity <entityName> [count]");
                return;
            }

            EntityAlive senderEntity = GetSenderEntity(senderInfo);
            if (senderEntity == null && !GameManager.IsDedicatedServer)
            {
                senderEntity = GameManager.Instance?.World?.GetPrimaryPlayer();
            }

            DevMenuEntitySpawnService.TrySpawnInFrontOfPlayer(senderEntity, entityName, frontCount, out string message);
            Output(message);
        }

        private static void ListBuffs(List<string> parameters)
        {
            string filter = parameters.Count > 1 ? string.Join(" ", parameters.GetRange(1, parameters.Count - 1).ToArray()) : null;
            int shown = 0;

            Output("Buffs/debuffs: " + DevMenuBuffCatalog.Entries.Count);
            foreach (DevMenuBuffEntry entry in DevMenuBuffCatalog.Entries)
            {
                if (!string.IsNullOrEmpty(filter) && !entry.MatchesSearch(filter))
                {
                    continue;
                }

                Output(entry.BuffName + " - " + entry.DisplayName + FormatSuffix(entry.Category) + FormatSuffix(entry.Flags));
                shown++;
                if (shown >= 80)
                {
                    Output("Output capped at 80 entries. Refine the filter to narrow the list.");
                    break;
                }
            }
        }

        private static void RunBuffAction(List<string> parameters, CommandSenderInfo senderInfo)
        {
            if (parameters.Count < 3)
            {
                Output("Usage: devmenu buff add <buffName> [durationSeconds] OR devmenu buff remove <buffName>");
                return;
            }

            string action = parameters[1];
            string buffName = parameters[2];
            EntityAlive entity = GetSenderEntity(senderInfo);
            if (entity == null && !GameManager.IsDedicatedServer)
            {
                entity = GameManager.Instance?.World?.GetPrimaryPlayer();
            }

            if (IsSubcommand(action, "add"))
            {
                float durationSeconds = 60f;
                if (parameters.Count >= 4 && !float.TryParse(parameters[3], NumberStyles.Float, CultureInfo.InvariantCulture, out durationSeconds))
                {
                    Output("Invalid duration. Usage: devmenu buff add <buffName> [durationSeconds]");
                    return;
                }

                DevMenuBuffService.TryAddToPlayer(entity, buffName, durationSeconds, out string message);
                Output(message);
                return;
            }

            if (IsSubcommand(action, "remove"))
            {
                DevMenuBuffService.TryRemoveFromPlayer(entity, buffName, out string message);
                Output(message);
                return;
            }

            Output("Unknown buff action: " + action);
            Output("Usage: devmenu buff add <buffName> [durationSeconds] OR devmenu buff remove <buffName>");
        }

        private static void ListCheats()
        {
            Output("Cheats:");
            foreach (DevMenuCheatEntry entry in DevMenuCheatCatalog.Entries)
            {
                Output(entry.Key + " - " + entry.DisplayName + " - " + entry.Description);
            }
        }

        private static void ToggleCheat(List<string> parameters, CommandSenderInfo senderInfo)
        {
            if (parameters.Count < 2)
            {
                Output("Usage: devmenu cheat <key>");
                return;
            }

            EntityAlive entity = GetSenderEntity(senderInfo);
            if (entity == null && !GameManager.IsDedicatedServer)
            {
                entity = GameManager.Instance?.World?.GetPrimaryPlayer();
            }

            IList<string> args = parameters.Count > 2
                ? parameters.GetRange(2, parameters.Count - 2)
                : null;

            DevMenuCheatService.RunForPlayer(entity, parameters[1], args, out string message);
            Output(message);
        }

        private static void ListTileEntities(List<string> parameters)
        {
            string filter = parameters.Count > 1 ? string.Join(" ", parameters.GetRange(1, parameters.Count - 1).ToArray()) : null;
            int shown = 0;

            Output("Lootable tile entities: " + LootableTileEntityCatalog.Entries.Count);
            foreach (LootableTileEntityEntry entry in LootableTileEntityCatalog.Entries)
            {
                if (!string.IsNullOrEmpty(filter) && !entry.MatchesSearch(filter))
                {
                    continue;
                }

                Output(entry.BlockName + " - " + entry.DisplayName + " - loot: " + entry.LootList + FormatSuffix(entry.Flags));
                shown++;
                if (shown >= 80)
                {
                    Output("Output capped at 80 entries. Refine the filter to narrow the list.");
                    break;
                }
            }
        }

        private static void SpawnTileEntity(List<string> parameters, CommandSenderInfo senderInfo)
        {
            if (parameters.Count < 2)
            {
                Output("Usage: devmenu tile <blockName> [x y z] [rotation]");
                return;
            }

            string blockName = parameters[1];
            if (parameters.Count >= 5)
            {
                if (!TryParsePosition(parameters, 2, out Vector3i blockPos))
                {
                    Output("Invalid position. Usage: devmenu tile <blockName> [x y z] [rotation]");
                    return;
                }

                byte rotation = 0;
                if (parameters.Count >= 6 && !byte.TryParse(parameters[5], out rotation))
                {
                    Output("Invalid rotation. Use a value from 0 to 27.");
                    return;
                }

                EntityAlive entity = GetSenderEntity(senderInfo);
                LootableTileEntitySpawnService.TrySpawnAt(blockName, blockPos, rotation, entity, out string spawnMessage);
                Output(spawnMessage);
                return;
            }

            LootableTileEntitySpawnService.RequestSpawnInFrontOfPrimaryPlayer(blockName, out string message);
            Output(message);
        }

        private static void RerollLoot(List<string> parameters)
        {
            if (parameters.Count < 4 || !TryParsePosition(parameters, 1, out Vector3i blockPos))
            {
                Output("Usage: devmenu rerollloot <x> <y> <z>");
                return;
            }

            DevMenuLootRerollService.TryRerollAt(blockPos, out string message);
            Output(message);
        }

        private static bool TryParsePosition(List<string> parameters, int startIndex, out Vector3i blockPos)
        {
            blockPos = Vector3i.zero;
            if (!int.TryParse(parameters[startIndex], out int x) ||
                !int.TryParse(parameters[startIndex + 1], out int y) ||
                !int.TryParse(parameters[startIndex + 2], out int z))
            {
                return false;
            }

            blockPos = new Vector3i(x, y, z);
            return true;
        }

        private static bool TryParseVector3(List<string> parameters, int startIndex, out Vector3 value)
        {
            value = Vector3.zero;
            return float.TryParse(parameters[startIndex], NumberStyles.Float, CultureInfo.InvariantCulture, out value.x) &&
                float.TryParse(parameters[startIndex + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out value.y) &&
                float.TryParse(parameters[startIndex + 2], NumberStyles.Float, CultureInfo.InvariantCulture, out value.z);
        }

        private static EntityAlive GetSenderEntity(CommandSenderInfo senderInfo)
        {
            if (GameManager.Instance?.World == null)
            {
                return null;
            }

            if (senderInfo.RemoteClientInfo != null)
            {
                return GameManager.Instance.World.GetEntity(senderInfo.RemoteClientInfo.entityId) as EntityAlive;
            }

            if (!GameManager.IsDedicatedServer)
            {
                return GameManager.Instance.World.GetPrimaryPlayer();
            }

            return null;
        }

        private static bool IsSubcommand(string value, string expected)
        {
            return value.Equals(expected, StringComparison.OrdinalIgnoreCase);
        }

        private static string FormatSuffix(string value)
        {
            return string.IsNullOrEmpty(value) ? "" : " - " + value;
        }

        private static void Output(string message)
        {
            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[DevMenu] " + message);
        }
    }
}
