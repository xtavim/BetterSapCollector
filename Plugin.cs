using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;

namespace BetterSapCollector
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public new static readonly ManualLogSource Logger =
            BepInEx.Logging.Logger.CreateLogSource(PluginInfo.PLUGIN_NAME);
        
        private static ConfigSync configSync = new(PluginInfo.PLUGIN_GUID)
        {
            DisplayName = PluginInfo.PLUGIN_NAME,
            CurrentVersion = PluginInfo.PLUGIN_VERSION,
            MinimumRequiredVersion = PluginInfo.PLUGIN_VERSION
        };

        public static ConfigEntry<int> sapCollectorInterval;
        public static ConfigEntry<int> sapCollectorCapacity;
        public static ConfigEntry<bool> enableSapCollectorHover;
        
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

            enableSapCollectorHover = ConfigSync("Sap Collector", "Sap Collector Hover Text", true,
                new ConfigDescription("If enabled, a hover text will be displayed when hovering over a Sap Collector containing the current level, capacity, and time remaining."));

            sapCollectorInterval = ConfigSync(
                "Sap Collector",
                "Sap Collector Interval",
                10,
                new ConfigDescription(
                    "Time (in seconds) required for a Sap Collector to produce 1 Sap. Vanilla is 10. Lower = faster.",
                    new AcceptableValueRange<int>(1, 60)
                )
            );

            sapCollectorCapacity = ConfigSync("Sap Collector",
                "Sap Collector Capacity",
                4,
                new ConfigDescription(
                    "Sap Collector inventory capacity. Vanilla is 4 items.",
                    new AcceptableValueRange<int>(1, 40)
                ));

            enableAncientRootHover = ConfigSync("Ancient Root", "Ancient Root Hover Text", true,
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
            sapCollectorInterval.SettingChanged += (_, _) => UpdateAllSapCollectors();
            sapCollectorCapacity.SettingChanged += (_, _) => UpdateAllSapCollectors();

            ancientRootRegenerationSpeed.SettingChanged += (_, _) => UpdateAllAncientRoots();
            ancientRootCapacity.SettingChanged += (_, _) => UpdateAllAncientRoots();
        }

        private static void UpdateAllSapCollectors()
        {
            foreach (var collector in FindObjectsOfType<SapCollector>())
            {
                collector.m_secPerUnit = sapCollectorInterval.Value;
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