using AgricultureEstate.l18n;
using Helpers;
using MCM.Abstractions.Base.Global;
using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using Extensions = TaleWorlds.Core.Extensions;

namespace AgricultureEstate
{
    internal class LandManagementVM : ViewModel
    {
        private readonly int PlotBuyPrice = SubModule.PlotBuyPrice;
        private readonly int PlotSellPrice = SubModule.PlotSellPrice;
        private readonly int UndevelopedPlotBuyPrice = SubModule.UndevelopedPlotBuyPrice;
        private readonly int UndevelopedPlotSellPrice = SubModule.UndevelopedPlotSellPrice;
        private readonly int ProjectCost = SubModule.ProjectCost;

        private string _settlementImageID;
        private readonly VillageLand _village_land;

        public LandManagementVM(VillageLand villageLand)
        {
            _village_land = villageLand;
            if (_village_land.Prisoners == null)
            {
                _village_land.Prisoners = TroopRoster.CreateDummyTroopRoster();
            }
            if (_village_land.Stockpile == null)
            {
                _village_land.Stockpile = new ItemRoster();
            }
            if (_village_land.Village == null)
            {
                _village_land.Village = Settlement.CurrentSettlement.Village;
            }
            if (_village_land.StorageCapacity < 500)
            {
                _village_land.StorageCapacity = 500;
            }
            SetVillageIcon();
        }

        private void SetVillageIcon()
        {
            string cultureId = _village_land?.Village?.Settlement?.Culture?.StringId;

            if (cultureId == "sturgia")
                SettlementImageID = "gui_bg_village_sturgia_t";
            else if (cultureId == "vlandia")
                SettlementImageID = "gui_bg_village_vlanda_t";
            else if (cultureId == "khuzait")
                SettlementImageID = "gui_bg_village_khuzait_t";
            else if (cultureId == "aserai")
                SettlementImageID = "gui_bg_village_aserai_t";
            else if (cultureId == "battania")
                SettlementImageID = "gui_bg_village_battania_t";
            else
                SettlementImageID = "gui_bg_village_empire_t";
        }

        [DataSourceProperty]
        public bool SellToMarket
        {
            get => _village_land.SellToMarket;
            set
            {
                if (value != _village_land.SellToMarket)
                {
                    _village_land.SellToMarket = value;
                    OnPropertyChangedWithValue(value, "SellToMarket");
                }
            }
        }

        [DataSourceProperty]
        public bool BuySlaves
        {
            get => _village_land.BuySlaves;
            set
            {
                if (value != _village_land.BuySlaves)
                {
                    _village_land.BuySlaves = value;
                    OnPropertyChangedWithValue(value, "BuySlaves");
                }
            }
        }

        [DataSourceProperty]
        public string CurrentProjectIconSting
        {
            get
            {
                if (_village_land.CurrentProject == "Land Clearance") return "building_daily_irrigation";
                if (_village_land.CurrentProject == "Increase Patrols") return "building_settlement_roads_and_paths";
                return (_village_land.CurrentProject == "Expand Storehouse") ? "building_granary" : "building_default";
            }
        }

        [DataSourceProperty]
        public string CloseString => new TextObject("{=agricultureestate_ui_close}Close", null).ToString();

        [DataSourceProperty]
        public string UpgradeStorehouseString => new TextObject("{=agricultureestate_ui_upgrade_storehouse}Expand Storehouse", null).ToString();

        [DataSourceProperty]
        public string UpgradePatrolsString => new TextObject("{=agricultureestate_ui_upgrade_patrols}Increase Patrols", null).ToString();

        [DataSourceProperty]
        public string LandClearanceString => new TextObject("{=agricultureestate_ui_land_clearance}Land Clearance", null).ToString();

        [DataSourceProperty]
        public string ProjectQueueString => new TextObject("{=agricultureestate_ui_project_queue}Project Queue", null).ToString();

        [DataSourceProperty]
        public string CancelString => new TextObject("{=agricultureestate_ui_cancel}Cancel", null).ToString();

        [DataSourceProperty]
        public string SellToMarketTitleString => new TextObject("{=agricultureestate_ui_sell_to_market}Sell to Market : ", null).ToString();

        [DataSourceProperty]
        public string ProductionDetailsString => new TextObject("{=agricultureestate_ui_production_details}Production Details", null).ToString();

        [DataSourceProperty]
        public string BuySlavesTitleString => new TextObject("{=agricultureestate_ui_buy_slaves}Buy Slaves :", null).ToString();

        [DataSourceProperty]
        public string LandUsageTitleString => new TextObject("{=agricultureestate_ui_land_usage}Land Usage : ", null).ToString();

        [DataSourceProperty]
        public string RevoltRiskTitleString => new TextObject("{=agricultureestate_ui_revolt_risk}Revolt Risk : ", null).ToString();

        [DataSourceProperty]
        public string SlaveDeclineTitleString => new TextObject("{=agricultureestate_ui_slave_decline}Slave Decline : ", null).ToString();

        [DataSourceProperty]
        public string AvailableLandPlotsString => new TextObject("{=agricultureestate_ui_availablelandplots}Available : ", null).ToString();

        [DataSourceProperty]
        public string OwnedLandPlotsString => new TextObject("{=agricultureestate_ui_owned_landplots}Owned : ", null).ToString();

