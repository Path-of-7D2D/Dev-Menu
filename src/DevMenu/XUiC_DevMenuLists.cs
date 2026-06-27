using UnityEngine.Scripting;

namespace DevMenu
{
    [Preserve]
    public class XUiC_DevMenuItemCategoryList : XUiC_List<DevMenuItemCategoryEntry>
    {
        public override void RebuildList(bool _resetFilter = false)
        {
            allEntries.Clear();
            allEntries.AddRange(DevMenuItemCatalog.Categories);
            allEntries.Sort();
            base.RebuildList(_resetFilter);
        }

        public override void OnOpen()
        {
            RebuildList(_resetFilter: true);
            base.OnOpen();
        }

        [Preserve]
        public class EntryController : XUiC_ListEntry
        {
            [XuiXmlBinding("name")]
            public string BindingName()
            {
                return entryData?.Category ?? "";
            }

            [XuiXmlBinding("count")]
            public string BindingCount()
            {
                return entryData == null ? "" : entryData.Count.ToString();
            }
        }
    }

    [Preserve]
    public class XUiC_DevMenuItemList : XUiC_List<DevMenuItemEntry>
    {
        public override void RebuildList(bool _resetFilter = false)
        {
            allEntries.Clear();
            foreach (DevMenuItemEntry entry in DevMenuItemCatalog.Entries)
            {
                if (!DevMenuItemFilterState.FilterByCategory ||
                    DevMenuItemFilterState.IsCategorySelected(entry))
                {
                    allEntries.Add(entry);
                }
            }

            allEntries.Sort();
            base.RebuildList(_resetFilter);
        }

        public override void OnOpen()
        {
            RebuildList(_resetFilter: true);
            base.OnOpen();
        }

        [Preserve]
        public class EntryController : XUiC_ListEntry
        {
            [XuiXmlBinding("name")]
            public string BindingName()
            {
                return entryData?.DisplayName ?? "";
            }

            [XuiXmlBinding("itemname")]
            public string BindingItemName()
            {
                return entryData?.ItemName ?? "";
            }

            [XuiXmlBinding("category")]
            public string BindingCategory()
            {
                return entryData?.Category ?? "";
            }

            [XuiXmlBinding("tags")]
            public string BindingTags()
            {
                return entryData?.Tags ?? "";
            }

            [XuiXmlBinding("tiered")]
            public string BindingTiered()
            {
                return entryData != null && entryData.HasQuality ? "1-6" : "";
            }
        }
    }

    [Preserve]
    public class XUiC_DevMenuCheatList : XUiC_List<DevMenuCheatEntry>
    {
        public override void RebuildList(bool _resetFilter = false)
        {
            allEntries.Clear();
            allEntries.AddRange(DevMenuCheatCatalog.Entries);
            allEntries.Sort();
            base.RebuildList(_resetFilter);
        }

        public override void OnOpen()
        {
            RebuildList(_resetFilter: true);
            base.OnOpen();
        }

        [Preserve]
        public class EntryController : XUiC_ListEntry
        {
            [XuiXmlBinding("name")]
            public string BindingName()
            {
                return entryData?.DisplayName ?? "";
            }

            [XuiXmlBinding("key")]
            public string BindingKey()
            {
                return entryData?.Key ?? "";
            }

            [XuiXmlBinding("description")]
            public string BindingDescription()
            {
                return entryData?.Description ?? "";
            }

            [XuiXmlBinding("status")]
            public string BindingStatus()
            {
                return entryData == null ? "" : DevMenuCheatService.GetStatus(entryData.Key);
            }
        }
    }

    [Preserve]
    public class XUiC_LootableTileEntityList : XUiC_List<LootableTileEntityEntry>
    {
        public override void RebuildList(bool _resetFilter = false)
        {
            allEntries.Clear();
            allEntries.AddRange(LootableTileEntityCatalog.Entries);
            allEntries.Sort();
            base.RebuildList(_resetFilter);
        }

        public override void OnOpen()
        {
            RebuildList(_resetFilter: true);
            base.OnOpen();
        }

        [Preserve]
        public class EntryController : XUiC_ListEntry
        {
            [XuiXmlBinding("name")]
            public string BindingName()
            {
                return entryData?.DisplayName ?? "";
            }

            [XuiXmlBinding("blockname")]
            public string BindingBlockName()
            {
                return entryData?.BlockName ?? "";
            }

            [XuiXmlBinding("lootlist")]
            public string BindingLootList()
            {
                return entryData?.LootList ?? "";
            }

            [XuiXmlBinding("flags")]
            public string BindingFlags()
            {
                return entryData?.Flags ?? "";
            }
        }
    }
}
