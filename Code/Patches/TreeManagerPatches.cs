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
    using AlgernonCommons.UI;
    using ColossalFramework;
    using HarmonyLib;
    using static TreeManager;

    /// <summary>
    /// Harmony patches to implement tree snapping.
    /// </summary>
    [HarmonyPatch(typeof(TreeManager))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    internal static class TreeManagerPatches
    {
        // Anarchy status.
        private static bool s_anarchyEnabled = false;

        /// <summary>
        /// Gets or sets a value indicating whether tree anarchy is enabled.
        /// </summary>
        internal static bool AnarchyEnabled
        {
            get => s_anarchyEnabled;

            set
            {
                s_anarchyEnabled = value;

                // Update status panel.
                StandalonePanelManager<TreeControlStatusPanel>.Panel?.Refresh();
            }
        }

        /// <summary>
        /// Harmony transpiler for TreeManager.CheckLimits to implement expanded tree limits.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(TreeManager.CheckLimits))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CheckLimitsTranspiler(IEnumerable<CodeInstruction> instructions)
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
        /// Harmony transpiler for TreeManager.UpdateData to implement expanded tree limits.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(TreeManager.UpdateData))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateDataTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            Logging.KeyMessage("transpiling TreeManager.UpdateData");

            FieldInfo m_trees = AccessTools.Field(typeof(TreeManager), nameof(m_trees));
            FieldInfo m_Buffer = AccessTools.Field(typeof(Array32<TreeInstance>), nameof(Array32<TreeInstance>.m_buffer));

            // Looking for hardcoded tree limit constants.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(MAX_TREE_COUNT))
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
        /// Harmony postfix for TreeManager.UpdateData to update tree and RenderGroup data.
        /// </summary>
        /// <param name="__instance">TreeManager instance.</param>
        [HarmonyPatch(nameof(TreeManager.UpdateData))]
        [HarmonyPostfix]
        private static void UpdateDataPostfix(TreeManager __instance)
        {
            // Local references.
            RenderManager renderManager = Singleton<RenderManager>.instance;
            int treeLayer = __instance.m_treeLayer;

            Logging.KeyMessage("updating RenderGroups for tree layer ", treeLayer);

            // 45x45 grid.
            for (int x = 0; x < 45; ++x)
            {
                for (int z = 0; z < 45; ++z)
                {
                    // This ensures that RenderGroups are created and initialized for all grid squares.
                    // The game doesn't actually always do this for the tree layer.
                    renderManager.UpdateGroup(x, z, treeLayer);
                }
            }
        }

        /// <summary>
        /// Harmony postfix to TreeManager.CreateTree to implement anarchy, snapping and scaling.
        /// </summary>>
        /// <param name="__instance">TreeManager instance.</param>
        /// <param name="tree">ID of newly-created tree.</param>
        [HarmonyPatch(nameof(TreeManager.CreateTree))]
        [HarmonyPostfix]
        private static void CreateTreePostfix(TreeManager __instance, uint tree)
        {
            // Set anarchy flag.
            TreeInstancePatches.SetAnarchyFlag(tree, AnarchyEnabled);

            // Record scale.
            TreeInstancePatches.ScalingArray[tree] = TreeToolPatches.Scaling;

            // Set fixed height sate if tree wasn't at standard height.
            if (TreeToolPatches.ElevationAdjustment != 0f)
            {
                __instance.m_trees.m_buffer[tree].m_flags |= (ushort)TreeInstance.Flags.FixedHeight;
            }
        }
    }
}