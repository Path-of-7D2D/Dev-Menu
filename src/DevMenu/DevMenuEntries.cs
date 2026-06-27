using System;

namespace DevMenu
{
    public sealed class DevMenuItemCategoryEntry : XUiListEntry<DevMenuItemCategoryEntry>
    {
        public DevMenuItemCategoryEntry(string category, int sortOrder, int count)
        {
            Category = category ?? DevMenuItemFilterState.AllCategory;
            SortOrder = sortOrder;
            Count = count;
        }

        public string Category { get; }

        public int SortOrder { get; }

        public int Count { get; }

        public override bool MatchesSearch(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return true;
            }

            return Category.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public override int CompareTo(DevMenuItemCategoryEntry other)
        {
            int sortCompare = SortOrder.CompareTo(other?.SortOrder ?? int.MaxValue);
            return sortCompare != 0
                ? sortCompare
                : string.Compare(Category, other?.Category, StringComparison.OrdinalIgnoreCase);
        }
    }

    public sealed class DevMenuItemEntry : XUiListEntry<DevMenuItemEntry>
    {
        public DevMenuItemEntry(string itemName, string displayName, string category, int categorySort, string tags, bool hasQuality)
        {
            ItemName = itemName ?? "";
            DisplayName = displayName ?? ItemName;
            Category = category ?? "";
            CategorySort = categorySort;
            Tags = tags ?? "";
            HasQuality = hasQuality;
        }

        public string ItemName { get; }

        public string DisplayName { get; }

        public string Category { get; }

        public int CategorySort { get; }

        public string Tags { get; }

        public bool HasQuality { get; }

        public override bool MatchesSearch(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return true;
            }

            return IndexOf(DisplayName, search) ||
                IndexOf(ItemName, search) ||
                IndexOf(Category, search) ||
                IndexOf(Tags, search);
        }

        public override int CompareTo(DevMenuItemEntry other)
        {
            int categoryCompare = CategorySort.CompareTo(other?.CategorySort ?? int.MaxValue);
            if (categoryCompare != 0)
            {
                return categoryCompare;
            }

            categoryCompare = string.Compare(Category, other?.Category, StringComparison.OrdinalIgnoreCase);
            if (categoryCompare != 0)
            {
                return categoryCompare;
            }

            int nameCompare = string.Compare(DisplayName, other?.DisplayName, StringComparison.OrdinalIgnoreCase);
            return nameCompare != 0
                ? nameCompare
                : string.Compare(ItemName, other?.ItemName, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IndexOf(string value, string search)
        {
            return !string.IsNullOrEmpty(value) &&
                value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    public sealed class DevMenuCheatEntry : XUiListEntry<DevMenuCheatEntry>
    {
        public DevMenuCheatEntry(string key, string displayName, string description, int sortOrder = 1000)
        {
            Key = key ?? "";
            DisplayName = displayName ?? Key;
            Description = description ?? "";
            SortOrder = sortOrder;
        }

        public string Key { get; }

        public string DisplayName { get; }

        public string Description { get; }

        public int SortOrder { get; }

        public override bool MatchesSearch(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return true;
            }

            return IndexOf(Key, search) ||
                IndexOf(DisplayName, search) ||
                IndexOf(Description, search);
        }

        public override int CompareTo(DevMenuCheatEntry other)
        {
            int sortCompare = SortOrder.CompareTo(other?.SortOrder ?? int.MaxValue);
            return sortCompare != 0
                ? sortCompare
                : string.Compare(DisplayName, other?.DisplayName, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IndexOf(string value, string search)
        {
            return !string.IsNullOrEmpty(value) &&
                value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    public sealed class LootableTileEntityEntry : XUiListEntry<LootableTileEntityEntry>
    {
        public LootableTileEntityEntry(string blockName, string displayName, string lootList, string flags)
        {
            BlockName = blockName ?? "";
            DisplayName = displayName ?? BlockName;
            LootList = lootList ?? "";
            Flags = flags ?? "";
        }

        public string BlockName { get; }

        public string DisplayName { get; }

        public string LootList { get; }

        public string Flags { get; }

        public override bool MatchesSearch(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return true;
            }

            return IndexOf(DisplayName, search) ||
                IndexOf(BlockName, search) ||
                IndexOf(LootList, search) ||
                IndexOf(Flags, search);
        }

        public override int CompareTo(LootableTileEntityEntry other)
        {
            int nameCompare = string.Compare(DisplayName, other?.DisplayName, StringComparison.OrdinalIgnoreCase);
            return nameCompare != 0
                ? nameCompare
                : string.Compare(BlockName, other?.BlockName, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IndexOf(string value, string search)
        {
            return !string.IsNullOrEmpty(value) &&
                value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
