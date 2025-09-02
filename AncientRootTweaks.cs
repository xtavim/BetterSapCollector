using HarmonyLib;
using UnityEngine;

namespace BetterSapCollector;

public static class AncientRootTweaks
{
    [HarmonyPatch(typeof(ResourceRoot), "Awake")]
    public static class AncientRootAwakePatch
    {
        public static bool Prefix(ref float ___m_regenPerSec, ref float ___m_maxLevel)
        {
            if (!Plugin.enableAncientRoot.Value) return true;

            ___m_regenPerSec = Plugin.ancientRootRegenerationSpeed.Value;
            ___m_maxLevel = Plugin.ancientRootCapacity.Value;

            return true;
        }
    }
    
    [HarmonyPatch(typeof(ResourceRoot), "GetHoverText")]
    public static class GetHoverTextPatch
    {
        public static string Postfix(string __result, ResourceRoot __instance)
        {
            if (!Plugin.enableAncientRoot.Value || !Plugin.enableAncientRootHover.Value) return __result;
            
            var level = __instance.GetLevel();
            var percentage = (level / __instance.m_maxLevel * 100).ToString("F0");
            return __result + $"\n<color=green>{percentage}%</color>";
        }
    }
}