// <copyright file="NaturalResourceManagerPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl.Patches
{
    using AlgernonCommons.UI;
    using HarmonyLib;

    /// <summary>
    /// Harmony patches to implement forestry resource locking.
    /// </summary>
    [HarmonyPatch(typeof(NaturalResourceManager))]
    internal static class NaturalResourceManagerPatches
    {
        // Lock forestry status.
        private static bool s_lockForestry = false;

        /// <summary>
        /// Gets or sets a value indicating whether forestry resources should be locked.
        /// </summary>
        internal static bool LockForestry
        {
            get => s_lockForestry;

            set
            {
                s_lockForestry = value;

                // Update status panel.
                StandalonePanelManager<StatusPanel>.Panel?.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether lock forestry should automatically be enabled on load.
        /// </summary>
        internal static bool LockForestryDefault { get; set; } = false;

        /// <summary>
        /// Harmony prefix to NaturalResourceManager.TreesModified to implement forestry resource locking.
        /// </summary>>
        /// <returns>False (don't execute original method) if forestry locking is enabled, true otherwise.</returns>
        [HarmonyPatch(nameof(NaturalResourceManager.TreesModified))]
        [HarmonyPrefix]
        private static bool TreesModifiedPrefix() => !LockForestry;
    }
}