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
    public class XUiC_DevMenuEntityCategoryList : XUiC_List<DevMenuEntityCategoryEntry>
    {
        public override void RebuildList(bool _resetFilter = false)
        {
            allEntries.Clear();
            allEntries.AddRange(DevMenuEntityCatalog.Categories);
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
    public class XUiC_DevMenuEntityList : XUiC_List<DevMenuEntityEntry>
    {
        public override void RebuildList(bool _resetFilter = false)
        {
            allEntries.Clear();
            foreach (DevMenuEntityEntry entry in DevMenuEntityCatalog.Entries)
            {
                if (!DevMenuEntityFilterState.FilterByCategory ||
                    DevMenuEntityFilterState.IsCategorySelected(entry))
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

            [XuiXmlBinding("entityname")]
            public string BindingEntityName()
            {
                return entryData?.EntityName ?? "";
            }

            [XuiXmlBinding("category")]
            public string BindingCategory()
            {
                return entryData?.Category ?? "";
            }

            [XuiXmlBinding("entitytype")]
            public string BindingEntityType()
            {
                return entryData?.EntityType ?? "";
            }

            [XuiXmlBinding("tags")]
            public string BindingTags()
            {
                return entryData?.Tags ?? "";
            }
        }
    }

    [Preserve]
    public class XUiC_DevMenuBuffCategoryList : XUiC_List<DevMenuBuffCategoryEntry>
    {
        public override void RebuildList(bool _resetFilter = false)
        {
            allEntries.Clear();
            allEntries.AddRange(DevMenuBuffCatalog.Categories);
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
    public class XUiC_DevMenuBuffList : XUiC_List<DevMenuBuffEntry>
    {
        public override void RebuildList(bool _resetFilter = false)
        {
            allEntries.Clear();
            foreach (DevMenuBuffEntry entry in DevMenuBuffCatalog.Entries)
            {
                if (!DevMenuBuffFilterState.FilterByCategory ||
                    DevMenuBuffFilterState.IsCategorySelected(entry))
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

            [XuiXmlBinding("buffname")]
            public string BindingBuffName()
            {
                return entryData?.BuffName ?? "";
            }

            [XuiXmlBinding("category")]
            public string BindingCategory()
            {
                return entryData?.Category ?? "";
            }

            [XuiXmlBinding("description")]
            public string BindingDescription()
            {
                return entryData?.Description ?? "";
            }

            [XuiXmlBinding("duration")]
            public string BindingDuration()
            {
                return entryData?.Duration ?? "";
            }

            [XuiXmlBinding("flags")]
            public string BindingFlags()
            {
                return entryData?.Flags ?? "";
            }

            [XuiXmlBinding("status")]
            public string BindingStatus()
            {
                return entryData == null ? "" : DevMenuBuffService.GetStatus(entryData.BuffName);
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
