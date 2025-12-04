using System.Collections.Generic;
using AgricultureEstate.l18n;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace AgricultureEstate
{
    [HarmonyPatch(typeof(SellPrisonersAction), "ApplyForAllPrisoners")]
    internal class SellPrisonersPatch
    {
        private static bool Prefix(PartyBase sellerParty, PartyBase buyerParty)
        {
            if (sellerParty == null ||
                sellerParty.MobileParty == MobileParty.MainParty ||
                sellerParty.LeaderHero == null ||
                sellerParty.MobileParty.CurrentSettlement == null ||
                (!sellerParty.MobileParty.CurrentSettlement.IsTown && !sellerParty.MobileParty.CurrentSettlement.IsCastle))
            {
                return true;
            }

            Settlement currentSettlement = sellerParty.MobileParty.CurrentSettlement;
            TroopRoster prisonRoster = sellerParty.PrisonRoster;

            foreach (Village village in currentSettlement.BoundVillages)
            {
                if (AgricultureEstateBehavior.VillageLands.TryGetValue(village.Settlement, out VillageLand villageLand) && villageLand.BuySlaves)
                {
                    int boughtCount = 0;
                    int totalCost = 0;
                    int capacity = villageLand.OwnedPlots * 10 - villageLand.Prisoners.TotalManCount;

                    CharacterObject bandit = SellPrisonersPatch.getBandit(prisonRoster);

                    while (bandit != null && capacity > 0 && totalCost + Campaign.Current.Models.RansomValueCalculationModel.PrisonerRansomValue(bandit, sellerParty.LeaderHero) <= Hero.MainHero.Gold)
                    {
                        int ransomValue = Campaign.Current.Models.RansomValueCalculationModel.PrisonerRansomValue(bandit, sellerParty.LeaderHero);
                        totalCost += ransomValue;

                        // Áthelyezés
                        prisonRoster.AddToCounts(bandit, -1, false, 0, 0, true, -1);
                        villageLand.Prisoners.AddToCounts(bandit, 1, false, 0, 0, true, -1);

                        capacity--;
                        boughtCount++;
                        bandit = SellPrisonersPatch.getBandit(prisonRoster);
                    }

                    if (boughtCount > 0)
                    {
                        if (Hero.MainHero.GetPerkValue(DefaultPerks.Roguery.Manhunter))
                        {
                            totalCost = (int)(0.8 * totalCost);
                        }

                        GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, sellerParty.LeaderHero, totalCost, false);

                        InformationManager.DisplayMessage(new InformationMessage(Localization.SetTextVariables(
                            "{=agricultureestate_boughtslaves}Your estate in {SETTLEMENT_NAME} bought a load of {SLAVE_BOUGHT_AMOUNT} slaves from {SELLER_LORD_NAME} in the markets of {SELLER_SETTLEMENT_NAME} at a cost of {PRICE}{GOLD_ICON} gold",
                            new KeyValuePair<string, string>[]
                            {
                                new KeyValuePair<string, string>("SETTLEMENT_NAME", village.Name.ToString()),
                                new KeyValuePair<string, string>("SLAVE_BOUGHT_AMOUNT", boughtCount.ToString()),
                                new KeyValuePair<string, string>("SELLER_LORD_NAME", sellerParty.LeaderHero.Name.ToString()),
                                new KeyValuePair<string, string>("SELLER_SETTLEMENT_NAME", currentSettlement.Name.ToString()),
                                new KeyValuePair<string, string>("PRICE", totalCost.ToString()),
                                new KeyValuePair<string, string>("GOLD_ICON", null)
                            }).ToString()));
                    }
                }
            }
            return true;
        }

        private static CharacterObject getBandit(TroopRoster roster)
        {
            if (roster == null) return null;

            foreach (TroopRosterElement troopRosterElement in roster.GetTroopRoster())
            {
                if ((int)troopRosterElement.Character.Occupation == 15 && !troopRosterElement.Character.IsHero)
                {
                    return troopRosterElement.Character;
                }
            }
            return null;
        }
    }
}