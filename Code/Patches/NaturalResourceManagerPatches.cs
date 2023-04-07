// <copyright file="NaturalResourceManagerPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl.Patches
{
    using HarmonyLib;

    /// <summary>
    /// Harmony patches to implement forestry resource locking.
    /// </summary>
    [HarmonyPatch(typeof(NaturalResourceManager))]
    internal static class NaturalResourceManagerPatches
    {
        /// <summary>
        /// Gets or sets a value indicating whether forestry resources should be locked.
        /// </summary>
        internal static bool LockForestry { get; set; } = false;

        /// <summary>
        /// Harmony prefix to NaturalResourceManager.TreesModified to implement forestry resource locking.
        /// </summary>>
        /// <returns>False (don't execute original method) if forestry locking is enabled, true otherwise.</returns>
        [HarmonyPatch(nameof(NaturalResourceManager.TreesModified))]
        [HarmonyPrefix]
        private static bool TreesModifiedPrefix() => !LockForestry;
    }
}