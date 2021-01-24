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
																	                                         0, 313, 314, 315, 316, 317, 318, 319, 320, 321, 452, 674, 675, 676, 677, 678, 679, 750, 784, 785, 786, 792, 793, 794, 882, 883, 884,
                                                                           //Barrels
                                                                           118, 120, 122, 124, 174,
                                                                           //Crates
                                                                           119, 121, 123, 125, 175
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
      //Monitor.Log("Beginning Explosion", LogLevel.Debug);
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

        //Monitor.Log("Beginning Custom Explosion Logic", LogLevel.Debug);
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
        //Monitor.Log("Breaking Clumps", LogLevel.Debug);
        //Make a list to hold the removed items
        var removed = new List<ResourceClump>(instance.resourceClumps.Count);
        foreach (var clump in instance.resourceClumps)
        {
          var loc = clump.tile.Value;
          //Monitor.Log($"Checking clump: {clump.parentSheetIndex} at {loc.X}, {loc.Y}", LogLevel.Debug);

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
            switch (clump.parentSheetIndex)
            {
              case ResourceClump.stumpIndex:
              case ResourceClump.hollowLogIndex:
                instance.playSound("stumpCrack");
                break;
              case ResourceClump.boulderIndex:
              case ResourceClump.mineRock1Index:
              case ResourceClump.mineRock2Index:
              case ResourceClump.mineRock3Index:
              case ResourceClump.mineRock4Index:
              case ResourceClump.meteoriteIndex:
                instance.playSound("boulderBreak");
                break;
            }
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
        //Monitor.Log("Collecting Minerals", LogLevel.Debug);
        var removed = new List<Vector2>();
        foreach (var obj in instance.Objects.Pairs)
        {
          if (MineralIgnoreList.Contains(obj.Value.ParentSheetIndex))
          {
            //Monitor.Log($"Ignored object: {ObjectParentSheetIndexToName(obj.Value.parentSheetIndex)} at {obj.Key.X}, {obj.Key.Y}", LogLevel.Debug);
            continue;
          } else if (!obj.Value.CanBeGrabbed)
          {
            //Monitor.Log($"Object not grabbable: {ObjectParentSheetIndexToName(obj.Value.parentSheetIndex)} at {obj.Key.X}, {obj.Key.Y}", LogLevel.Debug);
            continue;
          }
          //Monitor.Log($"Checking object: {ObjectParentSheetIndexToName(obj.Value.parentSheetIndex)} at {obj.Key.X}, {obj.Key.Y}", LogLevel.Debug);
          if (obj.Value.getBoundingBox(obj.Key).Intersects(area))
          {
            try
            {
              Game1.createObjectDebris(obj.Value.ParentSheetIndex, Convert.ToInt32(obj.Key.X), Convert.ToInt32(obj.Key.Y), who.uniqueMultiplayerID, instance);
              removed.Add(obj.Key);
            }catch(KeyNotFoundException ex){
              //do nothing, something went wrong translating an ground object to an inventory object
            }
          }
        }
        removed.ForEach(x => instance.destroyObject(x, who));
      }
    }


    private static string ObjectParentSheetIndexToName(int psi)
    {
      switch (psi)
      {
        //Weeds
        case 0:
        case 313:
        case 314:
        case 315:
        case 316:
        case 317:
        case 318:
        case 319:
        case 320:
        case 321:
        case 452:
        case 674:
        case 675:
        case 676:
        case 677:
        case 678:
        case 679:
        case 750:
        case 784:
        case 785:
        case 786:
        case 792:
        case 793:
        case 794:
        case 882:
        case 883:
        case 884:
          return $"Weeds ({psi})";

        //Barrels
        case 118:
        case 120:
        case 122:
        case 124:
        case 174:
          return $"Barrel ({psi})";

        //Crates
        case 119:
        case 121:
        case 123:
        case 125:
        case 175:
        case 922:
        case 923:
        case 924:
          return $"Crate ({psi})";

        //Stone
        case 2:
        case 4:
        case 75:
        case 76:
        case 77:
        case 95:
        case 668:
        case 670:
        case 751:
        case 760:
        case 762:
        case 764:
        case 765:
        case 816:
        case 817:
        case 818:
        case 819:
        case 843:
        case 844:
        case 845:
        case 846:
        case 847:
        case 849:
        case 850:
          return $"Stone ({psi})";

        default:
          return $"Unknown ({psi})";
      }
    }
  }
}
