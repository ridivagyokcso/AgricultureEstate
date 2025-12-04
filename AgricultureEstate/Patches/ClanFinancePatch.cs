using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace AgricultureEstate
{
    internal class ClanFinancePatch
    {
        private static void Postfix(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals = false)
        {
            bool flag = clan == Hero.MainHero.Clan;
            if (flag)
            {
                float num = 0f;
                float num2 = 0f;
                foreach (KeyValuePair<Settlement, VillageLand> keyValuePair in AgricultureEstateBehavior.VillageLands)
                {
                    num += AgricultureEstateBehavior.CalculateGold(keyValuePair.Value);
                    num2 += (float)keyValuePair.Value.LastDayIncome;
                }
                num2 -= num;
                goldChange.Add(num, new TextObject("{=agricultureestate_ui_income_unused_land}Rent from unused owned land", null), null);
                bool flag2 = num2 >= 0f;
                if (flag2)
                {
                    goldChange.Add(num2, new TextObject("{=agricultureestate_ui_income_slave_production}Sales from slave production", null), null);
                }
            }
        }
    }
}
