// <copyright file="TreeManagerPatches.cs" company="algernon (K. Algernon A. Sheppard)">
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
    using static TreeManager;

    /// <summary>
    /// Harmony patches to implement tree snapping.
    /// </summary>
    [HarmonyPatch(typeof(TreeManager))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    internal static class TreeManagerPatches
    {
        /// <summary>
        /// Harmony transpiler for TreeManager.CheckLimits to implement expanded tree limits.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(TreeManager.CheckLimits))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CheckOverlapTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo m_trees = AccessTools.Field(typeof(TreeManager), nameof(m_trees));
            FieldInfo m_Buffer = AccessTools.Field(typeof(Array32<TreeInstance>), nameof(Array32<TreeInstance>.m_buffer));

            // Looking for hardcoded tree limit constants.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(MAX_MAP_TREES) || instruction.LoadsConstant(262139))
                {
                    // Replace limit with the current limit - TreeManager.instance.m_trees.m_buffer.Length - 1.
                    Logging.Message("found tree constant ", instruction.operand);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, m_trees);
                    yield return new CodeInstruction(OpCodes.Ldfld, m_Buffer);
                    yield return new CodeInstruction(OpCodes.Ldlen);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    yield return new CodeInstruction(OpCodes.Sub);
                    continue;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony postfix to TreeManager.CreateTree to implement tree snapping.
        /// </summary>>
        /// <param name="__instance">TreeManager instance.</param>
        /// <param name="tree">ID of newly-created tree.</param>
        [HarmonyPatch(nameof(TreeManager.CreateTree))]
        [HarmonyPostfix]
        private static void CreateTreePostfix(TreeManager __instance, uint tree)
        {
            // Record scale.
            TreeInstancePatches.ScalingArray[tree] = TreeToolPatches.Scaling;
            Logging.KeyMessage("saving scaling amount of ", TreeToolPatches.Scaling);
        }
    }
}