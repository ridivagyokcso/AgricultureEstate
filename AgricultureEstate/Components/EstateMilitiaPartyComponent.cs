using MCM.Abstractions.Base.Global;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace AgricultureEstate
{
    public class EstateMilitiaPartyComponent : PartyComponent
    {
        private CampaignTime _startTime;
        public CampaignTime CreationTime { get; set; }
        [SaveableField(1)]
        private VillageLand _land;

        public EstateMilitiaPartyComponent()
        {
        }

        public EstateMilitiaPartyComponent(VillageLand land)
        {
            _land = land;
            _startTime = CampaignTime.Now;
            CreationTime = CampaignTime.Now;
        }

        public override Hero PartyOwner => _land?.Village?.Settlement?.OwnerClan?.Leader ?? Hero.MainHero;

        public override TextObject Name => new TextObject("{=ae_militia_name}{VILLAGE} Militia Force")
            .SetTextVariable("VILLAGE", _land?.Village?.Name?.ToString() ?? "Unknown");

        public override Settlement HomeSettlement => _land?.Village?.Settlement;

        public override Banner GetDefaultComponentBanner()
        {
            return _land?.Village?.Settlement?.OwnerClan?.Banner ?? Hero.MainHero.Clan.Banner;
        }

        public void OnHourlyTick()
        {

            if (MobileParty == null || !MobileParty.IsActive || _land == null) return;

            if (_startTime.ElapsedHoursUntilNow < 4.0f)
            {
                return;
            }

            if (MobileParty.GetPositionAsVec3().Distance(_land.Village.Settlement.GetPosition()) < 1.0f && MobileParty.MapEvent == null)
            {
                DisbandAndReturnResources();
                return;
            }

            if (MobileParty.MapEvent != null) return;

            bool hasValidTarget = MobileParty.TargetParty != null && MobileParty.TargetParty.IsActive;

            if (hasValidTarget)
            {
                if (MobileParty.DefaultBehavior == AiBehavior.Hold || MobileParty.DefaultBehavior != AiBehavior.EngageParty)
                {
                    MobileParty.SetMoveEngageParty(MobileParty.TargetParty, MobileParty.NavigationType.Default);
                }
            }
            else
            {
                if (MobileParty.TargetParty != null)
                {
                    MobileParty.SetMoveModeHold();
                }

                if (MobileParty.CurrentSettlement == null)
                {
                    MobileParty.SetMoveGoToPoint(_land.Village.Settlement.GatePosition, MobileParty.NavigationType.Default);
                }
            }
        }

        private void DisbandAndReturnResources()
        {
            if (_land == null) return;

            if (MobileParty.PrisonRoster.TotalManCount > 0)
            {
                foreach (var troop in MobileParty.PrisonRoster.GetTroopRoster())
                {
                    _land.Prisoners.AddToCounts(troop.Character, troop.Number);
                }
            }

            float returningTroops = (float)MobileParty.MemberRoster.TotalManCount;

            Settings instance = GlobalSettings<Settings>.Instance;
            float multiplier = (instance != null) ? instance.MilitiaStrengthMultiplier : 1.0f;

            if (multiplier < 1.0f) multiplier = 1.0f;

            float realMilitiaToReturn = returningTroops / multiplier;

            if (_land.Village != null && _land.Village.Settlement != null)
            {
                _land.Village.Settlement.Militia += realMilitiaToReturn;
            }

            DestroyPartyAction.Apply(null, this.MobileParty);
        }
    }
}