using System.Collections.Generic;
using MCM.Abstractions.Base.Global;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace AgricultureEstate
{
    public class VillageLand
    {
        private int _avalible_plots;
        private int _owned_plots;
        private int _avalible_undeveloped_plots;
        private int _owned_undeveloped_plots;
        private TroopRoster _prisoners;
        private ItemRoster _stockpile;
        private Village _village;
        private int _storage_capacity;
        private bool _sell_to_market;
        private int _gold;
        private string _current_project;
        private int _patrol_level;
        private Queue<string> _project_queue;
        private int _project_progress;
        private bool _buy_slaves;
        private int _last_day_income;

        public VillageLand(Village village)
        {
            Settings settings = GlobalSettings<Settings>.Instance;

            this._avalible_plots = (settings != null) ? settings.StartingAvailablePlots : 10;
            this._avalible_undeveloped_plots = (settings != null) ? settings.StartingUndevelopedPlots : 20;
            this._owned_plots = 0;
            this._owned_undeveloped_plots = 0;
            this._prisoners = TroopRoster.CreateDummyTroopRoster();
            this._stockpile = new ItemRoster();
            this._gold = 0;
            this._current_project = "None";
            this._patrol_level = 0;
            this._project_queue = new Queue<string>();
            this._project_progress = 0;
            this._last_day_income = 0;
            this._village = village;
        }

        [SaveableProperty(1)]
        public int AvaliblePlots
        {
            get { return this._avalible_plots; }
            set { this._avalible_plots = value; }
        }

        [SaveableProperty(2)]
        public int OwnedPlots
        {
            get { return this._owned_plots; }
            set { this._owned_plots = value; }
        }

        [SaveableProperty(3)]
        public int AvalibleUndevelopedPlots
        {
            get { return this._avalible_undeveloped_plots; }
            set { this._avalible_undeveloped_plots = value; }
        }

        [SaveableProperty(4)]
        public int OwnedUndevelopedPlots
        {
            get { return this._owned_undeveloped_plots; }
            set { this._owned_undeveloped_plots = value; }
        }

        [SaveableProperty(5)]
        public Village Village
        {
            get { return this._village; }
            set { this._village = value; }
        }

        [SaveableProperty(6)]
        public TroopRoster Prisoners
        {
            get { return this._prisoners; }
            set { this._prisoners = value; }
        }

        [SaveableProperty(7)]
        public ItemRoster Stockpile
        {
            get { return this._stockpile; }
            set { this._stockpile = value; }
        }

        [SaveableProperty(8)]
        public int StorageCapacity
        {
            get { return this._storage_capacity; }
            set { this._storage_capacity = value; }
        }

        [SaveableProperty(9)]
        public bool SellToMarket
        {
            get { return this._sell_to_market; }
            set { this._sell_to_market = value; }
        }

        [SaveableProperty(10)]
        public int Gold
        {
            get { return this._gold; }
            set { this._gold = value; }
        }

        [SaveableProperty(11)]
        public string CurrentProject
        {
            get { return this._current_project; }
            set { this._current_project = value; }
        }

        [SaveableProperty(12)]
        public int PatrolLevel
        {
            get { return this._patrol_level; }
            set { this._patrol_level = value; }
        }

        [SaveableProperty(13)]
        public Queue<string> ProjectQueue
        {
            get { return this._project_queue; }
            set { this._project_queue = value; }
        }

        [SaveableProperty(14)]
        public int ProjectProgress
        {
            get { return this._project_progress; }
            set { this._project_progress = value; }
        }

        [SaveableProperty(15)]
        public bool BuySlaves
        {
            get { return this._buy_slaves; }
            set { this._buy_slaves = value; }
        }

        [SaveableProperty(16)]
        public int LastDayIncome
        {
            get { return this._last_day_income; }
            set { this._last_day_income = value; }
        }

        [SaveableProperty(17)]
        public bool AttackBandits { get; set; }

        public float SlaveDeclineRate()
        {
            float num = (float)((5.0 - 0.5 * (double)this.PatrolLevel) * (Hero.MainHero.GetPerkValue(DefaultPerks.Riding.MountedPatrols) ? 0.800000011920929 : 1.0));
            Settings instance = GlobalSettings<Settings>.Instance;
            return num * ((instance != null) ? instance.SlaveDeclineModifier : 1f);
        }

        public float SlaveRevoltRisk
        {
            get
            {
                double prisonersCount = (double)this.Prisoners.TotalManCount;
                Village village = this.Village;
                float militia = (village != null) ? village.Militia : 0f;

                if (prisonersCount >= 5.0 * (double)militia)
                {
                    bool isHighRisk = (10.0 * (double)militia < prisonersCount);
                    return (isHighRisk ? 3f : 1f) * (float)(1.0 - 0.1 * (double)this.PatrolLevel);
                }
                return 0f;
            }
        }

        public TextObject CurrentProjectL18N
        {
            get
            {
                return new TextObject(this.ProjectName2L18N(this._current_project), null);
            }
        }

        public TextObject[] GetProjectQueueArrayL18N()
        {
            string[] array = this._project_queue.ToArray();
            TextObject[] array2 = new TextObject[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array2[i] = new TextObject(this.ProjectName2L18N(array[i]), null);
            }
            return array2;
        }

        public string ProjectName2L18N(string projectName)
        {
            if (projectName == "Increase Patrols")
            {
                return "{=agricultureestate_ui_upgrade_patrols}Increase Patrols";
            }
            if (projectName == "Land Clearance")
            {
                return "{=agricultureestate_ui_land_clearance}Land Clearance";
            }
            if (projectName == "Expand Storehouse")
            {
                return "{=agricultureestate_ui_upgrade_storehouse}Expand Storehouse";
            }
            if (projectName == "None")
            {
                return "{=agricultureestate_no_current_project}No Current Project";
            }
            return "";
        }
    }
}