using System;
using UnityEngine.Scripting;

namespace DevMenu
{
    [Preserve]
    internal static class DevMenuAccess
    {
        private static readonly string[] CommandNames = { "devmenu", "p7dev" };

        public static bool CanUseFromCommand(CommandSenderInfo senderInfo, out string message)
        {
            message = null;

            if (!DevMenuSettings.EnablePermissionCheck)
            {
                return true;
            }

            if (senderInfo.RemoteClientInfo != null)
            {
                return CanClientUseDevMenu(senderInfo.RemoteClientInfo, out message);
            }

            if (!GameManager.IsDedicatedServer)
            {
                return CanLocalPlayerUseDevMenu(out message);
            }

            return true;
        }

        public static bool CanLocalPlayerUseDevMenu(out string message)
        {
            message = null;

            if (!DevMenuSettings.EnablePermissionCheck)
            {
                return true;
            }

            if (GameManager.IsDedicatedServer)
            {
                return true;
            }

            ConnectionManager connectionManager = SingletonMonoBehaviour<ConnectionManager>.Instance;
            if (connectionManager == null || connectionManager.IsSinglePlayer)
            {
                return true;
            }

            ClientInfo localClientInfo = GetLocalClientInfo(connectionManager);
            if (localClientInfo != null)
            {
                return CanClientUseDevMenu(localClientInfo, out message);
            }

            if (connectionManager.IsServer)
            {
                return true;
            }

            message = "Dev Menu requires permission for the 'devmenu' command.";
            return false;
        }

        public static bool CanClientUseDevMenu(ClientInfo clientInfo, out string message)
        {
            message = null;

            if (!DevMenuSettings.EnablePermissionCheck)
            {
                return true;
            }

            if (clientInfo == null)
            {
                message = "Dev Menu permission could not be checked.";
                return false;
            }

            AdminTools adminTools = GameManager.Instance?.adminTools;
            if (adminTools == null)
            {
                message = "Dev Menu permission data is not available.";
                return false;
            }

            if (adminTools.CommandAllowedFor(CommandNames, clientInfo))
            {
                return true;
            }

            message = "You do not have permission to use the Dev Menu.";
            return false;
        }

        public static bool IsDevMenuWindow(string windowName)
        {
            return string.Equals(windowName, XUiC_DevMenuWindow.WindowGroupName, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsDevMenuButton(XUiController controller)
        {
            return IsDevMenuWindow(controller?.ViewComponent?.ID);
        }

        public static void ApplyDevMenuButtonVisibility(XUiController controller)
        {
            if (controller == null)
            {
                return;
            }

            bool canUse = CanLocalPlayerUseDevMenu(out string _);
            ApplyVisibility(controller, canUse);
        }

        private static ClientInfo GetLocalClientInfo(ConnectionManager connectionManager)
        {
            EntityPlayerLocal player = GameManager.Instance?.World?.GetPrimaryPlayer();
            if (player == null || connectionManager.Clients == null)
            {
                return null;
            }

            return connectionManager.Clients.ForEntityId(player.entityId);
        }

        private static void ApplyVisibility(XUiController controller, bool visible)
        {
            if (controller.ViewComponent != null)
            {
                controller.ViewComponent.IsVisible = visible;
                controller.ViewComponent.Enabled = visible;
            }

            XUiC_SimpleButton simpleButton = controller as XUiC_SimpleButton ?? controller.GetChildByType<XUiC_SimpleButton>();
            if (simpleButton != null)
            {
                simpleButton.Enabled = visible;
            }
        }
    }
}
