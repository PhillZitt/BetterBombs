using HarmonyLib;
using ItemExtensions.Models;
using ItemExtensions.Models.Enums;
using ItemExtensions.Models.Items;
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
        private const string itemExtensionsModDataKey = "mistyspring.ItemExtensions/CustomClumpId";

        private const string new16TileSheetName = "TileSheets\\Objects_2";
        private const int new16Boulder = 148;
        private const int greenRainWeedClump1 = 44;
        private const int greenRainWeedClump2 = 46;
        private readonly static List<int> vanillaStoneClumps = new() { ResourceClump.boulderIndex, ResourceClump.mineRock1Index, ResourceClump.mineRock2Index, ResourceClump.mineRock3Index, ResourceClump.mineRock4Index, ResourceClump.quarryBoulderIndex, new16Boulder };
        private readonly static List<int> vanillaWoodClumps = new() { ResourceClump.stumpIndex, ResourceClump.hollowLogIndex };
        private readonly static List<int> vanillaWeedsClumps = new() { greenRainWeedClump1, greenRainWeedClump2 };

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, ModConfig config)
        {
            Monitor = monitor;
            Config = config;
        }

        //Pull in anything that I might change as ref, because the default code will run after this
        [HarmonyPriority(Priority.High)]
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

                var area = new Rectangle(Convert.ToInt32(tileLocation.X - radius - 1f) * 64, Convert.ToInt32(tileLocation.Y - radius - 1f) * 64, (radius * 2 + 1) * 64, (radius * 2 + 1) * 64);
                if (Config.BreakClumps)
                {
                    Dictionary<ResourceClump, List<Item>> objectsToDrop = new();
                    foreach (ResourceClump clump in __instance.resourceClumps)
                    {
                        if (!area.Contains(clump.Tile)) continue;
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
                            else if (vanillaWeedsClumps.Contains(clump.parentSheetIndex.Value) && Config.BreakWeedsClumps)
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
                                // this is where the dependency on ItemExtensions comes in
                                if (ItemExtensions.ModEntry.BigClumps.TryGetValue(clumpId, out ResourceData clumpResourceData))
                                {
                                    // boomy no worky :'(
                                    if (clumpResourceData.ImmuneToBombs) continue;
                                    // skip any clumps that aren't configured to be broken
                                    if (clumpResourceData.Type == CustomResourceType.Stone && !Config.BreakStoneClumps) continue;
                                    else if (clumpResourceData.Type == CustomResourceType.Wood && !Config.BreakWoodClumps) continue;
                                    else if (clumpResourceData.Type == CustomResourceType.Weeds && !Config.BreakWeedsClumps) continue;
                                    else if (clumpResourceData.Type == CustomResourceType.Other && !Config.BreakOtherClumps) continue;
                                    // check for drops
                                    if (!objectsToDrop.ContainsKey(clump)) objectsToDrop.Add(clump, new());

                                    if (clumpResourceData.ItemDropped != null)
                                    {
                                        // we have something to spawn
                                        objectsToDrop[clump].Add(ItemRegistry.Create(clumpResourceData.ItemDropped, Game1.random.Next(clumpResourceData.MinDrops, clumpResourceData.MaxDrops)));
                                    }
                                    if (clumpResourceData.ExtraItems != null && clumpResourceData.ExtraItems.Any())
                                    {
                                        foreach (ExtraSpawn item in clumpResourceData.ExtraItems)
                                        {
                                            // test the chance of the bonus item
                                            if (Game1.random.NextBool(item.Chance))
                                            {
                                                objectsToDrop[clump].Add(ItemRegistry.Create(item.ItemId, Game1.random.Next(item.MinStack, item.MaxStack)));
                                            }
                                        }
                                    }
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
                        // I *would* worry about the lack of sanity check if using the clump as
                        // the Dictionary key didn't guarantee that they'd be unique
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
