using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace AgricultureEstate
{
    internal class HearthChangePatch
    {
        private static void Postfix(Village __instance, ref ExplainedNumber __result)
        {
            if (AgricultureEstateBehavior.VillageLands.TryGetValue(__instance.Settlement, out VillageLand villageLand)
                && (villageLand.AvaliblePlots + villageLand.OwnedPlots > 10))
            {
                int extraPlots = villageLand.OwnedPlots + villageLand.AvaliblePlots - 10;
                __result.Add((float)extraPlots * 0.1f, new TextObject("Land Clearance", null), null);
            }

            if (__instance.Hearth >= 600.0f)
            {
                float penalty = (__instance.Hearth - 600.0f) / -1000.0f;
                __result.Add(penalty, new TextObject("Overpopulation and Land Scarcity", null), null);
            }
        }
    }
}