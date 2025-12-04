using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Localization;

namespace AgricultureEstate
{
    public class MilitiaSpeedModel : DefaultPartySpeedCalculatingModel
    {
        public override ExplainedNumber CalculateFinalSpeed(MobileParty mobileParty, ExplainedNumber finalSpeed)
        {
            ExplainedNumber result = base.CalculateFinalSpeed(mobileParty, finalSpeed);

            if (mobileParty != null && mobileParty.PartyComponent is EstateMilitiaPartyComponent)
            {
                float sizeBonus = mobileParty.MemberRoster.TotalManCount * 0.40f;
                float totalBonus = 2.0f + sizeBonus;

                result.Add(totalBonus, new TextObject("{=ae_militia_speed}Estate Militia Bonus"));
            }

            return result;
        }
    }
}