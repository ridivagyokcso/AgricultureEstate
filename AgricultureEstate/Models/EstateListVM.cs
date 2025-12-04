using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace AgricultureEstate
{
    public class EstateListVM : ViewModel
    {
        public static string SortType = "Village Name";
        private string _currentSort = "";

        public MBBindingList<EstateEntryVM> Estates { get; set; }
        public MBBindingList<EstateEntryVM> Title { get; set; }

        [DataSourceProperty]
        public string CloseString => new TextObject("{=agricultureestate_ui_close}Close", null).ToString();

        [DataSourceProperty]
        public string SortString => new TextObject("{=agricultureestate_ui_sort}Sort", null).ToString();

        public EstateListVM()
        {
            Title = new MBBindingList<EstateEntryVM>
            {
                new EstateEntryVM(null, true)
            };
            Estates = new MBBindingList<EstateEntryVM>();

            foreach (KeyValuePair<Settlement, VillageLand> keyValuePair in AgricultureEstateBehavior.VillageLands)
            {
                VillageLand value = keyValuePair.Value;
                if (value.OwnedPlots > 0 || value.OwnedUndevelopedPlots > 0)
                {
                    Estates.Add(new EstateEntryVM(value, false));
                }
            }
        }

        public void sort()
        {
            Estates.Clear();
            List<EstateEntryVM> list = new List<EstateEntryVM>();

            foreach (KeyValuePair<Settlement, VillageLand> keyValuePair in AgricultureEstateBehavior.VillageLands)
            {
                VillageLand value = keyValuePair.Value;
                if (value.OwnedPlots > 0 || value.OwnedUndevelopedPlots > 0)
                {
                    list.Add(new EstateEntryVM(value, false));
                }
            }

            list.Sort(Compare);

            if (_currentSort == SortType)
            {
                list.Reverse();
            }

            _currentSort = SortType;

            foreach (EstateEntryVM item in list)
            {
                Estates.Add(item);
            }
        }

        private static int Compare(EstateEntryVM x, EstateEntryVM y)
        {
            if (x?._village_land == null) return -1;
            if (y?._village_land == null) return 1;

            switch (SortType)
            {
                case "Slaves":
                    return IntCompare(x._village_land.Prisoners.TotalManCount, y._village_land.Prisoners.TotalManCount);

                case "Village Name":
                    string nameX = x._village_land.Village?.Name?.ToString();
                    string nameY = y._village_land.Village?.Name?.ToString();
                    return string.Compare(nameX, nameY);

                case "Current Project":
                    return string.Compare(x._village_land.CurrentProject, y._village_land.CurrentProject);

                case "Last Day Income":
                    return IntCompare(x._village_land.LastDayIncome, y._village_land.LastDayIncome);

                case "Primary Production":
                    string prodX = x._village_land.Village?.VillageType?.PrimaryProduction?.Name?.ToString() ?? "";
                    string prodY = y._village_land.Village?.VillageType?.PrimaryProduction?.Name?.ToString() ?? "";
                    return string.Compare(prodX, prodY);

                case "Current Project Progress":
                    return IntCompare(x._village_land.ProjectProgress, y._village_land.ProjectProgress);

                case "Owned Undeveloped Plots":
                    return IntCompare(x._village_land.OwnedUndevelopedPlots, y._village_land.OwnedUndevelopedPlots);

                case "Owned Plots":
                    return IntCompare(x._village_land.OwnedPlots, y._village_land.OwnedPlots);

                case "Stockpile":
                    return IntCompare(GetCount(x._village_land.Stockpile), GetCount(y._village_land.Stockpile));

                default:
                    return 0;
            }
        }

        private static int GetCount(ItemRoster items)
        {
            int num = 0;
            foreach (ItemRosterElement itemRosterElement in items)
            {
                num += itemRosterElement.Amount;
            }
            return num;
        }

        private static int IntCompare(int x, int y)
        {
            if (y > x) return 1;
            return (x > y) ? -1 : 0;
        }

        public void Close()
        {
            AgricultureEstateBehavior.DeleteVMLayer2();
        }

        public void Sort()
        {
            List<InquiryElement> list = new List<InquiryElement>
            {
                new InquiryElement("Current Project Progress", new TextObject("{=agricultureestate_current_project_progress}Current Project Progress", null).ToString(), null),
                new InquiryElement("Current Project", new TextObject("{=agricultureestate_current_project}Current Project", null).ToString(), null),
                new InquiryElement("Primary Production", new TextObject("{=agricultureestate_primary_production}Primary Production", null).ToString(), null),
                new InquiryElement("Stockpile", new TextObject("{=agricultureestate_stockpile}Stockpile", null).ToString(), null),
                new InquiryElement("Slaves", new TextObject("{=agricultureestate_slaves}Slaves", null).ToString(), null),
                new InquiryElement("Last Day Income", new TextObject("{=agricultureestate_last_day_income}Last Day Income", null).ToString(), null),
                new InquiryElement("Owned Undeveloped Plots", new TextObject("{=agricultureestate_owned_undev_plots}Owned Undeveloped Plots", null).ToString(), null),
                new InquiryElement("Owned Plots", new TextObject("{=agricultureestate_owned_plots}Owned Plots", null).ToString(), null),
                new InquiryElement("Village Name", new TextObject("{=agricultureestate_village_name}Village Name", null).ToString(), null)
            };

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=agricultureestate_ui_sort_by}Sort by", null).ToString(),
                "",
                list,
                true,
                1,
                1,
                new TextObject("{=agricultureestate_ui_continue}Continue", null).ToString(),
                "",
                delegate (List<InquiryElement> args)
                {
                    if (args != null && args.Any())
                    {
                        InformationManager.HideInquiry();

                        SortType = args.Select(element => element?.Identifier?.ToString() ?? "").First();

                        AgricultureEstateBehavior.estateListVM?.sort();
                    }
                },
                null,
                "",
                false), false, false);
        }
    }
}