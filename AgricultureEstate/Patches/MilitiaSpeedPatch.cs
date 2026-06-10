using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Localization;

namespace AgricultureEstate
{
    [HarmonyPatch(typeof(DefaultPartySpeedCalculatingModel), "CalculateFinalSpeed")]
    internal class MilitiaSpeedPatch
    {
        private static void Postfix(MobileParty mobileParty, ref ExplainedNumber __result)
        {
            if (mobileParty != null && mobileParty.PartyComponent is EstateMilitiaPartyComponent)
            {
                float sizeBonus = mobileParty.MemberRoster.TotalManCount * 0.40f;
                float totalBonus = 2.0f + sizeBonus;

                __result.Add(totalBonus, new TextObject("{=ae_militia_speed}Estate Militia Bonus"));
            }
        }
    }
}
