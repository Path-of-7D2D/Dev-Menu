using System;
using System.IO;
using System.Xml;
using UnityEngine.Scripting;

namespace DevMenu
{
    [Preserve]
    internal static class DevMenuSettings
    {
        private const string ConfigFileName = "devmenu_config.xml";

        private static string configPath;

        public static bool EnablePermissionCheck { get; private set; } = true;

        public static void Load(Mod modInstance)
        {
            ResetDefaults();
            configPath = ResolveConfigPath(modInstance);

            if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
            {
                Log.Out("[DevMenu] Config not found; using defaults.");
                return;
            }

            try
            {
                var document = new XmlDocument();
                document.Load(configPath);

                XmlNode root = document.SelectSingleNode("/DevMenuConfig");
                if (root == null)
                {
                    Log.Out("[DevMenu] Config is missing DevMenuConfig root; using defaults.");
                    return;
                }

                EnablePermissionCheck = ReadBool(root, "EnablePermissionCheck", EnablePermissionCheck);

                Log.Out("[DevMenu] Loaded config: " + configPath);
            }
            catch (Exception ex)
            {
                Log.Out("[DevMenu] Failed to load config; using defaults. " + ex.Message);
            }
        }

        public static void Reload()
        {
            Load(null);
        }

        private static void ResetDefaults()
        {
            EnablePermissionCheck = true;
        }

        private static string ResolveConfigPath(Mod modInstance)
        {
            if (modInstance != null && !string.IsNullOrEmpty(modInstance.Path))
            {
                return Path.Combine(modInstance.Path, "Config", ConfigFileName);
            }

            if (!string.IsNullOrEmpty(configPath))
            {
                return configPath;
            }

            return Path.Combine(Environment.CurrentDirectory, "Mods", "1A-DevMenu", "Config", ConfigFileName);
        }

        private static bool ReadBool(XmlNode root, string path, bool fallback)
        {
            string raw = ReadString(root, path, null);
            if (string.IsNullOrEmpty(raw))
            {
                return fallback;
            }

            if (bool.TryParse(raw, out bool value))
            {
                return value;
            }

            if (raw == "1")
            {
                return true;
            }

            if (raw == "0")
            {
                return false;
            }

            return fallback;
        }

        private static string ReadString(XmlNode root, string path, string fallback)
        {
            XmlNode node = root.SelectSingleNode(path);
            if (node == null)
            {
                return fallback;
            }

            string value = node.InnerText;
            return string.IsNullOrEmpty(value) ? fallback : value.Trim();
        }
    }
}
