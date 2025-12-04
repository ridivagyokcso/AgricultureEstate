using TaleWorlds.CampaignSystem.Settlements;

namespace AgricultureEstate
{
    internal class HearthChange2Patch
    {
        private static void Postfix(Village __instance, ref float __result)
        {
            if (AgricultureEstateBehavior.VillageLands == null)
            {
                return;
            }

            if (AgricultureEstateBehavior.VillageLands.TryGetValue(__instance.Settlement, out VillageLand villageLand)
                && (villageLand.AvaliblePlots + villageLand.OwnedPlots > 10))
            {
                int extraPlots = villageLand.OwnedPlots + villageLand.AvaliblePlots - 10;
                __result += (float)extraPlots * 0.1f;
            }

            __result -= (__instance.Hearth - 600.0f) / 1000.0f;
        }
    }
}