using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using SwmarlyValheimPizzaMod.Config;
using SwmarlyValheimPizzaMod.Crops;
using SwmarlyValheimPizzaMod.Items;
using SwmarlyValheimPizzaMod.Localization;
using SwmarlyValheimPizzaMod.Milking;
using SwmarlyValheimPizzaMod.Pizza;

namespace SwmarlyValheimPizzaMod
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency("com.jotunn.jotunn", BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    public sealed class PizzaPlugin : BaseUnityPlugin
    {
        internal const string Guid = "com.swmarly.valheimpizzamod";
        internal const string Name = "Swmarly Valheim Pizza Mod";
        internal const string Version = "1.0.1";

        internal static ManualLogSource Log = null!;
        internal static ConfigFile ModConfig = null!;
        internal static PizzaConfiguration Configuration = null!;
        internal static CustomLocalization Localization = null!;

        private bool contentRegistered;

        private void Awake()
        {
            Log = Logger;
            ModConfig = base.Config;
            Configuration = new PizzaConfiguration(ModConfig);
            Localization = PizzaLocalization.Register();
            LoxMilkingSystem.Initialize();
            PrefabManager.OnVanillaPrefabsAvailable += RegisterContent;
            Log.LogInfo("" + Name + " " + Version + " initialized; waiting for vanilla prefabs.");
        }

        private void RegisterContent()
        {
            if (contentRegistered)
            {
                return;
            }

            contentRegistered = true;
            try
            {
                ItemRegistry.Register(Configuration);
                TomatoCropSystem.Register(Configuration);
                PizzaRegistry.Register(Configuration);
                Log.LogInfo("Pizza content registration completed.");
            }
            catch (System.Exception exception)
            {
                Log.LogError("Pizza content registration failed. The mod will remain loaded without incomplete content: " + exception);
            }
            finally
            {
                PrefabManager.OnVanillaPrefabsAvailable -= RegisterContent;
            }
        }
    }
}
