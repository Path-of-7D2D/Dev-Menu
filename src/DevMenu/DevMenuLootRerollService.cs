using System;
using UnityEngine.Scripting;

namespace DevMenu
{
    [Preserve]
    public static class DevMenuLootRerollService
    {
        public static bool RequestRerollOpenLootContainer(XUiC_ContainerStandardControls controls, out string message)
        {
            message = null;

            if (!DevMenuAccess.CanLocalPlayerUseDevMenu(out message))
            {
                Log.Warning("[DevMenu] " + message);
                return false;
            }

            XUiC_LootWindow lootWindow = controls?.GetParentByType<XUiC_LootWindow>();
            if (lootWindow == null)
            {
                message = "No loot window is open.";
                Log.Warning("[DevMenu] " + message);
                return false;
            }

            ITileEntityLootable lootable = lootWindow.te;
            if (lootable == null)
            {
                message = "No lootable tile entity is attached to the loot window.";
                Log.Warning("[DevMenu] " + message);
                return false;
            }

            if (lootable.bPlayerStorage)
            {
                message = "Reroll skipped for player storage.";
                Log.Warning("[DevMenu] " + message);
                return false;
            }

            Vector3i blockPos = lootable.ToWorldPos();
            bool success;
            if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            {
                success = TryRerollAt(blockPos, out message);
            }
            else
            {
                SendServerRerollCommand(blockPos);
                message = "Requested loot reroll at " + FormatPosition(blockPos) + ".";
                success = true;
            }

            if (success)
            {
                CloseLootWindow(lootWindow);
                Log.Out("[DevMenu] " + message);
            }
            else
            {
                Log.Warning("[DevMenu] " + message);
            }

            return success;
        }

        public static bool TryRerollAt(Vector3i blockPos, out string message)
        {
            message = null;

            World world = GameManager.Instance?.World;
            if (world == null)
            {
                message = "No world is loaded.";
                return false;
            }

            TileEntity tileEntity = GetTileEntityAtOrParent(world, blockPos);
            ITileEntityLootable lootable = GetLootable(tileEntity);
            if (lootable == null)
            {
                message = "No lootable tile entity exists at " + FormatPosition(blockPos) + ".";
                return false;
            }

            if (lootable.bPlayerStorage)
            {
                message = "Reroll skipped for player storage at " + FormatPosition(blockPos) + ".";
                return false;
            }

            ResetLootableState(lootable);
            tileEntity?.SetModified();

            message = "Rerolled loot at " + FormatPosition(lootable.ToWorldPos()) + ".";
            return true;
        }

        private static void ResetLootableState(ITileEntityLootable lootable)
        {
            lootable.bTouched = false;
            lootable.bWasTouched = false;
            lootable.worldTimeTouched = 0UL;

            ItemStack[] items = lootable.items;
            if (items == null || items.Length == 0)
            {
                Vector2i containerSize = lootable.GetContainerSize();
                int slotCount = Math.Max(0, containerSize.x * containerSize.y);
                if (slotCount > 0)
                {
                    lootable.items = ItemStack.CreateArray(slotCount);
                    items = lootable.items;
                }
            }

            if (items != null)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i] == null)
                    {
                        items[i] = ItemStack.Empty.Clone();
                    }
                    else
                    {
                        items[i].Clear();
                    }
                }
            }

            lootable.SlotLocks?.Clear();
            lootable.SetModified();
        }

        private static ITileEntityLootable GetLootable(TileEntity tileEntity)
        {
            if (tileEntity is TileEntityComposite composite)
            {
                return composite.GetFeature<ITileEntityLootable>();
            }

            return tileEntity as ITileEntityLootable;
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

        private static void CloseLootWindow(XUiC_LootWindow lootWindow)
        {
            lootWindow.UserLockMode = false;
            lootWindow.xui?.playerUI?.windowManager?.Close(XUiC_LootWindowGroup.ID);
        }

        private static void SendServerRerollCommand(Vector3i blockPos)
        {
            string command = "devmenu rerollloot " + blockPos.x + " " + blockPos.y + " " + blockPos.z;
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                NetPackageManager.GetPackage<NetPackageConsoleCmdServer>().Setup(command));
        }

        private static string FormatPosition(Vector3i blockPos)
        {
            return blockPos.x + "," + blockPos.y + "," + blockPos.z;
        }
    }
}
