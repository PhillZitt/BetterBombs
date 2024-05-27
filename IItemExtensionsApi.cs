﻿using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace BetterBombs
{

    public interface IItemExtensionsApi
    {
        /// <summary>
        /// Checks for resource data with the Stone type.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool IsStone(string id);

        /// <summary>
        /// Checks for resource data in the mod.
        /// </summary>
        /// <param name="id">Qualified item ID</param>
        /// <param name="health">MinutesUntilReady value</param>
        /// <param name="itemDropped">Item dropped by ore</param>
        /// <returns>Whether the object has ore data.</returns>
        bool IsResource(string id, out int? health, out string itemDropped);

        /// <summary>
        /// Checks mod's menu behaviors. If a target isn't provided, it'll search whether any exist.
        /// </summary>
        /// <param name="qualifiedItemId">Qualified item ID.</param>
        /// <param name="target">Item to search behavior for. (Qualified item ID)</param>
        /// <returns>Whether this item has menu behavior for target.</returns>
        bool HasBehavior(string qualifiedItemId, string target);

        /// <summary>
        /// Checks for a qualified id in modded clump data.
        /// </summary>
        /// <param name="qualifiedItemId">Qualified item ID.</param>
        /// <returns>Whether this id is a clump's.</returns>
        bool IsClump(string qualifiedItemId);

        /// <summary>
        /// Tries to spawn a clump.
        /// </summary>
        /// <param name="itemId">The clump ID.</param>
        /// <param name="position">Tile position.</param>
        /// <param name="locationName">Location name or unique name.</param>
        /// <param name="error">Error string, if applicable.</param>
        /// <param name="avoidOverlap">Avoid overlapping with other clumps.</param>
        /// <returns>Whether spawning succeeded.</returns>
        bool TrySpawnClump(string itemId, Vector2 position, string locationName, out string error, bool avoidOverlap = false);

        /// <summary>
        /// Tries to spawn a clump.
        /// </summary>
        /// <param name="itemId">The clump ID.</param>
        /// <param name="position">Tile position.</param>
        /// <param name="location">Location to use.</param>
        /// <param name="error">Error string, if applicable.</param>
        /// <param name="avoidOverlap">Avoid overlapping with other clumps.</param>
        /// <returns>Whether spawning succeeded.</returns>
        bool TrySpawnClump(string itemId, Vector2 position, GameLocation location, out string error, bool avoidOverlap = false);

        /// <summary>
        /// Checks custom mixed seeds.
        /// </summary>
        /// <param name="itemId">The 'main seed' ID.</param>
        /// <param name="includeSource">Include the main seed's crop in calculation.</param>
        /// <param name="parseConditions">Whether to pase GSQs before adding to list.</param>
        /// <returns>All possible seeds.</returns>
        List<string> GetCustomSeeds(string itemId, bool includeSource, bool parseConditions = true);

        /// <summary>
        /// Gets drops for a clump.
        /// </summary>
        /// <param name="clump">The clump instance.</param>
        /// <param name="parseConditions">Whether to pase GSQs before adding to list.</param>
        /// <returns>All possible drops, with %.</returns>
        Dictionary<string, (double, int)> GetClumpDrops(ResourceClump clump, bool parseConditions = false);

        /// <summary>
        /// Gets drops for a node.
        /// </summary>
        /// <param name="clump">The node instance.</param>
        /// <param name="parseConditions">Whether to pase GSQs before adding to list.</param>
        /// <returns>All possible drops, with %.</returns>
        Dictionary<string, (double, int)> GetObjectDrops(StardewValley.Object node, bool parseConditions = false);

        /// <summary>
        /// Checks for other resource information.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool GetExtraResourceData(string id, bool isClump, out bool bombImmunity, out Enum resourceType);

        bool GetAllResourceData(string id, bool isClump, out object data);
    }
}
