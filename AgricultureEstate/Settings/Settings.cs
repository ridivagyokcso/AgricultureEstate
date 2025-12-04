using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace AgricultureEstate
{
    public class Settings : AttributeGlobalSettings<Settings>
    {
        [SettingPropertyInteger("{=ae_settings_start_plots}Starting Available Plots", 0, 100, "0", Order = 10, RequireRestart = false, HintText = "{=ae_settings_start_plots_hint}The number of available plots when a village is first visited.")]
        [SettingPropertyGroup("{=ae_settings_group_general}General Settings")]
        public int StartingAvailablePlots { get; set; } = 10;

        [SettingPropertyInteger("{=ae_settings_start_undev_plots}Starting Undeveloped Plots", 0, 100, "0", Order = 11, RequireRestart = false, HintText = "{=ae_settings_start_undev_plots_hint}The number of undeveloped plots when a village is first visited.")]
        [SettingPropertyGroup("{=ae_settings_group_general}General Settings")]
        public int StartingUndevelopedPlots { get; set; } = 20;

        [SettingPropertyInteger("{=agricultureestate_mcm_proj_time}Project Time", 1, 100, "0", RequireRestart = false, Order = 5)]
        [SettingPropertyGroup("{=ae_settings_group_general}General Settings")]
        public int ProjectTime { get; set; } = 10;

        [SettingPropertyBool("{=agricultureestate_war_plots_destroy}Lose Estate at War", RequireRestart = false, Order = 9)]
        [SettingPropertyGroup("{=ae_settings_group_general}General Settings")]
        public bool DestroyPlotsOnWar { get; set; } = true;

        [SettingPropertyFloatingInteger("{=ae_settings_militia_strength}Militia Strength Multiplier", 1.0f, 5.0f, "0.0", Order = 12, RequireRestart = false, HintText = "{=ae_settings_militia_strength_hint}Multiplies the effectiveness (number of troops) of the spawned militia patrol.")]
        [SettingPropertyGroup("{=ae_settings_group_general}General Settings")]
        public float MilitiaStrengthMultiplier { get; set; } = 1.0f;



        [SettingPropertyInteger("{=agricultureestate_mcm_dev_buy}Developed Plot Buy Price", 0, 100000, "0", RequireRestart = false, Order = 0)]
        [SettingPropertyGroup("{=ae_settings_group_price}Price Settings")]
        public int PlotBuyPrice { get; set; } = 800;

        [SettingPropertyInteger("{=agricultureestate_mcm_dev_sell}Developed Plot Sell Price", 0, 100000, "0", RequireRestart = false, Order = 1)]
        [SettingPropertyGroup("{=ae_settings_group_price}Price Settings")]
        public int PlotSellPrice { get; set; } = 200;

        [SettingPropertyInteger("{=agricultureestate_mcm_undev_buy}Undeveloped Plot Buy Price", 0, 100000, "0", RequireRestart = false, Order = 2)]
        [SettingPropertyGroup("{=ae_settings_group_price}Price Settings")]
        public int UndevelopedPlotBuyPrice { get; set; } = 400;

        [SettingPropertyInteger("{=agricultureestate_mcm_undev_sell}Undeveloped Plot Sell Price", 0, 100000, "0", RequireRestart = false, Order = 3)]
        [SettingPropertyGroup("{=ae_settings_group_price}Price Settings")]
        public int UndevelopedPlotSellPrice { get; set; } = 200;

        [SettingPropertyInteger("{=agricultureestate_mcm_proj_cost}Project Cost", 0, 500000, "0", RequireRestart = false, Order = 4)]
        [SettingPropertyGroup("{=ae_settings_group_price}Price Settings")]
        public int ProjectCost { get; set; } = 20000;

        [SettingPropertyFloatingInteger("{=agricultureestate_mcm_scale_rent}Scaling of rent for plots", 0f, 20f, "0.00", RequireRestart = false, Order = 6)]
        [SettingPropertyGroup("{=ae_settings_group_price}Price Settings")]
        public float LandRentScale { get; set; } = 1f;

        [SettingPropertyFloatingInteger("{=agricultureestate_mcm_scale_production}Scaling of slave production", 0f, 20f, "0.00", RequireRestart = false, Order = 7)]
        [SettingPropertyGroup("{=ae_settings_group_slave}Slave Settings")]
        public float SlaveProductionScale { get; set; } = 1f;

        [SettingPropertyFloatingInteger("{=agricultureestate_mcm_slave_decline}Slave decline rate modifier", 0f, 2f, "0.00", RequireRestart = false, Order = 8, HintText = "{=agricultureestate_mcm_slave_decline_hint}Setting the modifier to 0 disables slave decline.")]
        [SettingPropertyGroup("{=ae_settings_group_slave}Slave Settings")]
        public float SlaveDeclineModifier { get; set; } = 1f;

        

        public override string Id
        {
            get
            {
                return "AgricultureEstate";
            }
        }

        public override string DisplayName
        {
            get
            {
                return "Agriculture Estate";
            }
        }

        public override string FolderName
        {
            get
            {
                return "AgricultureEstate";
            }
        }

        public override string FormatType
        {
            get
            {
                return "json";
            }
        }
    }
}