        [DataSourceProperty]
        public string LandPlotsString => new TextObject("{=agricultureestate_ui_landplots}Land Plots", null).ToString();

        [DataSourceProperty]
        public string UndevLandPlotsString => new TextObject("{=agricultureestate_ui_undev_landplots}Undeveloped Land Plots", null).ToString();

        [DataSourceProperty]
        public string LandManagementString => new TextObject("{=agricultureestate_ui_landmanagement}Land Management", null).ToString();

        [DataSourceProperty]
        public string LedgerString => new TextObject("{=agricultureestate_ui_ledger}Ledger", null).ToString();

        [DataSourceProperty]
        public string CurrentProjecProgressString
        {
            get
            {
                if (_village_land.CurrentProject != "None")
                {
                    Settings instance = GlobalSettings<Settings>.Instance;
                    int maxTime = (instance != null) ? instance.ProjectTime * 24 : 0;
                    return _village_land.ProjectProgress.ToString() + "/" + maxTime.ToString();
                }
                return "0/0";
            }
        }

        [DataSourceProperty]
        public string UpgradeString
        {
            get
            {
                return (_village_land.CurrentProject == "None")
                    ? string.Format("      {0}      ", new TextObject("{=agricultureestate_upgrade_string}Upgrade", null))
                    : string.Format("  {0}  ", new TextObject("{=agricultureestate_add_to_queue}Add to Queue", null));
            }
        }

        [DataSourceProperty]
        public string CurrentProjectString => _village_land.CurrentProjectL18N.ToString();

        [DataSourceProperty]
        public string SellToMarketString => SellToMarket ? new TextObject("{=agricultureestate_on}On", null).ToString() : new TextObject("{=agricultureestate_off}Off", null).ToString();

        [DataSourceProperty]
        public string BuyString => new TextObject("{=agricultureestate_ui_buy}Buy", null).ToString();

        [DataSourceProperty]
        public string SellString => new TextObject("{=agricultureestate_ui_sell}Sell", null).ToString();

        [DataSourceProperty]
        public string ManageString => new TextObject("{=agricultureestate_ui_manage}Manage", null).ToString();

        [DataSourceProperty]
        public string BuySlavesString => BuySlaves ? new TextObject("{=agricultureestate_on}On", null).ToString() : new TextObject("{=agricultureestate_off}Off", null).ToString();

        [DataSourceProperty]
        public string SellToMarketButton => SellToMarket ? "ButtonBrush1" : "ButtonBrush2";

        [DataSourceProperty]
        public string BuySlavesButton => BuySlaves ? "ButtonBrush1" : "ButtonBrush2";

        [DataSourceProperty]
        public string AttackBanditsTitleString => new TextObject("{=ae_ui_attack_bandits_title}Attack Bandits: ", null).ToString();

        [DataSourceProperty]
        public string AttackBanditsString => AttackBandits ? new TextObject("{=agricultureestate_on}On", null).ToString() : new TextObject("{=agricultureestate_off}Off", null).ToString();

        [DataSourceProperty]
        public string AttackBanditsButton => AttackBandits ? "ButtonBrush1" : "ButtonBrush2";

        [DataSourceProperty]
        public bool AttackBandits
        {
            get => _village_land.AttackBandits;
            set
            {
                if (value != _village_land.AttackBandits)
                {
                    _village_land.AttackBandits = value;
                    OnPropertyChangedWithValue(value, "AttackBandits");
                    OnPropertyChanged("AttackBanditsString"); // Frissíti a szöveget
                    OnPropertyChanged("AttackBanditsButton"); // Frissíti a gomb színét
                }
            }
        }

        [DataSourceProperty]
        public string UpgradesTitle => new TextObject("{=agricultureestate_ui_upgrades}Upgrades", null).ToString();

        [DataSourceProperty]
        public string SlavesTitle => new TextObject("{=agricultureestate_ui_slaves}Slaves", null).ToString();

        [DataSourceProperty]
        public string CapacityText => new TextObject("{=agricultureestate_ui_capacity}Capacity : ", null).ToString();

        [DataSourceProperty]
        public string StockpileTitle => new TextObject("{=agricultureestate_ui_stockpile}Stockpile", null).ToString();

        [DataSourceProperty]
        public string CurrentProjectTitle => new TextObject("{=agricultureestate_ui_current_project}Current Project", null).ToString();

        [DataSourceProperty]
        public string ProgressTitle => new TextObject("{=agricultureestate_ui_progress}Progress : ", null).ToString();

        [DataSourceProperty]
        public string SettlementImageID
        {
            get => _settlementImageID ?? "";
            set
            {
                if (value != _settlementImageID)
                {
                    _settlementImageID = value;
                    OnPropertyChangedWithValue<string>(value, "SettlementImageID");
                }
            }
        }

        [DataSourceProperty]
        public int AvaliblePlots
        {
            get => _village_land.AvaliblePlots;
            set
            {
                if (value != _village_land.AvaliblePlots)
                {
                    _village_land.AvaliblePlots = value;
                    OnPropertyChangedWithValue(value, "AvaliblePlots");
                }
            }
        }

        [DataSourceProperty]
        public string SlaveCapacityString => _village_land.Prisoners.TotalManCount.ToString() + "/" + (10 * OwnedPlots).ToString();

