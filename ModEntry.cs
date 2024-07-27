using HarmonyLib;
using StardewModdingAPI;

namespace BetterBombs
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            var config = Helper.ReadConfig<ModConfig>();

            GameLocationPatches.Initialize(Monitor, config, helper);
            ObjectPatches.Initialize(Monitor, config);

            //I considered trying to do this without harmony patching, but this results in a significantly reduced code footprint
            //If anyone has an idea of how to do this without harmony, shoot me a pull request
            //var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.GameLocation), nameof(StardewValley.GameLocation.explode)),
                prefix: new HarmonyMethod(typeof(GameLocationPatches), nameof(GameLocationPatches.Explode_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.onExplosion)),
                prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.onExplosion_Prefix))
            );

            helper.Events.GameLoop.GameLaunched += (sender, args) => { 
                if (helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu"))
                {
                    var gmcmApi = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

                    gmcmApi?.Register(ModManifest, () => config = new ModConfig(), () => helper.WriteConfig(config));
                    gmcmApi?.AddBoolOption(ModManifest, () => config.DamageFarmers, (damageFarmers) => config.DamageFarmers = damageFarmers, () => "Damage Farmers", () => "Toggles whether farmers are damaged by explosions. Covers *all* explosions.");
                    gmcmApi?.AddNumberOption(ModManifest, () => config.Damage, (damage) => config.Damage = damage, () => "Damage Multiplier", () => "Scales the damage inflicted by explosions. Affects damage to monsters and farmers if Damage Farmers is enabled. 2x damage is enough for mega bombs to kill almost anything.", 0.1f, 10f, 0.1f);
                    gmcmApi?.AddNumberOption(ModManifest, () => config.Radius, (radius) => config.Radius = radius, () => "Explosion Radius Multiplier", () => "Scales the size of explosions. Overly large values may cause visual glitches.", 0.1f, 5f, 0.1f);
                    gmcmApi?.AddBoolOption(ModManifest, () => config.BreakClumps, (breakClumps) => config.BreakClumps = breakClumps, () => "Break Clumps", () => "Toggles whether explosions are able to break Resource Clumps. Currently, clumps are simply removed and resources spawned in their place instead of playing a breaking animation. Acts as a master switch for the granular clump-breaking settings, if this is disabled they're all disabled.");
                    gmcmApi?.AddBoolOption(ModManifest, () => config.BreakStoneClumps, (breakStoneClumps) => config.BreakStoneClumps = breakStoneClumps, () => "Break Stone Clumps", () => "Toggles whether explosions are able to break stone Resource Clumps. This includes all boulders.");
                    gmcmApi?.AddBoolOption(ModManifest, () => config.BreakWoodClumps, (breakWoodClumps) => config.BreakWoodClumps = breakWoodClumps, () => "Break Wood Clumps", () => "Toggles whether explosions are able to break wooden Resource Clumps. This includes large stumps and hollow logs.");
                    gmcmApi?.AddBoolOption(ModManifest, () => config.BreakWeedsClumps, (breakWeedsClumps) => config.BreakWeedsClumps = breakWeedsClumps, () => "Break Weeds Clumps", () => "Toggles whether explosions are able to break weeds Resource Clumps. This includes the two large green rain grasses.");
                    gmcmApi?.AddBoolOption(ModManifest, () => config.BreakOtherClumps, (breakOtherClumps) => config.BreakOtherClumps = breakOtherClumps, () => "Break Other Clumps", () => "Toggles whether explosions are able to break miscellaneous Resource Clumps. There are none in vanilla Stardew Valley, only those added by mods.");
                    gmcmApi?.AddBoolOption(ModManifest, () => config.CollectMinerals, (collectMinerals) => config.CollectMinerals = collectMinerals, () => "Collect Minerals", () => "Toggles whether grabbable minerals in the explosion radius are dropped instead of being destroyed.");
                    gmcmApi?.AddBoolOption(ModManifest, () => config.CollectForage, (collectForage) => config.CollectForage = collectForage, () => "Collect Forage", () => "Toggles whether grabbable forage in the explosion radius is dropped instead of being destroyed. Respects foraging level and professions.");
                    gmcmApi?.AddBoolOption(ModManifest, () => config.CollectFish, (collectFish) => config.CollectFish = collectFish, () => "Collect Fish Forage", () => "Toggles whether grabbable fish and shells in the explosion radius are dropped instead of being destroyed. Respects foraging level and professions.");
                    gmcmApi?.AddBoolOption(ModManifest, () => config.CollectedQuality, (collectedQuality) => config.CollectedQuality = collectedQuality, () => "Collected Items Have Quality", () => "Toggles whether collecting forage or fish with bombs will give it a quality level. Enabled by default.");
                    gmcmApi?.AddBoolOption(ModManifest, () => config.CollectedDegrade, (collectedDegrade) => config.CollectedDegrade = collectedDegrade, () => "Degrade Collected Items", () => "Toggles whether quality levels are reduced by one when collecting items via explosion. Disabled by default.");
                }
            };
        }


    }
}