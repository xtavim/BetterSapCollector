using HarmonyLib;
using UnityEngine;

namespace BetterSapCollector;

public static class AncientRootTweaks
{
    [HarmonyPatch(typeof(ResourceRoot), "Awake")]
    public static class AncientRootAwakePatch
    {
        public static bool Prefix(ref float ___m_regenPerSec, ref float ___m_maxLevel, ref float ___m_highThreshold,
            ref float ___m_emptyTreshold)
        {
            ___m_regenPerSec = Plugin.ancientRootRegenerationSpeed.Value;
            ___m_maxLevel = Plugin.ancientRootCapacity.Value;
            ___m_highThreshold = Plugin.ancientRootCapacity.Value * 0.5f;
            ___m_emptyTreshold = Plugin.ancientRootCapacity.Value * 0.1f;

            return true;
        }
    }

    [HarmonyPatch(typeof(ResourceRoot), "GetHoverText")]
    public static class GetHoverTextPatch
    {
        private static string GetRootHoverTextColor(float level, ResourceRoot resourceRoot)
        {
            if (level >= resourceRoot.m_highThreshold) return "green";

            return level >= resourceRoot.m_emptyTreshold ? "orange" : "red";
        }

        public static string Postfix(string __result, ResourceRoot __instance)
        {
            if (!Plugin.enableAncientRootHover.Value) return __result;

            var level = __instance.GetLevel();
            var percentage = (level / __instance.m_maxLevel * 100).ToString("F0");
            var color = GetRootHoverTextColor(level, __instance);

            return __result + $"\n<color={color}>{percentage}%</color>";
        }
    }
}