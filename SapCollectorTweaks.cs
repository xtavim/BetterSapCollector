using System;
using HarmonyLib;
using UnityEngine;

namespace BetterSapCollector;

public static class SapCollectorTweaks
{
    [HarmonyPatch(typeof(SapCollector), "Awake")]
    public static class SapCollectorAwakePatch
    {
        public static bool Prefix(ref float ___m_secPerUnit, ref int ___m_maxLevel)
        {
            ___m_secPerUnit = Plugin.sapCollectorInterval.Value;
            ___m_maxLevel = Plugin.sapCollectorCapacity.Value;

            return true;
        }
    }
    
    [HarmonyPatch(typeof(SapCollector), "GetHoverText")]
    public static class GetHoverTextPatch
    {
        private static string GetSapCollectorHoverTextColor(SapCollector __instance)
        {
            var level = __instance.GetLevel();
            
            if (level == __instance.m_maxLevel) return "green";

            var hasRoot = (bool)(UnityEngine.Object)__instance.m_root;
            
            if (!hasRoot) return "red";
            
            return __instance.m_root.IsLevelLow() ? "orange" : "white";
        }
        public static bool Prefix(ref string __result, SapCollector __instance)
        {
            if (!Plugin.enableSapCollectorHover.Value) return true;
            
            var color =  GetSapCollectorHoverTextColor(__instance);
            var hasRoot = (bool)(UnityEngine.Object)__instance.m_root;
            
            var level = __instance.GetLevel();
            var statusText = $"{__instance.GetStatusText()}";
            var levelProgression = $"( {level} / {__instance.m_maxLevel} )";
            
            if (hasRoot) statusText = $"{statusText} {levelProgression}";
            
            var text = Localization.instance.Localize(
                $"{__instance.m_name} \n<color={color}>{statusText}</color>");
            
            if (level > 0)
            {
                text += Localization.instance.Localize("\n[<color=green><b>$KEY_Use</b></color>] " + __instance.m_extractText);
            }
            
            __result = text;
            return false;
        }
    }
}