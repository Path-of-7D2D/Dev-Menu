using System.Reflection;
using HarmonyLib;
using UnityEngine.Scripting;

namespace DevMenu
{
    [Preserve]
    public class DevMenuModApi : IModApi
    {
        public void InitMod(Mod _modInstance)
        {
            DevMenuSettings.Load(_modInstance);

            var harmony = new Harmony("com.pathof7d2d.devmenu");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            DevMenuHotkeyPatches.AddHotkeyToLiveInputSet();

            Log.Out("[DevMenu] Loaded. Use 'devmenu' or 'p7dev' to open the developer menu.");
        }
    }
}