        [DataSourceProperty]
        public float RevoltRisk => _village_land.SlaveRevoltRisk;

        [DataSourceProperty]
        public string RevoltRiskString => RevoltRisk.ToString("0.0") + "%";

        [DataSourceProperty]
        public float SlaveDecline => _village_land.SlaveDeclineRate();

        [DataSourceProperty]
        public string SlaveDeclineString => SlaveDecline.ToString("0.0") + "%";

        [DataSourceProperty]
        public string LandUsageString
        {
            get
            {
                float usage = (_village_land.OwnedPlots == 0) ? 0f : Math.Max(0f, (float)(100 * _village_land.Prisoners.TotalManCount) / (10f * _village_land.OwnedPlots));
                return string.Format("{0:0.0}", usage) + "%";
            }
        }

        [DataSourceProperty]
        public string StockpileCapacityString
        {
            get
            {
                int num = 0;
                foreach (ItemRosterElement itemRosterElement in _village_land.Stockpile)
                {
                    num += itemRosterElement.Amount;
                }
                return num.ToString() + "/" + _village_land.StorageCapacity.ToString();
            }
        }

        [DataSourceProperty]
        public string AvaliblePlotsString => AvaliblePlots.ToString();

        [DataSourceProperty]
        public int OwnedPlots
        {
            get => _village_land.OwnedPlots;
            set
            {
                if (value != _village_land.OwnedPlots)
                {
                    _village_land.OwnedPlots = value;
                    OnPropertyChangedWithValue(value, "OwnedPlots");
                }
            }
        }

        [DataSourceProperty]
        public string OwnedPlotsString => OwnedPlots.ToString();

        [DataSourceProperty]
        public int AvalibleUndevelopedPlots
        {
            get => _village_land.AvalibleUndevelopedPlots;
            set
            {
                if (value != _village_land.AvalibleUndevelopedPlots)
                {
                    _village_land.AvalibleUndevelopedPlots = value;
                    OnPropertyChangedWithValue(value, "AvalibleUndevelopedPlots");
                }
            }
        }

        [DataSourceProperty]
        public string AvalibleUndevelopedPlotsString => AvalibleUndevelopedPlots.ToString();

        [DataSourceProperty]
        public int OwnedUndevelopedPlots
        {
            get => _village_land.OwnedUndevelopedPlots;
            set
            {
                if (value != _village_land.OwnedUndevelopedPlots)
                {
                    _village_land.OwnedUndevelopedPlots = value;
                    OnPropertyChangedWithValue(value, "OwnedUndevelopedPlots");
                }
            }
        }

        [DataSourceProperty]
        public string OwnedUndevelopedPlotsString => OwnedUndevelopedPlots.ToString();

        public void Close()
        {
            AgricultureEstateBehavior.DeleteVMLayer();
        }

