using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace AgricultureEstate
{
    public class SaveDefiner : SaveableTypeDefiner
    {
        public SaveDefiner() : base(1436500002)
        {
        }

        protected override void DefineClassTypes()
        {
            base.AddClassDefinition(typeof(VillageLand), 1, null);
            base.AddClassDefinition(typeof(EstateMilitiaPartyComponent), 2, null);
        }

        protected override void DefineContainerDefinitions()
        {
            base.ConstructContainerDefinition(typeof(Dictionary<Settlement, VillageLand>));
        }
    }
}