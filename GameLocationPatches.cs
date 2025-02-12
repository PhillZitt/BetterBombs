using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterBombs
{
    public class GameLocationPatches
    {
        private static IMonitor Monitor;
        private static ModConfig Config;
        private static IModHelper Helper;
        private const string itemExtensionsModDataKey = "mistyspring.ItemExtensions/CustomClumpId";
        private static IItemExtensionsApi itemExtensionsApi;

        private const string new16TileSheetName = "TileSheets\\Objects_2";
        private const int new16Boulder = 148;
        private const int greenRainWeedClump1 = 44;
        private const int greenRainWeedClump2 = 46;
        private readonly static List<int> vanillaStoneClumps = new() { ResourceClump.boulderIndex, ResourceClump.mineRock1Index, ResourceClump.mineRock2Index, ResourceClump.mineRock3Index, ResourceClump.mineRock4Index, ResourceClump.quarryBoulderIndex, new16Boulder };
        private readonly static List<int> vanillaWoodClumps = new() { ResourceClump.stumpIndex, ResourceClump.hollowLogIndex };
        private readonly static List<int> vanillaWeedsClumps = new() { greenRainWeedClump1, greenRainWeedClump2 };

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            Monitor = monitor;
            Config = config;
            Helper = helper;

            Helper.Events.GameLoop.GameLaunched += (sender, args) =>
            {
                // if IE isn't loaded there's no point in trying to access their API
                if (!helper.ModRegistry.IsLoaded("mistyspring.ItemExtensions")) return;
                itemExtensionsApi = helper.ModRegistry.GetApi<IItemExtensionsApi>("mistyspring.ItemExtensions");
            };
        }

        //Pull in anything that I might change as ref, because the default code will run after this
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.explode))]
        [HarmonyPrefix]
        public static bool Explode_Prefix(GameLocation __instance, Vector2 tileLocation, ref int radius, Farmer who, ref bool damageFarmers, ref int damage_amount)
        {
            Monitor.Log("Beginning Explosion");
            try
            {
                //Do all of the changes early, if the custom method explodes, at least we did some of the changes
                damageFarmers = Config.DamageFarmers;
                radius = Convert.ToInt32(radius * Config.Radius);
                if (damage_amount > 0)
                {
                    damage_amount = Convert.ToInt32(damage_amount * Config.Damage);
                }
                else
                {
                    damage_amount = Convert.ToInt32((radius * Game1.random.Next(radius * 6, (radius * 8) + 1)) * Config.Damage);
                }

                var tileArea = new Rectangle(Convert.ToInt32(tileLocation.X - radius - 1f), Convert.ToInt32(tileLocation.Y - radius - 1f), (radius * 2 + 1), (radius * 2 + 1));
                if (Config.BreakClumps)
                {
                    Dictionary<ResourceClump, List<Item>> objectsToDrop = new();
                    foreach (ResourceClump clump in __instance.resourceClumps)
                    {
                        if (!tileArea.Contains(clump.Tile)) continue;
                        // vanilla resource clump
                        if (clump.textureName.Value == null || clump.textureName.Value == new16TileSheetName)
                        {
                            // stone
                            if (vanillaStoneClumps.Contains(clump.parentSheetIndex.Value) && Config.BreakStoneClumps)
                            {
                                // vanilla boulders always drop stone, amounts taken from original author's version of this portion
                                if (!objectsToDrop.ContainsKey(clump)) objectsToDrop.Add(clump, new());
                                objectsToDrop[clump].Add(ItemRegistry.Create(StardewValley.Object.stoneQID, clump.parentSheetIndex.Value == ResourceClump.boulderIndex ? 15 : 10));
                            }
                            // meteorite
                            else if (clump.parentSheetIndex.Value == ResourceClump.meteoriteIndex && Config.BreakStoneClumps)
                            {
                                // According to the wiki: They drop 6 Iridium Ore, 6 Stone, and 2 Geodes, and have a 25% chance to drop a Prismatic Shard. 
                                // if BetterMeteorites is installed, the simple act of removing the meteorite causes it to spawn extra resources so special compat is unnecessary
                                if (!objectsToDrop.ContainsKey(clump)) objectsToDrop.Add(clump, new());
                                objectsToDrop[clump].Add(ItemRegistry.Create(StardewValley.Object.iridiumQID, 6));
                                objectsToDrop[clump].Add(ItemRegistry.Create(StardewValley.Object.stoneQID, 6));
                                objectsToDrop[clump].Add(ItemRegistry.Create("(O)535", 2));
                                if (Game1.random.NextBool(0.25f)) objectsToDrop[clump].Add(ItemRegistry.Create(StardewValley.Object.prismaticShardQID));
                            }
                            // wood
                            else if (vanillaWoodClumps.Contains(clump.parentSheetIndex.Value) && Config.BreakWoodClumps)
                            {
                                // vanilla stumps and logs drop hardwood, (O)709
                                // stumps drop a base of 2, hollow logs drop 8
                                if (!objectsToDrop.ContainsKey(clump)) objectsToDrop.Add(clump, new());
                                objectsToDrop[clump].Add(ItemRegistry.Create("(O)709", clump.parentSheetIndex.Value == ResourceClump.stumpIndex ? 2 : 8));
                            }
                            // green rain weeds
                            else if (clump.IsGreenRainBush() && Config.BreakWeedsClumps)
                            {
                                // green rain weeds drop 2-3 moss, 2-3 fiber, and a 5% chance of a mossy seed
                                if (!objectsToDrop.ContainsKey(clump)) objectsToDrop.Add(clump, new());
                                objectsToDrop[clump].Add(ItemRegistry.Create("(O)Moss", Game1.random.Choose(2, 3)));
                                objectsToDrop[clump].Add(ItemRegistry.Create("(O)771", Game1.random.Choose(2, 3)));
                                if (Game1.random.NextBool(0.05f)) objectsToDrop[clump].Add(ItemRegistry.Create("(O)MossySeed"));
                            }
                        }
                        else // likely a modded clump like from ItemExtensions
                        {
                            //so if it's from IE, it'll have that mod's modData
                            if (clump.modData != null && clump.modData.TryGetValue(itemExtensionsModDataKey, out string clumpId))
                            {
                                // Something's hinky if we have ResourceClumps with IE's ModData but no API access so we'll play it safe and skip over it
                                // Should cover cases where someone removes ItemExtensions after clumps spawn without removing them somehow
                                // Otherwise just checking for the presence of the relevant ModData should be enough
                                if (itemExtensionsApi == null) continue;

                                // Grab the info we need to check if the clump is eligible for blowing up
                                // If the clump ID isn't valid it'll skip trying to parse it further
                                // Should probably log invalid custom clump IDs if they're encountered but that's the content pack author's ballpark
                                // There's a legitimate reason to be sketched out by the use of dynamic but it's the only way to properly utilize an Object without upcasting to a type we can't reference without an assembly reference
                                if (itemExtensionsApi.GetResourceData(clumpId, true, out dynamic clumpData))
                                {
                                    // If there are any issues with a dynamic ResourceData cast as an Object it'll be here
                                    // boomy no worky :'(
                                    if (clumpData.ImmuneToBombs) continue;
                                    // skip any clumps that aren't configured to be broken
                                    bool canBreak = clumpData.Type.ToString() switch
                                    {
                                        "Stone" => Config.BreakStoneClumps,
                                        "Wood" => Config.BreakWoodClumps,
                                        "Weeds" => Config.BreakWeedsClumps,
                                        "Other" => Config.BreakOtherClumps,
                                        _ => false
                                    };
                                    // ItemExtensions now handles drop parsing and spawning internally, we just have to say which one to do it for and to remove it as well
                                    if (canBreak) itemExtensionsApi.CheckClumpDrops(clump, true);
                                }
                            } // else other mod
                        }
                    }
                    // spawn drops and remove clumps at the same time
                    foreach (KeyValuePair<ResourceClump, List<Item>> clumpAndList in objectsToDrop)
                    {
                        // first, double-check for and iterate through all of the items to spawn
                        if (clumpAndList.Value != null && clumpAndList.Value.Any())
                        {
                            foreach (Item item in clumpAndList.Value)
                            {
                                // We used the Item stack as a container for the two bits of info we actually needed, QualifiedItemId and Stack size, in order to spawn the Objects as dropped items
                                Game1.createMultipleObjectDebris(item.QualifiedItemId, (int)clumpAndList.Key.Tile.X, (int)clumpAndList.Key.Tile.Y, item.Stack);
                            }
                        }
                        // remove the associated resource clump
                        // I *would* worry about the lack of sanity check if using the clump as the Dictionary key didn't guarantee that they'd be unique
                        // Perform the removal outside the check above so that clumps without drops get destroyed too
                        __instance.resourceClumps.Remove(clumpAndList.Key);
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Explode_Prefix)}:\n{ex}", LogLevel.Error);
            }

            //I'm only doing the changed things, so I let the default code handle the default logic
            return true;
        }

    }
}
