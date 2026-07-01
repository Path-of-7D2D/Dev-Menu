using HarmonyLib;
using UnityEngine.Scripting;

namespace DevMenu.Patches
{
    [Preserve]
    [HarmonyPatch(typeof(XUiC_ContainerStandardControls), nameof(XUiC_ContainerStandardControls.Init))]
    internal static class ContainerStandardControlsRerollPatch
    {
        private const string ButtonId = "btnDevMenuRerollLoot";

        private static void Postfix(XUiC_ContainerStandardControls __instance)
        {
            XUiController button = __instance.GetChildById(ButtonId);
            if (button == null)
            {
                return;
            }

            DevMenuAccess.ApplyDevMenuButtonVisibility(button);

            button.OnPress -= OnRerollLootPressed;
            button.OnPress += OnRerollLootPressed;
        }

        private static void OnRerollLootPressed(XUiController sender, int mouseButton)
        {
            if (!DevMenuAccess.CanLocalPlayerUseDevMenu(out string message))
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[DevMenu] " + message);
                return;
            }

            XUiC_ContainerStandardControls controls = sender?.GetParentByType<XUiC_ContainerStandardControls>();
            if (controls == null)
            {
                Log.Warning("[DevMenu] Could not find loot container controls for reroll button.");
                return;
            }

            DevMenuLootRerollService.RequestRerollOpenLootContainer(controls, out string _);
        }
    }
}
