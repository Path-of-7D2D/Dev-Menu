using System;
using System.Reflection;
using HarmonyLib;
using InControl;
using Platform;

namespace DevMenu
{
    [UnityEngine.Scripting.Preserve]
    public static class DevMenuHotkeyPatches
    {
        private const string ActionName = "DevMenuOpen";
        private const string ActionNameKey = "inpActDevMenuName";
        private const string ActionDescriptionKey = "inpActDevMenuDesc";

        private static readonly MethodInfo CreatePlayerActionMethod =
            AccessTools.Method(typeof(PlayerActionSet), "CreatePlayerAction", new[] { typeof(string) });

        public static void AddHotkey(PlayerActionsLocal actions)
        {
            if (actions == null)
            {
                return;
            }

            if (actions.GetPlayerActionByName(ActionName) != null)
            {
                return;
            }

            if (CreatePlayerActionMethod == null)
            {
                Log.Warning("[DevMenu] PlayerActionSet.CreatePlayerAction not found; Dev Menu hotkey not added.");
                return;
            }

            var action = (PlayerAction)CreatePlayerActionMethod.Invoke(actions, new object[] { ActionName });
            action.UserData = new PlayerActionData.ActionUserData(
                ActionNameKey,
                ActionDescriptionKey,
                PlayerActionData.GroupMenu,
                PlayerActionData.EAppliesToInputType.KbdMouseOnly);
            action.AddDefaultBinding(Key.Slash);

            Log.Out("[DevMenu] Added Dev Menu hotkey: '/' -> Dev Menu.");
        }

        public static void AddHotkeyToLiveInputSet()
        {
            try
            {
                AddHotkey(PlatformManager.NativePlatform?.Input?.PrimaryPlayer);
            }
            catch (Exception ex)
            {
                Log.Warning("[DevMenu] Failed to add Dev Menu hotkey to live input set: " + ex);
            }
        }

        private static void HandleHotkey(EntityPlayerLocal player)
        {
            PlayerActionsLocal actions = player?.playerInput;
            PlayerAction action = actions?.GetPlayerActionByName(ActionName);
            if (action == null || !action.WasPressed)
            {
                return;
            }

            LocalPlayerUI playerUi = LocalPlayerUI.GetUIForPlayer(player);
            if (playerUi == null || playerUi.windowManager == null)
            {
                return;
            }

            if (playerUi.windowManager.IsWindowOpen(XUiC_DevMenuWindow.WindowGroupName))
            {
                playerUi.windowManager.Close(XUiC_DevMenuWindow.WindowGroupName);
                return;
            }

            playerUi.windowManager.Open(XUiC_DevMenuWindow.WindowGroupName, _bModal: true);
        }

        [HarmonyPatch(typeof(PlayerActionsLocal), MethodType.Constructor, new Type[0])]
        private static class PlayerActionsLocalCtorPatch
        {
            private static void Postfix(PlayerActionsLocal __instance)
            {
                AddHotkey(__instance);
            }
        }

        [HarmonyPatch(typeof(EntityPlayerLocal), nameof(EntityPlayerLocal.Update))]
        private static class EntityPlayerLocalUpdatePatch
        {
            private static void Postfix(EntityPlayerLocal __instance)
            {
                HandleHotkey(__instance);
            }
        }
    }
}
