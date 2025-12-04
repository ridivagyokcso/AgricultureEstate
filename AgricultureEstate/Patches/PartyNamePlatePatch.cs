using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SandBox.ViewModelCollection.Nameplate;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace AgricultureEstate
{
    [HarmonyPatch(typeof(SettlementNameplateVM), "RefreshBindValues")]
    internal class PartyNamePlatePatch
    {
        private static void Postfix(SettlementNameplateVM __instance)
        {
            if (__instance.Settlement.IsVillage)
            {
                if (!HasTwo(__instance.SettlementEvents.EventsList) && OwnsLand(__instance.Settlement))
                {
                    string stringId = __instance.Settlement.Village.VillageType.PrimaryProduction.StringId;

                    string text = stringId;
                    if (stringId.Contains("camel"))
                    {
                        text = "camel";
                    }
                    else if (stringId.Contains("horse") || stringId.Contains("mule"))
                    {
                        text = "horse";
                    }

                    __instance.SettlementEvents.EventsList.Add(new SettlementNameplateEventItemVM(text));
                }
                else if (HasTwo(__instance.SettlementEvents.EventsList) && !OwnsLand(__instance.Settlement))
                {
                    SettlementNameplateEventItemVM item = __instance.SettlementEvents.EventsList.FirstOrDefault(e => (int)e.EventType == 6);
                    if (item != null)
                    {
                        __instance.SettlementEvents.EventsList.Remove(item);
                    }
                }
            }
        }

        private static bool OwnsLand(Settlement settlement)
        {
            if (AgricultureEstateBehavior.VillageLands == null)
            {
                AgricultureEstateBehavior.VillageLands = new Dictionary<Settlement, VillageLand>();
                return false;
            }

            if (AgricultureEstateBehavior.VillageLands.TryGetValue(settlement, out VillageLand villageLand))
            {
                return villageLand.OwnedPlots > 0 || villageLand.OwnedUndevelopedPlots > 0;
            }

            return false;
        }

        private static bool HasTwo(MBBindingList<SettlementNameplateEventItemVM> eventsList)
        {
            int count = eventsList.Count(e => (int)e.EventType == 6);
            return count >= 2;
        }
    }
}