using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace DevMenu
{
    [Preserve]
    public static class DevMenuItemFilterState
    {
        public const string AllCategory = "All";
        public const string OtherCategory = "Other";

        public static string SelectedCategory { get; set; } = AllCategory;

        public static bool FilterByCategory { get; set; } = true;

        public static bool IsCategorySelected(DevMenuItemEntry entry)
        {
            return entry != null &&
                (SelectedCategory == AllCategory ||
                entry.Category.Equals(SelectedCategory, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Preserve]
    public static class DevMenuItemCatalog
    {
        private static readonly List<DevMenuItemEntry> cachedEntries = new List<DevMenuItemEntry>();
        private static readonly List<DevMenuItemCategoryEntry> cachedCategories = new List<DevMenuItemCategoryEntry>();
        private static bool loaded;

        public static IReadOnlyList<DevMenuItemEntry> Entries
        {
            get
            {
                EnsureLoaded();
                return cachedEntries;
            }
        }

        public static IReadOnlyList<DevMenuItemCategoryEntry> Categories
        {
            get
            {
                EnsureLoaded();
                return cachedCategories;
            }
        }

        public static void Reload()
        {
            loaded = false;
            EnsureLoaded();
        }

        public static bool IsKnownItem(string itemName, out DevMenuItemEntry entry)
        {
            EnsureLoaded();
            entry = null;

            for (int i = 0; i < cachedEntries.Count; i++)
            {
                if (cachedEntries[i].ItemName.Equals(itemName, StringComparison.OrdinalIgnoreCase))
                {
                    entry = cachedEntries[i];
                    return true;
                }
            }

            return false;
        }

        private static void EnsureLoaded()
        {
            if (loaded)
            {
                return;
            }

            cachedEntries.Clear();
            cachedCategories.Clear();

            if (ItemClass.list == null)
            {
                return;
            }

            loaded = true;
            var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < ItemClass.list.Length; i++)
            {
                ItemClass itemClass = ItemClass.list[i];
                if (itemClass == null)
                {
                    continue;
                }

                string itemName = GetItemName(itemClass);
                if (string.IsNullOrEmpty(itemName) || !seenNames.Add(itemName))
                {
                    continue;
                }

                cachedEntries.Add(new DevMenuItemEntry(
                    itemName,
                    GetDisplayName(itemClass, itemName),
                    GetCategory(itemClass, itemName, out int categorySort),
                    categorySort,
                    GetTags(itemClass),
                    HasQuality(itemClass)));
            }

            cachedEntries.Sort();
            RebuildCategories();
        }

        private static void RebuildCategories()
        {
            cachedCategories.Clear();
            cachedCategories.Add(new DevMenuItemCategoryEntry(DevMenuItemFilterState.AllCategory, 0, cachedEntries.Count));

            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var sortOrders = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < cachedEntries.Count; i++)
            {
                DevMenuItemEntry entry = cachedEntries[i];
                if (!counts.ContainsKey(entry.Category))
                {
                    counts[entry.Category] = 0;
                    sortOrders[entry.Category] = entry.CategorySort;
                }

                counts[entry.Category]++;
                if (entry.CategorySort < sortOrders[entry.Category])
                {
                    sortOrders[entry.Category] = entry.CategorySort;
                }
            }

            foreach (KeyValuePair<string, int> count in counts)
            {
                cachedCategories.Add(new DevMenuItemCategoryEntry(count.Key, sortOrders[count.Key], count.Value));
            }

            cachedCategories.Sort();
        }

        private static string GetItemName(ItemClass itemClass)
        {
            try
            {
                return !string.IsNullOrEmpty(itemClass.Name)
                    ? itemClass.Name
                    : itemClass.GetItemName();
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static string GetDisplayName(ItemClass itemClass, string fallback)
        {
            try
            {
                string localizedName = itemClass.GetLocalizedItemName();
                return string.IsNullOrEmpty(localizedName) ? fallback : localizedName;
            }
            catch (Exception)
            {
                return fallback;
            }
        }

        private static string GetCategory(ItemClass itemClass, string itemName, out int categorySort)
        {
            string tags = GetTags(itemClass);
            string group = "";
            TryGetProperty(itemClass, ItemClass.PropGroupName, out group);

            string creativeSort = "";
            TryGetProperty(itemClass, ItemClass.PropCreativeSort1, out creativeSort);

            string haystack = (itemName + "," + tags + "," + group + "," + creativeSort).ToLowerInvariant();
            string name = itemName.ToLowerInvariant();

            if (ContainsAny(name, haystack, "meleetool", "tool", "miningtool", "repairtool", "toolstrap"))
            {
                categorySort = 10;
                return "Tools";
            }

            if (ContainsAny(name, haystack, "ammo", "arrow", "bolt"))
            {
                categorySort = 20;
                return "Ammo";
            }

            if (ContainsAny(name, haystack, "gun", "melee", "weapon", "ranged", "bow", "club", "knife", "spear", "stunbaton", "explosive"))
            {
                categorySort = 30;
                return "Weapons";
            }

            if (ContainsAny(name, haystack, "armor", "helmet", "gloves", "boots", "chest"))
            {
                categorySort = 40;
                return "Armor";
            }

            if (ContainsAny(name, haystack, "clothing", "cosmetic", "apparel"))
            {
                categorySort = 50;
                return "Clothing";
            }

            if (ContainsAny(name, haystack, "food", "drink", "water", "meal"))
            {
                categorySort = 60;
                return "Food/Drink";
            }

            if (ContainsAny(name, haystack, "medical", "medicine", "drug", "bandage", "firstaid"))
            {
                categorySort = 70;
                return "Medical";
            }

            if (itemClass is ItemClassModifier || ContainsAny(name, haystack, "mod", "modifier"))
            {
                categorySort = 80;
                return "Mods";
            }

            if (ContainsAny(name, haystack, "schematic", "book", "magazine", "perkbook"))
            {
                categorySort = 90;
                return "Books/Schematics";
            }

            if (itemClass is ItemClassBlock || ContainsAny(name, haystack, "block", "building", "decor", "furniture", "trap"))
            {
                categorySort = 100;
                return "Blocks";
            }

            if (ContainsAny(name, haystack, "resource", "ingredient", "material", "part"))
            {
                categorySort = 110;
                return "Resources";
            }

            if (ContainsAny(name, haystack, "vehicle", "accessoryvehicle", "drone"))
            {
                categorySort = 120;
                return "Vehicles";
            }

            if (ContainsAny(name, haystack, "quest", "admin", "debug", "challenge"))
            {
                categorySort = 130;
                return "Quest/Admin";
            }

            try
            {
                categorySort = 900;
                string creativeMode = itemClass.CreativeMode.ToString();
                return string.IsNullOrEmpty(creativeMode) ? DevMenuItemFilterState.OtherCategory : creativeMode;
            }
            catch (Exception)
            {
                categorySort = 999;
                return DevMenuItemFilterState.OtherCategory;
            }
        }

        private static string GetTags(ItemClass itemClass)
        {
            string value;
            if (TryGetProperty(itemClass, ItemClass.PropTags, out value))
            {
                return value;
            }

            return "";
        }

        private static bool HasQuality(ItemClass itemClass)
        {
            string value;
            if (TryGetProperty(itemClass, ItemClass.PropShowQuality, out value) &&
                bool.TryParse(value, out bool showQuality))
            {
                return showQuality;
            }

            if (TryGetProperty(itemClass, ItemClass.PropQualityMin, out value))
            {
                return true;
            }

            return false;
        }

        private static bool ContainsAny(string itemName, string haystack, params string[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                string value = values[i];
                if (itemName.StartsWith(value, StringComparison.OrdinalIgnoreCase) ||
                    haystack.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetProperty(ItemClass itemClass, string key, out string value)
        {
            value = null;
            return itemClass?.Properties != null &&
                itemClass.Properties.Values.TryGetValue(key, out value);
        }
    }

    [Preserve]
    public static class DevMenuCheatCatalog
    {
        private static readonly List<DevMenuCheatEntry> entries = new List<DevMenuCheatEntry>
        {
            new DevMenuCheatEntry("god", "God Mode", "Toggle player invulnerability.", 10),
            new DevMenuCheatEntry("ammo", "Unlimited Ammo", "Toggle the Dev Menu infinite ammo buff.", 20),
            new DevMenuCheatEntry("noclip", "Noclip", "Toggle fly mode and no-collision movement.", 30),
            new DevMenuCheatEntry("noaggro", "No Aggro", "Toggle vanilla invisible/spectator targeting so enemies ignore the player.", 40),
            new DevMenuCheatEntry("healneeds", "Heal / Fill Needs", "Restore health, stamina, food, and water.", 100),
            new DevMenuCheatEntry("cleardebuffs", "Clear Debuffs", "Remove active negative buffs from the player.", 110),
            new DevMenuCheatEntry("repairgear", "Repair Gear", "Repair toolbelt, backpack, and equipped gear durability.", 120),
            new DevMenuCheatEntry("unlockrecipes", "Unlock All Recipes", "Unlock every loaded recipe for the player.", 130),
            new DevMenuCheatEntry("addxp", "Add 10k XP", "Grant 10,000 debug XP.", 140),
            new DevMenuCheatEntry("addskillpoint", "Add Skill Point", "Grant 1 unspent skill point.", 150),
            new DevMenuCheatEntry("teleportcrosshair", "Teleport To Crosshair", "Teleport to the location you are looking at.", 160),
            new DevMenuCheatEntry("timemorning", "Time: Morning", "Set the current day to 08:00.", 200),
            new DevMenuCheatEntry("timenoon", "Time: Noon", "Set the current day to 12:00.", 210),
            new DevMenuCheatEntry("timenight", "Time: Night", "Set the current day to 22:00.", 220),
            new DevMenuCheatEntry("weatherclear", "Weather: Clear", "Reset forced weather and fog overrides.", 300),
            new DevMenuCheatEntry("weatherrain", "Weather: Rain", "Force heavy rain and cloud cover.", 310),
            new DevMenuCheatEntry("weatherfog", "Weather: Fog", "Force dense fog.", 320),
            new DevMenuCheatEntry("weatherstorm", "Weather: Storm", "Start a storm in the current biome.", 330)
        };

        public static IReadOnlyList<DevMenuCheatEntry> Entries => entries;
    }

    [Preserve]
    public static class LootableTileEntityCatalog
    {
        private static readonly List<LootableTileEntityEntry> cachedEntries = new List<LootableTileEntityEntry>();
        private static bool loaded;

        public static IReadOnlyList<LootableTileEntityEntry> Entries
        {
            get
            {
                EnsureLoaded();
                return cachedEntries;
            }
        }

        public static void Reload()
        {
            loaded = false;
            EnsureLoaded();
        }

        public static bool IsLootableBlock(Block block, out LootableTileEntityEntry entry)
        {
            EnsureLoaded();
            entry = null;

            if (block == null)
            {
                return false;
            }

            string blockName = block.GetBlockName();
            for (int i = 0; i < cachedEntries.Count; i++)
            {
                if (cachedEntries[i].BlockName.Equals(blockName, StringComparison.OrdinalIgnoreCase))
                {
                    entry = cachedEntries[i];
                    return true;
                }
            }

            return false;
        }

        private static void EnsureLoaded()
        {
            if (loaded)
            {
                return;
            }

            cachedEntries.Clear();

            if (!Block.BlocksLoaded || Block.list == null)
            {
                return;
            }

            loaded = true;

            for (int i = 0; i < Block.list.Length; i++)
            {
                Block block = Block.list[i];
                if (block == null || block.blockID == 0)
                {
                    continue;
                }

                if (!(block is BlockCompositeTileEntity composite) || composite.CompositeData == null)
                {
                    continue;
                }

                if (!composite.CompositeData.TryGetFeatureData<TEFeatureStorage>(out TileEntityFeatureData storageFeature))
                {
                    continue;
                }

                if (!storageFeature.Props.Values.TryGetValue(TEFeatureStorage.PropLootList, out string lootList) ||
                    string.IsNullOrEmpty(lootList))
                {
                    continue;
                }

                string blockName = block.GetBlockName();
                cachedEntries.Add(new LootableTileEntityEntry(
                    blockName,
                    GetDisplayName(block, blockName),
                    lootList,
                    BuildFlags(composite, storageFeature, lootList)));
            }

            cachedEntries.Sort();
        }

        private static string GetDisplayName(Block block, string fallback)
        {
            try
            {
                string localizedName = block.GetLocalizedBlockName();
                return string.IsNullOrEmpty(localizedName) ? fallback : localizedName;
            }
            catch (Exception)
            {
                return fallback;
            }
        }

        private static string BuildFlags(BlockCompositeTileEntity block, TileEntityFeatureData storageFeature, string lootList)
        {
            var flags = new List<string>();

            if (block.CompositeData.HasFeature<TEFeatureLockable>() ||
                block.CompositeData.HasFeature<TEFeatureLockPickable>())
            {
                flags.Add("locked");
            }

            if (lootList.StartsWith("player", StringComparison.OrdinalIgnoreCase))
            {
                flags.Add("player");
            }

            foreach (string key in storageFeature.Props.Values.Keys)
            {
                if (key.StartsWith(TEFeatureStorage.PropAlternateLootList, StringComparison.OrdinalIgnoreCase))
                {
                    flags.Add("alt loot");
                    break;
                }
            }

            return string.Join(", ", flags.ToArray());
        }
    }
}
