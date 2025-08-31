using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;

namespace BetterSapCollector
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private static ConfigSync configSync = new(PluginInfo.PLUGIN_GUID)
        {
            DisplayName = PluginInfo.PLUGIN_NAME,
            CurrentVersion = PluginInfo.PLUGIN_VERSION,
            MinimumRequiredVersion = PluginInfo.PLUGIN_VERSION
        };

                public static ConfigEntry<bool> enableSapCollector;
        public static ConfigEntry<int> sapCollectorSpeed;
        public static ConfigEntry<int> sapCollectorCapacity;
        public static ConfigEntry<bool> enableSapCollectorHover;
        
        public static ConfigEntry<bool> enableAncientRoot;
        public static ConfigEntry<int> ancientRootRegenerationSpeed;
        public static ConfigEntry<int> ancientRootCapacity;
        public static ConfigEntry<bool> enableAncientRootHover;

        private void Awake()
        {
            InitializeConfig();
            InitializeHarmonyPatches();
            InitializeEvents();
        }

        private ConfigEntry<T> ConfigSync<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription configDescription = new ConfigDescription(
                description.Description + (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind<T>(group, name, value, configDescription);
            configSync.AddConfigEntry<T>(configEntry).SynchronizedConfig = synchronizedSetting;
            return configEntry;
        }

        private void InitializeConfig()
        {
            Config.SaveOnConfigSet = false;

            // Server lock configuration
            var serverConfigLocked = ConfigSync("1 - ServerSync", "Lock Configuration", true,
                new ConfigDescription(
                    "If enabled, the configuration is locked and can be changed by server admins only."));
            configSync.AddLockingConfigEntry(serverConfigLocked);

            enableSapCollector = ConfigSync("Sap Collector", "Enable Sap Collector", true,
                new ConfigDescription("Enable Sap Collector tweaks."));

            enableSapCollectorHover = ConfigSync("Sap Collector", "Enable Sap Collector Hover Text", true,
                new ConfigDescription("Enable enhanced hover text for Sap Collectors."));

            sapCollectorSpeed = ConfigSync("Sap Collector",
                "Sap Collector Speed",
                10,
                new ConfigDescription(
                    "The time it takes for a Sap Collector to process 1 Sap. Vanilla is 10 seconds. (less = faster)",
                    new AcceptableValueRange<int>(1, 20)
                )
            );

            sapCollectorCapacity = ConfigSync("Sap Collector",
                "Sap Collector Capacity",
                4,
                new ConfigDescription(
                    "Sap Collector inventory capacity. Vanilla is 4 items.",
                    new AcceptableValueRange<int>(1, 40)
                ));

            enableAncientRoot = ConfigSync("Ancient Root", "Enable Ancient Root", true,
                new ConfigDescription("Enable Ancient Root tweaks."));

            enableAncientRootHover = ConfigSync("Ancient Root", "Enable Ancient Root Hover Text", true,
                new ConfigDescription("Enable enhanced hover text for Ancient Roots."));

            ancientRootRegenerationSpeed = ConfigSync("Ancient Root",
                "Ancient Root Regeneration speed",
                1,
                new ConfigDescription(
                    "Ancient Root Regeneration units per second. Vanilla is 1 unit per second. (more = faster)",
                    new AcceptableValueRange<int>(0, 50)
                ));

            ancientRootCapacity = ConfigSync("Ancient Root",
                "Ancient Root Capacity",
                100,
                new ConfigDescription(
                    "Ancient Root buffer capacity. Vanilla is 100 units.",
                    new AcceptableValueRange<int>(50, 500)
                ));

            Config.SaveOnConfigSet = true;
            Config.Save();
        }

        private static void InitializeHarmonyPatches()
        {
            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
        }

        private static void InitializeEvents()
        {
            sapCollectorSpeed.SettingChanged += (_, _) => UpdateAllSapCollectors();
            sapCollectorCapacity.SettingChanged += (_, _) => UpdateAllSapCollectors();

            ancientRootRegenerationSpeed.SettingChanged += (_, _) => UpdateAllAncientRoots();
            ancientRootCapacity.SettingChanged += (_, _) => UpdateAllAncientRoots();
        }

        private static void UpdateAllSapCollectors()
        {
            foreach (var collector in FindObjectsOfType<SapCollector>())
            {
                collector.m_secPerUnit = sapCollectorSpeed.Value;
                collector.m_maxLevel = sapCollectorCapacity.Value;
            }
        }

        private static void UpdateAllAncientRoots()
        {
            foreach (var root in FindObjectsOfType<ResourceRoot>())
            {
                root.m_regenPerSec = ancientRootRegenerationSpeed.Value;
                root.m_maxLevel = ancientRootCapacity.Value;
            }
        }
    }
}