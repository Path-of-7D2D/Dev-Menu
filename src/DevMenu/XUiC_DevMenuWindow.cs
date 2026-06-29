using UnityEngine.Scripting;

namespace DevMenu
{
    [Preserve]
    public class XUiC_DevMenuWindow : XUiController
    {
        public const string WindowGroupName = "devMenu";

        private XUiC_DevMenuItemCategoryList categoryList;
        private XUiC_DevMenuItemList itemList;
        private XUiC_DevMenuEntityCategoryList entityCategoryList;
        private XUiC_DevMenuEntityList entityList;
        private XUiC_DevMenuCheatList cheatList;
        private XUiC_LootableTileEntityList tileEntityList;

        private XUiC_ToggleButton categoryFilterToggle;
        private XUiC_ToggleButton entityCategoryFilterToggle;
        private XUiC_SimpleButton spawnItemOneButton;
        private XUiC_SimpleButton spawnItemTenButton;
        private XUiC_SimpleButton spawnEntityOneButton;
        private XUiC_SimpleButton spawnEntityFiveButton;
        private XUiC_SimpleButton toggleCheatButton;
        private XUiC_SimpleButton spawnTileEntityButton;
        private XUiC_SimpleButton reloadButton;
        private XUiC_SimpleButton closeButton;
        private readonly XUiC_SimpleButton[] qualityButtons = new XUiC_SimpleButton[6];
        private int selectedItemQuality = 6;

        public override void Init()
        {
            base.Init();

            categoryList = GetChildById("itemCategories") as XUiC_DevMenuItemCategoryList;
            if (categoryList != null)
            {
                categoryList.ListEntryClicked += CategoryList_ListEntryClicked;
            }

            itemList = GetChildById("items") as XUiC_DevMenuItemList;
            if (itemList != null)
            {
                itemList.ListEntryDoubleClicked += ItemList_ListEntryDoubleClicked;
            }

            categoryFilterToggle = GetToggleButton("toggleCategoryFilter");
            if (categoryFilterToggle != null)
            {
                categoryFilterToggle.Value = DevMenuItemFilterState.FilterByCategory;
                categoryFilterToggle.OnValueChanged += CategoryFilterToggle_OnValueChanged;
            }

            entityCategoryList = GetChildById("entityCategories") as XUiC_DevMenuEntityCategoryList;
            if (entityCategoryList != null)
            {
                entityCategoryList.ListEntryClicked += EntityCategoryList_ListEntryClicked;
            }

            entityList = GetChildById("entities") as XUiC_DevMenuEntityList;
            if (entityList != null)
            {
                entityList.ListEntryDoubleClicked += EntityList_ListEntryDoubleClicked;
            }

            entityCategoryFilterToggle = GetToggleButton("toggleEntityCategoryFilter");
            if (entityCategoryFilterToggle != null)
            {
                entityCategoryFilterToggle.Value = DevMenuEntityFilterState.FilterByCategory;
                entityCategoryFilterToggle.OnValueChanged += EntityCategoryFilterToggle_OnValueChanged;
            }

            cheatList = GetChildById("cheats") as XUiC_DevMenuCheatList;
            if (cheatList != null)
            {
                cheatList.ListEntryDoubleClicked += CheatList_ListEntryDoubleClicked;
            }

            tileEntityList = GetChildById("tileEntities") as XUiC_LootableTileEntityList;
            if (tileEntityList != null)
            {
                tileEntityList.ListEntryDoubleClicked += TileEntityList_ListEntryDoubleClicked;
            }

            spawnItemOneButton = GetSimpleButton("btnSpawnItemOne");
            if (spawnItemOneButton != null)
            {
                spawnItemOneButton.OnPressed += SpawnItemOneButton_OnPressed;
            }

            spawnItemTenButton = GetSimpleButton("btnSpawnItemTen");
            if (spawnItemTenButton != null)
            {
                spawnItemTenButton.OnPressed += SpawnItemTenButton_OnPressed;
            }

            spawnEntityOneButton = GetSimpleButton("btnSpawnEntityOne");
            if (spawnEntityOneButton != null)
            {
                spawnEntityOneButton.OnPressed += SpawnEntityOneButton_OnPressed;
            }

            spawnEntityFiveButton = GetSimpleButton("btnSpawnEntityFive");
            if (spawnEntityFiveButton != null)
            {
                spawnEntityFiveButton.OnPressed += SpawnEntityFiveButton_OnPressed;
            }

            WireQualityButton("btnQuality1", 1);
            WireQualityButton("btnQuality2", 2);
            WireQualityButton("btnQuality3", 3);
            WireQualityButton("btnQuality4", 4);
            WireQualityButton("btnQuality5", 5);
            WireQualityButton("btnQuality6", 6);
            UpdateQualityButtonSelection();

            toggleCheatButton = GetSimpleButton("btnToggleCheat");
            if (toggleCheatButton != null)
            {
                toggleCheatButton.OnPressed += ToggleCheatButton_OnPressed;
            }

            spawnTileEntityButton = GetSimpleButton("btnSpawnTileEntity");
            if (spawnTileEntityButton != null)
            {
                spawnTileEntityButton.OnPressed += SpawnTileEntityButton_OnPressed;
            }

            reloadButton = GetSimpleButton("btnReload");
            if (reloadButton != null)
            {
                reloadButton.OnPressed += ReloadButton_OnPressed;
            }

            closeButton = GetSimpleButton("btnClose");
            if (closeButton != null)
            {
                closeButton.OnPressed += CloseButton_OnPressed;
            }
        }

