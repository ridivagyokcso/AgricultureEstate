using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace AgricultureEstate
{
    public class EstateEntryVM : ViewModel
    {
        public VillageLand _village_land;
        private readonly bool _is_title;

        public EstateEntryVM(VillageLand land, bool Title)
        {
            _village_land = land;
            _is_title = Title;
        }

        public string Name
        {
            get
            {
                if (!_is_title)
                {
                    return _village_land?.Village?.Name?.ToString() ?? "";
                }
                return new TextObject("{=agricultureestate_estate_village}Village", null).ToString();
            }
        }

        public string OwnedPlots
        {
            get
            {
                if (!_is_title)
                {
                    return _village_land?.OwnedPlots.ToString() ?? "";
                }
                return new TextObject("{=agricultureestate_owned_plots}Owned Plots", null).ToString();
            }
        }

        public string OwnedUndevelopedPlots
        {
            get
            {
                if (!_is_title)
                {
                    return _village_land?.OwnedUndevelopedPlots.ToString() ?? "";
                }
                return new TextObject("{=agricultureestate_owned_undev_plots}Owned Undeveloped Plots", null).ToString();
            }
        }

        public string LastDayIncome
        {
            get
            {
                if (!_is_title)
                {
                    return _village_land?.LastDayIncome.ToString() ?? "";
                }
                return new TextObject("{=agricultureestate_last_day_income}Last Day Income", null).ToString();
            }
        }

        public string Slaves
        {
            get
            {
                if (_is_title)
                {
                    return new TextObject("{=agricultureestate_slaves}Slaves", null).ToString();
                }

                string current = (_village_land?.Prisoners?.TotalManCount ?? 0).ToString();
                string max = ((_village_land?.OwnedPlots ?? 0) * 10).ToString();
                return current + "/" + max;
            }
        }

        public string Stockpile
        {
            get
            {
                if (_is_title)
                {
                    return new TextObject("{=agricultureestate_stockpile}Stockpile", null).ToString();
                }

                int count = 0;
                if (_village_land != null)
                {
                    foreach (ItemRosterElement itemRosterElement in _village_land.Stockpile)
                    {
                        count += itemRosterElement.Amount;
                    }
                }
                return count.ToString() + "/" + (_village_land?.StorageCapacity.ToString() ?? "0");
            }
        }

        public string PrimaryProduction
        {
            get
            {
                if (!_is_title)
                {
                    return _village_land?.Village?.VillageType?.PrimaryProduction?.Name?.ToString() ?? "";
                }
                return new TextObject("{=agricultureestate_primary_production}Primary Production", null).ToString();
            }
        }

        public string CurrentProject
        {
            get
            {
                if (!_is_title)
                {
                    return _village_land?.CurrentProjectL18N?.ToString() ?? "";
                }
                return new TextObject("{=agricultureestate_current_project}Current Project", null).ToString();
            }
        }

        public string CurrentProjectProgress
        {
            get
            {
                if (!_is_title)
                {
                    return (_village_land?.ProjectProgress.ToString() ?? "0") + "/240";
                }
                return new TextObject("{=agricultureestate_current_project_progress}Current Project Progress", null).ToString();
            }
        }

        public void Click()
        {
            AgricultureEstateBehavior.DeleteVMLayer();
            AgricultureEstateBehavior.DeleteVMLayer2();
            AgricultureEstateBehavior.CreateVMLayer(_village_land);
        }
    }
}