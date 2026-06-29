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
    public static class DevMenuEntityFilterState
    {
        public const string AllCategory = "All";
        public const string OtherCategory = "Other";

        public static string SelectedCategory { get; set; } = AllCategory;

        public static bool FilterByCategory { get; set; } = true;

        public static bool IsCategorySelected(DevMenuEntityEntry entry)
        {
            return entry != null &&
                (SelectedCategory == AllCategory ||
                entry.Category.Equals(SelectedCategory, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Preserve]
    public static class DevMenuEntityCatalog
    {
        private static readonly List<DevMenuEntityEntry> cachedEntries = new List<DevMenuEntityEntry>();
        private static readonly List<DevMenuEntityCategoryEntry> cachedCategories = new List<DevMenuEntityCategoryEntry>();
        private static bool loaded;

        public static IReadOnlyList<DevMenuEntityEntry> Entries
        {
            get
            {
                EnsureLoaded();
                return cachedEntries;
            }
        }

        public static IReadOnlyList<DevMenuEntityCategoryEntry> Categories
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

        public static bool IsKnownEntity(string entityName, out DevMenuEntityEntry entry)
        {
            EnsureLoaded();
            entry = null;

            for (int i = 0; i < cachedEntries.Count; i++)
            {
                if (cachedEntries[i].EntityName.Equals(entityName, StringComparison.OrdinalIgnoreCase))
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

            if (EntityClass.list == null || EntityClass.list.Dict == null)
            {
                return;
            }

            loaded = true;
            var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<int, EntityClass> pair in EntityClass.list.Dict)
            {
                int entityClassKey = pair.Key;
                EntityClass entityClass = pair.Value;
                if (entityClass == null)
                {
                    continue;
                }

                string entityName = GetEntityName(entityClass, entityClassKey);
                if (string.IsNullOrEmpty(entityName) || !seenNames.Add(entityName))
                {
                    continue;
                }

                int entityClassId = GetEntityClassId(entityName);
                if (entityClassId <= 0)
                {
                    continue;
                }

                string tags = GetTags(entityClass);
                string entityType = GetEntityType(entityClass);
                cachedEntries.Add(new DevMenuEntityEntry(
                    entityClassId,
                    entityName,
                    GetDisplayName(entityName),
                    GetCategory(entityClass, entityName, entityType, tags, out int categorySort),
                    categorySort,
                    entityType,
                    tags));
            }

            cachedEntries.Sort();
            RebuildCategories();
        }

        private static void RebuildCategories()
        {
            cachedCategories.Clear();
            cachedCategories.Add(new DevMenuEntityCategoryEntry(DevMenuEntityFilterState.AllCategory, 0, cachedEntries.Count));

            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var sortOrders = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < cachedEntries.Count; i++)
            {
                DevMenuEntityEntry entry = cachedEntries[i];
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
                cachedCategories.Add(new DevMenuEntityCategoryEntry(count.Key, sortOrders[count.Key], count.Value));
            }

            cachedCategories.Sort();
        }

        private static string GetEntityName(EntityClass entityClass, int entityClassKey)
        {
            if (!string.IsNullOrEmpty(entityClass.entityClassName))
            {
                return entityClass.entityClassName;
            }

            try
            {
                return EntityClass.GetEntityClassName(entityClassKey);
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static int GetEntityClassId(string entityName)
        {
            if (string.IsNullOrEmpty(entityName))
            {
                return 0;
            }

            try
            {
                return EntityClass.GetId(entityName);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static string GetDisplayName(string entityName)
        {
            try
            {
                string localizedName = Localization.Get(entityName);
                return string.IsNullOrEmpty(localizedName) ? entityName : localizedName;
            }
            catch (Exception)
            {
                return entityName;
            }
        }

        private static string GetCategory(EntityClass entityClass, string entityName, string entityType, string tags, out int categorySort)
        {
            string name = entityName.ToLowerInvariant();
            string haystack = (entityName + "," + entityType + "," + tags + "," + GetClassName(entityClass) + "," + GetModelTypeName(entityClass)).ToLowerInvariant();

            if (HasEntityFlag(entityClass, EntityFlags.Zombie) || entityType.Equals("Zombie", StringComparison.OrdinalIgnoreCase) || ContainsAny(name, haystack, "zombie"))
            {
                if (ContainsAny(name, haystack, "infernal"))
                {
                    categorySort = 16;
                    return "Infernal Zombies";
                }

                if (ContainsAny(name, haystack, "charged"))
                {
                    categorySort = 15;
                    return "Charged Zombies";
                }

                if (ContainsAny(name, haystack, "radiated"))
                {
                    categorySort = 14;
                    return "Radiated Zombies";
                }

                if (ContainsAny(name, haystack, "feral"))
                {
                    categorySort = 13;
                    return "Feral Zombies";
                }

                if (ContainsAny(name, haystack, "boss", "special", "screamer", "demolition", "mutant", "spider", "burnt", "crawler"))
                {
                    categorySort = 12;
                    return "Special Zombies";
                }

                categorySort = 10;
                return "Zombies";
            }

            if (HasEntityFlag(entityClass, EntityFlags.Animal) ||
                entityType.Equals("Animal", StringComparison.OrdinalIgnoreCase) ||
                entityClass.bIsAnimalEntity ||
                ContainsAny(name, haystack, "animal", "bear", "boar", "chicken", "coyote", "deer", "dog", "lion", "rabbit", "snake", "stag", "vulture", "wolf"))
            {
                if (entityClass.bIsEnemyEntity ||
                    ContainsAny(name, haystack, "bear", "boar", "coyote", "direwolf", "dog", "lion", "mountain", "snake", "vulture", "wolf"))
                {
                    categorySort = 30;
                    return "Hostile Animals";
                }

                categorySort = 31;
                return "Passive Animals";
            }

            if (HasEntityFlag(entityClass, EntityFlags.Bandit) ||
                entityType.Equals("Bandit", StringComparison.OrdinalIgnoreCase) ||
                ContainsAny(name, haystack, "bandit"))
            {
                categorySort = 40;
                return "Bandits";
            }

            if (ContainsAny(name, haystack, "trader", "npc", "human"))
            {
                categorySort = 50;
                return "NPCs/Traders";
            }

            if (ContainsAny(name, haystack, "drone"))
            {
                categorySort = 60;
                return "Drones";
            }

            if (ContainsAny(name, haystack, "vehicle", "bicycle", "gyrocopter", "minibike", "motorcycle", "truck"))
            {
                categorySort = 70;
                return "Vehicles";
            }

            if (ContainsAny(name, haystack, "backpack", "loot", "dropped", "supply", "item", "fallingblock", "fallingtree"))
            {
                categorySort = 80;
                return "Loot/Utility";
            }

            if (HasEntityFlag(entityClass, EntityFlags.Player) ||
                entityType.Equals("Player", StringComparison.OrdinalIgnoreCase) ||
                ContainsAny(name, haystack, "player"))
            {
                categorySort = 90;
                return "Players";
            }

            categorySort = 999;
            return DevMenuEntityFilterState.OtherCategory;
        }

        private static bool HasEntityFlag(EntityClass entityClass, EntityFlags flag)
        {
            return entityClass != null && (entityClass.entityFlags & flag) == flag;
        }

        private static string GetEntityType(EntityClass entityClass)
        {
            if (entityClass?.Properties == null)
            {
                return "";
            }

            string value;
            if (entityClass.Properties.TryGetValue(EntityClass.PropEntityType, out value))
            {
                return value ?? "";
            }

            return "";
        }

        private static string GetTags(EntityClass entityClass)
        {
            if (entityClass?.Properties == null)
            {
                return "";
            }

            string value;
            if (entityClass.Properties.TryGetValue(EntityClass.PropTags, out value))
            {
                return value ?? "";
            }

            return "";
        }

        private static string GetClassName(EntityClass entityClass)
        {
            return entityClass?.classname == null ? "" : entityClass.classname.Name;
        }

        private static string GetModelTypeName(EntityClass entityClass)
        {
            return entityClass?.modelType == null ? "" : entityClass.modelType.Name;
        }

        private static bool ContainsAny(string entityName, string haystack, params string[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                string value = values[i];
                if (entityName.StartsWith(value, StringComparison.OrdinalIgnoreCase) ||
                    haystack.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }

    [Preserve]
    public static class DevMenuBuffFilterState
    {
        public const string AllCategory = "All";
        public const string OtherCategory = "Other";

        public static string SelectedCategory { get; set; } = AllCategory;

        public static bool FilterByCategory { get; set; } = true;

        public static bool IsCategorySelected(DevMenuBuffEntry entry)
        {
            return entry != null &&
                (SelectedCategory == AllCategory ||
                entry.Category.Equals(SelectedCategory, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Preserve]
    public static class DevMenuBuffCatalog
    {
        private static readonly List<DevMenuBuffEntry> cachedEntries = new List<DevMenuBuffEntry>();
        private static readonly List<DevMenuBuffCategoryEntry> cachedCategories = new List<DevMenuBuffCategoryEntry>();
        private static bool loaded;

        public static IReadOnlyList<DevMenuBuffEntry> Entries
        {
            get
            {
                EnsureLoaded();
                return cachedEntries;
            }
        }

        public static IReadOnlyList<DevMenuBuffCategoryEntry> Categories
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

        public static bool IsKnownBuff(string buffName, out DevMenuBuffEntry entry)
        {
            EnsureLoaded();
            entry = null;

            for (int i = 0; i < cachedEntries.Count; i++)
            {
                if (cachedEntries[i].BuffName.Equals(buffName, StringComparison.OrdinalIgnoreCase))
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

            if (BuffManager.Buffs == null)
            {
                return;
            }

            loaded = true;

            foreach (KeyValuePair<string, BuffClass> pair in BuffManager.Buffs)
            {
                BuffClass buffClass = pair.Value;
                if (buffClass == null)
                {
                    continue;
                }

                string buffName = !string.IsNullOrEmpty(buffClass.Name) ? buffClass.Name : pair.Key;
                if (string.IsNullOrEmpty(buffName))
                {
                    continue;
                }

                cachedEntries.Add(new DevMenuBuffEntry(
                    buffName,
                    GetDisplayName(buffClass, buffName),
                    GetCategory(buffClass, buffName, out int categorySort),
                    categorySort,
                    GetDescription(buffClass),
                    FormatDefinitionDuration(buffClass),
                    BuildFlags(buffClass)));
            }

            cachedEntries.Sort();
            RebuildCategories();
        }

        private static void RebuildCategories()
        {
            cachedCategories.Clear();
            cachedCategories.Add(new DevMenuBuffCategoryEntry(DevMenuBuffFilterState.AllCategory, 0, cachedEntries.Count));

            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var sortOrders = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < cachedEntries.Count; i++)
            {
                DevMenuBuffEntry entry = cachedEntries[i];
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
                cachedCategories.Add(new DevMenuBuffCategoryEntry(count.Key, sortOrders[count.Key], count.Value));
            }

            cachedCategories.Sort();
        }

        private static string GetDisplayName(BuffClass buffClass, string fallback)
        {
            if (!string.IsNullOrEmpty(buffClass.LocalizedName))
            {
                return buffClass.LocalizedName;
            }

            try
            {
                string localizedName = Localization.Get(fallback);
                return string.IsNullOrEmpty(localizedName) ? fallback : localizedName;
            }
            catch (Exception)
            {
                return fallback;
            }
        }

        private static string GetDescription(BuffClass buffClass)
        {
            if (!string.IsNullOrEmpty(buffClass.Description))
            {
                return buffClass.Description;
            }

            if (!string.IsNullOrEmpty(buffClass.DescriptionKey))
            {
                try
                {
                    string localizedDescription = Localization.Get(buffClass.DescriptionKey);
                    return string.IsNullOrEmpty(localizedDescription) ? buffClass.DescriptionKey : localizedDescription;
                }
                catch (Exception)
                {
                    return buffClass.DescriptionKey;
                }
            }

            return "";
        }

        private static string GetCategory(BuffClass buffClass, string buffName, out int categorySort)
        {
            string name = buffName.ToLowerInvariant();
            string haystack = (buffName + "," + GetDescription(buffClass) + "," + SafeTagString(buffClass.NameTag) + "," + SafeTagString(buffClass.Tags)).ToLowerInvariant();

            if (IsDebuff(buffClass, name, haystack))
            {
                if (ContainsAny(name, haystack, "injury", "abrasion", "laceration", "sprain", "sprained", "broken", "fracture", "concussion", "cripple"))
                {
                    categorySort = 10;
                    return "Injuries";
                }

                if (ContainsAny(name, haystack, "infection", "dysentery", "disease", "poison", "sick", "illness", "radiation"))
                {
                    categorySort = 20;
                    return "Disease/Poison";
                }

                if (ContainsAny(name, haystack, "bleed", "burn", "fire", "shock", "stun", "fatigue", "hungry", "thirst", "cold", "heat", "wet", "encumber"))
                {
                    categorySort = 30;
                    return "Status Debuffs";
                }

                categorySort = 40;
                return "Debuffs";
            }

            if (ContainsAny(name, haystack, "drug", "candy", "vitamin", "steroid", "recog", "fortbites", "beer", "coffee", "mega", "elixir", "sauce", "moonshine"))
            {
                categorySort = 100;
                return "Drugs/Candy";
            }

            if (ContainsAny(name, haystack, "food", "drink", "meal", "tea", "juice", "smoothie", "water"))
            {
                categorySort = 110;
                return "Food/Drink";
            }

            if (ContainsAny(name, haystack, "heal", "medical", "treated", "splint", "cast", "bandage", "firstaid", "health"))
            {
                categorySort = 120;
                return "Healing";
            }

            if (ContainsAny(name, haystack, "setbonus", "set_bonus", "armor", "lumberjack", "preacher", "rogue", "athletic", "enforcer", "farmer", "biker", "scavenger", "ranger", "nerd", "commando", "nomad", "miner", "assassin"))
            {
                categorySort = 130;
                return "Equipment/Set";
            }

            if (ContainsAny(name, haystack, "perk", "skill", "challenge", "quest", "progression", "learning", "xp", "level"))
            {
                categorySort = 140;
                return "Progression";
            }

            if (ContainsAny(name, haystack, "weather", "biome", "radiation", "storm", "cold", "heat", "shelter", "underwater"))
            {
                categorySort = 150;
                return "Environmental";
            }

            if (ContainsAny(name, haystack, "god", "debug", "test", "admin", "show", "devmenu", "twitch"))
            {
                categorySort = 160;
                return "Debug/Admin";
            }

            if (buffClass.Hidden || !buffClass.ShowOnHUD)
            {
                categorySort = 900;
                return "Hidden/Utility";
            }

            categorySort = 200;
            return "Buffs";
        }

        private static bool IsDebuff(BuffClass buffClass, string name, string haystack)
        {
            return buffClass.DamageType != EnumDamageTypes.None ||
                ContainsAny(name, haystack, "debuff", "injury", "infection", "dysentery", "disease", "poison", "bleed", "burn", "shock", "stun", "fatigue", "hungry", "thirst", "cold", "heat", "wet", "encumber", "sprain", "broken", "abrasion", "laceration", "concussion");
        }

        private static string BuildFlags(BuffClass buffClass)
        {
            var flags = new List<string>();

            if (buffClass.DamageType != EnumDamageTypes.None)
            {
                flags.Add("debuff");
            }

            if (buffClass.Hidden)
            {
                flags.Add("hidden");
            }

            if (buffClass.ShowOnHUD)
            {
                flags.Add("hud");
            }

            if (!buffClass.RemoveOnDeath)
            {
                flags.Add("keeps death");
            }

            if (buffClass.DurationMax > 0f)
            {
                flags.Add("timed");
            }

            return string.Join(", ", flags.ToArray());
        }

        private static string FormatDefinitionDuration(BuffClass buffClass)
        {
            return buffClass.DurationMax > 0f
                ? FormatDuration(buffClass.DurationMax)
                : "Default";
        }

        public static string FormatDuration(float seconds)
        {
            if (seconds <= 0f)
            {
                return "";
            }

            if (seconds > 3600f && Math.Abs(seconds % 3600f) < 0.01f)
            {
                return ((int)(seconds / 3600f)) + "h";
            }

            if (seconds >= 60f && Math.Abs(seconds % 60f) < 0.01f)
            {
                return ((int)(seconds / 60f)) + "m";
            }

            return ((int)Math.Round(seconds)) + "s";
        }

        private static string SafeTagString(FastTags<TagGroup.Global> tags)
        {
            try
            {
                return tags.ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static bool ContainsAny(string buffName, string haystack, params string[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                string value = values[i];
                if (buffName.StartsWith(value, StringComparison.OrdinalIgnoreCase) ||
                    haystack.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
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
