using HarmonyLib;
using MCM.Abstractions.Base.Global;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace AgricultureEstate
{
    public class SubModule : MBSubModuleBase
    {
        private static readonly List<Action> ActionsToExecuteNextTick = new List<Action>();
        private readonly Harmony harmony = new Harmony("AgricultureEstate");
        public static int PlotBuyPrice
        {
            get
            {
                Settings instance = GlobalSettings<Settings>.Instance;
                return (instance != null) ? instance.PlotBuyPrice : 800;
            }
        }

        public static int PlotSellPrice
        {
            get
            {
                Settings instance = GlobalSettings<Settings>.Instance;
                return (instance != null) ? instance.PlotSellPrice : 200;
            }
        }

        public static int UndevelopedPlotBuyPrice
        {
            get
            {
                Settings instance = GlobalSettings<Settings>.Instance;
                return (instance != null) ? instance.UndevelopedPlotBuyPrice : 400;
            }
        }

        public static int UndevelopedPlotSellPrice
        {
            get
            {
                Settings instance = GlobalSettings<Settings>.Instance;
                return (instance != null) ? instance.UndevelopedPlotSellPrice : 100;
            }
        }

        public static int ProjectCost
        {
            get
            {
                Settings instance = GlobalSettings<Settings>.Instance;
                return (instance != null) ? instance.ProjectCost : 20000;
            }
        }

        public static float LandRentScale
        {
            get
            {
                Settings instance = GlobalSettings<Settings>.Instance;
                return (instance != null) ? instance.LandRentScale : 1f;
            }
        }

        public static float SlaveProductionScale
        {
            get
            {
                Settings instance = GlobalSettings<Settings>.Instance;
                return (instance != null) ? instance.SlaveProductionScale : 1f;
            }
        }


        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            try
            {
                harmony.PatchAll();
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage("AgricultureEstate PatchAll Error: " + ex.Message));
            }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            if (game.GameType is Campaign)
            {
                ((CampaignGameStarter)gameStarterObject).AddBehavior(new AgricultureEstateBehavior());
                ((CampaignGameStarter)gameStarterObject).AddModel(new MilitiaSpeedModel());
            }
        }

        public override void OnAfterGameInitializationFinished(Game game, object starterObject)
        {
            base.OnAfterGameInitializationFinished(game, starterObject);

            try
            {
                var originalMethod = AccessTools.Method(typeof(DefaultClanFinanceModel), "CalculateClanIncomeInternal");

                if (originalMethod != null)
                {
                    harmony.Unpatch(originalMethod, HarmonyPatchType.Postfix, "AgricultureEstate");

                    var postfixPatch = new HarmonyMethod(typeof(ClanFinancePatch), "Postfix");
                    harmony.Patch(originalMethod, postfix: postfixPatch);
                }
                else
                {
                    InformationManager.DisplayMessage(new InformationMessage("AgricultureEstate Warning: Could not find CalculateClanIncomeInternal method. Income patch disabled.", Colors.Red));
                }
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage("AgricultureEstate Manual Patch Error: " + ex.Message));
            }
        }

        public static void ExecuteActionOnNextTick(Action action)
        {
            if (action != null)
            {
                ActionsToExecuteNextTick.Add(action);
            }
        }

        protected override void OnApplicationTick(float dt)
        {
            base.OnApplicationTick(dt);

            if (ActionsToExecuteNextTick.Count > 0)
            {
                List<Action> actionsCopy = new List<Action>(ActionsToExecuteNextTick);
                ActionsToExecuteNextTick.Clear();

                foreach (Action action in actionsCopy)
                {
                    action?.Invoke();
                }
            }
        }
    }
}