        public void ManageSlaves()
        {
            if (Input.IsKeyDown(InputKey.LeftShift) || Input.IsKeyDown(InputKey.RightShift))
            {
                int capacity = _village_land.OwnedPlots * 10 - _village_land.Prisoners.TotalManCount;
                CharacterObject bandit = GetBandit(MobileParty.MainParty);
                while (bandit != null && capacity > 0)
                {
                    MobileParty.MainParty.PrisonRoster.AddToCounts(bandit, -1, false, 0, 0, true, -1);
                    _village_land.Prisoners.AddToCounts(bandit, 1, false, 0, 0, true, -1);
                    bandit = GetBandit(MobileParty.MainParty);
                    capacity--;
                }
                AgricultureEstateBehavior.DeleteVMLayer();
                AgricultureEstateBehavior.CreateVMLayer(_village_land);
            }
            else
            {
                PartyScreenLogic partyScreenLogic = new PartyScreenLogic();
                try
                {
                    TroopRoster dummyRoster = TroopRoster.CreateDummyTroopRoster();
                    foreach (TroopRosterElement troopRosterElement in _village_land.Prisoners.GetTroopRoster())
                    {
                        dummyRoster.AddToCounts(troopRosterElement.Character, troopRosterElement.Number, false, 0, 0, true, -1);
                    }

                    PartyScreenLogicInitializationData data = default;
                    data.LeftOwnerParty = null;
                    data.RightOwnerParty = PartyBase.MainParty;
                    data.LeftMemberRoster = TroopRoster.CreateDummyTroopRoster();
                    data.LeftPrisonerRoster = dummyRoster;
                    data.RightMemberRoster = PartyBase.MainParty.MemberRoster;
                    data.RightPrisonerRoster = PartyBase.MainParty.PrisonRoster;
                    data.LeftLeaderHero = null;
                    data.RightLeaderHero = PartyBase.MainParty.LeaderHero;

                    data.LeftPartyMembersSizeLimit = 0;
                    data.LeftPartyPrisonersSizeLimit = _village_land.OwnedPlots * 10;

                    data.RightPartyMembersSizeLimit = PartyBase.MainParty.PartySizeLimit;
                    data.RightPartyPrisonersSizeLimit = PartyBase.MainParty.PrisonerSizeLimit;
                    data.LeftPartyName = new TextObject("Slaves", null);
                    data.RightPartyName = PartyBase.MainParty.Name;

                    data.PartyPresentationDoneButtonDelegate = new PartyPresentationDoneButtonDelegate(ManageDone);

                    data.IsDismissMode = false;

                    data.IsTroopUpgradesDisabled = false;
                    data.Header = new TextObject("Manage Slaves", null);
                    data.TransferHealthiesGetWoundedsFirst = false;
                    data.ShowProgressBar = false;

                    data.MemberTransferState = PartyScreenLogic.TransferState.NotTransferable;
                    data.PrisonerTransferState = PartyScreenLogic.TransferState.Transferable;
                    data.AccompanyingTransferState = PartyScreenLogic.TransferState.NotTransferable;

                    partyScreenLogic.Initialize(data);

                    PartyState partyState = Game.Current.GameStateManager.CreateState<PartyState>();

                    partyState.GetType().GetProperty("PartyScreenLogic", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                              .SetValue(partyState, partyScreenLogic);

                    Game.Current.GameStateManager.PushState(partyState, 0);
                }
                catch (Exception ex)
                {
                    InformationManager.DisplayMessage(new InformationMessage("Error opening slave menu: " + ex.Message));
                }
            }
        }

        private CharacterObject GetBandit(MobileParty party)
        {
            foreach (TroopRosterElement troopRosterElement in party.PrisonRoster.GetTroopRoster())
            {
                bool isBanditOrForcedLabor = (int)troopRosterElement.Character.Occupation == 15 || Hero.MainHero.GetPerkValue(DefaultPerks.Steward.ForcedLabor);
                if (isBanditOrForcedLabor && !troopRosterElement.Character.IsHero)
                {
                    return troopRosterElement.Character;
                }
            }
            return null;
        }

        private bool TroopTransferableDelegate(CharacterObject character, PartyScreenLogic.TroopType type, PartyScreenLogic.PartyRosterSide side, PartyBase LeftOwnerParty)
        {
            bool isBanditOrForcedLabor = (int)character.Occupation == 15 || Hero.MainHero.GetPerkValue(DefaultPerks.Steward.ForcedLabor);
            return !character.IsHero && type != PartyScreenLogic.TroopType.Member && type == PartyScreenLogic.TroopType.Prisoner && isBanditOrForcedLabor;
        }

        private bool ManageDone(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, FlattenedTroopRoster takenPrisonerRoster, FlattenedTroopRoster releasedPrisonerRoster, bool isForced, PartyBase leftParty = null, PartyBase rightParty = null)
        {
            if (leftPrisonRoster.TotalManCount > _village_land.OwnedPlots * 10)
            {
                InformationManager.DisplayMessage(new InformationMessage(Localization.SetTextVariables("{=agricultureestate_slave_capacity_exceeded}Slave Capacity of {SLAVE_CAPACITY} exceeded", new KeyValuePair<string, string>[]
                {
            new KeyValuePair<string, string>("SLAVE_CAPACITY", (_village_land.OwnedPlots * 10).ToString())
                }).ToString()));
                return false;
            }
            else
            {
                _village_land.Prisoners = leftPrisonRoster;
                OnPropertyChanged("SlaveCapacityString");
                OnPropertyChanged("LandUsageString");
                OnPropertyChanged("RevoltRiskString");
                OnPropertyChanged("SlaveDeclineString");

                return true;
            }
        }

        public void ManageStockpile()
        {
            bool flag = Input.IsKeyDown(InputKey.LeftShift) || Input.IsKeyDown(InputKey.RightShift);
                if (flag)
            {
                foreach (ItemRosterElement itemRosterElement in _village_land.Stockpile)
                {
                    ItemRoster itemRoster = MobileParty.MainParty.ItemRoster;
                    ItemObject item = itemRosterElement.EquipmentElement.Item;
                    int amount = itemRosterElement.Amount;
                    itemRoster.AddToCounts(item, amount);
                }
                _village_land.Stockpile = new ItemRoster();
                AgricultureEstateBehavior.DeleteVMLayer();
                AgricultureEstateBehavior.CreateVMLayer(_village_land);
            }
            else
            {
                InventoryScreenHelper.OpenScreenAsStash(_village_land.Stockpile);
            }
        }

        public void Click1()
        {
            if (Hero.MainHero.Gold >= PlotBuyPrice && AvaliblePlots > 0)
            {
                Hero mainHero = Hero.MainHero;
                Settlement settlement = _village_land?.Village?.Settlement;

                GiveGoldAction.ApplyForCharacterToSettlement(mainHero, settlement, PlotBuyPrice, false);

                _village_land.AvaliblePlots--;
                _village_land.OwnedPlots++;

                AgricultureEstateBehavior.DeleteVMLayer();
                AgricultureEstateBehavior.CreateVMLayer(_village_land);
            }
        }

        public void Click2()
        {
            if (OwnedPlots > 0)
            {
                if (_village_land.Prisoners.TotalManCount > (_village_land.OwnedPlots - 1) * 10)
                {
                    InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=agricultureestate_slave_capacity_exceed_when_land_sold}Selling land will cause slaves to exceede capacity. Remove slaves before selling", null).ToString()));
                }
                else
                {
                    _village_land?.Village?.ChangeGold(PlotSellPrice);
                    Settlement settlement = _village_land?.Village?.Settlement;

                    GiveGoldAction.ApplyForSettlementToCharacter(settlement, Hero.MainHero, PlotSellPrice, false);

                    _village_land.AvaliblePlots++;
                    _village_land.OwnedPlots--;

                    AgricultureEstateBehavior.DeleteVMLayer();
                    AgricultureEstateBehavior.CreateVMLayer(_village_land);
                }
            }
        }