        private XUiC_SimpleButton GetSimpleButton(string id)
        {
            XUiController controller = GetChildById(id);
            return controller?.GetChildByType<XUiC_SimpleButton>();
        }

        private XUiC_ToggleButton GetToggleButton(string id)
        {
            XUiController controller = GetChildById(id);
            return controller as XUiC_ToggleButton ?? controller?.GetChildByType<XUiC_ToggleButton>();
        }

        private void WireQualityButton(string id, int quality)
        {
            XUiC_SimpleButton button = GetSimpleButton(id);
            if (button != null)
            {
                qualityButtons[quality - 1] = button;
                button.OnPressed += (sender, mouseButton) => SetItemQuality(quality);
            }
        }

        [XuiXmlBinding("selectedquality")]
        public string BindingSelectedQuality()
        {
            return "Tier: Q" + selectedItemQuality;
        }

        [XuiXmlBinding("selectedcategory")]
        public string BindingSelectedCategory()
        {
            return "Category: " + DevMenuItemFilterState.SelectedCategory;
        }

        [XuiXmlBinding("selectedentitycategory")]
        public string BindingSelectedEntityCategory()
        {
            return "Category: " + DevMenuEntityFilterState.SelectedCategory;
        }

        private void SpawnItemOneButton_OnPressed(XUiController sender, int mouseButton)
        {
            SpawnSelectedItem(1);
        }

        private void SpawnItemTenButton_OnPressed(XUiController sender, int mouseButton)
        {
            SpawnSelectedItem(10);
        }

        private void SpawnEntityOneButton_OnPressed(XUiController sender, int mouseButton)
        {
            SpawnSelectedEntity(1);
        }

        private void SpawnEntityFiveButton_OnPressed(XUiController sender, int mouseButton)
        {
            SpawnSelectedEntity(5);
        }

        private void ToggleCheatButton_OnPressed(XUiController sender, int mouseButton)
        {
            ToggleSelectedCheat();
        }

        private void CategoryFilterToggle_OnValueChanged(XUiC_ToggleButton sender, bool newValue)
        {
            DevMenuItemFilterState.FilterByCategory = newValue;
            itemList?.RebuildList(_resetFilter: false);
            RefreshBindingsSelfAndChildren();
        }

        private void EntityCategoryFilterToggle_OnValueChanged(XUiC_ToggleButton sender, bool newValue)
        {
            DevMenuEntityFilterState.FilterByCategory = newValue;
            entityList?.RebuildList(_resetFilter: false);
            RefreshBindingsSelfAndChildren();
        }

        private void CategoryList_ListEntryClicked(XUiC_List<DevMenuItemCategoryEntry> list, DevMenuItemCategoryEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            DevMenuItemFilterState.SelectedCategory = entry.Category;
            DevMenuItemFilterState.FilterByCategory = true;

            if (categoryFilterToggle != null && !categoryFilterToggle.Value)
            {
                categoryFilterToggle.Value = true;
            }

            itemList?.RebuildList(_resetFilter: false);
            RefreshBindingsSelfAndChildren();
        }

        private void EntityCategoryList_ListEntryClicked(XUiC_List<DevMenuEntityCategoryEntry> list, DevMenuEntityCategoryEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            DevMenuEntityFilterState.SelectedCategory = entry.Category;
            DevMenuEntityFilterState.FilterByCategory = true;

            if (entityCategoryFilterToggle != null && !entityCategoryFilterToggle.Value)
            {
                entityCategoryFilterToggle.Value = true;
            }

            entityList?.RebuildList(_resetFilter: false);
            RefreshBindingsSelfAndChildren();
        }

        private void SetItemQuality(int quality)
        {
            selectedItemQuality = quality < 1 ? 1 : (quality > 6 ? 6 : quality);
            UpdateQualityButtonSelection();
            RefreshBindingsSelfAndChildren();
            Output("Item spawn tier set to Q" + selectedItemQuality + ".");
        }

