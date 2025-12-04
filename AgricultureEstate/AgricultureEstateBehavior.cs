using AgricultureEstate.l18n;
using Helpers;
using MCM.Abstractions.Base.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;
using Extensions = TaleWorlds.Core.Extensions;

namespace AgricultureEstate
{
    public class AgricultureEstateBehavior : CampaignBehaviorBase
    {
        private static GauntletLayer layer;
        private static GauntletMovieIdentifier gauntletMovie;
        private static LandManagementVM landManagementVM;
        private static GauntletLayer layer2;
        private static GauntletMovieIdentifier gauntletMovie2;
        public static EstateListVM estateListVM;
        public static Dictionary<Settlement, VillageLand> VillageLands = new Dictionary<Settlement, VillageLand>();
        public static int LastDayTotalSales;
        private Random rng = new Random();

        public override void RegisterEvents()
        {
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, MenuItems);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, DailyTick);
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, HourlyTick);
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnNewGameCreated);
            this.setAdditionalPerkDescriptions();
        }

        private void OnNewGameCreated(CampaignGameStarter campaignGameStarter)
        {
            AgricultureEstateBehavior.VillageLands.Clear();
        }

        private void setAdditionalPerkDescriptions()
        {
            this.AddPerkDescription(DefaultPerks.Riding.MountedPatrols, new TextObject("{=agricultureestate_perk_mountedpatrols}Agriculture Estate : Slave escape chance reduced by 20%", null).ToString());
            this.AddPerkDescription(DefaultPerks.Steward.ForcedLabor, new TextObject("{=agricultureestate_perk_forcedlabor}Agriculture Estate : Allows use of non bandit prisoners for slave labor", null).ToString());
            this.AddPerkDescription(DefaultPerks.Roguery.Manhunter, new TextObject("{=agricultureestate_perk_slavetrader}Agriculture Estate : Estates buy slaves at 20% reduced cost", null).ToString());
            this.AddPerkDescription(DefaultPerks.Trade.InsurancePlans, new TextObject("{=agricultureestate_perk_insuranceplans}Agriculture Estate : Half of the cost of land siezed durring war is returned", null).ToString());
            this.AddPerkDescription(DefaultPerks.Trade.RapidDevelopment, new TextObject("{=agricultureestate_perk_rapiddevelopment}Agriculture Estate : Half of the cost of land siezed durring war is returned", null).ToString());
            this.AddPerkDescription(DefaultPerks.Trade.TradeyardForeman, new TextObject("{=agricultureestate_perk_tradeyardforeman}Agriculture Estate : Estates in villages that have primary production clay, iron, cotton, or silver has 20% increased slave output", null).ToString());
            this.AddPerkDescription(DefaultPerks.Trade.GranaryAccountant, new TextObject("{=agricultureestate_perk_granaryaccountant}Agriculture Estate : Estates in villages that have primary production grain, olives, fish, date has 20% increased slave output", null).ToString());
            this.AddPerkDescription(DefaultPerks.Riding.Breeder, new TextObject("{=agricultureestate_perk_breeder}Agriculture Estate : Estates in villages that have primary production horses has 10% increased slave output", null).ToString());
            this.AddPerkDescription(DefaultPerks.Steward.Contractors, new TextObject("{=agricultureestate_perk_contractors}Agriculture Estate : Upgrades in estates cost 15% less", null).ToString());
            this.AddPerkDescription(DefaultPerks.Engineering.Foreman, new TextObject("{=agricultureestate_perk_foreman}Agriculture Estate : Completing a land clearance project adds 30 hearths to the village", null).ToString());
        }

        private void AddPerkDescription(PerkObject perk, string description)
        {
            TextObject value = new TextObject(perk.Description.ToString() + "\n \n" + description, null);
            typeof(PropertyObject).GetField("_description", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).SetValue(perk, value);
        }

        public static float CalculateGold(VillageLand villageLand)
        {
            float num = 0f;
            Village village = villageLand.Village;
            float num2 = (villageLand.OwnedPlots == 0) ? 0f : Math.Max(0f, (float)((10.0 * (double)villageLand.OwnedPlots - (double)villageLand.Prisoners.TotalManCount) / (10.0 * (double)villageLand.OwnedPlots)));

            if (villageLand.OwnedPlots > 0 && (village == null || village.VillageState != (Village.VillageStates)4)) // Castolva, bár a 4 valószínűleg Looted/Raided
            {
                int tradeTax = village?.TradeTaxAccumulated ?? 0;
                num += (float)(tradeTax * (double)num2 / 100.0) * (float)villageLand.OwnedPlots * SubModule.LandRentScale;
            }
            return num;
        }

        private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
        {
            CharacterObject prisoner = this.getPrisoner(party);

            if (prisoner != null && AgricultureEstateBehavior.VillageLands.TryGetValue(settlement, out VillageLand villageLand))
            {

                if (party.ActualClan == Hero.MainHero.Clan && hero != Hero.MainHero && hero != null)
                {
                    int transferredCount = 0;
                    int capacity = villageLand.OwnedPlots * 10 - villageLand.Prisoners.TotalManCount;

                    while (prisoner != null && capacity > 0)
                    {
                        party.PrisonRoster.AddToCounts(prisoner, -1, false, 0, 0, true, -1);
                        villageLand.Prisoners.AddToCounts(prisoner, 1, false, 0, 0, true, -1);
                        prisoner = this.getPrisoner(party);
                        capacity--;
                        transferredCount++;
                    }

                    if (transferredCount > 0)
                    {
                        InformationManager.DisplayMessage(new InformationMessage(Localization.SetTextVariables("{=agricultureestate_lord_brought_prisoners}{LORD_NAME} transfered {PRIOSNER_COUNT} prisoners to your estate in {SETTLEMENT_NAME}", new KeyValuePair<string, string>[]
                        {
                            new KeyValuePair<string, string>("LORD_NAME", hero.Name.ToString()),
                            new KeyValuePair<string, string>("PRIOSNER_COUNT", transferredCount.ToString()),
                            new KeyValuePair<string, string>("SETTLEMENT_NAME", settlement.Name.ToString())
                        }).ToString()));
                    }
                }

                if (party.ActualClan != Hero.MainHero.Clan && villageLand.BuySlaves && hero != null)
                {
                    int boughtCount = 0;
                    int totalCost = 0;
                    int capacity = villageLand.OwnedPlots * 10 - villageLand.Prisoners.TotalManCount;

                    while (prisoner != null && capacity > 0 && totalCost + Campaign.Current.Models.RansomValueCalculationModel.PrisonerRansomValue(prisoner, hero) <= Hero.MainHero.Gold)
                    {
                        totalCost += Campaign.Current.Models.RansomValueCalculationModel.PrisonerRansomValue(prisoner, hero);
                        party.PrisonRoster.AddToCounts(prisoner, -1, false, 0, 0, true, -1);
                        villageLand.Prisoners.AddToCounts(prisoner, 1, false, 0, 0, true, -1);
                        capacity--;
                        boughtCount++;
                        prisoner = this.getPrisoner(party);
                    }

                    if (boughtCount > 0)
                    {
                        if (Hero.MainHero.GetPerkValue(DefaultPerks.Roguery.Manhunter))
                        {
                            totalCost = (int)(0.8 * (double)totalCost);
                        }
                        GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, hero, totalCost, false);
                        InformationManager.DisplayMessage(new InformationMessage(Localization.SetTextVariables("{=agricultureestate_lord_sold_prisoners}{LORD_NAME} sold {PRISONER_COUNT} prisoners to your estate in {SETTLEMENT_NAME} for {SELL_PRICE}{GOLD_ICON} gold", new KeyValuePair<string, string>[]
                        {
                            new KeyValuePair<string, string>("LORD_NAME", hero.Name.ToString()),
                            new KeyValuePair<string, string>("PRISONER_COUNT", boughtCount.ToString()),
                            new KeyValuePair<string, string>("SETTLEMENT_NAME", settlement.Name.ToString()),
                            new KeyValuePair<string, string>("SELL_PRICE", totalCost.ToString()),
                            new KeyValuePair<string, string>("GOLD_ICON", null)
                        }).ToString()));
                    }
                }
            }
        }

        private CharacterObject getPrisoner(MobileParty party)
        {
            if (party == null || party.PrisonRoster == null)
            {
                return null;
            }

            foreach (TroopRosterElement troopRosterElement in party.PrisonRoster.GetTroopRoster())
            {
                if (!troopRosterElement.Character.IsHero)
                {
                    return troopRosterElement.Character;
                }
            }
            return null;
        }

        private void DailyTick()
        {
            this.SlaveDeclineTick();
            this.CollectGoldTick();
            this.StartSlaveRebellionTick();
            this.RemoveInHostileTick();
        }

        private void HourlyTick()
        {
            this.productionTick();
            this.projectProgressTick();
            this.HandleMilitiaPatrols();
            this.UpdateExistingMilitiaParties();
        }

        private void HandleMilitiaPatrols()
        {
            foreach (var kvp in AgricultureEstateBehavior.VillageLands)
            {
                VillageLand land = kvp.Value;
                Village village = kvp.Key.Village;

                if (!land.AttackBandits || village.Settlement.Militia < 20 || IsMilitiaAlreadyActive(village))
                    continue;

                MobileParty bestTarget = null;
                float minDistance = 50f;

                foreach (var bandit in MobileParty.AllBanditParties)
                {
                    if (bandit.IsActive && bandit.MapEvent == null && bandit.CurrentSettlement == null)
                    {
                        float dist = bandit.GetPositionAsVec3().Distance(village.Settlement.GetPosition());

                        if (dist < minDistance)
                        {
                            float sendingMilitiaCount = village.Settlement.Militia * 0.5f;
                            if (sendingMilitiaCount < 10) continue;

                            if (sendingMilitiaCount > bandit.MemberRoster.TotalManCount * 1.2f)
                            {
                                bestTarget = bandit;
                                minDistance = dist;
                            }
                        }
                    }
                }

                if (bestTarget != null)
                {
                    SpawnMilitiaParty(land, bestTarget);
                }
            }
        }

        private bool IsMilitiaAlreadyActive(Village village)
        {
            foreach (var party in MobileParty.AllLordParties)
            {
                if (party.IsActive &&
                    party.PartyComponent is EstateMilitiaPartyComponent component &&
                    component.HomeSettlement == village.Settlement)
                {
                    return true;
                }
            }
            return false;
        }

        private void SpawnMilitiaParty(VillageLand land, MobileParty targetBandit)
        {
            if (IsMilitiaAlreadyActive(land.Village))
            {
                return;
            }

            int currentSlaves = land.Prisoners.TotalManCount;
            int maxCapacity = land.OwnedPlots * 10;

            if ((maxCapacity - currentSlaves) <= 5)
            {
                return;
            }

            Village village = land.Village;

            int troopsToSend = (int)(village.Settlement.Militia * 0.5f);

            if (troopsToSend < 5) return;

            village.Settlement.Militia -= troopsToSend;

            Settings instance = GlobalSettings<Settings>.Instance;
            float multiplier = (instance != null) ? instance.MilitiaStrengthMultiplier : 1.0f;

            int buffedTroopCount = (int)(troopsToSend * multiplier);

            var militiaComponent = new EstateMilitiaPartyComponent(land);

            MobileParty militiaParty = MobileParty.CreateParty(
                "militia_patrol_" + village.Name,
                militiaComponent
            );

            var militiaTroop = village.Settlement.Culture.MilitiaPartyTemplate.Stacks[0].Character;

            militiaParty.MemberRoster.AddToCounts(militiaTroop, buffedTroopCount);

            ItemObject grain = MBObjectManager.Instance.GetObject<ItemObject>("grain");
            if (grain != null)
            {
                int foodAmount = buffedTroopCount * 2;
                militiaParty.ItemRoster.AddToCounts(grain, foodAmount);
            }

            militiaParty.ActualClan = village.Settlement.OwnerClan;
            militiaParty.Ai.SetDoNotMakeNewDecisions(true);
            militiaParty.InitializeMobilePartyAroundPosition(new TroopRoster(militiaParty.Party), new TroopRoster(militiaParty.Party), village.Settlement.GatePosition, 1f);
            militiaParty.SetMoveEngageParty(targetBandit, MobileParty.NavigationType.Default);
            militiaComponent.CreationTime = CampaignTime.Now;
        }

        private void UpdateExistingMilitiaParties()
        {
            List<EstateMilitiaPartyComponent> militiasToUpdate = new List<EstateMilitiaPartyComponent>();

            foreach (MobileParty party in MobileParty.All)
            {
                if (party.IsActive && party.PartyComponent is EstateMilitiaPartyComponent militiaComponent)
                {
                    militiasToUpdate.Add(militiaComponent);
                }
            }

            foreach (var militia in militiasToUpdate)
            {
                militia.OnHourlyTick();
            }
        }

        private void RemoveInHostileTick()
        {
            Settings instance = GlobalSettings<Settings>.Instance;
            if (instance != null && !instance.DestroyPlotsOnWar)
            {
                foreach (KeyValuePair<Settlement, VillageLand> keyValuePair in AgricultureEstateBehavior.VillageLands)
                {
                    Village village = keyValuePair.Key.Village;
                    VillageLand value = keyValuePair.Value;

                    if (village.Owner.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction) && (value.OwnedPlots > 0 || value.OwnedUndevelopedPlots > 0))
                    {
                        InformationManager.DisplayMessage(new InformationMessage(Localization.SetTextVariables("{=agricultureestate_land_lost_due_to_war}Your lands in the village of {SETTLEMENT_NAME} has been siezed due to war with {WAR_TARGET}", new KeyValuePair<string, string>[]
                        {
                            new KeyValuePair<string, string>("SETTLEMENT_NAME", village.Name.ToString()),
                            new KeyValuePair<string, string>("WAR_TARGET", village.Owner.MapFaction.Name.ToString())
                        }).ToString()));

                        value.AvaliblePlots += value.OwnedPlots;

                        if (Hero.MainHero.GetPerkValue(DefaultPerks.Trade.RapidDevelopment) || Hero.MainHero.GetPerkValue(DefaultPerks.Trade.InsurancePlans))
                        {
                            Hero.MainHero.Gold += (int)((double)SubModule.PlotSellPrice / 1.25 * (double)value.OwnedPlots);
                        }
                        value.OwnedPlots = 0;

                        value.AvalibleUndevelopedPlots += value.OwnedUndevelopedPlots;

                        if (Hero.MainHero.GetPerkValue(DefaultPerks.Trade.RapidDevelopment) || Hero.MainHero.GetPerkValue(DefaultPerks.Trade.InsurancePlans))
                        {
                            Hero.MainHero.Gold += (int)((double)SubModule.UndevelopedPlotSellPrice / 1.25 * (double)value.OwnedUndevelopedPlots);
                        }
                        value.OwnedUndevelopedPlots = 0;
                        value.Prisoners = TroopRoster.CreateDummyTroopRoster();
                    }
                }
            }
        }

        private void StartSlaveRebellionTick()
        {
            foreach (KeyValuePair<Settlement, VillageLand> keyValuePair in AgricultureEstateBehavior.VillageLands)
            {
                Village village = keyValuePair.Key.Village;
                VillageLand value = keyValuePair.Value;

                if (value.Prisoners.TotalManCount >= 20 && rng.Next(1000) <= 10.0 * value.SlaveRevoltRisk)
                {
                    Hideout nearestHideout = SettlementHelper.FindNearestHideoutToSettlement(
                        village.Settlement,
                        MobileParty.NavigationType.Default
                    );

                    Clan banditClan = Clan.BanditFactions.FirstOrDefault() ?? Clan.All.FirstOrDefault(x => x.StringId == "looters");
                    if (banditClan == null) continue;

                    MobileParty mobileParty = BanditPartyComponent.CreateBanditParty(
                        village.Name.ToString() + " slave revolt",
                        banditClan,
                        nearestHideout,
                        false,
                        banditClan.DefaultPartyTemplate,
                        village.Settlement.GatePosition
                    );

                    mobileParty.InitializeMobilePartyAroundPosition(
                        new TroopRoster(mobileParty.Party),
                        new TroopRoster(mobileParty.Party),
                        village.Settlement.GatePosition,
                        1f,
                        0f
                    );

                    mobileParty.IsVisible = true;

                    while (mobileParty.MemberRoster.TotalManCount < 20 && value.Prisoners.TotalManCount > 0)
                    {
                        var randomElement = Extensions.GetRandomElement(value.Prisoners.GetTroopRoster());
                        if (randomElement.Character != null)
                        {
                            value.Prisoners.AddToCounts(randomElement.Character, -1, false, 0, 0, true, -1);
                            mobileParty.MemberRoster.AddToCounts(randomElement.Character, 1, false, 0, 0, true, -1);
                        }
                    }

                    InformationManager.ShowInquiry(new InquiryData(new TextObject("{=agricultureestate_slave_revolt_title}Slave Revolt", null).ToString(), Localization.SetTextVariables("{=agricultureestate_slave_revolt_description}The slave at your estate in the village of {SETTLEMENT_NAME} have revolted.", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("SETTLEMENT_NAME", village.Name.ToString()) }).ToString(), true, false, new TextObject("{=agricultureestate_slave_revolt_button_text}Not Good", null).ToString(), "", null, null, "", 0f, null, null, null), false, false);

                    mobileParty.SetMoveRaidSettlement(village.Settlement, MobileParty.NavigationType.Default);
                }
            }
        }

        private void projectProgressTick()
        {
            foreach (KeyValuePair<Settlement, VillageLand> keyValuePair in AgricultureEstateBehavior.VillageLands)
            {
                Village village = keyValuePair.Key.Village;
                VillageLand value = keyValuePair.Value;

                if (value.CurrentProject != "None")
                {
                    value.ProjectProgress++;

                    Settings instance = GlobalSettings<Settings>.Instance;
                    int projectTimeRequired = (instance != null) ? instance.ProjectTime * 24 : 24;

                    if (value.ProjectProgress >= projectTimeRequired)
                    {
                        if (value.CurrentProject == "Land Clearance")
                        {
                            if (value.OwnedUndevelopedPlots > 0)
                            {
                                value.OwnedUndevelopedPlots--;
                                value.OwnedPlots++;

                                if (Hero.MainHero.GetPerkValue(DefaultPerks.Engineering.Foreman))
                                {
                                    village.Hearth += 30f;
                                }
                            }
                        }
                        else if (value.CurrentProject == "Increase Patrols")
                        {
                            if (value.PatrolLevel < 8)
                            {
                                value.PatrolLevel++;
                            }
                        }
                        else if (value.CurrentProject == "Expand Storehouse")
                        {
                            value.StorageCapacity += 500;
                        }

                        value.ProjectProgress = 0;
                        if (value.ProjectQueue == null)
                        {
                            value.ProjectQueue = new Queue<string>();
                        }
                        value.CurrentProject = ((!Extensions.IsEmpty<string>(value.ProjectQueue)) ? value.ProjectQueue.Dequeue() : new TextObject("{=agricultureestate_none}None", null).ToString());
                    }
                }
            }
        }

        private void CollectGoldTick()
        {
            AgricultureEstateBehavior.LastDayTotalSales = 0;
            foreach (KeyValuePair<Settlement, VillageLand> keyValuePair in AgricultureEstateBehavior.VillageLands)
            {
                Village village = keyValuePair.Key.Village;
                VillageLand value = keyValuePair.Value;
                float num = 0f;
                float num2 = (value.OwnedPlots == 0) ? 0f : Math.Max(0f, (float)((10.0 * (double)value.OwnedPlots - (double)value.Prisoners.TotalManCount) / (10.0 * (double)value.OwnedPlots)));

                if (value.OwnedPlots > 0 && village.VillageState != (Village.VillageStates)4)
                {
                    num += (float)((double)village.TradeTaxAccumulated * (double)num2 / 100.0) * (float)value.OwnedPlots * SubModule.LandRentScale;
                }

                value.LastDayIncome = value.Gold + (int)num;
                AgricultureEstateBehavior.LastDayTotalSales += value.Gold;
                value.Gold = 0;
            }
        }

        private void SlaveDeclineTick()
        {
            foreach (KeyValuePair<Settlement, VillageLand> keyValuePair in AgricultureEstateBehavior.VillageLands)
            {
                Village village = keyValuePair.Key.Village;
                VillageLand value = keyValuePair.Value;

                if (value.Prisoners.TotalManCount > 0 && village.VillageState != (Village.VillageStates)1 && village.VillageState != (Village.VillageStates)4)
                {
                    this.SlaveDecline(value);
                }
            }
        }

        private void SlaveDecline(VillageLand land)
        {
            foreach (TroopRosterElement troopRosterElement in land.Prisoners.GetTroopRoster())
            {
                for (int i = 0; i < troopRosterElement.Number; i++)
                {
                    Settings instance = GlobalSettings<Settings>.Instance;
                    if (instance != null && instance.SlaveDeclineModifier == 0f)
                    {
                        return;
                    }

                    if (troopRosterElement.Character != null && land.Prisoners != null)
                    {
                        if ((double)this.rng.Next(1000) < (double)land.SlaveDeclineRate() * 10.0)
                        {
                            land.Prisoners.AddToCounts(troopRosterElement.Character, -1, false, 0, 0, true, -1);
                        }
                    }
                }
            }
        }

        private void productionTick()
        {
            foreach (KeyValuePair<Settlement, VillageLand> keyValuePair in AgricultureEstateBehavior.VillageLands)
            {
                Village village = keyValuePair.Key.Village;
                VillageLand value = keyValuePair.Value;
                try
                {
                    if (value.Prisoners.TotalManCount > 0 && village.VillageState != (Village.VillageStates)1 && village.VillageState != (Village.VillageStates)4)
                    {
                        foreach (ValueTuple<ItemObject, float> valueTuple in village.VillageType.Productions)
                        {
                            float productionBase = valueTuple.Item2 * 10f;

                            bool isFood = (village.VillageType.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("grain") || village.VillageType.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("olives") || village.VillageType.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("fish") || village.VillageType.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("date_fruit"));
                            bool isMaterial = (village.VillageType.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("clay") || village.VillageType.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("iron") || village.VillageType.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("cotton") || village.VillageType.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("silver"));

                            if (isFood && Hero.MainHero.GetPerkValue(DefaultPerks.Trade.GranaryAccountant))
                            {
                                this.produce(value.Prisoners.TotalManCount, valueTuple.Item1, 1.2f * productionBase, value);
                            }
                            else if (isMaterial && Hero.MainHero.GetPerkValue(DefaultPerks.Trade.TradeyardForeman))
                            {
                                this.produce(value.Prisoners.TotalManCount, valueTuple.Item1, 1.2f * productionBase, value);
                            }
                            else if (village.VillageType.PrimaryProduction.Type == ItemObject.ItemTypeEnum.Horse && Hero.MainHero.GetPerkValue(DefaultPerks.Riding.Breeder))
                            {
                                this.produce(value.Prisoners.TotalManCount, valueTuple.Item1, 1.1f * productionBase, value);
                            }
                            else
                            {
                                this.produce(value.Prisoners.TotalManCount, valueTuple.Item1, productionBase, value);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    InformationManager.DisplayMessage(new InformationMessage(string.Format("village: {0}", village.Name)));
                    InformationManager.DisplayMessage(new InformationMessage(string.Format("village.VillageType: {0}", village.VillageType)));
                    InformationManager.DisplayMessage(new InformationMessage(string.Format("village.VillageType.Productions: {0}", village.VillageType.Productions)));
                }
            }
        }

        private void produce(int slaves, ItemObject item, float productionChance, VillageLand land)
        {
            int currentStock = 0;
            foreach (ItemRosterElement itemRosterElement in land.Stockpile)
            {
                currentStock += itemRosterElement.Amount;
            }

            if (currentStock <= land.StorageCapacity || land.SellToMarket)
            {
                for (int i = 0; i < slaves; i++)
                {
                    if ((double)productionChance > (double)this.rng.Next((int)(10000.0 / (double)SubModule.SlaveProductionScale)))
                    {
                        if (land.SellToMarket)
                        {
                            Village village = land.Village;
                            if (village != null)
                            {
                                village.Settlement.ItemRoster.AddToCounts(item, 1);
                                int price = village.MarketData.GetPrice(item, MobileParty.MainParty, true, PartyBase.MainParty);
                                land.Gold += price;
                            }
                        }
                        else
                        {
                            land.Stockpile.AddToCounts(item, 1);
                        }

                        if (MobileParty.MainParty != null)
                        {
                            PartyBase party = MobileParty.MainParty.Party;
                            Village village2 = land.Village;
                            int price = (village2 != null) ? village2.MarketData.GetPrice(item, MobileParty.MainParty, true, PartyBase.MainParty) : 0;
                            SkillLevelingManager.OnTradeProfitMade(party, Math.Max(1, price / 10));
                        }
                    }
                }
            }
        }

        private void MenuItems(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("village", "village_land", new TextObject("{=agricultureestate_gamemenu_land_management}Land Management", null).ToString(), delegate (MenuCallbackArgs args)
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Manage;
                return true;
            }, delegate (MenuCallbackArgs args)
            {
                AgricultureEstateBehavior.CreateVMLayer(null);
            }, false, 1, false, null);
        }

        public static void CreateVMLayer(VillageLand village = null)
        {
            if (village == null)
            {
                if (Settlement.CurrentSettlement == null) return;
                village = AgricultureEstateBehavior.GetVillageLand(Settlement.CurrentSettlement);
            }

            try
            {
                if (AgricultureEstateBehavior.layer == null)
                {
                    TaleWorlds.Engine.GauntletUI.UIResourceManager.SpriteData.SpriteCategories["ui_town_management"].Load(
                    TaleWorlds.Engine.GauntletUI.UIResourceManager.ResourceContext,
                    TaleWorlds.Engine.GauntletUI.UIResourceManager.ResourceDepot);
                    AgricultureEstateBehavior.layer = new GauntletLayer("GauntletLayer", 1000, false);

                    AgricultureEstateBehavior.landManagementVM = new LandManagementVM(village);
                    AgricultureEstateBehavior.landManagementVM.RefreshValues();

                    AgricultureEstateBehavior.gauntletMovie = AgricultureEstateBehavior.layer.LoadMovie("LandManagement", AgricultureEstateBehavior.landManagementVM);

                    AgricultureEstateBehavior.layer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
                    ScreenManager.TopScreen.AddLayer(AgricultureEstateBehavior.layer);
                    AgricultureEstateBehavior.layer.IsFocusLayer = true;
                    ScreenManager.TrySetFocus(AgricultureEstateBehavior.layer);
                }
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage("Error creating VM Layer: " + ex.Message));
            }
        }

        public static void DeleteVMLayer()
        {
            ScreenBase topScreen = ScreenManager.TopScreen;
            if (AgricultureEstateBehavior.layer != null)
            {
                AgricultureEstateBehavior.layer.InputRestrictions.ResetInputRestrictions();
                AgricultureEstateBehavior.layer.IsFocusLayer = false;
                if (AgricultureEstateBehavior.gauntletMovie != null)
                {
                    AgricultureEstateBehavior.layer.ReleaseMovie(AgricultureEstateBehavior.gauntletMovie);
                }
                topScreen.RemoveLayer(AgricultureEstateBehavior.layer);
            }
            AgricultureEstateBehavior.layer = null;
            AgricultureEstateBehavior.gauntletMovie = null;
            AgricultureEstateBehavior.landManagementVM = null;
        }

        public static void CreateVMLayer2()
        {
            try
            {
                if (AgricultureEstateBehavior.layer2 == null)
                {
                    AgricultureEstateBehavior.layer2 = new GauntletLayer("GauntletLayer", 1200, false);
                    if (AgricultureEstateBehavior.estateListVM == null)
                    {
                        AgricultureEstateBehavior.estateListVM = new EstateListVM();
                    }
                    AgricultureEstateBehavior.estateListVM.RefreshValues();
                    AgricultureEstateBehavior.gauntletMovie2 = AgricultureEstateBehavior.layer2.LoadMovie("EstateList", AgricultureEstateBehavior.estateListVM);
                    AgricultureEstateBehavior.layer2.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
                    ScreenManager.TopScreen.AddLayer(AgricultureEstateBehavior.layer2);
                    AgricultureEstateBehavior.layer2.IsFocusLayer = true;
                    ScreenManager.TrySetFocus(AgricultureEstateBehavior.layer2);
                }
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage(ex.ToString()));
                Console.WriteLine(ex);
            }
        }

        public static void DeleteVMLayer2()
        {
            ScreenBase topScreen = ScreenManager.TopScreen;
            if (AgricultureEstateBehavior.layer2 != null)
            {
                AgricultureEstateBehavior.layer2.InputRestrictions.ResetInputRestrictions();
                AgricultureEstateBehavior.layer2.IsFocusLayer = false;
                if (AgricultureEstateBehavior.gauntletMovie2 != null)
                {
                    AgricultureEstateBehavior.layer2.ReleaseMovie(AgricultureEstateBehavior.gauntletMovie2);
                }
                topScreen.RemoveLayer(AgricultureEstateBehavior.layer2);
            }
            AgricultureEstateBehavior.layer2 = null;
            AgricultureEstateBehavior.gauntletMovie2 = null;
            AgricultureEstateBehavior.estateListVM = null;
        }

        public static VillageLand GetVillageLand(Settlement settlement)
        {
            if (AgricultureEstateBehavior.VillageLands.TryGetValue(settlement, out VillageLand villageLand))
            {
                return villageLand;
            }
            else
            {
                VillageLand newVillageLand = new VillageLand(settlement.Village);
                AgricultureEstateBehavior.VillageLands.Add(settlement, newVillageLand);
                return newVillageLand;
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (!dataStore.SyncData<Dictionary<Settlement, VillageLand>>("_village_land", ref AgricultureEstateBehavior.VillageLands))
            {
                AgricultureEstateBehavior.VillageLands.Clear();
            }

            Dictionary<Settlement, VillageLand> dictionary = new Dictionary<Settlement, VillageLand>();
            foreach (KeyValuePair<Settlement, VillageLand> keyValuePair in AgricultureEstateBehavior.VillageLands)
            {
                VillageLand value = keyValuePair.Value;
                if (value != null && value.Village != null && value.Village.Owner != null)
                {
                    dictionary.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
            AgricultureEstateBehavior.VillageLands = dictionary;
        }
    }
}