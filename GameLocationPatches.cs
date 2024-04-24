using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace BetterBombs
{
    public class GameLocationPatches
    {
        private static IMonitor Monitor;
        private static ModConfig Config;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, ModConfig config)
        {
            Monitor = monitor;
            Config = config;
        }

        //Pull in anything that I might change as ref, because the default code will run after this
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
                    damage_amount = Convert.ToInt32(radius * Game1.random.Next(radius * 6, (radius * 8) + 1) * Config.Damage);
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