        public void Click3()
        {
            if (Hero.MainHero.Gold >= UndevelopedPlotBuyPrice && AvalibleUndevelopedPlots > 0)
            {
                Hero mainHero = Hero.MainHero;
                Settlement settlement = _village_land?.Village?.Settlement;

                GiveGoldAction.ApplyForCharacterToSettlement(mainHero, settlement, UndevelopedPlotBuyPrice, false);

                _village_land.AvalibleUndevelopedPlots--;
                _village_land.OwnedUndevelopedPlots++;

                AgricultureEstateBehavior.DeleteVMLayer();
                AgricultureEstateBehavior.CreateVMLayer(_village_land);
            }
        }

        public void Click4()
        {
            if (OwnedUndevelopedPlots > 0)
            {
                int activeClearanceCount = 0;
                if (_village_land.CurrentProject == "Land Clearance") activeClearanceCount++;
                foreach (string project in _village_land.ProjectQueue)
                {
                    if (project == "Land Clearance") activeClearanceCount++;
                }

                if (activeClearanceCount >= OwnedUndevelopedPlots)
                {
                    InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=agricultureestate_tried_selling_clearing_land}All owned land is being cleared.\nCancel land clearance project before selling", null).ToString()));
                }
                else
                {
                    _village_land?.Village?.ChangeGold(UndevelopedPlotSellPrice);
                    Settlement settlement = _village_land?.Village?.Settlement;

                    GiveGoldAction.ApplyForSettlementToCharacter(settlement, Hero.MainHero, UndevelopedPlotSellPrice, false);

                    _village_land.AvalibleUndevelopedPlots++;
                    _village_land.OwnedUndevelopedPlots--;

                    AgricultureEstateBehavior.DeleteVMLayer();
                    AgricultureEstateBehavior.CreateVMLayer(_village_land);
                }
            }
        }

        public void Click5()
        {
            if (Input.IsKeyDown(InputKey.LeftShift))
            {
                foreach (KeyValuePair<Settlement, VillageLand> keyValuePair in AgricultureEstateBehavior.VillageLands)
                {
                    keyValuePair.Value.SellToMarket = !SellToMarket;
                }
            }
            else
            {
                SellToMarket = !SellToMarket;
            }
            AgricultureEstateBehavior.DeleteVMLayer();
            AgricultureEstateBehavior.CreateVMLayer(_village_land);
        }

