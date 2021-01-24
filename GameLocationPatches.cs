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
    private static IModHelper Helper;
    private static ModConfig Config;

    private static readonly List<int> MineralIgnoreList = new List<int>(){
																	                                  //Weeds
																	                                  0, 313, 314, 315, 316, 317, 318, 319, 320, 321, 452, 674, 675, 676, 677, 678, 679, 750, 784, 785, 786, 792, 793, 794, 882, 883, 884
                                                                  };

    // call this method from your Entry class
    public static void Initialize(IMonitor monitor, IModHelper helper, ModConfig config)
    {
      Monitor = monitor;
      Helper = helper;
      Config = config;
    }

    //Pull in anything that I might change as ref, because the default code will run after this
    public static bool Explode_Prefix(GameLocation __instance, Vector2 tileLocation, ref int radius, Farmer who, ref bool damageFarmers, ref int damage_amount)
    {
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

        //Run only the custom explode logic
        ChangedExplode(__instance, tileLocation, radius, who);
      }
      catch (Exception ex)
      {
        Monitor.Log($"Failed in {nameof(Explode_Prefix)}:\n{ex}", LogLevel.Error);
      }

      //I'm only doing the changed things, so I let the default code handle the default logic
      return true;
    }

    private static void ChangedExplode(GameLocation instance, Vector2 tileLocation, int radius, Farmer who)
    {
      var area = new Rectangle(Convert.ToInt32(tileLocation.X - radius - 1f) * 64, Convert.ToInt32(tileLocation.Y - radius - 1f) * 64, (radius * 2 + 1) * 64, (radius * 2 + 1) * 64);

      //If anyone has any ideas on how to improve these two sections, I'd be happy to hear it
      if (Config.BreakClumps)
      {
        //Make a list to hold the removed items
        var removed = new List<ResourceClump>(instance.resourceClumps.Count);
        foreach (var clump in instance.resourceClumps)
        {
          var loc = clump.tile.Value;

          //check if the clump is inside of the bomb area
          if (clump.getBoundingBox(loc).Intersects(area))
          {
            var numChunks = ((clump.parentSheetIndex == 672) ? 15 : 10);
            if (Game1.IsMultiplayer)
            {
              Game1.createMultipleObjectDebris(390, (int)tileLocation.X, (int)tileLocation.Y, numChunks, who.UniqueMultiplayerID);
            }
            else
            {
              Game1.createRadialDebris(Game1.currentLocation, 390, (int)tileLocation.X, (int)tileLocation.Y, numChunks, resource: false, -1, item: true);
            }
            instance.playSound("boulderBreak");
            Game1.createRadialDebris(Game1.currentLocation, 32, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(6, 12), resource: false);
            //There was a concurrent modification type error if I removed the item during the loop, so store a copy to remove later
            removed.Add(clump);
          }
        }
        //Do the remove after the loop completes
        removed.ForEach(x => instance.resourceClumps.Remove(x));
      }

      if (Config.CollectMinerals)
      {
        var removed = new List<Vector2>();
        foreach (var obj in instance.Objects.Pairs)
        {
          if (obj.Value.getBoundingBox(obj.Key).Intersects(area) && obj.Value.CanBeGrabbed && !MineralIgnoreList.Contains(obj.Value.ParentSheetIndex))
          {
            Game1.createObjectDebris(obj.Value.ParentSheetIndex, Convert.ToInt32(obj.Key.X), Convert.ToInt32(obj.Key.Y), who.uniqueMultiplayerID, instance);
            removed.Add(obj.Key);
          }
        }
        removed.ForEach(x => instance.destroyObject(x, who));
      }
    }
  }
}
