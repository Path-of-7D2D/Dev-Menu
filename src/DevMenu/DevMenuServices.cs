using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

namespace DevMenu
{
    [Preserve]
    public static class DevMenuItemSpawnService
    {
        public static bool RequestGiveToPrimaryPlayer(string itemName, int count, out string message)
        {
            return RequestGiveToPrimaryPlayer(itemName, count, 0, out message);
        }

        public static bool RequestGiveToPrimaryPlayer(string itemName, int count, int quality, out string message)
        {
            message = null;

            if (GameManager.IsDedicatedServer)
            {
                message = "No local player exists on a dedicated server. Use 'devmenu item <itemName> [count]' from a client or with a player sender.";
                return false;
            }

            World world = GameManager.Instance?.World;
            EntityPlayerLocal player = world?.GetPrimaryPlayer();
            if (world == null || player == null)
            {
                message = "No local player is available.";
                return false;
            }

            if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            {
                return TryGiveToPlayer(player, itemName, count, quality, out message);
            }

            SendServerItemCommand(itemName, count, quality);
            message = "Requested " + FormatStack(itemName, count, quality) + ".";
            return true;
        }

        public static bool TryGiveToPlayer(EntityAlive player, string itemName, int count, out string message)
        {
            return TryGiveToPlayer(player, itemName, count, 0, out message);
        }

        public static bool TryGiveToPlayer(EntityAlive player, string itemName, int count, int quality, out string message)
        {
            message = null;

            if (player == null)
            {
                message = "No target player is available.";
                return false;
            }

            if (!TryCreateStack(itemName, count, quality, out ItemStack stack, out bool qualityApplied, out message))
            {
                return false;
            }

            if (player.inventory != null && player.inventory.AddItem(stack))
            {
                message = "Added " + FormatStack(itemName, stack.count, qualityApplied ? (int)stack.itemValue.Quality : 0) + " to inventory.";
                return true;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ItemDropServer(
                    stack,
                    player.position + Vector3.up,
                    Vector3.zero,
                    player.entityId,
                    60f,
                    false);

                message = "Inventory full. Dropped " + FormatStack(itemName, stack.count, qualityApplied ? (int)stack.itemValue.Quality : 0) + " near the player.";
                return true;
            }

            message = "Inventory full and no GameManager is available for dropping the item.";
            return false;
        }

        private static bool TryCreateStack(string itemName, int count, int quality, out ItemStack stack, out bool qualityApplied, out string message)
        {
            stack = ItemStack.Empty.Clone();
            qualityApplied = false;
            message = null;

            ItemValue itemValue = ItemClass.GetItem(itemName, false);
            if (itemValue == null || itemValue.IsEmpty())
            {
                message = "Unknown item: " + itemName;
                return false;
            }

            itemValue = itemValue.Clone();
            if (itemValue.ItemClass != null)
            {
                itemValue.Meta = itemValue.ItemClass.GetInitialMetadata(itemValue);
                if (quality > 0 && SupportsQuality(itemValue, itemValue.ItemClass))
                {
                    itemValue.Quality = (ushort)ClampQuality(quality);
                    itemValue.UseTimes = 0;
                    qualityApplied = true;
                }
            }

            stack = new ItemStack(itemValue, ClampCount(count));
            return true;
        }

        private static bool SupportsQuality(ItemValue itemValue, ItemClass itemClass)
        {
            if (itemValue != null && itemValue.HasQuality)
            {
                return true;
            }

            string value;
            if (itemClass?.Properties != null &&
                itemClass.Properties.Values.TryGetValue(ItemClass.PropShowQuality, out value) &&
                bool.TryParse(value, out bool showQuality))
            {
                return showQuality;
            }

            return itemClass?.Properties != null &&
                itemClass.Properties.Values.ContainsKey(ItemClass.PropQualityMin);
        }

        private static int ClampCount(int count)
        {
            if (count < 1)
            {
                return 1;
            }

            return count > 60000 ? 60000 : count;
        }

        private static int ClampQuality(int quality)
        {
            if (quality < 1)
            {
                return 1;
            }

            return quality > 6 ? 6 : quality;
        }

