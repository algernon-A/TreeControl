// <copyright file="TreeToolPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl.Patches
{
    using HarmonyLib;
    using static ToolBase;

    /// <summary>
    /// Harmony patch to implement tree anarchy.
    /// </summary>
    [HarmonyPatch(typeof(TreeTool))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    public static class TreeToolPatches
    {
        /// <summary>
        /// Gets or sets a value indicating whether tree anarchy is enabled.
        /// </summary>
        internal static bool AnarchyEnabled { get; set; } = false;

        /// <summary>
        /// Harmony pre-emptive prefix to TreeTool.CheckPlacementErrors to implement tree anarchy.
        /// </summary>
        /// <param name="__result">Original method result.</param>
        /// <returns>False (don't execute original method) if anarchy is enabled, true otherwise.</returns>
        [HarmonyPatch(nameof(TreeTool.CheckPlacementErrors))]
        [HarmonyPrefix]
        public static bool CheckPlacementErrorsPrefix(out ToolErrors __result)
        {
            // Set default original result to no errors.
            __result = ToolErrors.None;

            // If anarchy isn't enabled, go on to execute original method (will override default original result assigned above).
            return !AnarchyEnabled;
        }
    }
}