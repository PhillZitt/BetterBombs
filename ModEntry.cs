using HarmonyLib;
using StardewModdingAPI;

namespace BetterBombs
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        internal static ITranslationHelper i18n { get; private set; }
        internal static ModConfig Config { get; private set; }

        internal static IItemExtensionsApi ItemExtensionsApi { get; private set; }

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            i18n = Helper.Translation;

            GameLocationPatches.Initialize(Monitor);
            ObjectPatches.Initialize(Monitor);

            //I considered trying to do this without harmony patching, but this results in a significantly reduced code footprint
            //If anyone has an idea of how to do this without harmony, shoot me a pull request
            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.GameLocation), nameof(StardewValley.GameLocation.explode)),
                prefix: new HarmonyMethod(typeof(GameLocationPatches), nameof(GameLocationPatches.Explode_Prefix)),
                transpiler: new HarmonyMethod(typeof(GameLocationPatches), nameof(GameLocationPatches.Explode_Transpiler))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.onExplosion)),
                prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.onExplosion_Prefix))
            );

            helper.Events.GameLoop.GameLaunched += (sender, args) =>
            {

                if (helper.ModRegistry.IsLoaded("mistyspring.ItemExtensions"))
                {
                    ItemExtensionsApi = helper.ModRegistry.GetApi<IItemExtensionsApi>("mistyspring.ItemExtensions");
                }

                if (helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu"))
                {
                    var gmcmApi = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

                    gmcmApi?.Register(ModManifest, () => Config = new ModConfig(), () => helper.WriteConfig(Config));
                    gmcmApi?.AddBoolOption(ModManifest, () => Config.DamageFarmers, (damageFarmers) => Config.DamageFarmers = damageFarmers, () => i18n.Get("DamageFarmers.name"), () => i18n.Get("DamageFarmers.description"));
                    gmcmApi?.AddNumberOption(ModManifest, () => Config.Damage, (damage) => Config.Damage = damage, () => i18n.Get("Damage.name"), () => i18n.Get("Damage.description"), 0.1f, 10f, 0.1f);
                    gmcmApi?.AddNumberOption(ModManifest, () => Config.Radius, (radius) => Config.Radius = radius, () => i18n.Get("Radius.name"), () => i18n.Get("Radius.description"), 0.1f, 5f, 0.1f);
                    gmcmApi?.AddBoolOption(ModManifest, () => Config.BreakClumps, (breakClumps) => Config.BreakClumps = breakClumps, () => i18n.Get("BreakClumps.name"), () => i18n.Get("BreakClumps.description"));
                    gmcmApi?.AddBoolOption(ModManifest, () => Config.BreakStoneClumps, (breakStoneClumps) => Config.BreakStoneClumps = breakStoneClumps, () => i18n.Get("BreakStoneClumps.name"), () => i18n.Get("BreakStoneClumps.description"));
                    gmcmApi?.AddBoolOption(ModManifest, () => Config.BreakWoodClumps, (breakWoodClumps) => Config.BreakWoodClumps = breakWoodClumps, () => i18n.Get("BreakWoodClumps.name"), () => i18n.Get("BreakWoodClumps.description"));
                    gmcmApi?.AddBoolOption(ModManifest, () => Config.BreakWeedsClumps, (breakWeedsClumps) => Config.BreakWeedsClumps = breakWeedsClumps, () => i18n.Get("BreakWeedsClumps.name"), () => i18n.Get("BreakWeedsClumps.description"));
                    gmcmApi?.AddBoolOption(ModManifest, () => Config.BreakOtherClumps, (breakOtherClumps) => Config.BreakOtherClumps = breakOtherClumps, () => i18n.Get("BreakOtherClumps.name"), () => i18n.Get("BreakOtherClumps.description"));
                    gmcmApi?.AddBoolOption(ModManifest, () => Config.CollectMinerals, (collectMinerals) => Config.CollectMinerals = collectMinerals, () => i18n.Get("CollectMinerals.name"), () => i18n.Get("CollectMinerals.description"));
                    gmcmApi?.AddBoolOption(ModManifest, () => Config.CollectForage, (collectForage) => Config.CollectForage = collectForage, () => i18n.Get("CollectForage.name"), () => i18n.Get("CollectForage.description"));
                    gmcmApi?.AddBoolOption(ModManifest, () => Config.CollectFish, (collectFish) => Config.CollectFish = collectFish, () => i18n.Get("CollectFish.name"), () => i18n.Get("CollectFish.description"));
                    gmcmApi?.AddBoolOption(ModManifest, () => Config.CollectedQuality, (collectedQuality) => Config.CollectedQuality = collectedQuality, () => i18n.Get("CollectedQuality.name"), () => i18n.Get("CollectedQuality.description"));
                    gmcmApi?.AddBoolOption(ModManifest, () => Config.CollectedDegrade, (collectedDegrade) => Config.CollectedDegrade = collectedDegrade, () => i18n.Get("CollectedDegrade.name"), () => i18n.Get("CollectedDegrade.description"));
                    gmcmApi?.AddBoolOption(ModManifest, () => Config.TillDirt, (tillDirt) => Config.TillDirt = tillDirt, () => i18n.Get("TillDirt.name"), () => i18n.Get("TillDirt.description"));
                }
            };
        }


    }
}