        private static string FormatStack(string itemName, int count, int quality)
        {
            string qualityText = quality > 0 ? " Q" + ClampQuality(quality) : "";
            return ClampCount(count) + "x " + itemName + qualityText;
        }

        private static void SendServerItemCommand(string itemName, int count, int quality)
        {
            string command = "devmenu item " + Quote(itemName) + " " + ClampCount(count);
            if (quality > 0)
            {
                command += " " + ClampQuality(quality);
            }

            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                NetPackageManager.GetPackage<NetPackageConsoleCmdServer>().Setup(command));
        }

        private static string Quote(string value)
        {
            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }
    }

    [Preserve]
    public static class DevMenuCheatService
    {
        private const string InfiniteAmmoBuff = "devMenuInfiniteAmmo";
        private const int DebugXpAmount = 10000;
        private const int SkillPointAmount = 1;
        private const float TeleportRayDistance = 250f;

        public static bool ToggleForPrimaryPlayer(string key, out string message)
        {
            message = null;

            if (GameManager.IsDedicatedServer)
            {
                message = "No local player exists on a dedicated server. Use the command from a client.";
                return false;
            }

            EntityPlayerLocal player = GameManager.Instance?.World?.GetPrimaryPlayer();
            if (player == null)
            {
                message = "No local player is available.";
                return false;
            }

            if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            {
                if (key.Equals("teleportcrosshair", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryGetCrosshairTeleportDestination(player, out Vector3 destination, out message))
                    {
                        return false;
                    }

                    SendServerCheatCommand("teleportto", destination);
                    message = "Requested teleport to crosshair.";
                    return true;
                }

                SendServerCheatCommand(key);
                message = "Requested " + GetDisplayName(key) + ".";
                return true;
            }

            return ToggleForPlayer(player, key, out message);
        }

        public static bool ToggleForPlayer(EntityAlive player, string key, out string message)
        {
            return RunForPlayer(player, key, null, out message);
        }

        public static bool RunForPlayer(EntityAlive player, string key, IList<string> args, out string message)
        {
            message = null;

            if (player == null)
            {
                message = "No target player is available.";
                return false;
            }

            if (key.Equals("god", StringComparison.OrdinalIgnoreCase))
            {
                bool enabled = !player.IsGodMode.Value;
                player.IsGodMode.Value = enabled;

                if (player.Buffs != null)
                {
                    if (enabled)
                    {
                        player.Buffs.AddBuff("god");
                    }
                    else
                    {
                        player.Buffs.RemoveBuff("god");
                    }
                }

                message = "God mode " + FormatState(enabled) + ".";
                return true;
            }

            if (key.Equals("ammo", StringComparison.OrdinalIgnoreCase))
            {
                if (player.Buffs == null)
                {
                    message = "The player buff container is not available.";
                    return false;
                }

                bool enabled = !player.Buffs.HasBuff(InfiniteAmmoBuff);
                if (enabled)
                {
                    player.Buffs.AddBuff(InfiniteAmmoBuff);
                }
                else
                {
                    player.Buffs.RemoveBuff(InfiniteAmmoBuff);
                }

                message = "Unlimited ammo " + FormatState(enabled) + ".";
                return true;
            }

            if (key.Equals("noclip", StringComparison.OrdinalIgnoreCase))
            {
                bool enabled = !(player.IsFlyMode.Value && player.IsNoCollisionMode.Value);
                player.IsFlyMode.Value = enabled;
                player.IsNoCollisionMode.Value = enabled;
                message = "Noclip " + FormatState(enabled) + ".";
                return true;
            }

            if (key.Equals("noaggro", StringComparison.OrdinalIgnoreCase))
            {
                EntityPlayer entityPlayer = player as EntityPlayer;
                if (entityPlayer == null)
                {
                    message = "No aggro can only be toggled for players.";
                    return false;
                }

                bool enabled = !entityPlayer.IsSpectator;
                entityPlayer.IsSpectator = enabled;
                message = "No aggro " + FormatState(enabled) + ".";
                return true;
            }

            if (key.Equals("healneeds", StringComparison.OrdinalIgnoreCase))
            {
                return HealAndFillNeeds(player, out message);
            }

            if (key.Equals("cleardebuffs", StringComparison.OrdinalIgnoreCase))
            {
                return ClearDebuffs(player, out message);
            }

            if (key.Equals("repairgear", StringComparison.OrdinalIgnoreCase))
            {
                return RepairGear(player, out message);
            }

            if (key.Equals("unlockrecipes", StringComparison.OrdinalIgnoreCase))
            {
                return UnlockAllRecipes(player, out message);
            }

            if (key.Equals("addxp", StringComparison.OrdinalIgnoreCase))
            {
                return AddDebugXp(player, out message);
            }

            if (key.Equals("addskillpoint", StringComparison.OrdinalIgnoreCase))
            {
                return AddSkillPoint(player, out message);
            }

            if (key.Equals("teleportcrosshair", StringComparison.OrdinalIgnoreCase))
            {
                return TeleportToCrosshair(player, out message);
            }

            if (key.Equals("teleportto", StringComparison.OrdinalIgnoreCase))
            {
                return TeleportToPosition(player, args, out message);
            }

            if (key.Equals("timemorning", StringComparison.OrdinalIgnoreCase))
            {
                return SetCurrentDayTime(8, 0, out message);
            }

            if (key.Equals("timenoon", StringComparison.OrdinalIgnoreCase))
            {
                return SetCurrentDayTime(12, 0, out message);
            }

            if (key.Equals("timenight", StringComparison.OrdinalIgnoreCase))
            {
                return SetCurrentDayTime(22, 0, out message);
            }

            if (key.Equals("weatherclear", StringComparison.OrdinalIgnoreCase))
            {
                return SetWeatherClear(out message);
            }

            if (key.Equals("weatherrain", StringComparison.OrdinalIgnoreCase))
            {
                return SetWeatherRain(out message);
            }

            if (key.Equals("weatherfog", StringComparison.OrdinalIgnoreCase))
            {
                return SetWeatherFog(out message);
            }

            if (key.Equals("weatherstorm", StringComparison.OrdinalIgnoreCase))
            {
                return SetWeatherStorm(out message);
            }

            message = "Unknown cheat: " + key;
            return false;
        }

        public static string GetStatus(string key)
        {
            EntityAlive player = GameManager.Instance?.World?.GetPrimaryPlayer();
            if (player == null)
            {
                return "Unavailable";
            }

            if (key.Equals("god", StringComparison.OrdinalIgnoreCase))
            {
                return FormatState(player.IsGodMode.Value);
            }

            if (key.Equals("ammo", StringComparison.OrdinalIgnoreCase))
            {
                return player.Buffs != null && player.Buffs.HasBuff(InfiniteAmmoBuff)
                    ? "Enabled"
                    : "Disabled";
            }

            if (key.Equals("noclip", StringComparison.OrdinalIgnoreCase))
            {
                return FormatState(player.IsFlyMode.Value && player.IsNoCollisionMode.Value);
            }

            if (key.Equals("noaggro", StringComparison.OrdinalIgnoreCase))
            {
                EntityPlayer entityPlayer = player as EntityPlayer;
                return entityPlayer == null ? "Unavailable" : FormatState(entityPlayer.IsSpectator);
            }

            if (key.Equals("addxp", StringComparison.OrdinalIgnoreCase))
            {
                return "+10k XP";
            }

            if (key.Equals("addskillpoint", StringComparison.OrdinalIgnoreCase))
            {
                return "+1 SP";
            }

            if (IsActionKey(key))
            {
                return "Ready";
            }

            return "";
        }

        private static bool HealAndFillNeeds(EntityAlive player, out string message)
        {
            message = null;
            EntityStats stats = player.Stats;
            if (stats == null)
            {
                message = "The player stats container is not available.";
                return false;
            }

            FillStat(stats.Health);
            FillStat(stats.Stamina);
            FillStat(stats.Food);
            FillStat(stats.Water);
            MarkPlayerChanged(player);
            message = "Restored health, stamina, food, and water.";
            return true;
        }

        private static bool ClearDebuffs(EntityAlive player, out string message)
        {
            message = null;
            if (player.Buffs == null || player.Buffs.ActiveBuffs == null)
            {
                message = "The player buff container is not available.";
                return false;
            }

            int removed = 0;
            for (int i = 0; i < player.Buffs.ActiveBuffs.Count; i++)
            {
                BuffValue buff = player.Buffs.ActiveBuffs[i];
                if (buff?.BuffClass != null && buff.BuffClass.DamageType != EnumDamageTypes.None)
                {
                    buff.Remove = true;
                    removed++;
                }
            }

            message = removed == 0
                ? "No negative buffs were active."
                : "Marked " + removed + " negative buff(s) for removal.";
            return true;
        }

        private static bool RepairGear(EntityAlive player, out string message)
        {
            int repaired = 0;
            repaired += RepairInventory(player.inventory);
            repaired += RepairEquipment(player.equipment);

            EntityPlayer entityPlayer = player as EntityPlayer;
            if (entityPlayer?.bag != null)
            {
                repaired += RepairBag(entityPlayer.bag);
            }

            message = repaired == 0
                ? "No damaged gear was found."
                : "Repaired " + repaired + " damaged item(s).";
            return true;
        }

        private static bool UnlockAllRecipes(EntityAlive player, out string message)
        {
            EntityPlayer entityPlayer = player as EntityPlayer;
            if (entityPlayer == null)
            {
                message = "Recipe unlocks can only be applied to players.";
                return false;
            }

            List<Recipe> recipes = CraftingManager.GetAllRecipes();
            if (recipes == null)
            {
                message = "No recipe catalog is loaded.";
                return false;
            }

            int unlocked = 0;
            for (int i = 0; i < recipes.Count; i++)
            {
                Recipe recipe = recipes[i];
                if (recipe == null)
                {
                    continue;
                }

                try
                {
                    if (!recipe.IsUnlocked(entityPlayer))
                    {
                        unlocked++;
                    }

                    CraftingManager.UnlockRecipe(recipe, entityPlayer);
                }
                catch (Exception ex)
                {
                    Log.Warning("[DevMenu] Failed to unlock recipe {0}: {1}", recipe, ex.Message);
                }
            }

            MarkPlayerChanged(entityPlayer);
            message = unlocked == 0
                ? "All loaded recipes were already unlocked."
                : "Unlocked " + unlocked + " recipe(s).";
            return true;
        }

        private static bool AddDebugXp(EntityAlive player, out string message)
        {
            EntityPlayer entityPlayer = player as EntityPlayer;
            if (entityPlayer?.Progression == null)
            {
                message = "XP can only be applied to players with progression.";
                return false;
            }

            entityPlayer.Progression.AddLevelExp(DebugXpAmount, "_xpOther", Progression.XPTypes.Debug);
            MarkPlayerChanged(entityPlayer);
            message = "Added " + DebugXpAmount.ToString("N0", CultureInfo.InvariantCulture) + " XP.";
            return true;
        }

        private static bool AddSkillPoint(EntityAlive player, out string message)
        {
            EntityPlayer entityPlayer = player as EntityPlayer;
            if (entityPlayer?.Progression == null)
            {
                message = "Skill points can only be applied to players with progression.";
                return false;
            }

            entityPlayer.Progression.SkillPoints += SkillPointAmount;
            entityPlayer.Progression.bProgressionStatsChanged = true;
            MarkPlayerChanged(entityPlayer);
            message = "Added " + SkillPointAmount + " skill point.";
            return true;
        }

        private static bool TeleportToCrosshair(EntityAlive player, out string message)
        {
            EntityPlayerLocal localPlayer = player as EntityPlayerLocal;
            if (localPlayer == null)
            {
                message = "Teleport to crosshair is only available from the local player UI.";
                return false;
            }

            if (!TryGetCrosshairTeleportDestination(localPlayer, out Vector3 destination, out message))
            {
                return false;
            }

            return TeleportPlayer(localPlayer, destination, out message);
        }

        private static bool TeleportToPosition(EntityAlive player, IList<string> args, out string message)
        {
            if (!TryParseVector3(args, out Vector3 destination))
            {
                message = "Usage: devmenu cheat teleportto <x> <y> <z>";
                return false;
            }

            EntityPlayer entityPlayer = player as EntityPlayer;
            if (entityPlayer == null)
            {
                message = "Teleport can only be applied to players.";
                return false;
            }

            return TeleportPlayer(entityPlayer, destination, out message);
        }

        private static bool SetCurrentDayTime(int hour, int minute, out string message)
        {
            World world = GameManager.Instance?.World;
            if (world == null)
            {
                message = "No world is loaded.";
                return false;
            }

            int day = GameUtils.WorldTimeToDays(world.GetWorldTime());
            ulong worldTime = GameUtils.DayTimeToWorldTime(day, hour, minute);
            world.SetTimeJump(worldTime);
            message = "Set time to day " + day + " " + hour.ToString("00") + ":" + minute.ToString("00") + ".";
            return true;
        }

        private static bool SetWeatherClear(out string message)
        {
            WeatherManager.forceClouds = -1f;
            WeatherManager.forceRain = -1f;
            WeatherManager.forceSnowfall = -1f;
            WeatherManager.forceTemperature = -100f;
            WeatherManager.forceWind = -1f;
            WeatherManager.SetSimRandom(-1f);
            SkyManager.SetFogDebug();
            SkyManager.SetFogDebugColor();
            TriggerWeatherUpdate();
            message = "Reset forced weather and fog overrides.";
            return true;
        }

        private static bool SetWeatherRain(out string message)
        {
            WeatherManager.forceClouds = 0.85f;
            WeatherManager.forceRain = 1f;
            WeatherManager.forceSnowfall = -1f;
            WeatherManager.forceWind = 30f;
            SkyManager.SetFogDebug();
            TriggerWeatherUpdate();
            message = "Forced heavy rain.";
            return true;
        }

        private static bool SetWeatherFog(out string message)
        {
            WeatherManager.forceClouds = 0.65f;
            WeatherManager.forceRain = -1f;
            SkyManager.SetFogDebug(0.75f, 8f, 70f);
            TriggerWeatherUpdate();
            message = "Forced dense fog.";
            return true;
        }

        private static bool SetWeatherStorm(out string message)
        {
            if (WeatherManager.Instance == null)
            {
                message = "The weather manager is not available.";
                return false;
            }

            WeatherManager.forceClouds = 1f;
            WeatherManager.forceRain = 1f;
            WeatherManager.forceWind = 70f;
            WeatherManager.Instance.SetStorm(null, 4000);
            TriggerWeatherUpdate();
            message = "Started a storm in the current biome.";
            return true;
        }

        private static void FillStat(Stat stat)
        {
            if (stat == null)
            {
                return;
            }

            float max = stat.ModifiedMax > 0f ? stat.ModifiedMax : stat.Max;
            if (max > 0f)
            {
                stat.Value = max;
            }
        }

        private static int RepairInventory(Inventory inventory)
        {
            if (inventory == null)
            {
                return 0;
            }

            int repaired = 0;
            for (int i = 0; i < inventory.GetItemCount(); i++)
            {
                ItemStack stack = inventory.GetItem(i);
                if (!stack.IsEmpty() && RepairItemValue(stack.itemValue))
                {
                    inventory.SetItem(i, stack);
                    repaired++;
                }
            }

            if (repaired > 0)
            {
                inventory.Changed();
                inventory.ForceHoldingItemUpdate();
            }

            return repaired;
        }

        private static int RepairBag(Bag bag)
        {
            ItemStack[] slots = bag.GetSlots();
            if (slots == null)
            {
                return 0;
            }

            int repaired = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                ItemStack stack = slots[i];
                if (!stack.IsEmpty() && RepairItemValue(stack.itemValue))
                {
                    slots[i] = stack;
                    repaired++;
                }
            }

            if (repaired > 0)
            {
                bag.SetSlots(slots);
            }

            return repaired;
        }

        private static int RepairEquipment(Equipment equipment)
        {
            if (equipment == null)
            {
                return 0;
            }

            int repaired = 0;
            for (int i = 0; i < equipment.GetSlotCount(); i++)
            {
                ItemValue itemValue = equipment.GetSlotItem(i);
                if (RepairItemValue(itemValue))
                {
                    equipment.SetSlotItem(i, itemValue);
                    repaired++;
                }
            }

            return repaired;
        }

        private static bool RepairItemValue(ItemValue itemValue)
        {
            if (itemValue == null || itemValue.IsEmpty())
            {
                return false;
            }

            bool repaired = false;
            try
            {
                if (itemValue.MaxUseTimes > 0 && itemValue.UseTimes > 0f)
                {
                    itemValue.UseTimes = 0f;
                    repaired = true;
                }
            }
            catch (Exception)
            {
                return repaired;
            }

            repaired |= RepairItemValues(itemValue.Modifications);
            repaired |= RepairItemValues(itemValue.CosmeticMods);
            return repaired;
        }

        private static bool RepairItemValues(ItemValue[] itemValues)
        {
            if (itemValues == null)
            {
                return false;
            }

            bool repaired = false;
            for (int i = 0; i < itemValues.Length; i++)
            {
                repaired |= RepairItemValue(itemValues[i]);
            }

            return repaired;
        }

        private static bool TryGetCrosshairTeleportDestination(EntityPlayerLocal player, out Vector3 destination, out string message)
        {
            destination = Vector3.zero;
            message = null;

            World world = GameManager.Instance?.World;
            if (world == null)
            {
                message = "No world is loaded.";
                return false;
            }

            Ray ray = player.GetLookRay();
            if (!Voxel.Raycast(world, ray, TeleportRayDistance, bHitTransparentBlocks: true, bHitNotCollidableBlocks: false))
            {
                message = "No crosshair target was found within " + TeleportRayDistance.ToString("0", CultureInfo.InvariantCulture) + " meters.";
                return false;
            }

            WorldRayHitInfo hitInfo = Voxel.voxelRayHitInfo;
            if (hitInfo == null || !hitInfo.bHitValid)
            {
                message = "No valid crosshair target was found.";
                return false;
            }

            Vector3 normal = GetBlockFaceNormal(hitInfo.hit.blockFace);
            destination = hitInfo.hit.pos + normal * 0.8f;
            if (normal.y <= 0.5f)
            {
                destination += Vector3.up * 0.1f;
            }

            return true;
        }

        private static Vector3 GetBlockFaceNormal(BlockFace blockFace)
        {
            switch (blockFace)
            {
                case BlockFace.Top:
                    return Vector3.up;
                case BlockFace.Bottom:
                    return Vector3.down;
                case BlockFace.North:
                    return Vector3.back;
                case BlockFace.South:
                    return Vector3.forward;
                case BlockFace.East:
                    return Vector3.right;
                case BlockFace.West:
                    return Vector3.left;
                default:
                    return Vector3.up;
            }
        }

        private static bool TeleportPlayer(EntityPlayer entityPlayer, Vector3 destination, out string message)
        {
            if (entityPlayer == null)
            {
                message = "Teleport can only be applied to players.";
                return false;
            }

            if (LockManager.Instance != null)
            {
                LockManager.Instance.ForceUnlockByPlayer(entityPlayer.entityId);
            }

            EntityPlayerLocal localPlayer = entityPlayer as EntityPlayerLocal;
            if (localPlayer != null)
            {
                localPlayer.TeleportToPosition(destination);
            }
            else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            {
                ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(entityPlayer.entityId);
                if (clientInfo != null)
                {
                    clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(destination));
                }
                else
                {
                    entityPlayer.Teleport(destination);
                }
            }
            else
            {
                entityPlayer.Teleport(destination);
            }

            message = "Teleported to " + FormatPosition(destination) + ".";
            return true;
        }

        private static bool TryParseVector3(IList<string> args, out Vector3 value)
        {
            value = Vector3.zero;
            if (args == null || args.Count < 3)
            {
                return false;
            }

            return float.TryParse(args[0], NumberStyles.Float, CultureInfo.InvariantCulture, out value.x) &&
                float.TryParse(args[1], NumberStyles.Float, CultureInfo.InvariantCulture, out value.y) &&
                float.TryParse(args[2], NumberStyles.Float, CultureInfo.InvariantCulture, out value.z);
        }

        private static void TriggerWeatherUpdate()
        {
            WeatherManager.needToReUpdateWeatherSpectrums = true;
            if (WeatherManager.Instance != null)
            {
                WeatherManager.Instance.TriggerUpdate();
            }
        }

        private static void MarkPlayerChanged(EntityAlive player)
        {
            if (player != null)
            {
                player.bPlayerStatsChanged = !player.isEntityRemote;
            }
        }

        private static bool IsActionKey(string key)
        {
            return key.Equals("healneeds", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("cleardebuffs", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("repairgear", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("unlockrecipes", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("teleportcrosshair", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("timemorning", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("timenoon", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("timenight", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("weatherclear", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("weatherrain", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("weatherfog", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("weatherstorm", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetDisplayName(string key)
        {
            foreach (DevMenuCheatEntry entry in DevMenuCheatCatalog.Entries)
            {
                if (entry.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    return entry.DisplayName;
                }
            }

            return key;
        }

        private static string FormatState(bool enabled)
        {
            return enabled ? "Enabled" : "Disabled";
        }

        private static string FormatPosition(Vector3 position)
        {
            return position.x.ToString("0.##", CultureInfo.InvariantCulture) + "," +
                position.y.ToString("0.##", CultureInfo.InvariantCulture) + "," +
                position.z.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static string FormatFloat(float value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }

        private static void SendServerCheatCommand(string key)
        {
            string command = "devmenu cheat " + Quote(key);
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                NetPackageManager.GetPackage<NetPackageConsoleCmdServer>().Setup(command));
        }

        private static void SendServerCheatCommand(string key, Vector3 destination)
        {
            string command = "devmenu cheat " + Quote(key) + " " +
                FormatFloat(destination.x) + " " +
                FormatFloat(destination.y) + " " +
                FormatFloat(destination.z);

            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                NetPackageManager.GetPackage<NetPackageConsoleCmdServer>().Setup(command));
        }

        private static string Quote(string value)
        {
            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }
    }

    [Preserve]
    public static class LootableTileEntitySpawnService
    {
        private const float SpawnDistance = 2.5f;
        private const int SearchRadius = 3;

        public static bool RequestSpawnInFrontOfPrimaryPlayer(string blockName, out string message)
        {
            message = null;

            if (GameManager.IsDedicatedServer)
            {
                message = "No local player exists on a dedicated server. Use 'devmenu tile <blockName> x y z [rotation]'.";
                return false;
            }

            World world = GameManager.Instance?.World;
            EntityPlayerLocal player = world?.GetPrimaryPlayer();
            if (world == null || player == null)
            {
                message = "No local player is available.";
                return false;
            }

            if (!TryGetLootableBlock(blockName, out Block block, out LootableTileEntityEntry entry, out message))
            {
                return false;
            }

            if (!TryFindPlacementInFront(player, block, out Vector3i blockPos, out byte rotation, out message))
            {
                return false;
            }

            if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            {
                return TrySpawnAt(entry.BlockName, blockPos, rotation, player, out message);
            }

            SendServerSpawnCommand(entry.BlockName, blockPos, rotation);
            message = "Requested tile entity spawn for " + entry.BlockName + " at " + FormatPosition(blockPos) + ".";
            return true;
        }

        public static bool TrySpawnAt(string blockName, Vector3i blockPos, byte rotation, EntityAlive placingEntity, out string message)
        {
            message = null;

            if (GameManager.Instance?.World == null)
            {
                message = "No world is loaded.";
                return false;
            }

            if (!TryGetLootableBlock(blockName, out Block block, out LootableTileEntityEntry entry, out message))
            {
                return false;
            }

            BlockValue blockValue = block.ToBlockValue();
            blockValue.rotation = block.SupportsRotation(rotation) ? rotation : (byte)0;

            World world = GameManager.Instance.World;
            if (!block.CanPlaceBlockAt(world, blockPos, blockValue))
            {
                message = "Cannot place " + entry.BlockName + " at " + FormatPosition(blockPos) + ". Move to a clearer spot or pass exact coordinates.";
                return false;
            }

            var result = new BlockPlacement.Result(
                BlockPlacement.EnumPlacement.Voxel,
                blockPos + Vector3.one * 0.5f,
                blockPos,
                BlockFace.None,
                blockValue,
                default(PropTransform));

            block.PlaceBlock(world, result, null);
            ResetSpawnedLootState(world, blockPos);

            message = "Spawned untouched " + entry.BlockName + " at " + FormatPosition(blockPos) + ".";
            return true;
        }

        private static bool TryGetLootableBlock(string blockName, out Block block, out LootableTileEntityEntry entry, out string message)
        {
            block = Block.GetBlockByName(blockName, _caseInsensitive: true);
            entry = null;
            message = null;

            if (block == null)
            {
                message = "Unknown block: " + blockName;
                return false;
            }

            if (!LootableTileEntityCatalog.IsLootableBlock(block, out entry))
            {
                message = block.GetBlockName() + " is not a lootable composite tile entity with " + TEFeatureStorage.PropLootList + ".";
                return false;
            }

            return true;
        }

        private static bool TryFindPlacementInFront(EntityPlayerLocal player, Block block, out Vector3i blockPos, out byte rotation, out string message)
        {
            blockPos = Vector3i.zero;
            rotation = 0;
            message = null;

            Vector3 forward = GetFlatForward(player);
            Vector3 target = player.position + forward * SpawnDistance;
            Vector3i basePos = World.worldToBlockPos(target);

            BlockValue blockValue = block.ToBlockValue();
            rotation = GetSimpleRotation(player);
            if (!block.SupportsRotation(rotation))
            {
                rotation = 0;
            }

            blockValue.rotation = rotation;
            WorldBase world = GameManager.Instance.World;

            int[] yOffsets = { 0, 1, -1, 2, 3 };

            for (int radius = 0; radius <= SearchRadius; radius++)
            {
                for (int yIndex = 0; yIndex < yOffsets.Length; yIndex++)
                {
                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        for (int dz = -radius; dz <= radius; dz++)
                        {
                            if (radius > 0 && Math.Max(Math.Abs(dx), Math.Abs(dz)) != radius)
                            {
                                continue;
                            }

                            var candidate = new Vector3i(basePos.x + dx, basePos.y + yOffsets[yIndex], basePos.z + dz);
                            if (candidate.y < 1 || candidate.y > 253)
                            {
                                continue;
                            }

                            if (block.CanPlaceBlockAt(world, candidate, blockValue))
                            {
                                blockPos = candidate;
                                return true;
                            }
                        }
                    }
                }
            }

            message = "Could not find a clear placement spot in front of the player.";
            return false;
        }

        private static void ResetSpawnedLootState(World world, Vector3i blockPos)
        {
            TileEntity tileEntity = GetTileEntityAtOrParent(world, blockPos);
            if (!(tileEntity is TileEntityComposite composite))
            {
                return;
            }

            ITileEntityLootable lootable = composite.GetFeature<ITileEntityLootable>();
            if (lootable == null)
            {
                return;
            }

            if (composite.Owner != null)
            {
                composite.SetOwner(null);
            }

            lootable.bPlayerStorage = false;
            lootable.bTouched = false;
            lootable.bWasTouched = false;
            lootable.worldTimeTouched = 0UL;

            ItemStack[] items = lootable.items;
            for (int i = 0; i < items.Length; i++)
            {
                items[i].Clear();
            }

            lootable.SlotLocks?.Clear();
            composite.SetModified();
        }

        private static TileEntity GetTileEntityAtOrParent(World world, Vector3i blockPos)
        {
            BlockValue blockValue = world.GetBlock(blockPos);
            if (blockValue.ischild)
            {
                blockPos += blockValue.parent;
            }

            return world.GetTileEntity(blockPos);
        }

        private static Vector3 GetFlatForward(EntityPlayerLocal player)
        {
            Vector3 forward = player.finalCamera != null
                ? player.finalCamera.transform.forward
                : player.GetLookVector();

            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f)
            {
                forward = player.GetLookVector();
                forward.y = 0f;
            }

            return forward.sqrMagnitude < 0.001f ? Vector3.forward : forward.normalized;
        }

        private static byte GetSimpleRotation(EntityPlayerLocal player)
        {
            int rotation = Mathf.RoundToInt(player.rotation.y / 90f) & 3;
            return (byte)rotation;
        }

        private static void SendServerSpawnCommand(string blockName, Vector3i blockPos, byte rotation)
        {
            string command = "devmenu tile " + Quote(blockName) + " " +
                blockPos.x + " " + blockPos.y + " " + blockPos.z + " " + rotation;

            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                NetPackageManager.GetPackage<NetPackageConsoleCmdServer>().Setup(command));
        }

        private static string Quote(string value)
        {
            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }

        private static string FormatPosition(Vector3i blockPos)
        {
            return blockPos.x + "," + blockPos.y + "," + blockPos.z;
        }
    }
}