        private void UpdateQualityButtonSelection()
        {
            for (int i = 0; i < qualityButtons.Length; i++)
            {
                XUiC_SimpleButton button = qualityButtons[i];
                if (button?.Button != null)
                {
                    button.Button.Selected = i + 1 == selectedItemQuality;
                }
            }
        }

        private void SpawnTileEntityButton_OnPressed(XUiController sender, int mouseButton)
        {
            SpawnSelectedTileEntity();
        }

        private void ReloadButton_OnPressed(XUiController sender, int mouseButton)
        {
            DevMenuItemCatalog.Reload();
            DevMenuEntityCatalog.Reload();
            LootableTileEntityCatalog.Reload();

            categoryList?.RebuildList(_resetFilter: false);
            itemList?.RebuildList(_resetFilter: false);
            entityCategoryList?.RebuildList(_resetFilter: false);
            entityList?.RebuildList(_resetFilter: false);
            tileEntityList?.RebuildList(_resetFilter: false);
            cheatList?.RebuildList(_resetFilter: false);

            Output("Reloaded item, entity, and tile entity catalogs.");
        }

        private void CloseButton_OnPressed(XUiController sender, int mouseButton)
        {
            xui.playerUI.windowManager.Close(WindowGroupName);
        }

        private void ItemList_ListEntryDoubleClicked(XUiC_List<DevMenuItemEntry> list, DevMenuItemEntry entry)
        {
            SpawnItem(entry, 1, selectedItemQuality);
        }

        private void EntityList_ListEntryDoubleClicked(XUiC_List<DevMenuEntityEntry> list, DevMenuEntityEntry entry)
        {
            SpawnEntity(entry, 1);
        }

        private void CheatList_ListEntryDoubleClicked(XUiC_List<DevMenuCheatEntry> list, DevMenuCheatEntry entry)
        {
            ToggleCheat(entry);
        }

        private void TileEntityList_ListEntryDoubleClicked(XUiC_List<LootableTileEntityEntry> list, LootableTileEntityEntry entry)
        {
            SpawnTileEntity(entry);
        }

        private void SpawnSelectedItem(int count)
        {
            if (itemList == null)
            {
                Output("Item list is not available.");
                return;
            }

            DevMenuItemEntry entry = itemList.SelectedEntryData;
            if (entry == null)
            {
                Output("Select an item first.");
                return;
            }

            SpawnItem(entry, count, selectedItemQuality);
        }

        private void SpawnSelectedEntity(int count)
        {
            if (entityList == null)
            {
                Output("Entity list is not available.");
                return;
            }

            DevMenuEntityEntry entry = entityList.SelectedEntryData;
            if (entry == null)
            {
                Output("Select an entity first.");
                return;
            }

            SpawnEntity(entry, count);
        }

        private void ToggleSelectedCheat()
        {
            if (cheatList == null)
            {
                Output("Cheat list is not available.");
                return;
            }

            DevMenuCheatEntry entry = cheatList.SelectedEntryData;
            if (entry == null)
            {
                Output("Select a cheat first.");
                return;
            }

            ToggleCheat(entry);
        }

        private void SpawnSelectedTileEntity()
        {
            if (tileEntityList == null)
            {
                Output("Tile entity list is not available.");
                return;
            }

            LootableTileEntityEntry entry = tileEntityList.SelectedEntryData;
            if (entry == null)
            {
                Output("Select a tile entity first.");
                return;
            }

            SpawnTileEntity(entry);
        }

        private static void SpawnItem(DevMenuItemEntry entry, int count, int quality)
        {
            if (entry == null)
            {
                return;
            }

            DevMenuItemSpawnService.RequestGiveToPrimaryPlayer(entry.ItemName, count, quality, out string message);
            Output(message);
        }

        private static void SpawnEntity(DevMenuEntityEntry entry, int count)
        {
            if (entry == null)
            {
                return;
            }

            DevMenuEntitySpawnService.RequestSpawnInFrontOfPrimaryPlayer(entry.EntityName, count, out string message);
            Output(message);
        }

        private void ToggleCheat(DevMenuCheatEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            DevMenuCheatService.ToggleForPrimaryPlayer(entry.Key, out string message);
            Output(message);
            cheatList?.RebuildList(_resetFilter: false);
        }

        private static void SpawnTileEntity(LootableTileEntityEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            LootableTileEntitySpawnService.RequestSpawnInFrontOfPrimaryPlayer(entry.BlockName, out string message);
            Output(message);
        }

        private static void Output(string message)
        {
            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[DevMenu] " + message);
        }
    }
}
