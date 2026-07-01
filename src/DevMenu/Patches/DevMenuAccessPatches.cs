using System;
using HarmonyLib;
using UnityEngine.Scripting;

namespace DevMenu.Patches
{
    [Preserve]
    [HarmonyPatch(typeof(XUiC_WindowSelector), nameof(XUiC_WindowSelector.Init))]
    internal static class DevMenuWindowSelectorInitPatch
    {
        private static void Postfix(XUiC_WindowSelector __instance)
        {
            DevMenuAccess.ApplyDevMenuButtonVisibility(__instance.GetChildById(XUiC_DevMenuWindow.WindowGroupName));
        }
    }

    [Preserve]
    [HarmonyPatch(typeof(XUiC_WindowSelector), nameof(XUiC_WindowSelector.OnOpen))]
    internal static class DevMenuWindowSelectorOpenPatch
    {
        private static void Postfix(XUiC_WindowSelector __instance)
        {
            DevMenuAccess.ApplyDevMenuButtonVisibility(__instance.GetChildById(XUiC_DevMenuWindow.WindowGroupName));
        }
    }

    [Preserve]
    [HarmonyPatch(typeof(XUiC_WindowSelector), nameof(XUiC_WindowSelector.HandleOnPress))]
    internal static class DevMenuWindowSelectorPressPatch
    {
        private static bool Prefix(XUiController _sender)
        {
            if (!DevMenuAccess.IsDevMenuButton(_sender))
            {
                return true;
            }

            if (DevMenuAccess.CanLocalPlayerUseDevMenu(out string message))
            {
                return true;
            }

            Output(message);
            return false;
        }

        private static void Output(string message)
        {
            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[DevMenu] " + message);
        }
    }

    [Preserve]
    [HarmonyPatch(typeof(GUIWindowManager), nameof(GUIWindowManager.Open), new[] { typeof(string), typeof(bool) })]
    internal static class DevMenuGuiWindowOpenPatch
    {
        private static bool Prefix(string _windowName)
        {
            return CanOpen(_windowName);
        }

        internal static bool CanOpen(string windowName)
        {
            if (!DevMenuAccess.IsDevMenuWindow(windowName))
            {
                return true;
            }

            if (DevMenuAccess.CanLocalPlayerUseDevMenu(out string message))
            {
                return true;
            }

            Output(message);
            return false;
        }

        private static void Output(string message)
        {
            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[DevMenu] " + message);
        }
    }

    [Preserve]
    [HarmonyPatch(typeof(GUIWindowManager), nameof(GUIWindowManager.Open), new[] { typeof(string), typeof(bool), typeof(bool) })]
    internal static class DevMenuGuiWindowOpenWithEscPatch
    {
        private static bool Prefix(string _windowName)
        {
            return DevMenuGuiWindowOpenPatch.CanOpen(_windowName);
        }
    }
}
