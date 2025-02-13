using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;

namespace BetterBombs
{
    public class ObjectPatches
    {
        private static IMonitor Monitor;
        private static readonly List<int> forageCategories = new() { Object.GreensCategory, Object.VegetableCategory, Object.FruitsCategory, Object.flowersCategory };
        private static readonly List<int> fishForageCategories = new() { Object.FishCategory, Object.sellAtFishShopCategory };

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        [HarmonyPatch(typeof(Object), nameof(Object.onExplosion))]
        [HarmonyPriority(Priority.First)]
        public static void onExplosion_Prefix(bool __result, Object __instance, Farmer who)
        {
            // CanBeGrabbed is true only for the Objects we want
            if (__instance.CanBeGrabbed)
            {
                if (ModEntry.Config.CollectMinerals && __instance.Type == "Minerals")
                {
                    // minerals are thankfully simple, with the factory method working straight off the ID
                    Game1.createObjectDebris(__instance.QualifiedItemId, (int)__instance.TileLocation.X, (int)__instance.TileLocation.Y, who.UniqueMultiplayerID, __instance.Location);
                    __instance.performRemoveAction();
                }
                else if ((ModEntry.Config.CollectForage && forageCategories.Contains(__instance.Category))
                    || (ModEntry.Config.CollectFish && fishForageCategories.Contains(__instance.Category)))
                {
                    Item droppedItem = ItemRegistry.Create(__instance.QualifiedItemId);
                    // determine quality level
                    if (ModEntry.Config.CollectedQuality)
                    {
                        if (who.professions.Contains(16))
                        {
                            droppedItem.Quality = ModEntry.Config.CollectedDegrade ? 2 : 4;
                        }
                        else if (Game1.random.NextDouble() < (double)(who.ForagingLevel / 30f))
                        {
                            droppedItem.Quality = ModEntry.Config.CollectedDegrade ? 1 : 2;
                        }
                        else if (Game1.random.NextDouble() < (double)(who.ForagingLevel / 15f) && !ModEntry.Config.CollectedDegrade)
                        {
                            droppedItem.Quality = 1;
                        }
                    }
                    // This should convert tile location to pixel location just like how vanilla scythe harvesting is done
                    Vector2 objectPixelLocation = new(__instance.TileLocation.X * 64f + 32f, __instance.TileLocation.Y * 64f + 32f);
                    Game1.createItemDebris(droppedItem, objectPixelLocation, -1, __instance.Location, -1, false);
                    __instance.performRemoveAction();
                }
            }
        }
    }
}
