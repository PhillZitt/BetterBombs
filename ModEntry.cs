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

            GameLocationPatches.Initialize(Monitor, config);

            //I considered trying to do this without harmony patching, but this results in a significantly reduced code footprint
            //If anyone has an idea of how to do this without harmony, shoot me a pull request
            //var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.GameLocation), nameof(StardewValley.GameLocation.explode)),
                prefix: new HarmonyMethod(typeof(GameLocationPatches), nameof(BetterBombs.GameLocationPatches.Explode_Prefix))
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

                }
            };
        }


    }
}