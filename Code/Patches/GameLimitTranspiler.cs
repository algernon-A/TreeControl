// <copyright file="GameLimitTranspiler.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl.Patches
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using AlgernonCommons;
    using HarmonyLib;

    /// <summary>
    /// Harmony transpilers to replace hardcoded tree limits in the game.
    /// </summary>
    [HarmonyPatch]
    internal static class GameLimitTranspiler
    {
        // New maximum limit.
        private const int MaxTreeLimit = 4194304;

        /// <summary>
        /// Determines list of target methods to patch - in this case, identified methods with hardcoded tree limits.
        /// </summary>
        /// <returns>List of target methods to patch.</returns>
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(CommonBuildingAI), "HandleFireSpread");
            yield return AccessTools.Method(typeof(ForestFireAI), "FindClosestTree");
            yield return AccessTools.Method(typeof(DisasterHelpers), nameof(DisasterHelpers.DestroyTrees));
            yield return AccessTools.Method(typeof(DistrictManager), "MoveParkTrees");
            yield return AccessTools.Method(typeof(NaturalResourceManager), nameof(NaturalResourceManager.TreesModified));
            yield return AccessTools.Method(typeof(WeatherManager), "FindStrikeTarget");
            yield return AccessTools.Method(typeof(TreeManager), "EndRenderingImpl");
            yield return AccessTools.Method(typeof(TreeManager), nameof(TreeManager.SampleSmoothHeight));
            yield return AccessTools.Method(typeof(TreeManager), "FinalizeTree");
            yield return AccessTools.Method(typeof(TreeManager), nameof(TreeManager.UpdateTrees));
            yield return AccessTools.Method(typeof(TreeManager), nameof(TreeManager.OverlapQuad));
            yield return AccessTools.Method(typeof(TreeManager), nameof(TreeManager.RayCast));
            yield return AccessTools.Method(typeof(TreeManager), "HandleFireSpread");
            yield return AccessTools.Method(typeof(TreeManager), nameof(TreeManager.TerrainUpdated));
            yield return AccessTools.Method(typeof(TreeManager), nameof(TreeManager.AfterTerrainUpdate));
            yield return AccessTools.Method(typeof(TreeManager), nameof(TreeManager.CalculateAreaHeight));
            yield return AccessTools.Method(typeof(TreeManager), nameof(TreeManager.CalculateGroupData));
            yield return AccessTools.Method(typeof(TreeManager), nameof(TreeManager.PopulateGroupData));
            yield return AccessTools.Method(typeof(TreeTool), "ApplyBrush");
            yield return AccessTools.Method(typeof(FireCopterAI), "FindBurningTree");
        }

        // TODO: Need to be linked to current array size.
        //  yield return AccessTools.Method(typeof(TreeManager), nameof(TreeManager.UpdateData));
        //  yield return AccessTools.Method(typeof(BuildingDecoration), nameof(BuildingDecoration.ClearDecorations));

        /// <summary>
        /// Harmony transpiler to replace hardcoded tree limits.
        /// Finds ldc.i4 262144 (which is unique in the target methods to the tree limit checks) and replaces the operand with our updated maximum.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Patched ILCode.</returns>
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            // Instruction parsing.
            IEnumerator<CodeInstruction> instructionsEnumerator = instructions.GetEnumerator();
            CodeInstruction instruction;

            // Status flag.
            bool foundTarget = false;

            // Iterate through all instructions in original method.
            while (instructionsEnumerator.MoveNext())
            {
                // Get next instruction and add it to output.
                instruction = instructionsEnumerator.Current;

                // Is this ldc.i4 262144?
                if (instruction.opcode == OpCodes.Ldc_I4 && instruction.operand is int thisInt && thisInt == TreeManager.MAX_TREE_COUNT)
                {
                    // Yes - change operand to our new unit count max.
                    instruction.operand = MaxTreeLimit;

                    Logging.Message("changed 262144 in ", original.FullDescription(), " to ", MaxTreeLimit);

                    // Set flag.
                    foundTarget = true;
                }

                // Output instruction.
                yield return instruction;
            }

            // If we got here without finding our target, something went wrong.
            if (!foundTarget)
            {
                Logging.Error("no ldc.i4 262144 found for ", original);
            }
        }
    }
}