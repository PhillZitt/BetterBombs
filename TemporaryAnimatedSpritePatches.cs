using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace BetterBombs
{
    internal class TemporaryAnimatedSpritePatches
    {
        private static IMonitor Monitor;

        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        [HarmonyPatch(typeof(TemporaryAnimatedSprite), nameof(TemporaryAnimatedSprite.update))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Update_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new(instructions);

            CodeMatch[] flashAlphaInstructions = new CodeMatch[] {
                new(OpCodes.Ldc_R4, 1f),
                new(OpCodes.Stsfld) // Operand *could* be included, but there are no other ldc.r4 followed immediately by stsfld
            };

            // replace the two instructions with a call to our detour
            matcher.MatchStartForward(flashAlphaInstructions)
                .ThrowIfNotMatchForward("Can't find code to set flashAlpha.", flashAlphaInstructions)
                .SetAndAdvance(OpCodes.Call, typeof(TemporaryAnimatedSpritePatches).GetMethod("CheckAndFlash", BindingFlags.Static | BindingFlags.Public))
                .Set(OpCodes.Nop, null);

            return matcher.Instructions();
        }

        public static void CheckAndFlash()
        {
            Game1.flashAlpha = ModEntry.Config.FlashScreen ? 1f : 0f;
        }
    }
}