        public void Click6()
        {
            double cost = (Hero.MainHero.GetPerkValue(DefaultPerks.Steward.Contractors) ? 0.85 : 1.0) * (double)ProjectCost;
            if ((double)Hero.MainHero.Gold < cost)
            {
                InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=agricultureestate_not_enough_gold}Not enough gold", null).ToString()));
            }
            else
            {
                int limit = 10 + Hero.MainHero.GetSkillValue(DefaultSkills.Steward) / 10;
                if (_village_land.ProjectQueue.Count >= limit)
                {
                    InformationManager.DisplayMessage(new InformationMessage(Localization.SetTextVariables("{=agricultureestate_project_queue_limit}Project queue limit {QUEUE_LIMIT}\nIncrease steward skill to increase project queue limit", new KeyValuePair<string, string>[]
                    {
                        new KeyValuePair<string, string>("QUEUE_LIMIT", limit.ToString())
                    }).ToString()));
                }
                else
                {
                    int activeClearance = 0;
                    if (_village_land.CurrentProject == "Land Clearance") activeClearance++;
                    foreach (string p in _village_land.ProjectQueue)
                    {
                        if (p == "Land Clearance") activeClearance++;
                    }

                    if (activeClearance >= _village_land.OwnedUndevelopedPlots)
                    {
                        InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=agricultureestate_not_enough_undev_plots}Not enough owned undeveloped plots", null).ToString()));
                    }
                    else
                    {
                        if (_village_land.CurrentProject == "None")
                        {
                            _village_land.CurrentProject = "Land Clearance";
                        }
                        else
                        {
                            _village_land.ProjectQueue.Enqueue("Land Clearance");
                        }

                        GiveGoldAction.ApplyForCharacterToSettlement(Hero.MainHero, _village_land?.Village?.Settlement, (int)cost, false);
                        AgricultureEstateBehavior.DeleteVMLayer();
                        AgricultureEstateBehavior.CreateVMLayer(_village_land);
                    }
                }
            }
        }

        public void Click7()
        {
            double cost = (Hero.MainHero.GetPerkValue(DefaultPerks.Steward.Contractors) ? 0.85 : 1.0) * (double)ProjectCost;
            if ((double)Hero.MainHero.Gold < cost)
            {
                InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=agricultureestate_not_enough_gold}Not enough gold", null).ToString()));
            }
            else
            {
                int limit = 10 + Hero.MainHero.GetSkillValue(DefaultSkills.Steward) / 10;
                if (_village_land.ProjectQueue.Count >= limit)
                {
                    InformationManager.DisplayMessage(new InformationMessage(Localization.SetTextVariables("{=agricultureestate_project_queue_limit}Project queue limit {QUEUE_LIMIT}\nIncrease steward skill to increase project queue limit", new KeyValuePair<string, string>[]
                    {
                        new KeyValuePair<string, string>("QUEUE_LIMIT", limit.ToString())
                    }).ToString()));
                }
                else
                {
                    int currentLevel = _village_land.PatrolLevel;
                    if (_village_land.CurrentProject == "Increase Patrols") currentLevel++;
                    foreach (string p in _village_land.ProjectQueue)
                    {
                        if (p == "Increase Patrols") currentLevel++;
                    }

                    if (currentLevel >= 8)
                    {
                        InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=agricultureestate_max_patrol_upgrade}Can only be upgraded a max of 8 times", null).ToString()));
                    }
                    else
                    {
                        if (_village_land.CurrentProject == "None")
                        {
                            _village_land.CurrentProject = "Increase Patrols";
                        }
                        else
                        {
                            _village_land.ProjectQueue.Enqueue("Increase Patrols");
                        }

                        GiveGoldAction.ApplyForCharacterToSettlement(Hero.MainHero, _village_land?.Village?.Settlement, (int)cost, false);
                        AgricultureEstateBehavior.DeleteVMLayer();
                        AgricultureEstateBehavior.CreateVMLayer(_village_land);
                    }
                }
            }
        }

        public void Click8()
        {
            double cost = (Hero.MainHero.GetPerkValue(DefaultPerks.Steward.Contractors) ? 0.85 : 1.0) * (double)ProjectCost;
            if ((double)Hero.MainHero.Gold < cost)
            {
                InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=agricultureestate_not_enough_gold}Not enough gold", null).ToString()));
            }
            else
            {
                int limit = 10 + Hero.MainHero.GetSkillValue(DefaultSkills.Steward) / 10;
                if (_village_land.ProjectQueue.Count >= limit)
                {
                    InformationManager.DisplayMessage(new InformationMessage(Localization.SetTextVariables("{=agricultureestate_project_queue_limit}Project queue limit {QUEUE_LIMIT}\nIncrease steward skill to increase project queue limit", new KeyValuePair<string, string>[]
                    {
                        new KeyValuePair<string, string>("QUEUE_LIMIT", limit.ToString())
                    }).ToString()));
                }
                else
                {
                    if (_village_land.CurrentProject == "None")
                    {
                        _village_land.CurrentProject = "Expand Storehouse";
                    }
                    else
                    {
                        _village_land.ProjectQueue.Enqueue("Expand Storehouse");
                    }

                    GiveGoldAction.ApplyForCharacterToSettlement(Hero.MainHero, _village_land?.Village?.Settlement, (int)cost, false);
                    AgricultureEstateBehavior.DeleteVMLayer();
                    AgricultureEstateBehavior.CreateVMLayer(_village_land);
                }
            }
        }

        public void Click9()
        {
            _village_land.CurrentProject = "None";
            if (!Extensions.IsEmpty<string>(_village_land.ProjectQueue))
            {
                _village_land.CurrentProject = _village_land.ProjectQueue.Dequeue();
            }
            AgricultureEstateBehavior.DeleteVMLayer();
            AgricultureEstateBehavior.CreateVMLayer(_village_land);
        }

        public void Click10()
        {
            if (BuySlaves)
            {
                if (Input.IsKeyDown(InputKey.LeftShift) || Input.IsKeyDown(InputKey.RightShift))
                {
                    foreach (KeyValuePair<Settlement, VillageLand> keyValuePair in AgricultureEstateBehavior.VillageLands)
                    {
                        keyValuePair.Value.BuySlaves = false;
                    }
                }
                else
                {
                    BuySlaves = false;
                }
            }
            else
            {
                if (Input.IsKeyDown(InputKey.LeftShift) || Input.IsKeyDown(InputKey.RightShift))
                {
                    foreach (KeyValuePair<Settlement, VillageLand> keyValuePair2 in AgricultureEstateBehavior.VillageLands)
                    {
                        keyValuePair2.Value.BuySlaves = true;
                    }
                }
                else
                {
                    BuySlaves = true;
                }
            }
            AgricultureEstateBehavior.DeleteVMLayer();
            AgricultureEstateBehavior.CreateVMLayer(_village_land);
        }

        public void Click10b()
        {
            AttackBandits = !AttackBandits;

            AgricultureEstateBehavior.DeleteVMLayer();
            AgricultureEstateBehavior.CreateVMLayer(_village_land);
        }

        public void Click11()
        {
            ExecuteEndHint();
            AgricultureEstateBehavior.CreateVMLayer2();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
        }

        public void ExecuteBeginHint1()
        {
            MBInformationManager.ShowHint(new TextObject("{=agricultureestate_hint_land_suitable}Land suitable to agriculture and resource extraction", null).ToString());
        }

        public void ExecuteBeginHint2()
        {
            MBInformationManager.ShowHint(new TextObject("{=agricultureestate_hint_land_needs_clearing}Land that needs to be cleared before it can be put to use", null).ToString());
        }

        public void ExecuteBeginHint3()
        {
            MBInformationManager.ShowHint(Localization.SetTextVariables("{=agricultureestate_hint_buy_land}Buy for {PLOT_BUY_PRICE}{GOLD_ICON} gold", new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("PLOT_BUY_PRICE", PlotBuyPrice.ToString()),
                new KeyValuePair<string, string>("GOLD_ICON", null)
            }).ToString());
        }

        public void ExecuteBeginHint4()
        {
            MBInformationManager.ShowHint(Localization.SetTextVariables("{=agricultureestate_hint_sell_land}Sell for {PLOT_SELL_PRICE}{GOLD_ICON} gold", new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("PLOT_SELL_PRICE", PlotSellPrice.ToString()),
                new KeyValuePair<string, string>("GOLD_ICON", null)
            }).ToString());
        }

        public void ExecuteBeginHint5()
        {
            MBInformationManager.ShowHint(Localization.SetTextVariables("{=agricultureestate_hint_buy_undev_land}Buy for {PLOT_UNDEV_BUY_PRICE}{GOLD_ICON} gold", new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("PLOT_UNDEV_BUY_PRICE", UndevelopedPlotBuyPrice.ToString()),
                new KeyValuePair<string, string>("GOLD_ICON", null)
            }).ToString());
        }

        public void ExecuteBeginHint6()
        {
            MBInformationManager.ShowHint(Localization.SetTextVariables("{=agricultureestate_hint_sell_undev_land}Sell for {PLOT_UNDEV_SELL_PRICE}{GOLD_ICON} gold", new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("PLOT_UNDEV_SELL_PRICE", UndevelopedPlotSellPrice.ToString()),
                new KeyValuePair<string, string>("GOLD_ICON", null)
            }).ToString());
        }

        public void ExecuteBeginHint7()
        {
            MBInformationManager.ShowHint(new TextObject("{=agricultureestate_hint_prisoner_as_slaves}Bandits prisoners can be used as labor.\nCapacity for slaves determined by number of owned plots\nShift click to quick deposit", null).ToString());
        }

        public void ExecuteBeginHint8()
        {
            MBInformationManager.ShowHint(new TextObject("{=agricultureestate_hint_slave_decline}Slave decline is daily chance for each slaves to escape.\nRevolt risk is daily chance for slaves to violently rebel in mass.\nRevolt risk is increase if slaves outnumber the village militia\nBuilding upgrades can improve these number.\nLand not used by slave labor will generate a small amount of land rent every day", null).ToString());
        }

        public void ExecuteBeginHint9()
        {
            string text = new TextObject("{=agricultureestate_estimated_daily_output}Estimated Daily Output", null).ToString();
            if (_village_land.Village != null)
            {
                foreach (ValueTuple<ItemObject, float> valueTuple in _village_land.Village.VillageType.Productions)
                {
                    if ((double)valueTuple.Item2 > 0.0 && _village_land.Prisoners.TotalManCount > 0)
                    {
                        float multiplier = 1f;
                        bool isFood = (_village_land.Village.VillageType.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("grain") || _village_land.Village.VillageType.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("olives") || _village_land.Village.VillageType.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("fish") || _village_land.Village.VillageType.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("date_fruit"));
                        bool isMaterial = (_village_land.Village.VillageType.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("clay") || _village_land.Village.VillageType.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("iron") || _village_land.Village.VillageType.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("cotton") || _village_land.Village.VillageType.PrimaryProduction == MBObjectManager.Instance.GetObject<ItemObject>("silver"));

                        if (isFood && Hero.MainHero.GetPerkValue(DefaultPerks.Trade.GranaryAccountant))
                        {
                            multiplier = 1.2f;
                        }
                        else if (isMaterial && Hero.MainHero.GetPerkValue(DefaultPerks.Trade.TradeyardForeman))
                        {
                            multiplier = 1.2f;
                        }
                        else if (_village_land.Village.VillageType.PrimaryProduction.Type == ItemObject.ItemTypeEnum.Horse && Hero.MainHero.GetPerkValue(DefaultPerks.Riding.Breeder))
                        {
                            multiplier = 1.1f;
                        }

                        text += new TextObject("{=agricultureestate_estimated_daily_output_entry}{NEWLINE}{MULTIPLIER} X {ITEM}", new Dictionary<string, object>
                        {
                            {
                                "MULTIPLIER",
                                string.Format("{0:0.00}", (float)((double)multiplier * 24.0 * (double)SubModule.SlaveProductionScale * (double)_village_land.Prisoners.TotalManCount * (double)valueTuple.Item2 / 1000.0))
                            },
                            {
                                "ITEM",
                                valueTuple.Item1.Name
                            }
                        }).ToString();
                    }
                }

                float usage = (_village_land.OwnedPlots == 0) ? 0f : Math.Max(0f, (float)((10.0 * (double)_village_land.OwnedPlots - (double)_village_land.Prisoners.TotalManCount) / (10.0 * (double)_village_land.OwnedPlots)));
                float rent = (float)((double)_village_land.Village.TradeTaxAccumulated * usage / 100.0) * (float)_village_land.OwnedPlots * SubModule.LandRentScale;

                string str = "\n";
                TextObject textObject = new TextObject("{=agricultureestate_estimated_daily_output_rent}{MULTIPLIER}{GOLD_ICON} X Land Rent", new Dictionary<string, object>
                {
                    {
                        "MULTIPLIER",
                        string.Format("{0:0.00}", rent)
                    }
                });
                MBInformationManager.ShowHint(text + str + (textObject?.ToString()));
            }
        }

        public void ExecuteBeginHint10()
        {
            MBInformationManager.ShowHint(new TextObject("{=agricultureestate_hint_production}Goods produced will be added to the stockpile and can be withdrawn at a later time.\n  If storage capacity is exceeded, then production will stop.\nShift click to quick withdrawl all", null).ToString());
        }

        public void ExecuteBeginHint11()
        {
            MBInformationManager.ShowHint(new TextObject("{=agricultureestate_hint_sell_goods_to_village}Goods produce can be set to be automatically sold to the village market.\nBe aware the the village market tend to pay less for goods than town markets\nClick to turn {STATUS}", null).SetTextVariable("STATUS", SellToMarket ? new TextObject("{=agricultureestate_off}Off", null).ToString() : new TextObject("{=agricultureestate_on}On", null).ToString()).ToString());
        }

        public void ExecuteBeginHint12()
        {
            MBInformationManager.ShowHint(Localization.SetTextVariables("{=agricultureestate_hint_land_clearing}Land Clearance will convert 1 owned undeveloped plot into a normal plot\nCost: {LAND_CLEARING_COST}{GOLD_ICON}\nEach additional plot of land cleared provides a small increase to village growth rate\nTime: 240 hours", new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("LAND_CLEARING_COST", ((Hero.MainHero.GetPerkValue(DefaultPerks.Steward.Contractors) ? 0.85f : 1f) * (float)ProjectCost).ToString()),
                new KeyValuePair<string, string>("GOLD_ICON", null)
            }).ToString());
        }

        public void ExecuteBeginHint13()
        {
            MBInformationManager.ShowHint(Localization.SetTextVariables("{=agricultureestate_hint_patrol_upgrade}Increasing patrol decreases escape chance by 0.5% and revolt risk by 0.1% per level.  This Upgrade can be done a max of 8 times\nCost: {PATROL_UPGRADE_COST}{GOLD_ICON}\nTime: 240 hours", new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("PATROL_UPGRADE_COST", ((Hero.MainHero.GetPerkValue(DefaultPerks.Steward.Contractors) ? 0.85f : 1f) * (float)ProjectCost).ToString()),
                new KeyValuePair<string, string>("GOLD_ICON", null)
            }).ToString());
        }

        public void ExecuteBeginHint14()
        {
            MBInformationManager.ShowHint(Localization.SetTextVariables("{=agricultureestate_hint_storehouse_upgrade}Expanding Storehouse increases storage capacity by 500\nCost: {STOREHOUSE_UPGRADE_COST}{GOLD_ICON}\nTime: 240 hours", new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("STOREHOUSE_UPGRADE_COST", ((Hero.MainHero.GetPerkValue(DefaultPerks.Steward.Contractors) ? 0.85f : 1f) * (float)ProjectCost).ToString()),
                new KeyValuePair<string, string>("GOLD_ICON", null)
            }).ToString());
        }

        public void ExecuteBeginHint15()
        {
            MBInformationManager.ShowHint(new TextObject("{=agricultureestate_hint_abort_upgrade}All progress will be lost and gold cost will not be refunded", null).ToString());
        }

        public void ExecuteBeginHint16()
        {
            string text = "";
            if (_village_land.ProjectQueue == null || Extensions.IsEmpty<string>(_village_land.ProjectQueue))
            {
                if (_village_land.ProjectQueue == null)
                {
                    _village_land.ProjectQueue = new Queue<string>();
                }
                text = new TextObject("{=agricultureestate_build_queue_empty}Build Queue Empty", null).ToString();
            }
            else
            {
                TextObject[] projectQueueArrayL18N = _village_land.GetProjectQueueArrayL18N();
                for (int i = 0; i < projectQueueArrayL18N.Length; i++)
                {
                    text += new TextObject("{=agricultureestate_build_queue_hint}{ORDER}-{BUILDING}{NEWLINE}", new Dictionary<string, object>
                    {
                        { "ORDER", (i + 1).ToString() },
                        { "BUILDING", projectQueueArrayL18N[i] }
                    }).ToString();
                }
            }
            MBInformationManager.ShowHint(text);
        }

        public void ExecuteBeginHint17()
        {
            MBInformationManager.ShowHint(new TextObject("{=agricultureestate_hint_auto_buy_prisoners}Automatically buy bandits prisoners from any party that visits this village\nClick to turn {STATUS}", null).SetTextVariable("STATUS", BuySlaves ? new TextObject("{=agricultureestate_off}Off", null).ToString() : new TextObject("{=agricultureestate_on}On", null).ToString()).ToString() + new TextObject("{=agricultureestate_shift_click_hint}{NEWLINE}Shift click will set all estates", null).ToString());
        }

        public void ExecuteBeginHint17b()
        {
            MBInformationManager.ShowHint(
                new TextObject("{=ae_hint_attack_bandits}Automatically attack nearby bandit parties using the estate's militia/guards.\nClick to turn {STATUS}", null)
                .SetTextVariable("STATUS", AttackBandits ? new TextObject("{=agricultureestate_off}Off", null).ToString() : new TextObject("{=agricultureestate_on}On", null).ToString())
                .ToString()
            );
        }

        public void ExecuteBeginHint18()
        {
            MBInformationManager.ShowHint(new TextObject("{=agricultureestate_hint_show_ledger}Open bussiness ledger that show stats of all owned estates", null).ToString());
        }

        public void ExecuteEndHint()
        {
            MBInformationManager.HideInformations();
        }
    }
}