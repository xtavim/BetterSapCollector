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
            if (!Plugin.enableSapCollector.Value) return true;

            ___m_secPerUnit = Plugin.sapCollectorSpeed.Value;
            ___m_maxLevel = Plugin.sapCollectorCapacity.Value;

            return true;
        }
    }
    
    [HarmonyPatch(typeof(SapCollector), "GetHoverText")]
    public static class GetHoverTextPatch
    {
        public static string Postfix(string __result, SapCollector __instance)
        {
            if (!Plugin.enableSapCollectorHover.Value) return __result;
            
            var level = __instance.GetLevel();
            var statusText = __instance.GetStatusText();
            var text = Localization.instance.Localize(
                $"{__instance.m_name} \n{statusText} ( {level} / {__instance.m_maxLevel} ) <color=yellow>{TimeLeft(__instance)}</color>");
            if (level > 0)
            {
                return text + Localization.instance.Localize("\n[<color=green><b>$KEY_Use</b></color>] " + __instance.m_extractText);
            }
            return text;
        }

        private static string TimeLeft(SapCollector __sapCollector)
        {
            var text = "";
            if (__sapCollector.GetLevel() == __sapCollector.m_maxLevel)
            {
                return text;
            }
            var @float = __sapCollector.m_nview.GetZDO().GetFloat("product", 0f);
            var num = __sapCollector.m_secPerUnit - @float;
            var num2 = (float)Mathf.FloorToInt((float)((int)num / 60));
            var num3 = (float)Mathf.FloorToInt((float)((int)num % 60));
            return $"\n( {num2:00}:{num3:00} )";
        }
    }
}