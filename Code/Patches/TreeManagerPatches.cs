// <copyright file="TreeManagerPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl.Patches
{
    using HarmonyLib;

    /// <summary>
    /// Harmony patches to implement tree snapping.
    /// </summary>
    [HarmonyPatch(typeof(TreeManager))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    internal static class TreeManagerPatches
    {
        /// <summary>
        /// Harmony postfix to TreeManager.CreateTree to implement tree snapping.
        /// </summary>>
        /// <param name="__instance">TreeManager instance.</param>
        /// <param name="tree">ID of newly-created tree.</param>
        [HarmonyPatch(nameof(TreeManager.CreateTree))]
        [HarmonyPostfix]
        private static void CreateTreePostfix(TreeManager __instance, uint tree)
        {
            // Set fixed height flag for tree.
            if (tree != 0)
            {
                __instance.m_trees.m_buffer[tree].FixedHeight = true;
            }

            // Record scale.
            TreeInstancePatches.ScalingArray[tree] = TreeToolPatches.Scaling;
        }
    }
}