using HarmonyLib;
using TaleWorlds.CampaignSystem.Settlements;

namespace AgricultureEstate
{
    [HarmonyPatch(typeof(Village), "GetHearthLevel")]
    internal class HearthLevelPatch
    {
        private static bool Prefix(Village __instance, ref int __result)
        {
            if (__instance.Hearth < 1000.0)
            {
                return true;
            }

            __result = (int)((__instance.Hearth + 200.0) / 400.0);

            return false;
        }
    }
}